# Architecture — Re-Review Round 2

**Dokument:** `_bmad-output/planning-artifacts/architecture.md` v2.0
**Review-Datum:** 2026-04-24
**Review-Modus:** Re-Review durch 2 Round-1-Personas (RimWorld-Specialist + Stability-Engineer), Sign-Off-Anforderung aus Round-1-Report erfüllt (min. 2 der 4 Personas, davon die zwei CRIT-heißen)
**Gesamtverdict:** **APPROVE-WITH-MINOR-CHANGES** — alle Round-2-Findings in v2.1 eingearbeitet

---

## Executive Summary

Beide Re-Reviewer geben **APPROVE-WITH-MINOR-CHANGES**. Round-1-Findings aus ihren jeweiligen Perspektiven sind vollständig adressiert:

- **RimWorld-Specialist:** 8/8 eigene Findings + 4/4 Cross-Cutting → alle RESOLVED
- **Stability-Engineer:** 7/8 eigene Findings RESOLVED, 1 PARTIAL (F-STAB-04 Mod-Konflikt-Liste hardcoded) + 4/4 Cross-Cutting RESOLVED

Beide identifizierten zusammen **7 neue Findings** aus der Revision v2.0:

| Finding | Severity | Reviewer | Thema |
|---|---|---|---|
| F-EVENT-01 / F-STAB-09 | HIGH | beide (Dublette) | EventQueue-Spec fehlt (Bounds, Drop, Ordering, Stale-Check) |
| F-STAB-10 | HIGH | Stability | pendingPhaseIndex Post-Load-Reconcile fehlt |
| F-HARMONY-09 | MED | RimWorld | H6 WindowStack.Add ohne Typ-Filter → Event-Flood |
| F-COMPAT-01 | MED | RimWorld | CompatMode exact-match + unklare per-Target-Semantik |
| F-STAB-11 | MED | Stability | Global\Mutex + AbandonedMutex-Handling fehlt |
| F-STAB-12 | MED | Stability | BotErrorBudget Thread-Safety + Reset-Policy |
| F-PATCH-02 | LOW | RimWorld | H9 semantisch falsch in Patch-Tabelle |
| F-STAB-13 | LOW | Stability | GC.GetTotalMemory(false) false-positive-anfällig |

Keine neuen CRITs, keine Lade-Blocker. Beide Verdicts: APPROVE-WITH-MINOR-CHANGES. Nach Round-2-Einarbeitung (v2.1) sind alle neuen Findings adressiert.

---

## Status-Check (Round-1 Findings in v2.0)

### RimWorld-Specialist-Lens

| Finding | Severity | Status in v2.0 | Evidence |
|---|---|---|---|
| F-HARMONY-01 | CRIT | RESOLVED | §4: H1 entfernt; §3: "Kein Harmony-Patch auf Tick-Loop"; AI-1 Invariante; D-12 |
| F-HARMONY-02 | CRIT | RESOLVED | §4: H7 entfernt; §2.5: `MainTabWindow_BotControl` via `MainButtonDef` |
| F-HARMONY-03 | HIGH | RESOLVED | §4: H8 entfernt; §2.5: `ITab_Pawn_BotControl`; §10.2: `HumanInspectTabs.xml` |
| F-SAVE-01 | HIGH | RESOLVED | §5: `schemaVersion` + `Migrate()`; `BotMapComponent`; `MapAnalysisSummary` (schlank, Per-Cell rekomputierbar) |
| F-SAVE-02 | MED | RESOLVED | §5: `Dictionary<string, bool>` + `GetUniqueLoadID()`; Cleanup-Block 60000 Ticks |
| F-DLC-01 | HIGH | RESOLVED | §2.6: `DlcCapabilities`; §2.2: `EndingFeasibility` filtert; §11: `[MayRequire*]` Pflicht; AI-6 |
| F-PERF-01 | MED | RESOLVED | §8: "Coroutine" entfernt, "tick-budgeted iterator" etabliert |
| F-LEARN-01 | MED | RESOLVED | §6.1: `GenFilePaths.ConfigFolderPath` + atomic-Write + Mutex + `.corrupt-<ts>`-Fallback |
| CC-01 | CRIT | RESOLVED | siehe F-HARMONY-01 |
| CC-02 | CRIT | RESOLVED | siehe F-SAVE-01 |
| CC-03 | HIGH | RESOLVED | §4.1: Exception-Skelett verpflichtend; `BotErrorBudget` + `BotSafe.Get()`; §7.1 |
| CC-04 | HIGH | RESOLVED | siehe F-LEARN-01 |

### Stability-Engineer-Lens

