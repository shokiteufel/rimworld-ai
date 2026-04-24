# Story 4.9c: Emergency-Handler E-MENTALBREAK

**Status:** ready-for-dev
**Epic:** Epic 4
**Size:** S

## Story
Als Mod-Entwickler möchte ich **E-MENTALBREAK** implementieren: bei Pawn nahe Mental-Break-Threshold Mitigation (Drug-Use, Recreation, Social-Interaction).

## Acceptance Criteria
1. `E_MentalBreak : EmergencyHandler` mit `BasePrio = 7`, `LockPriority = 30`
2. Eligibility: `I8_MentalBreakRisk.Violated`
3. Apply: Drug-Use wenn Pawn Drug-Affinity (Ideology-Precept-Check via DlcCapabilities) ODER Isolation (Pawn in Schlafraum) + Recreation-Priority
4. Score: `base + (break_proximity * 150)` — proximity = 1.0 wenn Pawn < 5 Ticks von Break
5. Launch-Critical: nein direkt (aber Mental-Break kann Downstream I5/I9 triggern)
6. Unit-Tests

## Tasks
- [ ] `Source/Emergency/E_MentalBreak.cs`
- [ ] Drug-Verfügbarkeits-Check
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §2. Review-Finding 4.9-MEDIUM: Drugs sind Vanilla, Ideology beeinflusst nur Moralitäts-Precepts.
**Vorausgesetzt:** 3.1, 3.13, 4.8.

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_MentalBreak.cs` | create |

## Testing
Unit: Drug-vs-Isolation-Entscheidung.

## Review-Gate
Code-Review gegen D-16, DlcCapabilities-Guards.

## Transient/Persistent
Transient.
