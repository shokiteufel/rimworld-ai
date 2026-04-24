# Story 3.4: Emergency-Handler E-BLEED

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** S

## Story
Als Mod-Entwickler möchte ich den **E-BLEED-Handler** implementieren, der bei Bleed-Invariant-Violation (I3) Rettung/Behandlung des verblutenden Pawns koordiniert — mit Pawn-Exclusivity-Lock (Architecture D-16 F-AI-02-Fix) damit Doctor nicht unter aktiver Intrusion ins Raid-Feuer läuft.

## Acceptance Criteria
1. `E_Bleed : EmergencyHandler` mit `BasePrio = 11`
2. `Eligibility`: `I3_Bleed.Violated == true` AND ≥ 1 Doctor (Medicine ≥ 3) verfügbar
3. `Score`: `base + (bleed_rate * 100) + (medical_supplies_available * 5)`; **−50 wenn E-INTRUSION aktiv UND Pawn außerhalb Base** (F-AI-02 kontext-sensitiv)
4. `Claim(pawns)`: bester Doctor (höchstes Medicine-Skill) + Rescue-Target-Pawn
5. `Apply(controller)`: `Rescue`-Job für Doctor (nur wenn Pawn unreachable sicher ist) ODER `Tend`-Job wenn bereits in Medical-Bed
6. **Pawn-Exclusivity-Lock**: Doctor wird für 60s nicht von anderen Handlern claimable
7. Unit-Tests: Intrusion-Context reduziert Score korrekt
8. Integration: Raid + bleedender Pawn → E-Intrusion gewinnt wenn Pawn unreachable

## Tasks
- [ ] `Source/Emergency/E_Bleed.cs`
- [ ] Score-Modifier: `is_under_intrusion && pawn_reachable` check
- [ ] Pawn-Exclusivity-Lock-Mechanismus (in `EmergencyResolver`)
- [ ] Rescue-vs-Tend-Entscheidung
- [ ] Unit-Tests
- [ ] Integration mit Raid-Simulation

## Dev Notes
**Architektur-Kontext:** F-AI-02 Fix (kontext-sensitiver Score), D-16.
**Nehme an, dass:** `pawn.health.hediffSet.BleedRateTotal` ist verlässlich.
**Vorausgesetzt:** Story 3.1, Story 3.2 (I3).

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_Bleed.cs` | create |
| `Source/Emergency/EmergencyResolver.cs` | modify (Pawn-Exclusivity-Lock) |

## Testing
- Unit: Score mit/ohne Intrusion-Context
- Integration: E-Intrusion + E-Bleed parallel → korrekte Wahl

## Review-Gate
Code-Review gegen F-AI-02-Fix, D-16 Utility-Scoring.
