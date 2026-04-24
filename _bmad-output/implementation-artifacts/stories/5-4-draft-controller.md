# Story 5.4: DraftController (Apply-Seite für DraftOrder)

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** S

## Story
Als Mod-Entwickler möchte ich **`DraftController` als Execution-Klasse** die DraftOrder (aus 5.3) appliziert via `pawn.drafter.Drafted = true/false` und Retreat-Job setzt.

## Acceptance Criteria
1. `DraftController.Apply(DraftOrder, Map)` in `Source/Execution/DraftController.cs`
2. Für jede Pawn-UniqueLoadID in `order.Draft`: resolve zu Pawn → `pawn.drafter.Drafted = true`
3. Undraft analog
4. Retreat-Point: wenn gesetzt, alle gedrafteten Pawns bekommen Move-Job zu RetreatPoint
5. Skip wenn Pawn destroyed/dead/null (F-STAB-06)
6. Unit-Tests (mit Pawn-Mock)
7. Integration: DraftOrder mit 3 Pawns → alle gedrafted

## Tasks
- [ ] `Source/Execution/DraftController.cs`
- [ ] Pawn-Resolve via `Find.Maps.SelectMany(...).FirstOrDefault(...)` (F-RW4-03)
- [ ] Retreat-Job via `JobMaker.MakeJob(JobDefOf.Goto, retreatCell)`
- [ ] Unit-Tests
- [ ] Integration

## Dev Notes
**Kontext:** §2.4 Execution, F-STAB-06, F-RW4-03.
**Vorausgesetzt:** 5.3.

## File List
| Pfad | Op |
|---|---|
| `Source/Execution/DraftController.cs` | create |

## Testing
Unit: Draft/Undraft. Integration: Order-Apply auf real Map.

## Review-Gate
Code-Review gegen §2.4, Defensive-Annahmen.
