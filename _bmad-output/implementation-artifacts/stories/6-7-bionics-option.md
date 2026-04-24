# Story 6.7: Bionics-Option (optional, wenn Ressourcen genügen)

**Status:** ready-for-dev (optional)
**Epic:** Epic 6 — Industrialization
**Size:** S

## Story
Als Mod-Entwickler möchte ich **optionale Bionics-Installation**: wenn Steel/Plasteel-Surplus + Bionic-Parts verfügbar, Bionics-Upgrades für Top-Pawns (Gladiators/Crafters).

## Acceptance Criteria
1. `BionicsPlanner.Plan(ColonySnapshot, PawnSnapshot[]) → BionicPlan`
2. Nur aktiv wenn Steel+Plasteel-Stock > 500 und Components > 20
3. Target-Pawns: Combat-Specialists + Crafter-Specialists
4. Bionic-Priorität: Bionic-Arm > Bionic-Leg > Bionic-Eye
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/BionicsPlanner.cs`
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 6. Status `optional` in sprint-status.yaml (nicht launch-critical).
**Vorausgesetzt:** 6.5, 6.6.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/BionicsPlanner.cs` | create |

## Testing
Unit: Trigger bei Surplus.

## Review-Gate
Code-Review gegen D-15, non-launch-critical.
