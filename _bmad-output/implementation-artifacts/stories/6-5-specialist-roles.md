# Story 6.5: Spezialisten-Rollen (Crafter/Medic/Researcher)

**Status:** ready-for-dev
**Epic:** Epic 6 — Industrialization
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Specialist-Role-Assignment**: per-Pawn Specialist-Flag (via Story-1.6-ähnlicher Config pro Pawn: Crafter/Medic/Researcher/Combat), WorkPriorities entsprechend fokussiert.

## Acceptance Criteria
1. `Specialization` enum: None, Crafter, Medic, Researcher, Combat
2. `BotGameComponent.pawnSpecializations: Dictionary<string UniqueLoadID, Specialization>`
3. `WorkPlanner` priorisiert Specialization-WorkTypes stark (8 statt 4 Priority)
4. Settings-Panel (Story 1.7) bekommt Specialization-Dropdown pro Pawn
5. Unit-Tests
6. **Schema-Bump** (HIGH-Fix Round-2-Stability, CC-STORIES-01): `pawnSpecializations` in `BotGameComponent` ist neues Feld → via Story 1.9 `SchemaVersionRegistry` einen Eintrag `BotGameComponent.pawnSpecializations` anlegen; Migrate setzt leeres Dict (Specialization=None default für alle Pawns).

## Tasks
- [ ] `Source/Data/Specialization.cs` enum
- [ ] BotGameComponent-Erweiterung (persistent)
- [ ] WorkPlanner-Integration
- [ ] Settings-Panel-UI
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 6.
**Vorausgesetzt:** 1.6, 1.7, 3.10.

## File List
| Pfad | Op |
|---|---|
| `Source/Data/Specialization.cs` | create |
| `Source/Data/BotGameComponent.cs` | modify |
| `Source/Decision/WorkPlanner.cs` | modify |
| `Source/UI/SettingsRenderer.cs` | modify |

## Testing
Unit: Priority-Matrix mit Specialization.

## Review-Gate
Code-Review gegen D-14 (UniqueLoadID), D-15.
