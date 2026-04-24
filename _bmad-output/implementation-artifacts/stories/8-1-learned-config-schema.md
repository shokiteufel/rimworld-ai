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
8. **XXE/ReDoS-Schutz im XML-Reader** (HIGH-Fix): `XmlReaderSettings` explizit mit
   - `DtdProcessing = DtdProcessing.Prohibit` (verhindert XXE-Entity-Injection)
   - `XmlResolver = null` (kein External-Entity-Fetch)
   - `MaxCharactersFromEntities = 1024` (Schutz gegen Billion-Laughs)
   - `MaxCharactersInDocument = 2_000_000` (~2 MB Hard-Cap für learned-config)
   - `async`-Read via `XmlReader.Create(fs, settings)` mit `CancellationToken` + **15s-Timeout** (Task.Delay race; bei Timeout → `.corrupt-<ts>`-Umbenennung identisch zum Parse-Fehler-Pfad)
   - Begründung: `learned-config.xml` liegt im User-ConfigFolder und kann durch Dritt-Tools oder absichtlich maliziös bearbeitet werden. Default `XmlReader` in älteren .NET-Versionen hat DTD-Processing aktiv.
9. Unit-Tests: Roundtrip, Corruption-Handling, **XXE-Entity-Payload-Test** (Mock-File mit `<!DOCTYPE …>`-Entity → Parse scheitert mit XmlException → `.corrupt`-Pfad), **Billion-Laughs-Test** (tief-verschachtelte Entities → MaxCharactersFromEntities-Exception), **Timeout-Test** (bewusst langsamer FileStream → 15s-Abbruch)

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
