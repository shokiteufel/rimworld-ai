# Story 7.13: PHASE_ROYAL Stellarch-Belagerung (7 Tage)

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Stellarch-Belagerung-Management** (nur Royalty): nach Stellarch-Ankunft 7 Tage Waves abwehren, Psycaster-Reserve nutzen, Imperium-Radio-Drops.

## Acceptance Criteria
1. **DLC-Guard** (MED-Fix, CC-STORIES-05): `if (!DlcCapabilities.EndingAvailable(Ending.Royal)) return;` — Royal-Ending ist Royalty-gated (`Ending.Royal` erfordert `DlcCapabilities.HasRoyalty`).
2. `StellarchSiegePhaseRunner` aktiv nach Stellarch-Arrival-Event
3. Prep-Phase: Killpoints mit Turrets, Ammo, Psycaster-Reserve
4. 7-Tage-Counter
5. Radio-Drop-Trigger bei kritischen Momenten
6. Unit-Tests

## Tasks
- [ ] `Source/Decision/StellarchSiegePhaseRunner.cs`
- [ ] Radio-Drop-Logic
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Royal Siege.
**Sub-Phase (Story 7.0):** Implementiert `StellarchSiege` aus `EndingSubPhaseStateMachine` (letzte Sub-Phase des Royal-Endings).
**Vorausgesetzt:** 4.7, 7.0, 7.12.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/StellarchSiegePhaseRunner.cs` | create |

## Testing
Unit: Counter + Radio-Drop.

## Review-Gate
Code-Review gegen D-15.
