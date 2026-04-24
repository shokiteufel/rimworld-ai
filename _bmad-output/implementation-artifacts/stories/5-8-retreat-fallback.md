# Story 5.8: Retreat-Fallback (wenn Killpoint fällt)

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Retreat-Fallback-Logik**: wenn Killpoint überrannt wird oder 50%+ Pawns downed, automatische Retreat zum innersten Schutzraum (Panic-Room).

## Acceptance Criteria
1. `RetreatPlanner.CheckAndPlan(ColonySnapshot, ThreatReport) → DraftOrder?`
2. Trigger: Killpoint-Breached ODER ≥50% Pawns downed
3. Retreat-Ziel: Panic-Room (erkannt via Home-Area-Innerst, mind. 2×2 Stone-Walls)
4. Alle lebenden Pawns + Downed via Rescue
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/RetreatPlanner.cs`
- [ ] Panic-Room-Detection
- [ ] Downed-Rescue-Integration
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §8.1.
**Vorausgesetzt:** 5.3, 5.4.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/RetreatPlanner.cs` | create |

## Testing
Unit: Trigger-Conditions. Integration: Killpoint-Overrun-Scenario.

## Review-Gate
Code-Review gegen D-15, F-STAB-06.
