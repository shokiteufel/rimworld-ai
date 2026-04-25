# Story 1.9: Schema-Version-Registry (Cross-Cutting)

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle (Cross-Cutting-Infrastruktur)
**Size:** S
**Decisions referenced:** D-14 (Persistence-Scoping + Schema-Versioning), D-31 (Pass-2-Revision), CC-STORIES-01

## Story
Als Mod-Entwickler möchte ich ein **zentrales Schema-Version-Registry** für BotGameComponent + BotMapComponent, damit alle Folge-Stories (Story 2.3, 3.9, 4.3, 6.5 etc.) konsistent Schema-Felder bumpen + Migration-Pfade dokumentieren.

## Acceptance Criteria
1. `BotGameComponent` hat `const int CurrentSchemaVersion = 3` + `Migrate()` (bereits aus Story 1.3, v1→v2 perPawnPlayerUse-keying, v2→v3 no-op)
2. **`BotMapComponent` hat `const int CurrentSchemaVersion = 3`** + `Migrate()` (bereits aus Story 1.3, v3 = +excludedCells). **Retroaktiv 2026-04-25:** Story-AC sagte v1, das wurde bereits in 1.3 vorgezogen. `SchemaRegistry` dokumentiert die History.
3. `SchemaRegistry`-Klasse in `Source/Data/SchemaRegistry.cs` dokumentiert alle Schema-Bumps als `record SchemaBump(string ComponentName, int FromVersion, int ToVersion, string FieldChanges, string Reason)` — IReadOnlyList als statisches Single-Source-of-Truth.
4. **Pflicht-AC-Template** für Feature-Stories die Schema erweitern (zur Aufnahme in Stories 2.3, 2.7, 3.9, 4.3, 6.5, 7.9):
   - „Feature-Feld X hinzugefügt — `CurrentSchemaVersion` auf N+1 bumpen"
   - „Migration-Pfad: alte Saves ohne Feld → Default-Wert Y"
   - „SchemaRegistry-Eintrag in Source/Data/SchemaRegistry.cs hinzugefügt"
   - „TC: Savegame-Roundtrip v(N)→v(N+1) ohne Data-Loss"
5. **Geplante Schema-Bumps** als planned-Einträge in SchemaRegistry (werden in den Stories selbst auf "applied" umgestellt). Sequenz konsistent mit Code in `Source/Data/SchemaRegistry.cs`:
   - BotMapComponent v3 `excludedCells` (Story 2.3 — bereits in v3 enthalten via Story 1.3, kein weiterer Bump nötig)
   - BotMapComponent v3→v4 `botManagedBills` (Story 3.9)
   - BotMapComponent v4→v5 `overlayVisible` (Story 2.7)
   - BotGameComponent v3→v4 `botManagedGuests` (Story 4.3)
   - BotGameComponent v4→v5 `pawnSpecializations` (Story 6.5)
   - BotGameComponent v5→v6 `journeyQuest` (Story 7.9)
6. Migrations sind idempotent (mehrfach angewendet → gleicher Zustand) — schemaVersion-Field schützt vor Doppel-Apply
7. ~~Unit-Tests~~ — verschoben auf Story 1.13 (Test-Infrastructure liefert FakeSnapshotProvider + Save-Roundtrip-Helpers); Story 1.9 dokumentiert Test-Plan in `Source/Data/SchemaRegistry.cs`-Kommentar.

## Tasks
- [x] `Source/Data/SchemaRegistry.cs` mit `record SchemaBump` + statische Bumps-Liste
- [x] `BotMapComponent.ExposeData` + `Migrate()` (bereits aus Story 1.3, hier nur Cross-Reference)
- [x] Migration-Chains v1→v2→v3 für beide Components (bereits in Story 1.3 implementiert; geplante Bumps v4-v6 als "planned" markiert)
- [ ] **Unit-Tests verschoben auf Story 1.13** (Test-Infrastructure liefert FakeSnapshotProvider + Save-Roundtrip-Helpers; Test-Plan im SchemaRegistry-Kommentar dokumentiert)
- [ ] **Integration-Test verschoben auf Story 1.13** analog
- [ ] Cross-Story-Documentation: in den 6 Feature-Stories (2.3, 3.9, 4.3, 6.5, 2.7, 7.9) wird beim Implement `SchemaRegistry.Bumps`-Eintrag von "planned" auf "applied" umgestellt + Schema-Bump-AC ergänzt

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
**Verschoben auf Story 1.13 (Test-Infrastructure):**
- Unit: pro Migration-Bump einzeln (Fake-Save mit `FromVersion` → Migrate → assert Felder == Defaults)
- Unit: kaskadiert (Fake-Save mit ältester Version → Migrate → assert alle Felder migriert)
- Unit: Doppel-Apply (Migrate zweimal → idempotenter Endzustand, Schutz via schemaVersion-Check)
- Integration: Fake-v1-Save → geladen als latest-Version

## Review-Gate
Code-Review gegen D-14, Migration-Sauberkeit, Integration mit 6 Feature-Stories.

## Transient/Persistent
Feld `SchemaVersion` ist persistent (immer). `SchemaRegistry` selbst ist Compile-Time-Konstante (nicht persistiert).
