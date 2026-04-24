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
6. **Read-After-Write-Check** (HIGH-Fix, CC-STORIES-10, F-STAB-04): nach jeder `pawn.drafter.Drafted = targetValue`-Zuweisung sofortiges Read-Back: `if (pawn.drafter.Drafted != targetValue) → WARN-Log + Retry 1× nach 60 Ticks`. Bei 2. Mismatch: Pawn in `BotGameComponent.poisonedPawns: HashSet<string>` eintragen (F-STAB-04 Poisoned-Set analog Story 3.10) + DecisionLog `drafter-mutation-failed` (auto-pinned). Grund: Combat Extended, CAI 5000 und Simple Sidearms patchen `Pawn_DraftController.Drafted`-Setter — ohne Read-Back wäre der Bot-State desynchronisiert.
7. Unit-Tests (mit Pawn-Mock): Draft/Undraft + Mismatch-Simulation (Mock-Setter der Wert nicht übernimmt) → Retry-Path + Poisoned-Set-Eintrag
8. Integration: DraftOrder mit 3 Pawns → alle gedrafted, Read-Back bestätigt
9. **Exception-Wrapper** (HIGH-Fix Round-2-Stability, CC-STORIES-02): `DraftController.Apply(order, map)`-Hauptkörper + Per-Pawn-Mutate-Loop wrapped via Story 1.10 `ExceptionWrapper.Execution(...)`. Bei 2 Exceptions/min → `FallbackToOff()`.

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
