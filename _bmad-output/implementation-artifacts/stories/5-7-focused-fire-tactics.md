# Story 5.7: Focused-Fire-Tactics

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Focused-Fire-Logik**: alle Shooter auf dasselbe High-Value-Target (Thrumbo, Mech-Boss, Schwarzmarkt-Leader) statt spread-Fire.

## Acceptance Criteria
1. `FocusedFirePlanner.SelectTarget(ThreatReport) → string TargetUniqueLoadID` (identifier-only D-23, nicht `Pawn`-Ref)
2. Target-Priority: Mech-Boss > Healer > Höchst-Threat-Single > Random
3. `DraftController.Apply` erweitert: wenn Target gesetzt, alle Shooters bekommen `JobDefOf.AttackMelee`/`AttackStatic` (siehe api-reference.md) auf resolvtes Target-Pawn
4. **Read-After-Write-Check** (MED-Fix, CC-STORIES-10): nach `pawn.jobs.StartJob(attackJob)` Read-Back: `pawn.CurJobDef == attackJob.def && pawn.CurJob.targetA.Thing == targetPawn`; bei Mismatch (Combat Extended / CAI 5000 können Attack-Jobs patchen): WARN-Log + Retry 1× nach 60 Ticks + Poisoned-Set analog 3.10.
5. Re-target wenn Target dies → nächstes High-Value
6. Unit-Tests Target-Selection + Read-After-Write-Mismatch-Simulation
7. **Exception-Wrapper** (HIGH-Fix Round-2-Stability, CC-STORIES-02): `DraftController.Apply`-Extension für Focused-Fire wrapped via Story 1.10 `ExceptionWrapper.Execution(...)` (erbt von Story 5.4 AC 9 Wrapper-Pattern).

## Tasks
- [ ] `Source/Decision/FocusedFirePlanner.cs`
- [ ] Apply-Erweiterung in DraftController
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §8.1.
**Vorausgesetzt:** 5.1, 5.4.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/FocusedFirePlanner.cs` | create |
| `Source/Execution/DraftController.cs` | modify (Attack-Job-Integration) |

## Testing
Unit: Target-Priority-Matrix. Integration: Mech-Cluster-Raid.

## Review-Gate
Code-Review gegen D-15, AI-3 (nur Execution mutiert).
