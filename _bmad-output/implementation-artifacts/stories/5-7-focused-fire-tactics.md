# Story 5.7: Focused-Fire-Tactics

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Focused-Fire-Logik**: alle Shooter auf dasselbe High-Value-Target (Thrumbo, Mech-Boss, Schwarzmarkt-Leader) statt spread-Fire.

## Acceptance Criteria
1. `FocusedFirePlanner.SelectTarget(ThreatReport) → Pawn target`
2. Target-Priority: Mech-Boss > Healer > Höchst-Threat-Single > Random
3. `DraftController.Apply` erweitert: wenn Target gesetzt, alle Shooters bekommen `Attack`-Job auf selbes Target
4. Re-target wenn Target dies → nächstes High-Value
5. Unit-Tests Target-Selection

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
