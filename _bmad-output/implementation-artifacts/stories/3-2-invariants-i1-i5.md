# Story 3.2: Invariants I1-I5 (Shelter, Food, Bleed, Fire, Temp)

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** M

## Story
Als Mod-Entwickler möchte ich die **5 MVP-Invariants I1-I5** (Shelter, Food, Bleed, Fire, Temp — aus Mod-Leitfaden §2) als konkrete Klassen implementieren, damit der Bot in Phase 0+1 die überlebenskritischen Bedingungen überwacht.

## Acceptance Criteria
1. `I1_Shelter`: prüft `colony.HasHomeAreaWithRoof > 0 cells`; Severity proportional zu `AvgOutdoorTimeHours`
2. `I2_Food`: prüft `FoodStockDays >= 3 * ColonistCount`; Severity = `max(0, 3*count - days)`
3. `I3_Bleed`: prüft kein Pawn mit `BleedRatePerDay > 0.5 && UnattendedMinutes > 10`; Severity pro bleedender Pawn
4. `I4_Fire`: prüft keine Fire-Instanz in Home-Area; Severity = Fire-Count + Distanz zu Vorräten
5. `I5_Temp`: prüft `indoor_temp in [comfort_min, comfort_max]` für alle Schlafräume; Severity = Abweichung
6. Jede Invariant ist **launch-critical** (für `GoalHealthScore`-Aggregation F-AI-16)
7. Unit-Tests pro Invariant mit 3 Fällen: passing, violation-minor, violation-critical
8. Idempotenz-Test: gleicher Snapshot 2× → identisches Result

## Tasks
- [ ] `Source/Invariants/I1_Shelter.cs` bis `I5_Temp.cs`
- [ ] ColonySnapshot erweitern um Felder: `HasHomeAreaWithRoof`, `FoodStockDays`, `BleedingPawns`, `ActiveFires`, `IndoorTempRange`
- [ ] `RimWorldSnapshotProvider.GetColony` implementiert alle neuen Felder
- [ ] `launch-critical: true` als Property auf Invariant markiert
- [ ] Unit-Tests für alle 5

## Dev Notes
**Architektur-Kontext:** Mod-Leitfaden §2 P0 (nach D-16 `base_prio`-Werte), F-AI-16 Launch-Critical-Klassifikation.
**Nehme an, dass:** `BleedRatePerDay` ist Vanilla-Pawn-Property aus Health-System.
**Vorausgesetzt:** Story 3.1 (Framework).

## File List
| Pfad | Op |
|---|---|
| `Source/Invariants/I1_Shelter.cs` | create |
| `Source/Invariants/I2_Food.cs` | create |
| `Source/Invariants/I3_Bleed.cs` | create |
| `Source/Invariants/I4_Fire.cs` | create |
| `Source/Invariants/I5_Temp.cs` | create |
| `Source/Snapshot/ColonySnapshot.cs` | modify (neue Felder) |
| `Source/Snapshot/RimWorldSnapshotProvider.cs` | modify |

## Testing
- Unit: 3 Fälle pro Invariant (pass, minor, critical)
- Integration: Naked-Start-Colony → I1 + I2 violated in Tick 1

## Review-Gate
Code-Review gegen Mod-Leitfaden §2 + D-16 + F-AI-16.
