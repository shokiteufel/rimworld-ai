# Story 8.2: Sample-Count-Weight-Update (ersetzt Bayesian)

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** S
**Decisions referenced:** F-AI-05 (konvergente Sample-Count-Formel statt einfacher Bayesian), D-07

## Story
Als Mod-Entwickler möchte ich **Sample-Count-Weight-Update-Formel** aus §6.3: `new = old + (observed - old) / (sampleCount + 20)`, per-Biome partitioniert, mit Overfitting-Schutz (sampleCount < 5 → Default).

## Acceptance Criteria
1. `WeightUpdater.UpdateWeight(LearnedConfig, string biome, ConfigKey key, double observed) → void`
2. Pro Biome separater Counter
3. k = 20 Bayesian-Prior (hardcoded in §6.3)
4. sampleCount < 5 → no update, Default bleibt
5. Unit-Tests: Konvergenz, Overfitting-Schutz, Cross-Biome-Isolation

## Tasks
- [ ] `Source/Data/WeightUpdater.cs`
- [ ] Unit-Tests

## Dev Notes
**Kontext:** §6.3, F-AI-05-Fix.
**Vorausgesetzt:** 8.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Data/WeightUpdater.cs` | create |

## Testing
Unit: Konvergenz-Test, Biome-Isolation.

## Review-Gate
Code-Review gegen §6.3 Formel exakt.
