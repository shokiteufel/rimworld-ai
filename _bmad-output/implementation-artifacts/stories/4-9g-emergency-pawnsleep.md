# Story 4.9g: Emergency-Handler E-PAWNSLEEP

**Status:** ready-for-dev
**Epic:** Epic 4
**Size:** S

## Story
Als Mod-Entwickler möchte ich **E-PAWNSLEEP** implementieren: bei Pawn mit Sleep-Need<0.2 Force-Sleep via `JobDefOf.LayDown`.

## Acceptance Criteria
1. `E_PawnSleep : EmergencyHandler` mit `BasePrio = 5`, `LockPriority = 25`
2. Eligibility: `I12_PawnSleep.Violated`
3. Apply: `JobDefOf.LayDown`-Job auf Pawn-Bed; wenn kein Bed verfügbar → `JobDefOf.LayDownAwake` (Anywhere)
4. Launch-Critical: nein (aber Sleep-Debt kann zu Mental-Break kaskadieren)
5. **Nicht aktiv** während E-RAID (LockPriority-Matrix: 100 vs. 25)
6. Unit-Tests

## Tasks
- [ ] `Source/Emergency/E_PawnSleep.cs`
- [ ] Bed-Availability-Check via Snapshot
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §2. Review-Finding 4.9-HIGH: korrekter JobDef ist `JobDefOf.LayDown` (nicht generisches „LayDown").
**Vorausgesetzt:** 3.1, 3.13, 4.8.

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_PawnSleep.cs` | create |

## Testing
Unit: Bed-vs-Anywhere-Entscheidung, Lock-Deprecation bei E-RAID.

## Review-Gate
Code-Review gegen D-16, JobDef-Korrektheit.

## Transient/Persistent
Transient.
