# Story 1.5: Keybinding Ctrl+K

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle
**Size:** S
**Sprint:** 1
**Decisions referenced:** PRD FR-02 (Ctrl+K toggelt identisch zum Button), D-03 (Master-Toggle-Semantik)

---

## Story

Als **Spieler** möchte ich **mit `Ctrl+K` den Master-Toggle-State zyklieren** können, damit **ich die Mod ohne Maus-Navigation zum Top-Bar-Button umschalten kann — z. B. während eines Raids zügig auf `On` schalten**.

---

## Acceptance Criteria

1. **`Defs/KeyBindingDefs.xml`** enthält einen `KeyBindingDef` mit `defName: RimWorldBot_ToggleMaster`, `defaultKeyCodeA: K`, `modifierA: Control`, Label „Toggle RimWorldBot" (lokalisierbar über Language-Keys)
2. **Keybinding-Check im Update-Loop** — in `BotGameComponent.GameComponentUpdate()` (läuft bei jedem UI-Frame, **nicht** Tick): wenn `RimWorldBot_ToggleMaster.KeyDownEvent`, cycle `masterState` via `SetMasterState` (aus Story 1.4)
3. **Cycle-Reihenfolge identisch zum Button**: `Off → Advisory → On → Off` (AC 2 Epic 1)
4. **Keybinding in RimWorld-Settings-Menü konfigurierbar** (`Options → Keyboard Configuration`) — User kann Key ändern, persistiert in Vanilla-KeyBindings-Save
5. **Log-Eintrag identisch zu Button-Klick** (`[RimWorldBot] state changed: …` + DecisionLog)
6. **Keine Kollision** mit Vanilla-Keybindings (`Ctrl+K` ist unbelegt in Vanilla 1.5/1.6 — prüfen)
7. **Pause-kompatibel**: Ctrl+K funktioniert auch wenn Spiel pausiert (KeyBindingDef hat `doDesignatorInterrupt: false`)
8. **Exception-Wrapper** (HIGH-Fix, CC-STORIES-02): `GameComponentUpdate()`-Hauptkörper inkl. KeyDownEvent-Check wrapped in try/catch → `BotErrorBudget.Report("GameComponentUpdate", ex)` → bei ≥ 2 Exceptions/min: `FallbackToOff()` (via Story 1.10 `ExceptionWrapper`-Helper). **Begründung:** Update-Loop läuft pro Frame (~60 FPS); unbehandelte Exception blockiert sonst die gesamte UI-Refresh-Pipeline. Vanilla RimWorld hat keinen Top-Level-Try dort.

---

## Tasks

- [ ] `Defs/KeyBindingDefs.xml` anlegen mit `RimWorldBot_ToggleMaster`
- [ ] `BotGameComponent.GameComponentUpdate()` implementieren (override) mit `KeyBindingDef.Named("RimWorldBot_ToggleMaster").KeyDownEvent`-Check
- [ ] `Languages/Deutsch/Keyed/KeyBindings.xml` + `Languages/English/Keyed/KeyBindings.xml` mit Label-Keys (Vorgriff auf Story 1.8, minimale Sprach-Strings hier)
- [ ] Integration-Test TC-05-KEYBIND: Ctrl+K → State wechselt → TabWindow-Display aktualisiert
- [ ] Kollisions-Check: alle Vanilla KeyBindingDefs auflisten (via `DefDatabase<KeyBindingDef>.AllDefs`) → prüfen dass keine `Ctrl+K` hat

---

## Dev Notes

**Architektur-Kontext (v2.3):**
- `GameComponentUpdate()` ist der korrekte Hook für UI-Key-Events (läuft ~60 FPS statt 60 Ticks/sec). `GameComponentTick()` ist für Sim-Tick-Arbeit ungeeignet, weil KeyDownEvent in Pause-State verpasst werden könnte.
- **Kein Harmony-Patch** (AI-1): `KeyBindingDef`-Lookup + `KeyDownEvent`-Property ist Vanilla-API.

**Nehme an, dass:**
- `Ctrl+K` ist in Vanilla 1.5 und 1.6 unbelegt. Verifiziert durch Kollisions-Check in Tasks. Falls doch belegt: Fallback auf `Ctrl+Shift+K`.
- KeyBindingDef-Name `RimWorldBot_ToggleMaster` ist eindeutig (Vendor-Prefix-Konvention).

**Vorausgesetzt:**
- Story 1.4 ist done (`SetMasterState`-Helper existiert).

---

## File List

| Pfad | Operation | Zweck |
|---|---|---|
| `Defs/KeyBindingDefs.xml` | create | Keybinding-Registration |
| `Source/Data/BotGameComponent.cs` | modify | Add `GameComponentUpdate()` override |
| `Languages/Deutsch/Keyed/KeyBindings.xml` | create | DE-Labels |
| `Languages/English/Keyed/KeyBindings.xml` | create | EN-Labels |

---

## Testing

**Integration-Tests:**
- **TC-05-KEYBIND:** Ingame Ctrl+K drücken → State zyklisch wechseln → TabWindow zeigt korrekt.
- **TC-05-KEYBIND-PAUSE:** Spiel pausieren → Ctrl+K → State wechselt (Test dass Pause keine Blockade ist).
- **TC-05-KEYBIND-REBIND:** Options → Keyboard Configuration → Key umbinden auf `Ctrl+B` → neuer Key funktioniert, alter Key (Ctrl+K) ist frei.

---

## Review-Gate

- Code-Review gegen AI-1 (kein Patch), KeyBindingDef-Standard, Kollisions-Test-Ergebnis.
- Visual-Review: nicht relevant (keine neue UI — Keybinding ist im Vanilla-KeyConfig-Menü).

---

## Aufgelöste Entscheidungen

- **TQ-S5-01 resolved:** **`GameComponentUpdate()` statt `GameComponentTick()`** für KeyDownEvent-Check. Begründung: Update läuft pro Frame (~60 FPS), Tick nur pro Sim-Tick (manuell pausierbar). Keybinding soll in Pause funktionieren → Update ist die korrekte Ebene.
- **TQ-S5-02 resolved:** **`Ctrl+K` als Default**, Fallback `Ctrl+Shift+K` wenn Kollisions-Check positiv. Begründung: `K` intuitiv für „Kolonie-AI", `Ctrl`-Modifier vermeidet Kollision mit Single-Key-Bindings; Fallback-Option falls doch blockiert.
- **TQ-S5-03 resolved:** **Minimale Language-Keys in dieser Story** (KeyBindings.xml), volle Localization-Coverage in Story 1.8. Begründung: Keybinding ohne Label wäre fragmentary, aber nicht alle Languages-Files werden hier gebraucht.
