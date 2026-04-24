# Story 5.1: ThreatAssessment (Raid-Strength vs. Colony-Strength)

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** M

## Story
Als Mod-Entwickler möchte ich **ThreatAssessment** das pro Raid-Event Raid-Points vs. Colony-Strength berechnet, damit CombatCommander (Story 5.3) Fight-vs-Flee-Entscheidung treffen kann.

## Acceptance Criteria
1. `ThreatAssessment.Evaluate(RaidEvent, ColonySnapshot, PawnSnapshot[]) → ThreatReport`
2. `ThreatReport` record: `(double RaidPoints, double ColonyStrength, double Ratio, ThreatLevel Level)`
3. ColonyStrength = Σ(pawn.Shooting * 1.5 + pawn.Melee + equipment_quality)
4. ThreatLevel enum: `Negligible, Manageable, Hard, Overwhelming` (Ratios ≤ 0.3 / 0.7 / 1.2 / > 1.2)
5. Berücksichtigung von Drafted-Pawns und Killpoint-Readiness
6. Unit-Tests mit 4 Ratio-Bins

## Tasks
- [ ] `Source/Analysis/ThreatAssessment.cs`
- [ ] `ThreatReport` record
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Architecture §2.2, Mod-Leitfaden §8.1.
**Vorausgesetzt:** 2.1, 3.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Analysis/ThreatAssessment.cs` | create |

## Testing
Unit: 4 Bin-Cases.

## Review-Gate
Code-Review gegen §2.2, Identifier-only-Pattern.
