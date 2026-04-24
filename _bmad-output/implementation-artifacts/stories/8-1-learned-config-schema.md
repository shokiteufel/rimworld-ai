# Story 8.1: LearnedConfig-Schema (XML + atomic write + Mutex)

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** M
**Decisions referenced:** D-07 (Cross-Game-Lernsystem), CC-04 (LearnedConfig Race/Platform), F-STAB-17 (Mutex-Hash), F-STAB-22 (Orphan-tmp-Cleanup)

## Story
Als Mod-Entwickler möchte ich **LearnedConfig-Storage-Layer** implementieren mit vollem Schema aus Architecture §6.2, atomic-write, Path-Hash-Mutex, Corruption-Backup.

## Acceptance Criteria
1. `Source/Data/LearnedConfig.cs` mit IExposable-Pattern, via Scribe für XML-Persist
2. Pfad: `GenFilePaths.ConfigFolderPath + "/RimWorldBot/learned-config.xml"`
3. Schema gemäß §6.2: BiomeWeights (per-Biome), Thresholds, OverrideLibrary, Statistics
4. Write-Pattern §6.1: Mutex mit SHA1(Path)-Hash + Orphan-tmp-Cleanup + tmp→bak→final Rename
5. Read-Pattern: bei Parse-Fehler → `.corrupt-<ts>` umbenennen, `.bak` laden
6. `LoadOrDefault()` static method
7. `SchemaVersion = 2` Field
8. Unit-Tests: Roundtrip, Corruption-Handling

## Tasks
- [ ] `Source/Data/LearnedConfig.cs`
- [ ] Write-Pattern mit Mutex + atomic
- [ ] Read-Pattern mit Corruption-Backup
- [ ] Schema-Serialization
- [ ] Unit-Tests

## Dev Notes
**Kontext:** §6.1 + §6.2, CC-04.
**Vorausgesetzt:** 1.3 (BotGameComponent).

## File List
| Pfad | Op |
|---|---|
| `Source/Data/LearnedConfig.cs` | create |

## Testing
Unit: Write-Read-Roundtrip, Parse-Corruption, Mutex-AbandonedException.

## Review-Gate
Code-Review gegen §6.1/§6.2, F-STAB-17/22, D-07.
