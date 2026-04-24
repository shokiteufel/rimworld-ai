# Story 3.4: Emergency-Handler E-BLEED

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** S

## Story
Als Mod-Entwickler möchte ich den **E-BLEED-Handler** implementieren, der bei Bleed-Invariant-Violation (I3) Rettung/Behandlung des verblutenden Pawns koordiniert — mit Pawn-Exclusivity-Lock (Architecture D-16 F-AI-02-Fix) damit Doctor nicht unter aktiver Intrusion ins Raid-Feuer läuft.

## Acceptance Criteria
1. `E_Bleed : EmergencyHandler` mit `BasePrio = 11`, `LockPriority = 90` (aus Story 3.1 CC-STORIES-06 Matrix)
2. `Eligibility`: `I3_Bleed.Violated == true` AND ≥ 1 Doctor (Medicine ≥ 3) verfügbar. **Eligibility = false (nicht Penalty)** (HIGH-Fix) wenn E-INTRUSION aktiv UND Target-Pawn außerhalb Base/Home-Region — damit Doctor nicht ins Raid-Feuer läuft. Penalty -50 ist zu weich, kann bei starkem Bleed-Rate übersteuert werden.
3. `Score`: `base + (bleed_rate * 100) + (medical_supplies_available * 5)` (Modifier nur wenn Eligibility = true)
4. `Claim(pawns)`: bester Doctor (höchstes Medicine-Skill) + Rescue-Target-Pawn via `EmergencyResolver.pawnClaims` (Story 3.1 Framework-Lock)
5. `Apply(controller)`: **`JobDefOf.TendPatient`** (HIGH-Fix, nicht `Tend`) als Job-Def für Treatment; **`JobDefOf.Rescue`** wenn Pawn downed und nicht in Medical-Bed. `TendPatient` ist verifizierter Def-Name (nicht `Tend`, was zu Compile-Error führt — CC-STORIES-08).
6. **Pawn-Exclusivity-Lock** via Story 3.1 CC-STORIES-06 Framework: `LockPriority = 90`, `LockDurationTicks = 3600` (60s); Re-Claim nur durch E-RAID (LockPriority=100) möglich
7. Unit-Tests: Intrusion-Eligibility-Flip verifizieren (false statt Score-Penalty)
8. Integration: Raid + bleedender Pawn außerhalb Base → E-BLEED Eligibility=false → Doctor bleibt bei E-RAID-Claim

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