| Finding | Severity | Status in v2.0 | Evidence |
|---|---|---|---|
| F-STAB-01 | CRIT | RESOLVED | §5: schemaVersion + Migrate; `UniqueLoadID`-Keying; `BotMapComponent` separation; try/catch + ResetToDefaults |
| F-STAB-02 | CRIT | RESOLVED | §4.1: Exception-Skelett; §7.1: StackOverflow/OOM-Clarification; §8: GC-Monitoring |
| F-STAB-03 | HIGH | RESOLVED | §6.1: cross-platform Pfad + Atomic-Write + Mutex + Corruption-Backup; §6.4: Sanity-Clamps |
| F-STAB-04 | HIGH | **PARTIAL** | §4.2 Runtime-Konflikt-Scan + `Priority.Low` + TC-06-Erweiterung; aber: Konflikt-Liste hardcoded, kein Update-Pfad. WorkPriorityWriter Read-After-Write-Check erwähnt aber nicht gezeigt |
| F-STAB-05 | MED | RESOLVED | §7.2: retryCount + 3-Fail-Threshold + poisonedGoals + Unlock-Timer + Tick-Cap |
| F-STAB-06 | MED | RESOLVED | §7.3: erweiterte Defensive-Annahmen; `BotSafe.Get`; Lazy ThingDef; ShutdownHooks |
| F-STAB-07 | MED | RESOLVED | §2.6 + §11: Telemetry default OFF, ID-only, 3×10MB Rotation |
| F-STAB-08 | LOW | RESOLVED | §10.3: SHA256SUMS + Token-Scope + Pre-Release-Lint |
| CC-01 — CC-04 | — | RESOLVED | siehe oben |

---

## Neue Findings aus Round 2 (identifiziert in v2.0)

### F-EVENT-01 / F-STAB-09 (HIGH) — Event-Queue ohne Spec

**Location (v2.0):** §3 Zeile 195–201, §4.1 `bot?.EnqueueEvent(...)`

**Issue:** `EnqueueEvent` und "Event-Queue" referenziert, aber ungelöst: (a) Max-Kapazität, (b) Drop-Policy bei Overflow, (c) Ordering-Garantie, (d) Stale-Event-Handling (Map/Pawn disposed vor Dispatch), (e) Event-Klassen-Hierarchie.

**Evidence (v2.0):** Nur textuelle Erwähnung, keine Service-Klasse in §2.6. Bei Raid-Kaskade + Quest-Flood potentiell 50+ Events/Tick.

**Fix in v2.1 (D-18):** `BoundedEventQueue<BotEvent>` als explizites Service in §2.6. Kapazität 256. Drop-Oldest + WARN-Log rate-limited (1/100 Drops). Stale-Check via `Map.uniqueID`/`UniqueLoadID`-Lookup. FIFO per Frame, zwischen Frames per Timestamp. `BotEvent`-Klassen-Hierarchie (`MapFinalizedEvent`, `RaidEvent`, `DraftedEvent`, `QuestWindowEvent`, `PawnExitMapEvent`). Events enthalten nur Identifikatoren, keine Live-Referenzen.

### F-STAB-10 (HIGH) — pendingPhaseIndex Recovery nach Crash

**Location (v2.0):** §3 + §5 Zwei-Phasen-Commit

**Issue:** Savegame kann mit `pendingPhaseIndex != currentPhaseIndex` geladen werden wenn Crash zwischen Pending-Set und Goal-Init stattfindet. Ohne Post-Load-Reconcile strandet Bot permanent in "pending".

**Fix in v2.1:** Neue `BotGameComponent.FinalizeInit()`-Override: wenn `pendingPhaseIndex != currentPhaseIndex`, rollback + `DecisionLogEntry("crash-recovery", …)`. Dokumentiert in §3 und §5.

### F-HARMONY-09 (MED) — H6 WindowStack.Add ungefiltert

**Location (v2.0):** §3 + §4 H6

**Issue:** `Verse.WindowStack.Add(Window window)` Postfix feuert bei JEDEM Window (Settings, Pause, Designator-Floats, Dialog). Ungefiltert flutet das die Event-Queue mit irrelevanten Events.

**Fix in v2.1:** H6-Patch-Skelett in §4.1 mit Typ-Filter: `if (window is not Dialog_NodeTree && window is not MainTabWindow_Quests) return;`. Nur Quest-relevante Windows werden enqueued.

### F-COMPAT-01 (MED) — CompatMode Match-Pattern + Semantik

**Location (v2.0):** §4.2

**Issue:** (a) Exact-Match gegen fixe Mod-ID-Liste verpasst Forks/Re-Uploads mit leicht anderen IDs. (b) Per-Target-Aufruf `EnableReducedPerformance(target)` unklar — Tick-Intervall-Änderung ist global.

**Fix in v2.1:** `compat-patterns.xml` als Mod-Asset in `About/` mit substring/regex-Match-Patterns (dynamisch patchbar ohne Recompile bei neuen bekannten Problem-Mods). `CompatMode.IsReduced` globaler Single-Set-Flag. Dokumentation in §4.2.

### F-STAB-11 (MED) — Mutex-Scope + AbandonedMutex

**Location (v2.0):** §6.1 `Global\RimWorldBot_LearnedConfig`

