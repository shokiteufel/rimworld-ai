# Story 5.3: CombatCommander-Entscheidungstree

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** L

## Story
Als Mod-Entwickler möchte ich den **`CombatCommander` als Plan-Producer** implementieren, der basierend auf `ThreatReport` einen `DraftOrder` (+ Retreat-Point) erzeugt — via Decision-Tree: Manageable→Fight-at-Killpoint, Hard→Fight-or-Flee-based-on-Ratio, Overwhelming→Flee.

## Acceptance Criteria
1. `CombatCommander.Plan(ThreatReport, ColonySnapshot, PawnSnapshot[]) → DraftOrder`
2. Decision-Tree:
   - Negligible: nur Wachposten draften
   - Manageable: Retreat-zu-Killpoint, alle Combat-Pawns draften
   - Hard: Killpoint + `IF killpoint_ready == false THEN fight_at_current_position`
   - Overwhelming: Caravan-Flee (Story 5.5)
3. DraftOrder identifier-only (D-23): `ImmutableHashSet<string UniqueLoadID>`
4. Retreat-Point = `killpoint.center` oder `map.AveragePlayerHome` fallback
5. Unit-Tests pro ThreatLevel
6. Integration: Simulate Raid → DraftOrder korrekt

## Tasks
- [ ] `Source/Decision/CombatCommander.cs`
- [ ] Decision-Tree-Logik
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Architecture §2.3, Mod-Leitfaden §8.1.
**Vorausgesetzt:** 5.1, 4.7, 3.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/CombatCommander.cs` | create |

## Testing
Unit: 4 ThreatLevel-Wege. Integration: Raid-Simulation je Level.

## Review-Gate
Code-Review gegen D-15, D-23, D-16.
