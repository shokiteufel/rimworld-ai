# Story 1.6 Visual-Review

**Verdict:** APPROVE-WITH-CHANGES

Reviewed: `Source/UI/ITab_Pawn_BotControl.cs`, `Languages/Deutsch/Keyed/PerPawnToggle.xml`, `Languages/English/Keyed/PerPawnToggle.xml`. Statisches Layout-Review, kein Live-Screenshot moeglich.

## UI-Konvention

- **WinSize 360x180**: Vanilla-ITabs liegen typisch bei 432x165 (Health/Needs) bis 630x430 (Gear). 360 ist am unteren Rand des sinnvollen Bereichs, 180 hoch ist vanilla-konform fuer einen Single-Toggle-Tab. OK.
- **Tab-Label „Bot"**: Drei Buchstaben, fitted in jede Tab-Bar selbst bei vielen DLC-Tabs. Gut. Abkuerzung-Risiko durch RimWorld auto-truncation: kein Problem.
- **ContentPadding 12px**: Vanilla nutzt meist 10-17px (z.B. `Window.StandardMargin = 18f`, ITab-internes Padding ~10-14px). 12 ist im Mittelfeld — OK.
- **Tab-Icon nicht gesetzt**: Vanilla-ITabs setzen kein Icon (nur Label). Konform.

## Layout-Qualitaet

- **CheckboxRowHeight 28px**: Vanilla `Widgets.CheckboxLabeled` rendert mit `Text.LineHeight ~ 22-24px` plus 24x24 Checkbox-Glyph. 28 gibt dem Glyph Atem — OK.
- **HelpText-Height 60px**: Bei `Text.SmallFontHeight ~18-22px` reichen 60px fuer 2-3 Zeilen. DE-Text aktuell ca. 145 Zeichen → bei 336px verfuegbarer Breite ergibt das 2-3 Wraps. Knapp aber ausreichend. Bei Inspector-Skalierung >100% (UI-Scale-Setting) bricht der Text auf 4 Zeilen → **Clip-Risiko**.
- **Spacing 8px Checkbox→Help**: Visuell genug Trennung. OK.
- **HelpText-Farbe (0.7,0.7,0.7)**: Vanilla nutzt fuer Hilfstexte `Color(0.6f,0.6f,0.6f)` (siehe `Widgets.NoneLabel`-Style). 0.7 ist heller — gegen den dunklen Vanilla-Background lesbar, aber leicht ueber Konvention. Akzeptabel.

## Label-Texte

- **„Spieler steuert" / „Player controls"**: Story spec sagt „Player Use". User-facing-Text divergiert bewusst — **das ist ok**: Aktiv-Verb („steuert") ist verstaendlicher als nominaler „Player Use". DE+EN parallel gewaehlt, semantisch deckungsgleich. Approved.
- **HelpText**: Klar, Default-State explizit erwaehnt („Standard: deaktiviert (Bot steuert)" / „Default: unchecked (bot controls)") — sehr gut, beantwortet Erstnutzer-Frage direkt.
- **Tab-Label „Bot"**: Minimalistisch, aber im Kontext eines Pawn-Inspectors klar — konkurriert mit „Health/Needs/Gear", da ist „Bot" die einzige nicht-vanilla-Quelle. OK.

## Findings

- **MED-1 (Konsistenz Story 1.4 vs 1.6)**: Story 1.4 nutzt „Bot State: Off/Advisory/On" (Modus des Bots), Story 1.6 nutzt „Spieler steuert" (Wer steuert). Semantisch unterschiedliche Achsen, daher kein direkter Sprach-Konflikt — **aber**: Beide UIs koennten gleichzeitig sichtbar sein, und der User koennte „Bot State: On" + „Spieler steuert: an" als widerspruechlich wahrnehmen. Empfehlung: HelpText um einen Halbsatz erweitern, der die Beziehung zum Master-Toggle klaert (z.B. „Wirkt nur wenn Master-Bot an ist.").
- **LOW-1 (HelpText-Clip bei UI-Scale 125%+)**: 60px reichen bei Default-Scale, knapp bei 125%, brechen bei 150%. Fix: Hoehe via `Text.CalcHeight(text, rect.width)` dynamisch berechnen statt hardcoded 60. Alternativ: `WinSize.y` auf 200 erhoehen als Puffer.
- **LOW-2 (HelpText-Farbe)**: 0.7 leicht ueber Vanilla-Konvention 0.6. Nicht kritisch, aber auf 0.65 oder Vanilla-Konstante `Widgets.SubtleMouseoverColor` aequivalent angleichen.
- **LOW-3 (Tab-Sichtbarkeit fuer Slaves)**: `IsVisible` filtert nur auf `Faction.IsPlayer && Humanlike` — Slaves sind Player-Faction, sehen also den Tab. Story-Intent („Colonists+Slaves") deckt das, aber dokumentiere das via Code-Comment als bewusste Wahl.

## Recommendation

APPROVE-WITH-CHANGES. Keine HIGH-Findings, Layout sitzt. **Vor Story-Done: MED-1 fixen** (HelpText-Erweiterung um Master-Toggle-Beziehung), **LOW-1 fixen** (dynamische HelpText-Hoehe oder WinSize.y=200). LOW-2/LOW-3 Polish nach Wahl. Code- und Lokalisierungs-Struktur sind sauber und vanilla-konform.
