# Story 1.9 Code-Review

**Verdict:** APPROVE-WITH-CHANGES
**Reviewer:** Code-Review-Agent
**Datum:** 2026-04-25
**Build:** 0/0 (clean)
**Files:** `Source/Data/SchemaRegistry.cs` (NEU), `Source/Data/BotGameComponent.cs` (verifiziert), `Source/Data/BotMapComponent.cs` (verifiziert)

## AC-Coverage (7 ACs)

| AC | Status | Evidence |
|----|--------|----------|
| 1 BotGame v3 + Migrate | PASS | `BotGameComponent.cs:17` `CurrentSchemaVersion = 3`; `Migrate()` Z.135-154 mit V1→V2 (UniqueLoadID-Rekey, NEW-STAB-05-Toast) + V2→V3 (no-op). Migrate-Trigger Z.106 `if (schemaVersion < CurrentSchemaVersion)`. |
| 2 BotMap v3 + Migrate | PASS | `BotMapComponent.cs:16` `CurrentSchemaVersion = 3`; `Migrate()` Z.92-101 setzt `excludedCells` defensiv. Retroaktiv-Drift in Story-File AC 2 dokumentiert. |
| 3 SchemaRegistry-Record | PASS | `SchemaRegistry.cs:23-29` `sealed record SchemaBump(Component, FromVersion, ToVersion, FieldChanges, StoryId, Status)` als positional record. `IReadOnlyList<SchemaBump> Bumps` Z.35. |
| 4 Pflicht-AC-Template | PASS | Code-Kommentar Z.5-15, 5-Punkte-Checkliste klar formuliert + Idempotenz-Vertrag Z.14-15. |
| 5 Geplante TBD-Bumps | PASS-mit-Drift | 5 `Status: "planned"`-Einträge vorhanden; intern konsistent sequenziert. Drift zu Story-AC-5-Wortlaut: siehe MED-1. |
| 6 Idempotenz | PASS | `BotGameComponent.cs:140-141` Pro-Step-Guard `if (schemaVersion < N)`. `BotMapComponent.cs:95` `if (schemaVersion < 3)`. Beide setzen `schemaVersion = CurrentSchemaVersion` am Ende. Doppel-Apply safe. |
| 7 Test-Plan-Doc | PASS | `SchemaRegistry.cs:17-20` 3-Zeilen-Test-Plan (Per-Bump / Kaskadiert / Doppel-Apply) für Story 1.13 dokumentiert. |

## Findings

### MED-1 — Story-File-Drift in AC 5 vs. Code-Sequenzierung
Story-AC 5 listet `BotMapComponent v5→v6 overlayVisible (Story 2.7)` und `BotGameComponent v5→v6 journeyQuest (7.9)`. Code `SchemaRegistry.cs:64` hat aber `BotMap v4→v5` für 2.7 (korrekt sequenziert nach v3→v4 für 3.9). Code ist intern konsistent + sauberer; Story-File sollte synchronisiert werden, sonst leakt Drift in Folge-Stories.
**Fix:** Story-File AC 5 anpassen: `BotMap v4→v5 overlayVisible (2.7)`. Retroaktiv-Note ergänzen.

### MED-2 — `BotGame v2→v3 "no-op"`-Eintrag in Bumps-Liste semantisch fragwürdig
`SchemaRegistry.cs:41-43` enthält einen no-op Bump nur um die Versions-Synchronisation BotGame=BotMap=3 zu erklären. Das sind eigentlich keine zwei separaten "Bumps" sondern ein History-Artefakt. Risiko: Folge-Stories interpretieren Pattern als "wir bumpen mit, auch ohne Feldänderung".
**Fix-Option:** entweder (a) Eintrag löschen + LatestAppliedVersion auf v2 zurück, dann v3→v4 für Story 4.3; oder (b) FieldChanges-Text präziser: "synchronization-only — keine Feldänderung, nur Version-Bump für Cross-Component-Konsistenz". Empfehlung: (b), weil Synchronisation tatsächlich gewollt ist.

### LOW-1 — Status-Field als string statt enum
`SchemaRegistry.cs:29` `string Status` mit Inline-Kommentar `// "applied" | "planned"`. Type-Safety fehlt; Tippfehler ("aplied") würde silent durchrutschen.
**Fix:** `enum SchemaBumpStatus { Applied, Planned }` einführen.

### LOW-2 — Story-Tasks nicht synchronisiert mit AC 7
`1-9-schema-version-registry.md:33-35` listet Tasks `Unit-Tests pro Migration` + `Integration-Test: v1-Fake-Save → v9 geladen` + `Migration-Chains für v1→v2→…→v9` als unchecked, obwohl AC 7 sie auf 1.13 verschiebt und Migration-Chains nur v1→v3 existieren.
**Fix:** Tasks-Liste anpassen: betroffene Zeilen als `[verschoben auf 1.13]` markieren oder streichen.

### LOW-3 — `LatestAppliedVersion`-Baseline `latest = 1`
`SchemaRegistry.cs:74` startet bei 1. Für eine zukünftige neue Komponente ohne Bumps würde 1 zurückgegeben (statt 0). Akzeptabel — initial schema = v1 ist Konvention. Doku als XML-Kommentar empfohlen.

## Recommendation

APPROVE-WITH-CHANGES. Kern-Implementation ist solide — record-basierte Single-Source-of-Truth, korrekte Idempotenz, vollständige Pflicht-AC-Template. Vor `done`:

1. **MED-1** fixen (Story-File AC 5 sync mit Code-Sequenzierung).
2. **MED-2** fixen (no-op-Eintrag-Text präziser oder entfernen).
3. **LOW-1, LOW-2, LOW-3** fixen (Guardian-Regel 4: alle Findings, kein Cherry-Pick).

Nach Fixes: re-review nicht nötig (alle Changes sind dokumentation/textuell, kein Logik-Risiko). Build bleibt 0/0. Visual-Review entfällt korrekt (kein UI). Unit-Test-Verschiebung auf 1.13 ist konsistent mit fehlender Test-Infrastructure und im Code-Kommentar dokumentiert.
