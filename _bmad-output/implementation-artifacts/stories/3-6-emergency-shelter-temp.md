# Story 3.6: Emergency-Handler E-SHELTER + E-TEMP

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** S

## Story
Als Mod-Entwickler möchte ich die **E-SHELTER und E-TEMP Handler** implementieren, die bei Obdachlosigkeit (I1) oder Temperatur-Extremen (I5) Emergency-Bau/Kleidung koordinieren.

## Acceptance Criteria
1. `E_Shelter : EmergencyHandler` mit `BasePrio = 9`; bei I1-Violation Build-Plan für Primitive Walls + Door (Wood-Material, Minimum 3×3)
2. `E_Temp : EmergencyHandler` mit `BasePrio = 8`; bei I5-Violation ergänzt Heater/Cooler zur Home-Area oder triggert Kleidung-Crafting
3. Beide: Eligibility + Score + Apply nach Framework-Kontrakt (Story 3.1)
4. **Szenario-Awareness**: E-Temp wählt Heater bei `outdoor_temp < -10°C`, Cooler bei `> 35°C`, Kleidung bei mildem Extreme
5. Unit-Tests pro Handler
6. Integration: Extreme-Biome (Ice Sheet) → E-Temp early-triggered

## Tasks
- [ ] `Source/Emergency/E_Shelter.cs` + `E_Temp.cs`
- [ ] BuildPlan-Erzeugung für Primitive-Walls (benötigt Story 3.8)
- [ ] Heater/Cooler-Placement-Heuristik
- [ ] Cloth-Crafting-BillPlan bei Kälte
- [ ] Unit-Tests
- [ ] Integration Ice-Sheet

## Dev Notes
**Architektur-Kontext:** Mod-Leitfaden §2 E-SHELTER/E-TEMP.
**Nehme an, dass:** Primitive-Wall-ThingDef ist vorhanden (Vanilla `Wall`).
**Vorausgesetzt:** Story 3.1, 3.2, 3.8 (BuildPlanner).

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_Shelter.cs` | create |
| `Source/Emergency/E_Temp.cs` | create |

## Testing
- Unit: Heater-vs-Cooler-vs-Cloth-Entscheidung
- Integration: Ice-Sheet-Tag-1 → E-Temp mit Heater-Plan

## Review-Gate
Code-Review gegen Mod-Leitfaden §2, Biome-Context-Awareness.
