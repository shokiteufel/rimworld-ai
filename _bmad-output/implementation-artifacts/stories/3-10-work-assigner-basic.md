# Story 3.10: WorkAssigner Basic (Skill-Matching)

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** M
**Decisions referenced:** D-15, F-STAB-05 (Retry-Cap + Poisoned-Goals), F-STAB-04 (Mod-Konflikt-Scan)

## Story
Als Mod-Entwickler möchte ich den **`WorkPlanner` + `WorkPriorityWriter`** implementieren, der basierend auf Pawn-Skills + Phase-Goals Work-Priorities setzt (Priority 1-4 pro Pawn pro WorkType) — unter Berücksichtigung von Per-Pawn-Toggle und Mod-Kompat (Work Tab etc.).

## Acceptance Criteria
1. `WorkPlanner.Plan(PawnSnapshot[], PhaseGoals) → WorkPriorityPlan` mit `ImmutableDictionary<string UniqueLoadID, ImmutableDictionary<string WorkTypeDefName, int>>`
2. Priorities basieren auf: Pawn-Skill-Level + Passion + Goal-Priority (Cook für E-Food, Build für E-Shelter, etc.)
3. **Per-Pawn-Toggle-Respekt** (Story 1.6): Pawns mit `BotGameComponent.perPawnPlayerUse[uniqueID] == true` werden übersprungen
4. `WorkPriorityWriter.Apply(plan, map)` schreibt `pawn.workSettings.priorities` + **Read-After-Write-Check** (F-STAB-04): nach Write erneut lesen, bei Mismatch WARN-Log (Work-Tab-Mod-Konflikt)
5. **Retry-Cap + Poisoned-Goals** (F-STAB-05): wenn Write 3× in 60s für denselben Pawn fehlschlägt → Pawn in Poisoned-Set, 10min Unlock
6. Unit-Tests: Priority-Matrix für 3 Pawn-Skill-Profile
7. Integration: Colony mit Work-Tab-Mod aktiv → Read-After-Write-Check greift

## Tasks
- [ ] `Source/Decision/WorkPlanner.cs`
- [ ] `Source/Execution/WorkPriorityWriter.cs` mit Read-After-Write-Check
- [ ] Poisoned-Set + Unlock-Timer
- [ ] Per-Pawn-Toggle-Filter
- [ ] Unit-Tests
- [ ] Integration mit Work-Tab

## Dev Notes
**Architektur-Kontext:** §2.3/§2.4 + F-STAB-04 + F-STAB-05.
**Nehme an, dass:** `pawn.workSettings.priorities` ist ein `DefMap<WorkTypeDef, int>` (Vanilla).
**Vorausgesetzt:** Story 1.3, Story 1.6 (Per-Pawn-Toggle), Story 3.1 (Framework).

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/WorkPlanner.cs` | create |
| `Source/Execution/WorkPriorityWriter.cs` | create |

## Testing
- Unit: Priority-Matrix-Generation
- Integration: Work-Tab-Mod-Kompat, Poisoned-Pawn-Workflow

## Review-Gate
Code-Review gegen D-15, F-STAB-04, F-STAB-05.
