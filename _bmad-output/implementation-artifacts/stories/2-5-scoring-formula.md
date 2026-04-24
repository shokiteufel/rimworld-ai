# Story 2.5: Site-Scoring-Formel (alle W_*-Gewichte)

**Status:** ready-for-dev
**Epic:** Epic 2 — Map-Analyzer
**Size:** M
**Decisions referenced:** D-09 (W_HAZARD=0.30 negativ), PRD §FR-04 Scoring, Architecture §5a ConfigResolver, D-14 (schemaVersion)

## Story
Als Mod-Entwickler möchte ich die **aggregierte Site-Scoring-Formel** implementieren, die pro Cell alle Gewichts-Beiträge (`W_FOOD`, `W_DEFENSE`, `W_WATER`, `W_TEMP`, `W_HAZARD`, `W_RESOURCES`) zu einem Gesamt-Score zusammenfasst und Gewichte aus `ConfigResolver` liest (per-Biome-Learning-ready aus Epic 8).

## Acceptance Criteria
1. `SiteScoring.ScoreCell(CellSnapshot, ColonySnapshot, ConfigResolver) → double` in `Source/Analysis/SiteScoring.cs`
2. Formel: `Score = W_FOOD * food_contribution + W_DEFENSE * defense_contrib + W_WATER * water + W_TEMP * temp + W_HAZARD * hazard_penalty + W_RESOURCES * resources`
3. Gewichte via `ConfigResolver.Get<double>(ConfigKeys.W_FOOD)` etc. — Defaults aus PRD (W_FOOD=0.25, W_DEFENSE=0.23, W_HAZARD=-0.30, …)
4. Hard-Filter: wenn `HazardKind != None && proximity < 3` → return `-∞` (Cell excluded, aus Story 2.3)
5. Unit-Tests mit fake-Snapshots für bekannte Score-Werte (Golden-Master-Style)
6. Score-Breakdown-Rückgabe für UI-Display: `ScoreBreakdown(double Total, IReadOnlyDictionary<string, double> Contributions)` — für `SiteMarkerOverlay` Hover-Tooltip (Architecture §2.5 F-AI-06)

## Tasks
- [ ] `Source/Analysis/SiteScoring.cs`
- [ ] `ScoreBreakdown` record
- [ ] `ConfigKey<double>`-Definitionen für W_FOOD/DEFENSE/WATER/TEMP/HAZARD/RESOURCES
- [ ] Compiled Defaults in `Configuration` (PRD-Werte)
- [ ] Unit-Tests mit 5-6 Golden-Master-Fällen
- [ ] Integration mit `ConfigResolver` (Invalidate-Hook test)

## Dev Notes
**Architektur-Kontext:** §5a Precedence: `BotGameComponent > LearnedConfig > Configuration > Defaults`. Learning kommt in Epic 8 (Story 8.2 Bayesian-Update).
**Nehme an, dass:** Food-/Defense-Contributions bereits in CellSnapshot/Colony-Context enthalten (Fertility, WildPlant aus 2.1/2.2; ChokepointScore aus 2.4).
**Vorausgesetzt:** Stories 2.1–2.4.

## File List
| Pfad | Op |
|---|---|
| `Source/Analysis/SiteScoring.cs` | create |
| `Source/Analysis/ScoreBreakdown.cs` | create |
| `Source/Data/Configuration.cs` | modify (Weight-Defaults) |

## Testing
- Unit: 6 Golden-Master-Fälle (Food-rich, Defense-rich, Hazard-penalty, Mixed, Hard-Filter, Water-Only)
- Integration: ConfigResolver Invalidate nach Settings-Change → Scores recomputen

## Review-Gate
Code-Review gegen D-09 (W_HAZARD=0.30 negativ), ScoreBreakdown für UI bereit, Unit-testbar ohne Runtime.
