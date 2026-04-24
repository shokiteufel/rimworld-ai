# Story 1.9: Schema-Version-Registry (Cross-Cutting)

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle (Cross-Cutting-Infrastruktur)
**Size:** S
**Decisions referenced:** D-14 (Persistence-Scoping + Schema-Versioning), D-31 (Pass-2-Revision), CC-STORIES-01

## Story
Als Mod-Entwickler möchte ich ein **zentrales Schema-Version-Registry** für BotGameComponent + BotMapComponent, damit alle Folge-Stories (Story 2.3, 3.9, 4.3, 6.5 etc.) konsistent Schema-Felder bumpen + Migration-Pfade dokumentieren.

## Acceptance Criteria
1. `BotGameComponent` hat `const int CurrentSchemaVersion = 3` + `Migrate()` (bereits aus 1.3)
2. **`BotMapComponent` bekommt ebenfalls `const int CurrentSchemaVersion = 1`** + `Migrate()` (war in 1.3 implizit gelassen)
3. `SchemaRegistry`-Klasse in `Source/Data/SchemaRegistry.cs` dokumentiert alle Schema-Bumps als `record SchemaBump(int FromVersion, int ToVersion, string ComponentName, string FieldChanges)` — kompiliert zu Migration-Tests
4. **Pflicht-AC-Template** für Feature-Stories die Schema erweitern:
   - „Feature-Feld X hinzugefügt — `CurrentSchemaVersion` auf N+1 bumpen"
   - „Migration-Pfad: alte Saves ohne Feld → Default-Wert Y"
   - „TC: Savegame-Roundtrip v(N)→v(N+1) ohne Data-Loss"
5. **Retroaktive Schema-Bumps** für bereits gedraftete Stories: 2.3 (v3→v4 `excludedCells`), 3.9 (v4→v5 `botManagedBills`), 4.3 (v5→v6 `botManagedGuests`), 6.5 (v6→v7 `pawnSpecializations`), 2.7 (v7→v8 `overlayVisible`), 7.9 (v8→v9 `journeyQuest`)
6. Migrations sind idempotent (mehrfach angewendet → gleicher Zustand)
7. Unit-Tests: jeder Migration-Pfad einzeln + kaskadiert (v1→v9)

## Tasks
- [ ] `Source/Data/SchemaRegistry.cs` mit Records
- [ ] `BotMapComponent.ExposeData` + `Migrate()` ergänzen (Story 1.3 File-Edit)
- [ ] Migration-Chains für v1→v2→…→v9
- [ ] Unit-Tests pro Migration
- [ ] Integration-Test: v1-Fake-Save → v9 geladen
- [ ] Cross-Story-Documentation in allen betroffenen Feature-Stories (2.3, 3.9, 4.3, 6.5, 2.7, 7.9)

## Dev Notes
**Architektur-Kontext:** D-14 + CC-STORIES-01. Dies ist die **zentrale Story** für Schema-Konsistenz — ohne sie leaken 6+ andere Feature-Stories Daten bei Mod-Update.
**Nehme an, dass:** Scribe-API-Backward-Compat funktioniert zwischen RimWorld 1.5 und 1.6 (Standard-Annahme, per RimWorld-Konvention).
**Vorausgesetzt:** 1.3.

## File List
| Pfad | Op |
|---|---|
| `Source/Data/SchemaRegistry.cs` | create |
| `Source/Data/BotGameComponent.cs` | modify (Migrate-Chain) |
| `Source/Data/BotMapComponent.cs` | modify (SchemaVersion-Feld + Migrate) |
| Dependent Stories (2.3, 3.9, 4.3, 6.5, 2.7, 7.9) | reference |

## Testing
- Unit: Migration-Chain v1..v9
- Integration: Fake-Save v1 → geladen als v9

## Review-Gate
Code-Review gegen D-14, Migration-Sauberkeit, Integration mit 6 Feature-Stories.

## Transient/Persistent
Feld `SchemaVersion` ist persistent (immer). `SchemaRegistry` selbst ist Compile-Time-Konstante (nicht persistiert).
