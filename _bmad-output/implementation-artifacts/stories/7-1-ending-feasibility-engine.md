# Story 7.1: EndingFeasibility-Engine

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** L
**Decisions referenced:** D-06 (Opportunistisches Switching), D-17 (Hysterese + Phase-7-Commit), D-11 (keine Zeitvorgabe), F-DLC-01 (DLC-Filter)

## Story
Als Mod-Entwickler möchte ich **EndingFeasibility** als zentrale Engine: pro Ending (Ship/Journey/Royal/Archonexus/Void) wird ein `feasibility[0..1]`-Score berechnet basierend auf Colony-State, Tech-State, DLC-Verfügbarkeit.

## Acceptance Criteria
1. `EndingFeasibility.ComputeAll(ColonySnapshot, DlcCapabilities) → FeasibilityMap`
2. `FeasibilityMap` record mit Scores pro Ending als ImmutableDictionary
3. Ship-Score: Research-Progress, Steel/Components/Plasteel-Stock, AIPersonaCore-Verfügbarkeit
4. Journey-Score: Karawanen-Stärke, Weltkarten-Reise-Feasibility
5. Royal-Score: Imperium-Honor-Level, Stellarch-Bereitschaft (nur wenn Royalty)
6. Archonexus-Score: Wealth-Level, Transition-Items
7. Void-Score: Monolith-State, Dark-Study-Research (nur wenn Anomaly)
8. Endings ohne DLC werden ausgefiltert (feasibility undefined, nicht 0)
9. Unit-Tests mit 5 Colony-State-Profilen

## Tasks
- [ ] `Source/Analysis/EndingFeasibility.cs`
- [ ] `FeasibilityMap` record
- [ ] 5 per-Ending-Score-Methoden
- [ ] DlcCapabilities-Filter
- [ ] Unit-Tests

## Dev Notes
**Kontext:** §2.2, D-06, Ending-Pfade.md.
**Annahmen:** AIPersonaCore kann via Quest, Raid-Loot, Archaeology bekommen werden.
**Vorausgesetzt:** 1.3, 2.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Analysis/EndingFeasibility.cs` | create |

## Testing
Unit: 5 Profile. Integration: Full-Phase-6-Colony → Scores konvergent.

## Review-Gate
Code-Review gegen D-06, F-DLC-01, D-11.
