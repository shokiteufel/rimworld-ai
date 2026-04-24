# Story 1.4: Master-Toggle-Button (MainButtonDef + MainTabWindow)

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle
**Size:** M
**Sprint:** 1
**Decisions referenced:** D-03 (Zwei-Ebenen-Toggle: Master + Per-Pawn), D-08 (AI_OFF/AI_ADVISORY/AI_ON Semantik), D-13 (H7 raus, MainButtonDef via XML)

---

## Story

Als **Spieler** möchte ich **einen Toggle-Button in der Top-Bar der RimWorld-UI sehen und klicken können**, der **zwischen den States `Off` → `Advisory` → `On` → `Off` zykliert** und den aktuellen State visuell (Icon + Label) anzeigt, damit **ich die Mod jederzeit im Spiel de-/aktivieren kann ohne das Settings-Menü zu öffnen**.

---

## Acceptance Criteria

1. **`Defs/MainButtonDefs.xml`** existiert mit Snippet aus Architecture §10.2 (`defName: BotControl`, `tabWindowClass: RimWorldBot.UI.MainTabWindow_BotControl`, `order: 99`, `iconPath: UI/Buttons/BotIcon`, `minimized: false`). **Retroaktiv gefixt 2026-04-24:** `defaultHidden`-Feld ENTFERNT — existiert in RimWorld 1.6 MainButtonDef nicht und verursacht XML-Parse-Error. Namespace auf `RimWorldBot.UI.*` korrigiert (Folder-basierte Namespace-Konvention des Projekts).
2. **`MainTabWindow_BotControl : MainTabWindow`** in `Source/UI/MainTabWindow_BotControl.cs` implementiert `DoWindowContents(Rect inRect)` mit:
   - Zentriertes Label „Bot State: {currentState}" (Off/Advisory/On)
   - Drei `Widgets.ButtonText`-Buttons: „Off", „Advisory", „On" (jeweils aktiv, setzen `masterState` direkt)
   - Current-State ist highlighted (z. B. dickere Umrandung)
3. **`UI/Buttons/BotIcon.png`** Platzhalter-Texture (32×32) unter `Textures/UI/Buttons/BotIcon.png` (Path wird von RimWorld automatisch aufgelöst)
4. **State-Lesen/Schreiben** über `Current.Game.GetComponent<BotGameComponent>().masterState` (aus Story 1.3)
5. **Click-Cycle über Button-Tab-Icon** (nicht aus dem TabWindow heraus): Click auf Top-Bar-Button → default-`MainTabWindow`-Verhalten öffnet das Fenster. Alternative Zyklus-Klick nur im TabWindow selbst
6. **State-Persistenz** via BotGameComponent (Story 1.3 ExposeData verifiziert AC 5 aus Epic 1)
7. **Log-Eintrag bei State-Wechsel**: `Log.Message($"[RimWorldBot] state changed: {oldState} → {newState}")` — auch in `recentDecisions` via `Add(new DecisionLogEntry("state_change", …))`
8. **Kein Harmony-Patch** für den Button (D-13 H7 entfernt, nur MainButtonDef via XML)
9. **Button erscheint korrekt** in Top-Bar neben Vanilla-Buttons (Menu, Architect, Work, etc.) bei `order: 99`
10. **Kein Crash** bei raschem Klicken oder State-Wechsel während Pause

---

## Tasks

- [ ] `Defs/MainButtonDefs.xml` anlegen (exakt Architecture-§10.2-Snippet)
- [ ] `Textures/UI/Buttons/BotIcon.png` als 32×32 Platzhalter (neutraler Hintergrund, Bot-Symbol oder „B")
- [ ] `Source/UI/MainTabWindow_BotControl.cs` implementieren mit `DoWindowContents`
- [ ] State-Change-Helper-Methode `SetMasterState(ToggleState newState)` in BotGameComponent (aus Story 1.3), loggt + persistiert + feuert DecisionLog
- [ ] Integration-Test TC-04-TOGGLE-BUTTON: Button klicken, States zyklieren, Log verifiziert

---

## Dev Notes

**Architektur-Kontext (v2.3):**
- `MainTabWindow_BotControl` ist reine UI-Darstellung — keine Bot-Logik (die kommt in späteren Stories). Nur State-Read/Write.
- **Keine Harmony-Patches** in dieser Story. D-13 hat H7 entfernt, Top-Bar-Button kommt über XML-`MainButtonDef`.
- `SetMasterState`-Helper kapselt Logging + Persistenz + DecisionLog — damit weitere Call-Sites (Story 1.5 Keybinding, Story 1.7 Settings) konsistent laufen.

**Nehme an, dass:**
- `MainTabWindow.requestedTabSize` = standard (wird von RimWorld aus Button-Größe abgeleitet).
- `iconPath: UI/Buttons/BotIcon` wird relativ zu `Textures/`-Root aufgelöst (RimWorld-Konvention).
- Drei State-Buttons im TabWindow sind UI-ausreichend — keine Dropdown oder Slider nötig für 3 Werte.

**Vorausgesetzt:**
- Story 1.3 ist done (`BotGameComponent.masterState` existiert und ist persistiert).

---

## File List

| Pfad | Operation | Zweck |
|---|---|---|
| `Defs/MainButtonDefs.xml` | create | Top-Bar-Button-Registration |
| `Source/UI/MainTabWindow_BotControl.cs` | create | TabWindow-UI mit State-Zyklus |
| `Textures/UI/Buttons/BotIcon.png` | create | Button-Icon Platzhalter |
| `Source/Data/BotGameComponent.cs` | modify | Add `SetMasterState(ToggleState)` Helper |

---

## Testing

**Unit-Tests:**
- `SetMasterState`: State-Wechsel feuert DecisionLog mit korrektem Kind.

**Integration-Tests:**
- **TC-04-TOGGLE-BUTTON:** Button sichtbar → TabWindow öffnet → drei Buttons klickbar → State wechselt → Log-Message + DecisionLog-Entry verifiziert.
- **TC-04-TOGGLE-ROUNDTRIP:** State auf Advisory setzen → Save → Load → State bleibt Advisory (validiert Story-1.3-Persistenz).

---

## Review-Gate

- UI-Review: Button sichtbar, TabWindow readable, kein Overflow.
- Code-Review gegen AI-1 (kein Patch), D-13, AC-Coverage.
- Visual-Review beim Design-Critic-Watchdog wenn DEV-Phase aktiv.

---

## Aufgelöste Entscheidungen

- **TQ-S4-01 resolved:** **Drei getrennte Buttons für `Off`/`Advisory`/`On`** statt Single-Toggle mit Cycle. Begründung: User-Intent direkt sichtbar (kein „wo bin ich gerade, was kommt als nächstes?"-Rätsel); State-Display im Label ersetzt Cycle-Preview.
- **TQ-S4-02 resolved:** **`order: 99`** für MainButtonDef. Begründung: platziert Button am rechten Rand der Top-Bar (hohe order-Werte werden rechts gerendert), vermeidet Kollision mit Vanilla-Buttons bei `order: 1..10`.
- **TQ-S4-03 resolved:** **Platzhalter-Icon 32×32**, finales Icon in Story 8.x. Begründung: konsistent mit TQ-S1-03, Asset-Design ist Polish-Phase.
