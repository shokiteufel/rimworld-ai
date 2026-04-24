# Story 1.11: Plan-Arbiter (AI-7 Layer-Präzedenz, Cross-Cutting)

**Status:** ready-for-dev
**Epic:** Epic 1 (Cross-Cutting-Infrastruktur)
**Size:** M
**Decisions referenced:** D-22 (AI-7 Emergency > Override > PhaseGoal), D-15 (Plan/Apply), D-23 (Plan-Schema), CC-STORIES-04

## Story
Als Mod-Entwickler möchte ich einen **zentralen Plan-Arbiter** der konkurrierende Plan-Producer (Emergency-Handler, Override, PhaseGoal, Combat, Skill-Grind) nach AI-7-Layer-Präzedenz sortiert und zu einem einzigen applyable Plan pro Typ merged — damit Write-Race-Conditions auf Pawn-State ausgeschlossen sind.

## Acceptance Criteria
1. `Source/Decision/PlanArbiter.cs` mit:
   - `ArbitrateDraftOrder(producers: IEnumerable<(DraftOrder plan, PlanLayer layer)>) → DraftOrder`
   - `ArbitrateWorkPriorityPlan(producers) → WorkPriorityPlan`
   - `ArbitrateBuildPlan(producers) → BuildPlan`
   - `ArbitrateBillPlan(producers) → BillPlan`
2. `PlanLayer` enum: `Emergency, Override, PhaseGoal, SkillGrind, Default`
3. Präzedenz (höhere Layer überschreiben niedrigere pro Pawn/Cell/Workbench): `Emergency > Override > PhaseGoal > SkillGrind > Default`
4. **Merge-Semantik** (nicht Last-Write-Wins): pro Pawn-UniqueLoadID → höchste Layer gewinnt; Pawn-Exclusivity-Lock (aus 3.1) wird respektiert
5. DecisionLog-Eintrag bei Arbitration-Conflict mit Breakdown
6. Unit-Tests: 3 Producer mit overlapping Pawns, Emergency gewinnt
7. **Retroaktive Integration** in BotController-Tick-Flow (§3): nach allen Planner.Plan()-Calls → Arbiter merged → Apply

## Tasks
- [ ] `Source/Decision/PlanArbiter.cs`
- [ ] 4 Arbitrate-Methoden pro Plan-Typ
- [ ] `PlanLayer` enum
- [ ] Unit-Tests für 5 Konfliktszenarien
- [ ] BotController-Tick-Integration

## Dev Notes
**Kontext:** AI-7 (D-22), CC-STORIES-04. Löst Multi-Producer-Race-Conditions systematisch.
**Nehme an, dass:** Plan-Layer kann per Produzent hartkodiert werden (EmergencyResolver = Emergency, WorkPlanner = PhaseGoal, etc.) — keine Laufzeit-Klassifikation nötig.
**Vorausgesetzt:** 1.3 (Plan-Types), 3.1 (Pawn-Exclusivity-Lock).

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/PlanArbiter.cs` | create |
| `Source/Decision/PlanLayer.cs` | create (enum) |

## Testing
- Unit: 5 Konflikt-Szenarien (Emergency vs. PhaseGoal, Override vs. PhaseGoal, Emergency + Exclusivity-Lock, Pool-gleichem Layer, Empty-Producer-List)
- Integration: BotController-Tick mit 3 parallelen Producern

## Review-Gate
Code-Review gegen D-22 AI-7 strict, Merge-Semantik korrekt (kein Last-Write-Wins).

## Transient/Persistent
PlanArbiter ist **stateless** (pro Tick neu aufgerufen) — nichts zu persistieren.
