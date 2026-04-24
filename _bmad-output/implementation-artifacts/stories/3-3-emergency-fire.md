# Story 3.3: Emergency-Handler E-FIRE

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** S

## Story
Als Mod-Entwickler möchte ich den **E-FIRE-Emergency-Handler** implementieren, der bei Fire-Invariant-Violation (I4) Feuerwehr-Aktivitäten koordiniert (Draft, Beat-Fire-Jobs, Evakuierung).

## Acceptance Criteria
1. `E_Fire : EmergencyHandler` mit `BasePrio = 12`
2. `Eligibility`: `I4_Fire.Violated == true`
3. `Score`: `base + (nearby_storage_value * 2) + (nearby_bleeding_pawns * 50)` — Modifier für nahen Vorräte + Pawns-Risiko
4. `Claim(pawns)`: alle Non-Player-Use-Pawns innerhalb Radius 30 around Fire
5. `Apply(controller)`: erzeugt `BeatFireJob` für geclaimte Pawns (Vanilla-Job `JobDefOf.BeatFire`)
6. Unit-Tests: Eligibility + Score + Claim-Set
7. Integration: Simulate Fire in Home-Area → E-Fire aktiviert → Pawns gehen löschen

## Tasks
- [ ] `Source/Emergency/E_Fire.cs`
- [ ] BaseScore + context_modifiers
- [ ] Claim-Logik (Radius + Player-Use-Filter)
- [ ] BeatFireJob-Erzeugung via `JobMaker.MakeJob(JobDefOf.BeatFire, fire)`
- [ ] Unit-Tests
- [ ] Integration-Test mit fake-Fire

## Dev Notes
**Architektur-Kontext:** Mod-Leitfaden §2 E-FIRE, D-16 Utility-Scoring.
**Nehme an, dass:** `JobDefOf.BeatFire` ist Vanilla-Job und funktioniert ohne Skill-Bottleneck.
**Vorausgesetzt:** Story 3.1 (Framework), Story 3.2 (I4 existiert).

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_Fire.cs` | create |

## Testing
- Unit: Score-Modifier bei Fire nahe Vorräten / Pawns
- Integration: Fire-Incident → Pawns löschen

## Review-Gate
Code-Review gegen Mod-Leitfaden §2, AI-7 (Emergency > Override > PhaseGoal).
