# Story 4.10: Skill-Grinding-Strategie

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Skill-Grinding** (Crafting-Loops für Skill-UP, Construction-Grind bei niedrig-Priority-Walls), damit Pawns bis Phase 6 auf Crafting/Construction ≥ 8 kommen.

## Acceptance Criteria
1. `SkillGrindPlanner.Plan(PawnSnapshot[], PhaseIndex) → WorkPriorityPlan`-Addenda
2. Pawns mit Passion + Level < Target bekommen erhöhte Priority auf entsprechenden WorkType
3. Opportunistisches Crafting: Craft-Stone-Block, Craft-Wood-Furniture für XP
4. Läuft nur wenn keine höheren Prio-Emergencies
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/SkillGrindPlanner.cs`
- [ ] Level-Target-Table pro Phase
- [ ] Unit-Tests

## Dev Notes
**Kontext:** PRD FR-Skill-Caps.md, Skill Cap.md.
**Vorausgesetzt:** 3.10.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/SkillGrindPlanner.cs` | create |

## Testing
Unit: Priority-Erhöhung für Passion-Skill < Target.

## Review-Gate
Code-Review gegen D-15.
