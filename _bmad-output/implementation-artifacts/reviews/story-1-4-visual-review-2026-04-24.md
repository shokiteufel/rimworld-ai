# Story 1.4 Visual-Review

**Verdict:** APPROVE-WITH-CHANGES

Statische UI-Spec-Review (keine Live-Screenshots verfügbar). Code-Analyse gegen RimWorld-UI-Konventionen + Asset-Inspektion.

## UI-Konvention

- `order=99` platziert den Button rechts hinter allen Vanilla-Buttons (Menu/Architect/…/History laufen 1–10, DLC-Buttons bis ~20). Isolation ist akzeptabel und bewusst: signalisiert „Mod-Eintrag, nicht Teil der Core-Kolonie-Verwaltung". Konform zu anderen prominenten Gameplay-Mods (z.B. RimHUD, Dubs).
- `tabWindowClass` korrekt registriert, kein Harmony-Patch nötig — folgt D-13 aus decisions.md.
- `defaultHidden=false` + `minimized=false` ist für First-Install sinnvoll (User muss die Mod sofort sehen).
- `RequestedTabSize 360×140` ist **kompakter als Vanilla-Defaults** (meist 400–600×200–400). Für einen reinen 3-State-Toggle mit Label ist das vertretbar — nicht jeder TabWindow muss Architect-Größe haben. OK.

## Layout-Qualität

- Padding 10px + ButtonSpacing 8px: sauber, folgt Vanilla-Verhältnis (Vanilla nutzt oft 10/6-10).
- Label center-aligned mit `Text.Anchor` + korrektem Restore — sauber (kein globaler State-Leak).
- Buttons zentriert via `(inRect.width - buttonsTotalWidth)/2` — horizontal-ausgewogen.
- **MED-Finding**: `buttonsY = labelRect.yMax + Padding + 4f` — das magische `+4f` ist undokumentiert. Mit RowHeight=32 + Padding=10 + Label-RowHeight=32 + Padding=10 + 4 + ButtonHeight=32 = 120px von 140px TabSize. 20px Bottom-Margin, aber asymmetrisch (10 top vs ~20 bottom). Entweder `+ 4f` weg oder als Konstante `ExtraGap`.
- **LOW-Finding**: Active-Indikator wird **hinter** dem Button gezeichnet (`Widgets.DrawBoxSolid` vor `ButtonText`). 2px ExpandedBy heißt: nur 2px-Rand sichtbar um den Vanilla-Button herum. Das ist subtil — gut wenn Button selbst seine Form hält, aber bei alpha=0.5 auf dunkel-grauem RimWorld-Background schlecht kontrastierend. RGB(1, 0.85, 0.2, 0.5) ergibt effektiv einen gedämpft-orangen Schimmer, nicht das „helles Gelb" das vermutet wird. Empfehlung: alpha auf 0.85 oder `Widgets.DrawBox(outline, 2)` mit `GUI.color = Color.yellow` für eine echte Outline statt Solid-Füllung.

## Icon-Design

- **HIGH-Finding**: `BotIcon.png` ist **140 Bytes groß**. Für 32×32 RGBA mit einem Glyph erwartet man 300–800 B (PNG-komprimiert). 140B entspricht einem nahezu leeren oder monochromen 1-Farbe-PNG. Preview-Rendering bestätigt den Verdacht: kein erkennbarer Glyph sichtbar. Entweder fehlt das 'B' ganz, oder es ist als weiß-auf-weiß/transparent-auf-transparent erzeugt worden. **Muss visuell im Game verifiziert werden** — wenn Vanilla-MainButton-Hintergrund grau ist und das Icon voll-transparent/voll-weiß, ist der Button unsichtbar oder nicht unterscheidbar.
- Erwartung Vanilla-Konvention: bold Glyph, 60–80% Pixel-Fill, leichte AA-Ränder, reinweiß (#FFFFFF) auf α=0 Background. Bei 32×32 braucht ein 'B' mindestens 18×24px Glyph-Bereich um lesbar zu sein.

## Accessibility

- Labels „Off/Advisory/On" mit `Bot State: X`-Header sind klar — kein Ambiguitätsproblem.
- Tri-State gut auf drei diskrete Buttons gemappt (besser als ein Cycle-Button).
- **LOW-Finding**: keine Tooltips auf den Buttons. `TooltipHandler.TipRegion(rect, "…")` würde Advisory-Semantik erklären (ist die Mod passiv beobachtend? greift sie nie ein?). Besonders für i18n (DE/EN Ziel) hilfreich.
- Keyboard-Navigation: Vanilla ignoriert Tab-Focus in MainTabWindows, das ist konsistent — kein Finding.
- **LOW-Finding**: bei UI-Scaling >100% (Vanilla-Option) skaliert `RequestedTabSize` nicht automatisch — Button-Text könnte bei 150% Scale überlaufen. `ButtonWidth=90` bei 150% Scale und längeren Localization-Strings („Beratend"=8 Zeichen, „Advisory"=8, ok; aber „Aus/Off" ok) vermutlich grenzwertig aber tragbar.

## Findings

### HIGH
- **H1**: `BotIcon.png` mit 140B zu klein für sichtbares Glyph. Asset neu erstellen (32×32 RGBA, weißes bold 'B' bei ~70% Pixel-Fill) und visuell im Game verifizieren.

### MED
- **M1**: `buttonsY + 4f` undokumentiertes Magic-Offset. Als benannte Konstante extrahieren oder entfernen; vertikales Padding symmetrisch ausbalancieren (10/10 statt 10/~20).

### LOW
- **L1**: Active-State-Outline mit alpha=0.5 zu dezent. `Widgets.DrawBox` mit solider Gelb-Linie statt halbtransparenter Fläche.
- **L2**: Keine Tooltips auf den State-Buttons (Advisory-Semantik nicht selbsterklärend).
- **L3**: Bei UI-Scale >100% mögliche Text-Overflow-Risiken in Lokalisierungen — nach i18n-Story prüfen.

## Recommendation

APPROVE-WITH-CHANGES. H1 ist blockend (unsichtbares Icon = toter Button in Top-Bar). M1 und L1/L2/L3 per Guardian-Regel 4 vor `done`-Status fixen — keine Cherry-Picks. Layout-Grundstruktur, State-Machine-Anbindung und Def-Registrierung sind sauber und konform. Nach Icon-Rebuild + Fixes: Re-Review ohne neue Runde nötig (Sichtprüfung Icon + Code-Diff reicht).
