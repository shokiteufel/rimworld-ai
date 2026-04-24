# Story 3.11: Phase 1 Goals + Exit-Conditions

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** M
**Decisions referenced:** D-11, D-26, F-AI-07 (Regression-Detector)

## Story
Als Mod-Entwickler möchte ich **Phase 1 (Basic Stability)** als `PhaseDefinition` implementieren — Hütte, Kleidung, Anbau — mit Regression-Detector der Rückschritte (z. B. Shelter-Verlust) erkennt.

## Acceptance Criteria
1. `Phase1_BasicStability : PhaseDefinition`
2. **Goals**:
   - G1.1: Solid Shelter (Stone oder Reinforced-Wood mit Möbeln)
   - G1.2: Growing-Zone (mind. 50 tiles, Potato/Rice je Biome)
   - G1.3: 2+ Pawns mit Cooking-Skill ≥ 4
   - G1.4: Stockpile-Zone mit geregelter Storage-Priority
3. **Exit-Conditions**: G1.1–G1.4 done (inkl. `skippedNonCriticalGoals` aus Story 3.7 Stuck-Pattern) UND `stableCounter >= 2` (D-26) UND **`EmergencyResolver.ActiveEmergencies.Count == 0`** (MED-Fix, CC-STORIES-12, F-AI-01: Emergency aktiv blockt Phase-Transition)
4. **Regression-Detector** (F-AI-07): `GoalHealthMonitor` prüft alle 5000 Ticks ob completed Goals regredieren — wenn ja, Goal retriggert
5. **Launch-Critical**: alle 4 Goals
6. Unit-Tests Exit + Regression
7. Integration: Phase 1 → Shelter wird zerstört → G1.1 regrediert → Priority-Flush

## Tasks
- [ ] `Source/Phases/Phase1_BasicStability.cs`
- [ ] Growing-Zone-Detection in ColonySnapshot
- [ ] Stockpile-Zone-Check
- [ ] Regression-Monitor (GoalHealthMonitor)
- [ ] Unit-Tests + Integration

## Dev Notes
**Architektur-Kontext:** D-11, §7.2 GoalHealthScore-Formel, F-AI-07.
**Nehme an, dass:** `Zone_Growing` und `Zone_Stockpile` sind Vanilla-Klassen.
**Vorausgesetzt:** Story 3.7, Story 3.8, Story 3.9.

## File List
| Pfad | Op |
|---|---|
| `Source/Phases/Phase1_BasicStability.cs` | create |
| `Source/Phases/GoalHealthMonitor.cs` | create |

## Testing
- Unit: Regression-Trigger bei Goal-Score-Drop
- Integration: Phase 1 → destructive event → Regression sichtbar

## Review-Gate
Code-Review gegen D-11, F-AI-07, F-AI-16 (Launch-Critical).
