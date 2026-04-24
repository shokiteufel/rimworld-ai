# Story 3.9: BillManager MVP (Simple Meal, Tribal Wear, Medicine)

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** M
**Decisions referenced:** D-15 (Plan/Apply), D-21 (Plan-Records), D-25 (Goal-Tag)

## Story
Als Mod-Entwickler möchte ich den **`BillPlanner` + `BillManager`** implementieren, der für Phase 0+1 Bills auf Workbenches setzt (Cook Simple Meal, Craft Tribal Wear, Make Herbal Medicine) — als pure Plan-Klasse + Apply-Seite.

## Acceptance Criteria
1. `BillPlanner.Plan(ColonySnapshot, WorkbenchSnapshot[]) → BillPlan`
2. Recipes Phase 0+1:
   - Simple Meal (Campfire/Stove) — Target-Count = `food_target_days * colonist_count * 2 meals/day`
   - Tribalwear (Crafting Spot) — 1 pro Pawn
   - Herbal Medicine (Crafting Spot) — Target = 10 per Colony
3. **BillIntent**-Schema (D-23 identifier-only): `(BillIntentKind, string RecipeDefName, int WorkbenchThingIDNumber, int TargetCount)`
4. **Idempotenz**: Plan gleich bei gleichem Colony-State
5. `BillManager.Apply(plan, map)` setzt `Bill`-Objekte via `BillStack.AddBill` + trägt Goal-Tag (D-25 `botPlacedThings` analog für Bills → eigenes `botManagedBills` in `BotMapComponent`)
6. Unit-Tests: 3 Scenarios (leer, Food-only, full)
7. Integration: nach BuildPlan-Apply Campfire steht → BillManager setzt Simple-Meal-Bill
8. **Exception-Wrapper + Read-After-Write** (HIGH/MED-Fix Round-2-Stability, CC-STORIES-02+10): `BillManager.Apply(plan, map)`-Hauptkörper wrapped via Story 1.10 `ExceptionWrapper.Execution(...)`. Nach `billStack.AddBill(bill)` Read-Back: `billStack.Bills.Contains(bill) && bill.recipe.defName == targetRecipe`. Bei Mismatch (Bill-Mod-Konflikt, Workbench disposed): WARN-Log + Retry 1× nach 60 Ticks + `poisonedWorkbenches: HashSet<int thingIDNumber>` (transient).
9. **Schema-Bump** (HIGH-Fix Round-2-Stability, CC-STORIES-01): `botManagedBills: Dictionary<int, PhaseGoalTag>` in `BotMapComponent` ist neues Feld → via Story 1.9 `SchemaVersionRegistry` registriert; Migrate setzt leeres Dict.

## Tasks
- [ ] `Source/Decision/BillPlanner.cs`
- [ ] `Source/Execution/BillManager.cs` (Apply)
- [ ] `botManagedBills: Dictionary<int /*bill.loadID*/, PhaseGoalTag>` in BotMapComponent (analog §2.3b)
- [ ] Target-Count-Heuristik pro Recipe
- [ ] Unit-Tests
- [ ] Integration

## Dev Notes
**Architektur-Kontext:** §2.3 + §2.4 + §2.3b erweitert.
**Nehme an, dass:** WorkbenchSnapshot kommt von Snapshot-Provider (Story 2.1 erweitert).
**Vorausgesetzt:** Story 3.8 (Workbenches platziert), Story 1.3.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/BillPlanner.cs` | create |
| `Source/Execution/BillManager.cs` | create |
| `Source/Snapshot/WorkbenchSnapshot.cs` | create |
| `Source/Data/BotMapComponent.cs` | modify (botManagedBills) |

## Testing
- Unit: Plan-Generation für 3 Fälle
- Integration: Campfire + BillManager → Simple-Meal-Bill sichtbar

## Review-Gate
Code-Review gegen D-15, D-21, D-23, D-25 (Tag-Schema erweitert auf Bills).
