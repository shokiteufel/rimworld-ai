# Story 7.13: PHASE_ROYAL Stellarch-Belagerung (7 Tage)

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Stellarch-Belagerung-Management** (nur Royalty): nach Stellarch-Ankunft 7 Tage Waves abwehren, Psycaster-Reserve nutzen, Imperium-Radio-Drops.

## Acceptance Criteria
1. `StellarchSiegeManager` aktiv nach Stellarch-Arrival-Event
2. Prep-Phase: Killpoints mit Turrets, Ammo, Psycaster-Reserve
3. 7-Tage-Counter
4. Radio-Drop-Trigger bei kritischen Momenten
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/StellarchSiegeManager.cs`
- [ ] Radio-Drop-Logic
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Royal Siege.
**Vorausgesetzt:** 7.12, 4.7.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/StellarchSiegeManager.cs` | create |

## Testing
Unit: Counter + Radio-Drop.

## Review-Gate
Code-Review gegen D-15.
