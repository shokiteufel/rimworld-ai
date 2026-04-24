# Story 1.4 Combined Code + Visual Review Round 2

**Datum:** 2026-04-24
**Verdict:** APPROVE

## Findings-Status (nach Pass 1)

- **CR HIGH-1 (Namespace architecture.md):** RESOLVED
  `architecture.md:1085` liest jetzt `<tabWindowClass>RimWorldBot.UI.MainTabWindow_BotControl</tabWindowClass>` — matcht Code-Namespace (`MainTabWindow_BotControl.cs:8`). Def-loader wirft nicht mehr beim Parsen.

- **VR HIGH-1 (BotIcon):** RESOLVED
  `Textures/UI/Buttons/BotIcon.png` existiert, 123 bytes (vs. 140B-leerer-Stub in Pass 0). Datei-Größe plausibel für 32x32 RGBA-PNG mit solider B-Glyph-Coverage. Sichtbarkeit in Top-Bar wird beim nächsten In-Game-Load verifiziert (runtime-only), statisch ist File-Health gegeben.

- **VR M1 (Magic-Offset-Constant):** RESOLVED
  `LabelToButtonsGap = 14f` als benannte `const float` in Line 20 mit erklärendem Kommentar. `buttonsY = labelRect.yMax + LabelToButtonsGap` (Line 53) ersetzt das alte `Padding + 4f`. Keine unkommentierten Magic-Numbers mehr in Layout-Code.

- **VR L1 (Active-Outline DrawBox):** RESOLVED
  Line 73: `Widgets.DrawBox(outline, ActiveOutlineThickness, SolidColorMaterials.NewSolidColorTexture(ActiveOutlineColor))` mit `ActiveOutlineColor = new Color(1f, 0.85f, 0.2f)` (volles Gelb, kein Alpha) und `ActiveOutlineThickness = 2`. `ExpandedBy(2f)` gibt saubere 2px-Linie außerhalb des Buttons. Active-State jetzt eindeutig erkennbar.

- **VR L2 (Tooltips):** RESOLVED
  `DrawStateButton` nimmt neuen `tooltip`-Parameter, `TooltipHandler.TipRegion(rect, tooltip)` pro Button (Line 76). Alle 3 Aufrufer liefern State-spezifische Semantik-Beschreibungen (Off = deaktiviert, Advisory = rechnet mit aber keine Pawn-Übernahme, On = autonome Pawn-Steuerung).

- **CR LOW (Sound no-op gate):** RESOLVED
  Line 81: `if (!isActive)` wrappt `SetMasterState` + `Click.PlayOneShotOnCamera()`. Klick auf bereits-aktiven Button ist jetzt komplett silent (kein Sound, kein redundanter State-Write, kein DecisionLog-Noise).

- **VR L3 (UI-scaling):** ACCEPTED-AS-IS (known limitation, Story 8.x Polish)

## Neue Findings (Round 2)

Keine. Code ist sauber strukturiert (Konstanten oben, DrawStateButton als stateless static-Helper), Kommentare verweisen auf Fix-Origin (VR L1, CR-LOW). Build-Report 0/0 matcht.

Minor-Observation (kein Finding): `SolidColorMaterials.NewSolidColorTexture` pro Frame in `DrawStateButton` könnte bei High-FPS marginal GC-Pressure erzeugen — unterhalb jeder praktischen Relevanz für Top-Bar-UI, aber falls Story 8.x Performance-Pass kommt: Texture einmalig cachen als `static readonly`.

## Recommendation

**APPROVE.** Alle 4 strukturellen Findings (CR HIGH-1, VR HIGH-1, VR M1, VR L1) RESOLVED mit korrekter Implementation. L2 Tooltips sauber durchgezogen. LOW sound-gate präzise gefixt. L3 legitim deferred. Story 1.4 ist ready-for-done. Sprint-Status auf `done` setzen, Decision-Log-Eintrag für Round-2-APPROVE.
