# Story 1.10: Exception-Wrapper-Pattern (Cross-Cutting)

**Status:** ready-for-dev
**Epic:** Epic 1 (Cross-Cutting-Infrastruktur)
**Size:** S
**Decisions referenced:** D-13 (Harmony-Exception-Skelett), CC-STORIES-02

## Story
Als Mod-Entwickler möchte ich ein **einheitliches Exception-Wrapper-Pattern** für Tick-Host-Code (GameComponentTick, MapComponentTick, GameComponentUpdate, MapComponentOnGUI) und Execution-Apply-Code — analog zum D-13-Harmony-Skelett — damit keine Exception in Vanilla-Callstack propagiert.

## Acceptance Criteria
1. `Source/Core/BotSafe.cs` erweitert um:
   - `SafeTick(Action body, string context)` — Tick-Host-Wrapper
   - `SafeApply<T>(Func<T, bool> body, T plan, string context)` — Execution-Apply-Wrapper
2. Bei Exception: BotErrorBudget + Log.Error + FallbackToOff (BotSafe.Get) analog D-13
3. **Staleness-Cooldown:** Bei 2 Exceptions/min in derselben Context-ID → Context-Poisoned für 10 min (Pawn-/Map-/Method-Level)
4. **Pflicht-AC-Template** für Stories die Tick-Host oder Execution-Code schreiben:
   - „Hauptkörper wrapt via `BotSafe.SafeTick/SafeApply(..., context)`"
   - „Unit-Test: wirft im Happy-Path nichts, fängt in Error-Path, BotErrorBudget wird dekrementiert"
5. **Retroaktive Pflicht-Edits in Stories:** 1.4 (SetMasterState), 1.5 (GameComponentUpdate), 2.7 (MapComponentOnGUI), 2.9 (Tick-Iterator), 5.4 (DraftController.Apply), 5.7 (Focused-Fire-Apply), alle Planner-Apply-Stories
6. Unit-Tests für BotSafe.SafeTick + SafeApply mit mock-Exception

## Tasks
- [ ] `BotSafe.cs` um `SafeTick` / `SafeApply` erweitern
- [ ] Context-Level-Poisoned-Set in `BotSafe` (Dictionary<string, int /*unlock_tick*/>)
- [ ] Unit-Tests
- [ ] Cross-Story-Documentation

## Dev Notes
**Kontext:** D-13 + CC-STORIES-02.
**Nehme an, dass:** `Log.Error` + `Messages.Message` sind reentrancy-safe (Vanilla-Konvention).
**Vorausgesetzt:** 1.3 (BotSafe existiert), 1.9 (Infrastruktur-Story).

## File List
| Pfad | Op |
|---|---|
| `Source/Core/BotSafe.cs` | modify |

## Testing
- Unit: SafeTick/SafeApply mit mock-Exception
- Integration: Story-1.5-KeyDown-Event wirft Exception → State bleibt konsistent

## Review-Gate
Code-Review gegen D-13-Analogie + CC-STORIES-02-Coverage.

## Transient/Persistent
Poisoned-Set ist **transient** (re-initialisiert bei LoadedGame/StartedNewGame analog BotErrorBudget).
