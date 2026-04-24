# Story 6.9: Turret-Placement (Killpoint-Integration)

**Status:** ready-for-dev
**Epic:** Epic 6 — Industrialization
**Size:** S

## Story
Als Mod-Entwickler möchte ich **auto-Turret-Placement** am Killpoint (Story 4.7): 3-4 Mini-Turrets + optional 1 Autocannon wenn Research done.

## Acceptance Criteria
1. `TurretPlanner.Plan(KillpointLayout, ColonySnapshot) → BuildPlan`
2. Platzierung in Killpoint-Shooter-Area
3. Power-Anschluss sichergestellt
4. Research-Check: Autocannon nur wenn `HeavyTurret` research done
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/TurretPlanner.cs`
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §8.1, Story 4.7.
**Vorausgesetzt:** 4.7, 6.1, 4.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/TurretPlanner.cs` | create |

## Testing
Unit: Turret-Layout.

## Review-Gate
Code-Review gegen D-15.
