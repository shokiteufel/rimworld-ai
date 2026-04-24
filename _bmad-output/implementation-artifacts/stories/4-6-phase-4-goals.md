# Story 4.6: Phase 4 Goals + Exit (Stone Fortress)

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Phase 4 (Stone Fortress)** implementieren: Stone-Walls ringsum, Killpoint (Story 4.7), Traps, Stone-Beds/Möbel.

## Acceptance Criteria
1. `Phase4_StoneFortress : PhaseDefinition`
2. Goals: G4.1 Stone-Wall-Ring um Home-Area, G4.2 Killpoint existiert (Story 4.7), G4.3 Traps platziert, G4.4 Stone-Möbel statt Wood
3. Exit: alle Goals + `stableCounter >= 2` (D-26) UND **`EmergencyResolver.ActiveEmergencies.Count == 0`** (MED-Fix, CC-STORIES-12, F-AI-01)
4. Launch-Critical: G4.1, G4.2
5. Unit-Tests

## Tasks
- [ ] `Source/Phases/Phase4_StoneFortress.cs`
- [ ] Stone-Wall-Ring-Detection (Flood-Fill)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 4.
**Vorausgesetzt:** 4.5, 4.7.

## File List
| Pfad | Op |
|---|---|
| `Source/Phases/Phase4_StoneFortress.cs` | create |

## Testing
Unit: Exit-Conds.

## Review-Gate
Code-Review gegen D-11, F-AI-16.
