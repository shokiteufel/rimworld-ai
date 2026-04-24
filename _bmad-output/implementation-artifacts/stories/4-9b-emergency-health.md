# Story 4.9b: Emergency-Handler E-HEALTH

**Status:** ready-for-dev
**Epic:** Epic 4
**Size:** S
**Decisions referenced:** D-16, CC-STORIES-06, CC-STORIES-07

## Story
Als Mod-Entwickler möchte ich **E-HEALTH** implementieren: bei Hp<0.5 pro Pawn Doctor + Medicine-Use-Priorisierung.

## Acceptance Criteria
1. `E_Health : EmergencyHandler` mit `BasePrio = 9`, `LockPriority = 80`
2. Eligibility: `I7_Health.Violated`
3. Score: `base + (avg_hp_deficit * 80) + (bleeding_pawns * 40)`
4. Apply: Doctor-Claim + Medicine-Craft-BillPlan (wenn Medicine<5); `JobDefOf.TendPatient` auf Wounded-Pawn
5. **Pawn-Exclusivity-Lock** via 3.1: Doctor für 60s gelockt (LockPriority=80)
6. **Interaktion mit E-BLEED** (3.4): E-BLEED hat LockPriority=90 → gewinnt bei Konflikt
7. Unit-Tests

## Tasks
- [ ] `Source/Emergency/E_Health.cs`
- [ ] JobDefOf.TendPatient-Integration
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §2, D-16, CC-STORIES-06.
**Vorausgesetzt:** 3.1, 3.13, 4.8, 3.9 (BillManager für Medicine-Craft).

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_Health.cs` | create |

## Testing
Unit: Score, Doctor-Claim, Lock-Konflikt mit E-BLEED.

## Review-Gate
Code-Review gegen D-16, F-AI-02.

## Transient/Persistent
Handler-State transient.
