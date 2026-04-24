# Story 1.3 Code-Review

**Datum:** 2026-04-24
**Verdict:** APPROVE-WITH-CHANGES

Build grün (0 Warnungen, 0 Fehler). Struktur erfüllt Architecture §5 vollständig. Zwei MED-Findings bremsen ein Plain-APPROVE, beides nicht-blockierend für Merge, aber vor Story-Close zu fixen.

## AC-Coverage (17 ACs)

- **AC 1 — PASS.** Alle §5-Snippet-Felder in `BotGameComponent.cs:16-44` vorhanden, `schemaVersion = 3` als `CurrentSchemaVersion`-Const.
- **AC 2 — PASS mit Hinweis.** `eventQueue = new BoundedEventQueue<BotEvent>(32, 224)` im Konstruktor (`BotGameComponent.cs:48-51`). Story-Wortlaut „`BotGameComponent(Game game) : base(game)`" ist gegen RimWorld-1.6-API nicht compilierbar — `Verse.GameComponent.ctor()` ist parameterless (via Reflection-Check verifiziert). Der gewählte Pfad `BotGameComponent(Game game)` ohne `: base(game)` + Game-Ref ignorieren ist korrekt. Story-AC-Text sollte retroaktiv geklärt werden (Doku-Fix in Architecture §5 oder Story-Amendment), damit kein Zukunfts-Reviewer stolpert.
- **AC 3 — PASS.** `schemaVersion` ist erste Scribe-Zeile (Z.57), try/catch + `ResetToDefaults()` vorhanden (Z.85-89).
- **AC 4 — PASS.** `FinalizeInit → BuildController()` loggt „deferred to Epic 2" (Z.169-179).
- **AC 5 — PASS.** `LoadedGame` Reihenfolge exakt: Clear → Invalidate → BuildController → ReconcilePendingPhase → ReconcilePhaseGoalOrphans → CheckBudgetExhaustHistory (Z.181-189).
- **AC 6 — PASS.** `StartedNewGame` Z.191-196.
- **AC 7 — PASS.** `ReconcilePendingPhase` Z.198-209; `crash-recovery-phase-rollback` wird durch `AutoPinKinds`-`StartsWith("crash-recovery-")`-Regel in `RecentDecisionsBuffer.Add()` auto-pinned. Korrekt.
- **AC 8 — PASS.** `BotMapComponent` hat alle 5 Felder (Z.17-23).
- **AC 9 — PASS.** Beide Methoden Z.58-99; Blueprint-Cancel via `Destroy(DestroyMode.Cancel)` korrekt, Job-Cancel via `EndCurrentJob(InterruptForced)` semantisch richtig.
- **AC 10 — PASS.** Alle 6 Kinds abgedeckt: 5 explizit + `crash-recovery-*` via Prefix (`RecentDecisionsBuffer.cs:13-21,65-71`).
- **AC 11 — PASS** (als class statt record, siehe Kritischer Check unten).
- **AC 12 — PASS.** `PhaseGoalTag.cs` record mit (int PhaseIndex, string GoalId).
- **AC 13 — PASS** (Code-Smell-Check: keine gefunden, alle Felder Scribe-gehandled, Null-Guards im PostLoadInit).
- **AC 14 — PASS.** Migrate-Kette v1→v2→v3 + Exception-Fallback auf `ResetToDefaults()`.
- **AC 15 — PARTIAL (MED, siehe Findings).**
- **AC 16 — PASS.** `MigrateV2ToV3` no-op korrekt — `excludedCells` ist per Story-Text BotMapComponent-scoped und wird dort lazily initialisiert.
- **AC 17 — DEFERRED** (Unit-Test-Task, laut Story explizit „wird später als Unit-Test implementiert").

## Decisions-Coverage

- **D-12 (Tick-Host = GameComponentTick):** PASS. `GameComponentTick`-Override Z.232-247; kein Harmony-Patch auf Tick-Loops gefunden.
- **D-14 (UniqueLoadID-Keying):** PASS. `Dictionary<string, bool> perPawnPlayerUse`; Cleanup verwendet `p.GetUniqueLoadID()`.
- **D-19 (EventQueue transient):** PASS. `eventQueue` nicht in `ExposeData`; `Clear()` bei Load/NewGame.
- **D-23 (Identifier-only):** PASS. Records enthalten nur `int`/`string`-Identifier.
- **D-24 (EventQueue-Init im Ctor):** PASS. Erste Zeile im Konstruktor.
- **D-25 (Goal-Tag-Dicts):** PASS. `botPlacedThings` + `botAssignedJobs` in `BotMapComponent`.
- **D-26 (StableConsecutiveCounter):** PASS. Beide Felder vorhanden (BotGameComponent: `autoEscapeStableCounter`; BotMapComponent: `stableCounter`).

## Findings

### HIGH
Keine.

### MED
- **MED-1 — `MigrateV1ToV2` implementiert AC 15 nur als Reset-Stub, nicht als WorldPawns-Lookup.** `BotGameComponent.cs:139-163`: Dropped-Counter ist trivial `oldDict.Count` (alles wird gedroppt), `percentBranch = dropped/Math.Max(oldCount,1) > 0.10` mit `oldCount = droppedCount` → immer `dropped/dropped = 1.0 > 0.10 = true` sobald `dropped >= 1`. Die Threshold-Logik ist damit faktisch `dropped >= 1 → Toast`. Das TC-10a-Szenario (10-Pawn, 1 dropped, **kein** Toast) ist mit dem aktuellen Code nicht erfüllbar. Pre-Release-Annahme (keine v1-Saves existieren) rechtfertigt Reset-Strategie akzeptabel, aber dann muss die Toast-Threshold-Logik entweder (a) entfernt werden (da nie false) oder (b) echter Lookup mit `Find.WorldPawns.AllPawnsAliveOrDead.FirstOrDefault(p => p.thingIDNumber == oldKey)?.GetUniqueLoadID()` ergänzt werden wie in Story-AC 15 spezifiziert. Empfehlung: (b) implementieren + Kommentar „bleibt dormant bis echter v1→v2-Scribe-Pfad existiert". Heutiger Code ist Dead-Code der Story-Wortlaut verfehlt.
- **MED-2 — `excludedCells` fehlt in `BotMapComponent` vollständig.** AC 16 sagt „Migrate setzt `excludedCells = new HashSet<(int,int)>()`" — aber das Feld ist gar nicht deklariert in `BotMapComponent.cs`. Die Migrate-Zusage ist leer-no-op. Entweder (a) Feld `public HashSet<(int,int)> excludedCells` hinzufügen + in `ExposeData` persistieren + im Schema-v1→v2-Upgrade initialisieren, oder (b) Story-AC 16 zurückziehen als „Story 2.5/2.6-Territorium" (wo `MapAnalysisSummary` befüllt wird). Klare Entscheidung nötig; aktuell Inkonsistenz Story ↔ Code.

### LOW
- **LOW-1 — `BotGameComponent.eventQueue`-Feld ist nicht nullable-annotiert, aber im Konstruktor garantiert assigned.** Okay; aber in `LoadedGame` wird `eventQueue.Clear()` aufgerufen ohne Null-Check. Falls RimWorld je einen parameterless-Path benutzt (z.B. Savegame-Rehydration ohne Ctor-Call), NRE. Null-safe-Idiom `eventQueue?.Clear()` oder `?? throw` konsistent mit `configResolver?.Invalidate()`-Stil würde Resilienz erhöhen. Spielstand-irrelevant, aber Code-Hygiene.
- **LOW-2 — `MigrateV1ToV2` Z.154: `droppedCount / Math.Max(oldCount,1) > 0.10` mit `oldCount = droppedCount` ist mathematisch unsauber** (Division durch sich selbst). Symptom von MED-1; beim Fix mitlösen.
- **LOW-3 — `DecisionLogEntry` als `sealed class` ist pragmatisch richtig** (Scribe_Values braucht `ref`-Zugriff auf Felder → settable). Die Begründung im Class-Kommentar ist tragfähig. Record-mit-init wäre via `ExposeData`-Reflection möglich aber gegen RimWorld-Scribe-Konvention; ein Eintrag in `_bmad/decisions.md` wäre dennoch sinnvoll damit künftige Reviewer nicht dieselbe Frage haben. Kein Code-Fix.
- **LOW-4 — `IsExternalInit`-Polyfill in `Enums.cs`** (`Core/Enums.cs:35-38`): Als `internal static class` korrekt gescoped — pro-Assembly-Kollisionen mit anderen Mods unmöglich da `internal` nur im eigenen Assembly sichtbar. OK. Minor: wenn später zusätzliche Polyfills (z.B. `System.Range`) dazukommen, eigene `Polyfills.cs` besser als in `Enums.cs`. Kosmetisch.
- **LOW-5 — `Log.Message` in `MigrateV1ToV2` ist `Log.Warning` + DecisionLog — gut. Aber `MigrateV2ToV3` ist komplett silent.** Wenn AC 16 tatsächlich umgesetzt wird (siehe MED-2), ein Info-Log spiegelt den v2→v3-Pfad wenigstens wider.
- **LOW-6 — `Scribe_Values.Look(ref schemaVersion, "schemaVersion", 1);` default 1** ist korrekt (falls gänzlich fehlt, ist es der v1-Pfad), aber `Migrate()` wird nur aufgerufen wenn `schemaVersion < CurrentSchemaVersion`. Bei einem Brand-New-Save ohne vorherigen Scribe (Ctor-Init `schemaVersion = 3`) greift der Scribe-Default nicht — ok. Grenzfall-Check empfehlen: StartedNewGame + Savegame-ohne-Bot-Daten-Load sollte NICHT Migrate triggern. Aktuell tut es das nicht (PostLoadInit-Guard), aber ein Test wäre gut.

## Architektur-Invarianten

- **AI-1 (keine Tick-Patches):** PASS. Alle Tick-Arbeit via `GameComponentTick`-Override; keine `[HarmonyPatch]`-Einträge auf Tick-Loop-Methoden im gesamten Source-Tree zu dieser Story.
- **AI-4 (nur ein Singleton):** PASS. `RimWorldBotMod.Instance` bleibt einziger statischer Singleton. `BotGameComponent` ist per-Game-Instanz via RimWorld-GameComponent-Pattern, wird via `Current.Game.GetComponent<BotGameComponent>()` abgerufen (kein `public static Instance`). Korrekt.

## Recommendation

**APPROVE-WITH-CHANGES.** Die Persistenz-Infrastruktur ist vollständig, solide und architecture-konform. Zwei MED-Findings zu fixen bevor Story-Close:

1. **MED-1 fixen** — entweder echten WorldPawns-Lookup in `MigrateV1ToV2` implementieren (Story-Wortlaut), oder die Threshold-Branch-Logik + TC-10-Test-Spec anpassen weil der Reset-Pfad sie inherent nicht erfüllen kann.
2. **MED-2 klären** — `excludedCells`-Feld in `BotMapComponent` ergänzen oder AC 16 an die Story-2.5/2.6-Grenze zurückschieben (mit `_bmad/decisions.md`-Entry).

LOWs im selben Pass mitnehmen (insbesondere LOW-1 Null-safety + LOW-2 rechnerisch aufräumen mit MED-1). Nach Re-Review → done.

Konstruktive Antworten zu den kritischen User-Fragen:
- **`base(game)` entfernt:** Korrekt. RimWorld 1.6 `Verse.GameComponent` hat nur parameterless `.ctor()` (Reflection-verifiziert). Game-Ref via `Current.Game` erreichbar — aktuell nicht benötigt.
- **Migrate v1→v2 Reset:** Nicht akzeptabel wie ist. Entweder echten Lookup oder AC 15 explizit entschärfen — Dead-Code mit Fake-Threshold verwirrt mehr als er hilft.
- **IsExternalInit:** Okay wie platziert (internal-scoped, keine Kollision möglich).
- **Scribe-Timing:** Korrekt. `schemaVersion` zuerst + Migrate in PostLoadInit nach allen Feld-Loads ist exakt das Architecture-Pattern.
- **DecisionLogEntry als class:** Überzeugend. RimWorld-Scribe-Idiom ist settable fields; Kommentar im Code reicht, optional `_bmad/decisions.md`-Entry für Nachwelt.
