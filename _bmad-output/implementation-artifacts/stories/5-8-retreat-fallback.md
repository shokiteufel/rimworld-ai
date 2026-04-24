# Story 5.8: Retreat-Fallback (wenn Killpoint fällt)

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Retreat-Fallback-Logik**: wenn Killpoint überrannt wird oder 50%+ Pawns downed, automatische Retreat zum innersten Schutzraum (Panic-Room).

## Acceptance Criteria
1. **`RetreatPlanner` als Sub-Phase von `CombatCommander`** (HIGH-Fix): `RetreatPlanner` wird NICHT als unabhängiger Plan-Producer registriert, sondern als interner Entscheidungs-Sub-Tree in `CombatCommander` (Story 5.3). `CombatCommander.Plan` ruft `RetreatPlanner.ShouldRetreat(snapshot, report)` als Frühes-Branch im Decision-Tree — bei `true` liefert `RetreatPlanner.PlanRetreat()` den DraftOrder, der dann von CombatCommander als Gesamt-Plan zurückgegeben wird. Kein eigener `PlanArbiter`-Layer → eliminiert Race-Condition zwischen CombatCommander (Manageable→Fight) und RetreatPlanner (Fight→Retreat).
2. `RetreatPlanner.ShouldRetreat(ColonySnapshot, ThreatReport) → bool` + `RetreatPlanner.PlanRetreat(ColonySnapshot, ThreatReport) → DraftOrder`
3. Trigger: Killpoint-Breached ODER ≥50% Pawns downed
4. Retreat-Ziel: Panic-Room (erkannt via Home-Area-Innerst, mind. 2×2 Stone-Walls)
5. Alle lebenden Pawns + Downed via Rescue
6. **Keine parallele Claim-Konkurrenz**: RetreatPlanner nutzt denselben `E_Raid`-Lock (via CombatCommander) — kein eigener EmergencyHandler-Lock. Falls CombatCommander noch nicht aktiv (E-RAID inactive): RetreatPlanner liefert `null` → kein Fallback-Plan außerhalb E-RAID-Kontext.
7. Unit-Tests: ShouldRetreat-Trigger-Logic + PlanRetreat-Output
8. Integration: Killpoint-Overrun-Scenario → CombatCommander ruft RetreatPlanner → DraftOrder ist Retreat-Plan (kein Race)

## Tasks
- [ ] `Source/Decision/RetreatPlanner.cs`
- [ ] Panic-Room-Detection
- [ ] Downed-Rescue-Integration
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §8.1. RetreatPlanner als interner Sub-Tree von CombatCommander (Story 5.3), kein eigener Plan-Producer — verhindert Plan-Arbiter-Race.
**Vorausgesetzt:** 5.3 (CombatCommander ruft RetreatPlanner), 5.4.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/RetreatPlanner.cs` | create |

## Testing
Unit: Trigger-Conditions. Integration: Killpoint-Overrun-Scenario.

## Review-Gate
Code-Review gegen D-15, F-STAB-06.