**Issue:** (a) `Global\`-Prefix macht Mutex system-weit, blockt Terminal-Server-Multi-User. (b) `AbandonedMutexException` bei Writer-Crash nicht behandelt — nächster Acquire wirft. (c) Nicht versioniert.

**Fix in v2.1:** Mutex-Name → `Local\RimWorldBot_LearnedConfig_v2` (per-Session). Explizite `AbandonedMutexException`-Behandlung: Integrity-Check auf XML-parse, bei Fehler rollback auf `.bak`.

### F-STAB-12 (MED) — BotErrorBudget Thread-Safety + Reset

**Location (v2.0):** §2.6

**Issue:** Static Token-Bucket ohne `lock` → NullRef bei Early-Exception vor Init. Reset-Verhalten bei Save-Load / Mod-Reload undokumentiert.

**Fix in v2.1:** §2.6-Eintrag präzisiert: lock-basiert, Reset on `GameComponent.StartedNewGame` + `LoadedGame`.

### F-PATCH-02 (LOW) — H9 semantisch falsch klassifiziert

**Location (v2.0):** §4 Patch-Tabelle H9

**Issue:** `Verse.Mod.DoSettingsWindowContents` ist virtual-Override, kein Harmony-Target.

**Fix in v2.1:** H9-Zeile aus §4-Tabelle entfernt. Separater Absatz "Kein Patch, aber Mod-Subclass-Override" klargestellt.

### F-STAB-13 (LOW) — GC.GetTotalMemory(false) false-positive

**Location (v2.0):** §8

**Issue:** Absolute `5× Budget = 50 MB`-Schwelle triggert False-Positive bei großen Modded-Colonies (500+ Pawns können legitim 200+ MB brauchen). `false`-Parameter ist approximativ.

**Fix in v2.1:** Relative Baseline-Methode: Baseline beim ersten Tick via `GC.GetTotalMemory(true)` (forced-GC einmalig akzeptabel), danach `GC.GetTotalMemory(false)` alle 2500 Ticks. Trigger bei `current > baseline * 5` für ≥ 3 konsekutive Samples. Periodische Re-Baseline alle 60 × 2500 Ticks.

### F-STAB-04 (PARTIAL → RESOLVED in v2.1)

**Issue (v2.0):** Konflikt-Liste hardcoded, kein Update-Pfad. WorkPriorityWriter Read-After-Write-Check erwähnt aber nicht gezeigt.

**Fix in v2.1:** `compat-patterns.xml` als externes Mod-Asset (siehe F-COMPAT-01) — dynamisch patchbar per Release ohne Recompile. WorkPriorityWriter Read-After-Write-Check bleibt in §2.4 als Execution-Kontrakt dokumentiert, konkrete Implementierung Story-gebunden in Epic 3.

---

## Sign-Off Round 2

| Kriterium | Status |
|---|---|
| Alle 7 Round-1-CRITs resolved | ✅ (bestätigt durch beide Re-Reviewer) |
| Alle 9 Round-1-HIGHs resolved | ✅ (8 RESOLVED, 1 PARTIAL → in v2.1 RESOLVED via `compat-patterns.xml`) |
| TQ-02 aufgelöst | ✅ (§4 + §4.2) |
| PRD-Konsistenz (D-11 reflektiert) | ✅ (§2.2 EndingFeasibility + D-17 Hysterese) |
| Re-Review durch ≥ 2 Personas | ✅ (RimWorld-Specialist + Stability) |
| Alle Round-2-Findings adressiert | ✅ (v2.1 Change-Log listet alle 7 neuen + F-STAB-04 Promotion) |

**Verdict Round 2 konsolidiert:** APPROVE-WITH-MINOR-CHANGES → nach v2.1-Einarbeitung: ready for User-Sign-Off.

---

## Offene Items für User-Entscheidung

1. **Finales Approval-Gate:** Nach Review der v2.1-Änderungen durch den User → Status auf `Approved` setzen, sub_phase auf STORY_DRAFTING wechseln, Story-Drafting für Epic 1 starten.
2. **Optional Round 3:** Nicht erforderlich laut Sign-Off-Kriterien (beide Round-2-Reviewer gaben APPROVE-WITH-MINOR-CHANGES; alle MINOR eingearbeitet). Nur wenn User explizit zusätzliche Personas-Lens haben will (C#-Architect, Game-AI), wäre Round 3 sinnvoll.

---

## Meta: Round-2-Methodik

- **Reviewer-Auswahl:** Die beiden Round-1-Personas mit den meisten CRITs (RimWorld-Specialist = 2 CRIT, Stability = 2 CRIT). C#-Architect und Game-AI hatten in Round 1 keine lade-blockierenden Findings — ihr Re-Review ist optional.
- **Fokus:** Status-Check vorhandener Findings (RESOLVED / PARTIAL / NOT-RESOLVED) + Suche nach neuen Problemen aus der Revision.
- **Kontext-Isolation:** Reviewer sahen Round-1-Report (als Check-Grundlage), v2.0-Architecture, Decisions D-12 bis D-17 — aber **nicht** die andere Round-2-Review.
- **Konsolidierung:** Main-Agent hier. Dublette F-EVENT-01/F-STAB-09 wurde als stärkeres Signal gewertet (zwei Perspektiven unabhängig → HIGH-Fix priorisiert).
