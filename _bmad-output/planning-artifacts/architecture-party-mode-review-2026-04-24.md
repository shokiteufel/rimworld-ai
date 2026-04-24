# Architecture — Party-Mode-Review

**Dokument:** `_bmad-output/planning-artifacts/architecture.md` v1.0 (PLANNING)
**Review-Datum:** 2026-04-24
**Review-Modus:** Party Mode (4 parallele Persona-Reviews, unabhängige Kontexte)
**Gesamtverdict:** **REJECT — Revision erforderlich vor Approval**

---

## Executive Summary

Die Architecture wurde von vier spezialisierten Reviewern parallel und unabhängig voneinander begutachtet. Zwei von vier Reviewern vergaben **REJECT** bzw. **APPROVE-WITH-CHANGES** mit CRIT-Findings, die Architecture-Revision blockieren. Drei Themen-Cluster wurden von mehreren Reviewern übereinstimmend gefunden — das ist das stärkste Signal des Reviews.

### Reviewer-Verdicts
| Reviewer | Perspektive | Verdict | # CRIT | # HIGH | # MED/LOW |
|---|---|---|---|---|---|
| RimWorld-/Harmony-Specialist | Mod-API, Patches, DLC, Perf | **REJECT** | 2 | 2 | 3 |
| C#-Software-Architect | SOLID, Testability, DI | APPROVE-WITH-CHANGES | 1 | 2 | 2 |
| Game-AI/Decision-Systems-Expert | FSM, Utility, Endings | APPROVE-WITH-CHANGES | 2 | 3 | 2 |
| Stability/Security/Compat-Engineer | Save, Exception, Mod-Konflikte | APPROVE-WITH-CHANGES | 2 | 2 | 4 |
| **Summe** | — | **REJECT** | **7** | **9** | **11** |

### Gesamtverdict-Begründung
Ein einzelner CRIT-Reviewer reicht für REJECT, wenn das Finding einen lade-verhindernden oder daten-zerstörenden Defekt benennt. Hier liegen **drei Cluster** solcher Findings vor (siehe Cross-Cutting Findings unten). Die Architecture ist in der Grundstruktur (Observation → Decision → Execution, 8 Epics, Lern-System, Phase-State-Machine) tragfähig. Die Findings sind reparierbar — aber Revision ist Pflicht bevor Story-Drafting für Epic 1 beginnen darf.

---

## Cross-Cutting Findings (mehrfach unabhängig gefunden)

Diese Findings wurden von **mindestens zwei Reviewern** aus unterschiedlichen Perspektiven unabhängig identifiziert. Sie haben Vorrang vor Einzel-Findings.

### CC-01 (CRIT): H1 patcht nicht-existente Tick-Methode
**Gefunden von:** RimWorld-Specialist (F-HARMONY-01) · C#-Architect (F-ARCH-04, verwandt) · Stability (F-STAB-02)
**Location:** architecture.md §4 Zeile 146
**Kern:** `RimWorld.Game.UpdatePlay` existiert in RimWorld 1.5/1.6 nicht als patchbare Tick-Quelle. Harmony wirft `patched method not found` → Mod lädt nicht. Der korrekte Weg ist `GameComponent.GameComponentTick()` (hat `BotGameComponent` ohnehin), oder für UI-Frames `MapComponent.MapComponentUpdate()`.
**Konsequenz bei Nicht-Fix:** Mod lädt beim Ersten Start gar nicht.
**Fix (konsolidiert):** H1 streichen. Tick-Loop in `BotGameComponent.GameComponentTick()` ziehen. Für Per-Map-Analyse separaten `BotMapComponent : MapComponent` anlegen. §3 Tick-Diagramm entsprechend anpassen.

### CC-02 (CRIT): Scribe_Deep auf `MapAnalysisResult` ohne Schema-Versioning
**Gefunden von:** RimWorld-Specialist (F-SAVE-01) · Stability (F-STAB-01)
**Location:** architecture.md §5 Zeile 181
**Kern:** `Scribe_Deep.Look(ref cachedAnalysis, "cachedAnalysis")` ohne `schemaVersion`-Feld führt bei jedem Field-Rename / neuem HazardType / gedropten Feld zu Load-Crash. Zusätzlich: `cachedAnalysis` ist per-Map, `BotGameComponent` aber Game-global — bei Multi-Map-Setups (Caravan-Map, Quest-Map) falsche Daten. Mod-Remove-Szenario produziert orphaned nodes und Red-Exceptions.
**Fix (konsolidiert):**
1. `Scribe_Values.Look(ref schemaVersion, "schemaVersion", 1)` als erste Zeile in `ExposeData`; bei Mismatch `cachedAnalysis = null` und Re-Scan.
2. `cachedAnalysis` aus `BotGameComponent` in neuen `BotMapComponent : MapComponent` mit schlankem `MapAnalysisSummary` (nur Top-3-Sites + Scores, kein Per-Cell-Array — Per-Cell rekomputierbar).
3. `MapAnalysisResult` implementiert `IExposable` mit eigenem try/catch pro Feld.
4. `perPawnPlayerUse` neu keyen auf `Pawn.GetUniqueLoadID()` (string) statt `thingIDNumber` — plus periodisches Cleanup destroyed/discarded Pawns.

### CC-03 (HIGH): Harmony-Patch-Exception-Wrapper fehlen für alle Patches
**Gefunden von:** C#-Architect (F-ARCH-04) · Stability (F-STAB-02)
**Location:** architecture.md §7
**Kern:** §7 zeigt try/catch nur um `botController.Tick()`. Exceptions in Harmony-Postfixes propagieren **im Target-Caller-Frame**, nicht im Bot-Frame — sie werden vom §7-try also gar nicht gefangen. H4/H5/H6 laufen aus kritischen Vanilla-Flows (Raid-Announce, Draft-Setter, Window-Stack). Ein throw dort kann User-Draft-Klicks blocken oder Red-Error-Spam produzieren.
**Fix (konsolidiert):** Jede Harmony-Patch-Methode (H1–H9) bekommt identisches Skelett:
```csharp
static void Postfix(...) {
  try { ... }
  catch (Exception ex) {
    if (!BotErrorBudget.CanLog()) return;   // max 5/min
    Log.Error($"[RimWorldBot] {nameof(MethodName)}: {ex}");
    BotController.FallbackToOff();
  }
}
```
Plus: `FallbackToOff()` selbst try-geschützt, enthält nur `masterState = Off; return;` — keine Non-trivial-Logic. OutOfMemory/StackOverflow sind nicht fangbar → periodisches `GC.GetTotalMemory`-Monitoring (Soft-Self-Disable bei 5× Overshoot des §8-Budgets).

### CC-04 (HIGH): LearnedConfig Race-Condition, Cross-Platform und Corruption-Loss
**Gefunden von:** RimWorld-Specialist (F-LEARN-01) · Stability (F-STAB-03)
**Location:** architecture.md §6 Zeile 196
**Kern:** Pfad `%APPDATA%\..\LocalLow\...\Config\` ist (a) Windows-spezifisch — Linux/Proton crasht, (b) Ludeon-Owned-Folder, (c) ohne File-Lock bei zwei parallelen Instanzen, (d) Corruption-on-write = Totalverlust der gelernten Overrides ohne User-Zustimmung, (e) User-Edit trivial → Weights können extreme Werte bekommen, keine Sanity-Clamps dokumentiert.
**Fix (konsolidiert):**
1. Pfad via `GenFilePaths.ConfigFolderPath` + Subfolder `RimWorldBot/` (cross-platform, RimWorld-Standard).
2. Write-Pattern: tmp-file + atomic rename + `.bak` der letzten Version.
3. Parse-Error: Datei in `.corrupt-<timestamp>` umbenennen, Default neu starten, User-Toast.
4. Global-Mutex während Write.
5. Sanity-Clamps: Weights auf `[0, 1]`, Thresholds auf plausible Ranges; Log bei Out-of-Range.
6. Expliziter `<schemaVersion>` + Migration-Chain (1→2→3 …).

---

## Einzel-Findings nach Reviewer

### Reviewer A — Senior RimWorld Mod Engineer

> Perspektive: Harmony-Patch-Korrektheit, RimWorld-API, Performance im Tick-Loop, DLC-Matrix, Vanilla-Kompatibilität, Mod-Konflikte.

#### F-HARMONY-01 (CRIT) — H1 auf nicht-existente `Game.UpdatePlay`
Siehe CC-01. Bereits konsolidiert oben.

#### F-HARMONY-02 (CRIT) — Widerspruch H7 vs. `MainButtonDefs.xml`
**Location:** §4 H7 + §10 Zeile 358
H7 patcht `MainButtonsRoot` per Inject, gleichzeitig listet §10 `MainButtonDefs.xml` als Def-Registrierung. `MainButtonDef` wird über XML + `MainTabWindow`-Subklasse registriert — kein Patch nötig. `MainButtonsRoot` hat zudem keine Inject-freundliche API.
**Fix:** H7 streichen. Nur `MainButtonDef` + `MainTabWindow_BotControl`. Für Top-Bar-Gizmo: `PlaySettings.DoPlaySettingsGlobalControls` per Harmony-Postfix (sauberer Weg).

#### F-HARMONY-03 (HIGH) — `ITab_Pawn_Character` ist falscher Ort
**Location:** §4 H8 Zeile 153
Char-Tab rendert Backstory/Traits; Per-Pawn-Flag-Checkbox dort kollidiert mit Character Editor, EdB Prepare Carefully, Yayo's Animation.
**Fix:** Entweder eigener `ITab_Pawn_BotControl : ITab` registriert via `ThingDef`-Patch auf `Human.inspectorTabs`, ODER `Pawn.GetGizmos`-Postfix mit Toggle-Gizmo. Beide weniger intrusiv.

#### F-SAVE-01 (HIGH) — Scribe_Deep auf cachedAnalysis
Siehe CC-02.

#### F-SAVE-02 (MED) — perPawnPlayerUse-Leak bei Pawn-Tod/Banish/Slave-Verkauf
**Location:** §5 Zeile 183
`Dictionary<int, bool>` mit `thingIDNumber` als Key verwaist Einträge für immer.
**Fix:** `Scribe_References` auf `List<Pawn>` (LookMode.Reference) mit Cleanup von Destroyed/Discarded-Pawns im `GameComponentTick()` alle 60000 Ticks.

#### F-DLC-01 (HIGH) — DLC-Feature-Detection fehlt komplett
**Location:** §11 Zeile 382 + §2.2 `EndingFeasibility`
PRD fordert alle 5 Endings; ohne `ModLister.RoyaltyInstalled`-Guards crasht `EndingFeasibility.Score()` bei fehlenden DefOfs (Journey/Archonexus brauchen Royalty/Ideology, Biotech-Pfade Archotech-Genes). `[DefOf]`-Felder werfen `NullReferenceException` bei Init ohne `[MayRequire*]`.
**Fix:** Expliziter `DlcCapabilities`-Service, alle DLC-Defs mit `[MayRequire*]` annotiert, `EndingFeasibility` filtert unerreichbare Endings raus.

#### F-PERF-01 (MED) — "Coroutine" existiert in RimWorld nicht
**Location:** §8 Zeile 292
Single-Thread-Engine. `LongEventHandler.QueueLongEvent` blockiert UI.
**Fix:** Yield-`IEnumerator` in `MapComponent.MapComponentTick()` mit 2 ms/Tick-Budget bis fertig. §8 und §3: Begriff "Coroutine" durch "tick-budgeted iterator" ersetzen.

#### F-LEARN-01 (MED) — Config-Pfad-Ownership und Locking
Siehe CC-04.

### Reviewer B — Senior C# Software Architect

> Perspektive: SOLID, Schicht-Sauberkeit, Testbarkeit, Dependency-Management, Lifecycle.

#### F-ARCH-01 (CRIT) — Schicht-Leck Decision ↔ Execution
**Location:** §2.3 + §2.4 + §3
Widerspruch: `BillScheduler` in Decision "Cooking-Bills, Crafting-Bills", `BillManager` in Execution "Setzt/entfernt Bills auf Workbenches" — beide arbeiten auf Bills. §3 ruft `BillScheduler.Ensure()` direkt auf → Decision mutiert RimWorld-State. Analog `WorkAssigner` (Decision) "updates Pawn work priorities" (= Execution). `DraftController` vs. `CombatCommander` — ungeklärt wer `Pawn.drafter` anfasst.
**Fix:** Plan-Objekte einführen. Decision-Klassen geben `BillPlan`, `WorkPriorityPlan`, `DraftOrder` zurück; Execution appliziert. Controller orchestriert: `var plan = billScheduler.Plan(state); billManager.Apply(plan);`. Umbenennung: `BillScheduler` → `BillPlanner`, `WorkAssigner` split in `WorkPlanner` + `WorkPriorityWriter`.

#### F-ARCH-02 (HIGH) — Unit-Tests hängen an RimWorld-Runtime
**Location:** §9
`MapAnalyzer.ScoreCell()` hängt an `TerrainDef` (kein xUnit-testbar). `EndingFeasibility` an `ResearchProjectDef`. `PhaseDefinition.AreExitConditionsMet()` an Colony-State.
**Fix:** Domain-DTOs einführen — `CellSnapshot` (POCO mit Fertility:float, HasWater:bool, HazardKind:enum), `ColonySnapshot`, `PawnSnapshot`. Analyzer arbeitet nur auf Snapshots. `ISnapshotProvider`-Schicht zwischen Harmony und Analysis mappt `Map → CellSnapshot`. Unit-Tests konstruieren Snapshots frei.

#### F-ARCH-03 (HIGH) — Kein Dependency-Injection-Modell, keine Config-Precedence
**Location:** §2.1, §2.6, §5, §6
`BotController`-Deps in §2.1 nur 3, tatsächlich 6+. Drei Konfig-Quellen (`Configuration` ModSettings + `LearnedConfig` + `BotGameComponent`) ohne Precedence-Regel bei Konflikt (z. B. User ModSettings vs. LearnedConfig). Kein Lifecycle-Owner für `BotController`.
**Fix:** Composition-Root in `BotGameComponent.FinalizeInit()`; baut `BotController` mit Konstruktor-Injection. Neue §5a "Configuration Resolution": Precedence `BotGameComponent > LearnedConfig > Configuration > Defaults`, als `ConfigResolver.Get<T>(key)`. Keine Singletons/Statics außer `RimWorldBotMod` Entry.

#### F-ARCH-04 (MED) — Harmony-Exception-Wrap + Idempotenz-Kontrakt
Siehe CC-03. Zusätzlich: Phase-Transition als zweiphasiges Commit (`pendingPhaseIndex` + `currentPhaseIndex`, Commit erst nach Goal-Init), Invariants dokumentiert idempotent.

#### F-ARCH-05 (MED) — TQ-02 ist architektur-blockierend, nicht kosmetisch
**Location:** §12
Harmony-Priority-Matrix bei Konflikten mit Common Sense / Better Pawn Control betrifft H1 + H5. Ohne dokumentierte `[HarmonyPriority]`-Matrix ist Verhalten nicht-deterministisch. TQ-05 (Profiling-Toolchain) nötig für §8-Budget-Verifikation.
**Fix:** TQ-02 vor Story-Drafting klären: `[HarmonyPriority(Priority.Last)]` für Postfixes dokumentieren + Konflikt-Matrix in §4. TQ-05: Dev-Mode-Stats vs. custom `[PerfCounter]` entscheiden. TQ-01/03/04 in Stories verschieben.

### Reviewer C — Game-AI / Decision-Systems Expert

> Perspektive: State-Machine-Design, Invariant/Emergency-Priorisierung, Ending-Switching, Entscheidungs-Transparenz. Review-Lens inkl. **Decision D-11** (Zeit ist keine Phase-Erreichungs-Voraussetzung).

#### F-AI-01 (CRIT) — Phase-Transition respektiert Invariant-Violations nicht
**Location:** §3 + Mod-Leitfaden §13
Phase-Transition läuft in Step 2 unbedingt. Es gibt keine Guard `if any_emergency_active: skip phase_transition`. Szenario: Phase 3 Exit-Cond `food_stock_days ≥ 90` ist erfüllt, gleichzeitig E-FIRE im Kühllager triggert, Phase transitioniert trotzdem auf Phase 4, Bot plant Steinmauern während Lager brennt. Re-Entry in Phase 3 nicht vorgesehen — `currentPhaseIndex` monoton.
**Fix:**
1. Tick-Loop: Phase-Transition nur wenn `emergency_active == false && invariants.all_passing()` für ≥ 2 konsekutive Evaluations.
2. `currentPhaseIndex` erlaubt Rückwärts-Transitions — bei Phase-(N-1)-Exit-Cond-Bruch Rollback auf N-1 mit Decision-Log-Eintrag.
3. ExitCondition-Check mit `min_hold_duration` (z. B. 5000 Ticks stabil), verhindert Rand-Wert-Flackern.

#### F-AI-02 (CRIT) — Emergency-Prio ist fix statt kontext-sensitiv
**Location:** Mod-Leitfaden §2 + architecture.md §2.1
Fixe Priorität (E-FIRE > E-BLEED > E-INTRUSION) ohne Situations-Awareness. Szenario: Raid betritt Home-Area + Pawn A verblutet außerhalb → fixe Prio wählt E-BLEED, Doctor läuft ins Raid-Feuer → beide tot statt einer.
**Fix:** `EmergencyScore(e) = base_prio(e) + context_modifiers(e, world)`. Modifier z. B. `+100 wenn E-BLEED-Pawn unreachable unter aktiver Intrusion`. Handler-Execution als Utility-Maximizer, nicht Strict-Priority. Handler deklarieren Pawn-Exclusivity (Draft-Lock), E-BLEED geht auf Rescue-Later-Queue wenn Pawn aktuell nicht assignable.

#### F-AI-03 (HIGH) — Ending-Switching ohne Sunk-Cost-Guard / Hysterese
**Location:** §2.2 + PRD §FR-07 + D-11
Rebalance alle 2500 Ticks. Mit D-11 kein "bald entscheiden"-Druck → Feasibility-Flips bei zwei fast-gleichen Endings. Szenario: Ship 0.62 / Archonexus 0.60 → Raid zerstört Steel → flip → Ressourcen-Verluste → re-flip → Half-Ending.
**Fix:**
1. Switch nur wenn `new > current + HYSTERESIS_MARGIN` (z. B. 0.15).
2. Sunk-Cost: `switch_cost = resources_invested * 0.7` abziehen.
3. Ending-Commitment ab Phase 7: nur noch Switch bei Game-Breaking-Trigger (Monolith weg, Reaktor zerstört), keine Feasibility-Flips.
4. Dokumentation in §2.2 dass `Reevaluate()` Hysterese-Gate checkt.

#### F-AI-04 (HIGH) — Strukturelle Stuck-States nicht behandelt
**Location:** Mod-Leitfaden §3 + §7
Wenn `top_3_sites == null` (z. B. 80% polluted terrain → Hard-Filter schließt alles aus), bleibt Bot in Phase 0a stuck. Null-Result ist keine Exception, fällt nicht auf AI_OFF. Kein Handler für "colony_extinct".
**Fix:**
1. Phase 0a: Wenn `top_3_sites.empty`, Fallback "best-available-with-warning" + Toast.
2. Neue Global-Invariant I0 `colonist_count > 0` + Handler E-EXTINCT: Bot stoppt, UI-Meldung.
3. Jede Phase braucht Fallback-Path bei "Exit-Cond strukturell unerreichbar" (z. B. Crafting-Passion-Pawn tot → Grind-Fallback oder Ending-Switch auf Journey).

#### F-AI-05 (HIGH) — Bayesian-Update konvergiert nicht bei Biome-Drift
**Location:** §6 Zeile 245 + 247-252
`new = 0.95*old + 0.05*observed` konvergiert nicht, wenn `observed` driftet (verschiedene Biomes in aufeinanderfolgenden Runs). Keine Sample-Count-Awareness — 100. Run gleichstark wie 2.
**Fix:**
1. Weights **per Biome** speichern.
2. Sample-Count-Formel: `new = old + (observed - old) / (count + k)` mit k = 20 Prior — konvergiert garantiert.
3. Pro Biome min. 5 Runs bevor Abweichung vom Default.
4. LearnedConfig-Corruption: Schema-Validation + Fallback auf Defaults + `.xml.bak`.
5. Statistics: separiere `EndingsReached` per Biome + Storyteller-Difficulty.

#### F-AI-06 (MED) — Score-Breakdown nicht im UI exponiert
**Location:** §2 UI + PRD §FR-04
`SiteMarkerOverlay` zeigt Gesamt-Score, nicht Breakdown. Ending-Switches nur im JSONL-Log, nicht sichtbar.
**Fix:** `SiteMarkerOverlay` Hover-Tooltip mit Score-Breakdown-Tabelle. `DebugPanel` Tab "Decisions" mit Trail der letzten 20 Bot-Entscheidungen + Begründung. `BotGameComponent.recentDecisions` (bounded FIFO 100, persistiert).

#### F-AI-07 (MED) — Regression-Detector für abgeschlossene Phase-Goals fehlt
**Location:** §2.3 `CombatCommander` + Mod-Leitfaden §8.1 + §11 Anti-Pattern 7
Phase-Cond `food_stock_days ≥ 90` einmal erreicht, dann durch Raid auf 30 gefallen → Bot pusht weiter Stonecutting-Research statt Food-Replenishment. Lücke zwischen Exit-Cond (90) und Invariant-Trigger (9) ist 81 Tage breit.
**Fix:** Pro Exit-Cond `GoalHealthScore` (0-1), alle 5000 Ticks geprüft. Bei Regression: Goal `retriggered` vor allen offenen. Bei Score < 0.7: Priority-Flush — alle niedriger-prio Goals pausiert bis Health > 0.85.

### Reviewer D — Stability / Security / Compatibility Engineer

> Perspektive: Save-Compat, Exception-Härte, Mod-Konflikte, LearnedConfig-Security, Telemetry-Hygiene, Build/Release.

#### F-STAB-01 (CRIT) — Scribe_Deep ohne Versioning + perPawnPlayerUse-Keying
Siehe CC-02.

#### F-STAB-02 (CRIT) — Harmony-Exception-Claim überoptimistisch
Siehe CC-03. Zusätzlich: Error-Budget (max 5/min dann silent), OutOfMemory nicht fangbar → `GC.GetTotalMemory`-Soft-Self-Disable bei 5× §8-Overshoot.

#### F-STAB-03 (HIGH) — LearnedConfig-Storage: Race, Cross-Platform, Corruption-Loss
Siehe CC-04.

#### F-STAB-04 (HIGH) — Mod-Patch-Konflikte nicht konkret adressiert
**Location:** §4 H1 + TQ-02 + PRD NFR-02
Common Sense / RocketMan / Performance Fish / Work Tab patchen ähnliche Targets bzw. transpilen `Game.UpdatePlay`. Kein Priority-Attribut, keine Runtime-Konflikt-Erkennung.
**Fix:**
1. `[HarmonyPriority(Priority.Low)]` auf alle Postfixes.
2. Startup-Scan `Harmony.GetAllPatchedMethods()`: wenn H1-Target schon von {RocketMan, Performance Fish, Dubs PA} gepatcht → Reduced-Mode (Tick-Intervall ×2) + Log-Warnung.
3. WorkAssigner Read-After-Write-Check auf `pawn.workSettings.priorities`.
4. Test-Gate TC-06 erweitern um Top-10-Mods, nicht nur DLC-Kombos.
5. TQ-02 vor Epic 1 auflösen.

#### F-STAB-05 (MED) — "Goal re-queue, continue" kann endlos-loopen
**Location:** §7
Deterministisch failendes Goal → Re-Queue → selbes Fail → Infinite-Loop.
**Fix:** `retryCount` pro Goal. Bei ≥ 3 Fails in < 60 s: Poisoned-Set, Phase-Evaluator überspringt, nach 10 min Retry-Unlock. Tick-Cap: max 100 Goal-Evaluations/Tick.

#### F-STAB-06 (MED) — Defensive-Annahmen unvollständig
**Location:** §7
Fehlt: `ThingDef != null` (Mod-Def-Replacer dangling), `Faction != null` (Anomaly-Szenarien), `map.thingGrid != null` (während `Map.Dispose`). Dev-Mode Mod-Reload setzt Harmony zurück, Bot läuft ohne Patches weiter.
**Fix:** `BotSafe.Get<T>(Func<T>)` catch-all null-bubble. ThingDef-Caches als `Lazy<ThingDef>` mit re-resolve. `BotController.ShutdownHooks()` in `Game.FinalizeInit`. Pre-Check im Tick: `Current.Game != null && Current.Game.CurrentMap != null`.

#### F-STAB-07 (MED) — Telemetry PII und Rotation
**Location:** §2.6 + NFR-06 + §6 Statistics
Pawn-Namen sind user-customized (Familie/Freunde/Real-Namen). JSONL in GitHub-Issues geposted → PII-Leak. `opt-in default off` nicht hart.
**Fix:**
1. Default OFF, ModSettings-Checkbox + Restart-Prompt.
2. Pawn-Namen immer durch `pawn.GetUniqueLoadID()` ersetzen.
3. Rotation: 10 MB → `.1.jsonl` → `.2` (keep 3, total 30 MB hard cap).
4. README-Doku: "Telemetry enthält Colony-IDs, keine Klarnamen".
5. Statistics in LearnedConfig nur IDs.

#### F-STAB-08 (LOW) — GitHub-Release ohne Checksum/Signatur
**Location:** §10
ZIP ohne SHA256, Token-Scopes unspezifiziert, `LoadFolders.xml` ↔ `About.xml/supportedVersions` Divergenz nicht gelintet.
**Fix:** SHA256SUMS als Release-Asset. Token-Scope `contents:write` only. Pre-Release-Lint-Step für Version-Konsistenz.

---

## Konsolidierte Action-Matrix

Sortiert nach Severity + Cross-Cutting-Bonus. Jeder Fix ist vor Story-Drafting des jeweiligen Epic zu adressieren.

| ID | Severity | Kurz | Revision in Architecture-§ | Blockt Epic |
|---|---|---|---|---|
| CC-01 | CRIT×3 | H1 raus, GameComponent.GameComponentTick() | §3, §4 | Epic 1 (1.2, 1.3) |
| CC-02 | CRIT×2 | Scribe-Versioning + BotMapComponent + UniqueLoadID | §5 | Epic 1 (1.3), Epic 2 (2.8) |
| CC-03 | HIGH×2 | Harmony-Exception-Wrap-Skelett + FallbackToOff | §7 | Epic 1 (1.2) |
| CC-04 | HIGH×2 | GenFilePaths.ConfigFolderPath + atomic write + mutex | §6 | Epic 8 (8.1) |
| F-ARCH-01 | CRIT | Decision→Plan / Execution→Apply, Bill/Work split | §2.3, §2.4, §3 | Epic 3 (3.8, 3.9, 3.10) |
| F-ARCH-02 | HIGH | Snapshot-DTOs, ISnapshotProvider | §2.2, §9 | Epic 2 (2.1, 2.5) |
| F-ARCH-03 | HIGH | Composition-Root + ConfigResolver + Precedence | §2.1, §5a neu | Epic 1 (1.3, 1.7) |
| F-ARCH-05 | MED | TQ-02 HarmonyPriority-Matrix | §4, §12 | Epic 1 (1.2) |
| F-DLC-01 | HIGH | DlcCapabilities + `[MayRequire*]` | §11, §2.2 | Epic 7 (alle) |
| F-AI-01 | CRIT | Phase-Transition-Guard + Rollback + min_hold_duration | §3, §2.1 | Epic 3 (3.12), Epic 4 |
| F-AI-02 | CRIT | Emergency-Utility-Scoring statt Fix-Prio | §2.1 | Epic 3 (3.3–3.6) |
| F-AI-03 | HIGH | Ending-Hysterese + Sunk-Cost + Phase-7-Commitment | §2.2 | Epic 7 (7.1–7.4) |
| F-AI-04 | HIGH | Stuck-State-Fallbacks + Invariant I0 colonist_count | §7, §2.1 | Epic 3 (3.1, 3.2) |
| F-AI-05 | HIGH | Bayesian mit Sample-Count + per-Biome | §6 | Epic 8 (8.2) |
| F-AI-06 | MED | Score-Breakdown-Tooltip + Decision-Trail | §2.5 | Epic 2 (2.7), Epic 8 (8.7) |
| F-AI-07 | MED | GoalHealthScore + Regression-Detector | §2.1 | Epic 3 (3.7, 3.11) |
| F-HARMONY-02 | CRIT | H7 raus, MainButtonDef + MainTabWindow | §4, §10 | Epic 1 (1.4) |
| F-HARMONY-03 | HIGH | ITab_Pawn_BotControl statt H8 | §4 | Epic 1 (1.6) |
| F-SAVE-02 | MED | perPawnPlayerUse Cleanup-Tick | §5 | Epic 1 (1.3) |
| F-PERF-01 | MED | "Coroutine" → tick-budgeted iterator | §3, §8 | Epic 2 (2.9) |
| F-STAB-04 | HIGH | Startup-Scan + Priority.Low + Reduced-Mode | §4, §11 | Epic 1 (1.2) |
| F-STAB-05 | MED | Retry-Cap + Poisoned-Queue | §7 | Epic 3 (3.10) |
| F-STAB-06 | MED | BotSafe.Get + Lazy ThingDef + ShutdownHooks | §7 | Epic 1 (1.3) |
| F-STAB-07 | MED | Telemetry default-off + PII-Scrub + Rotation-30MB | §2.6, §10 | Epic 8 (8.8) |
| F-STAB-08 | LOW | SHA256SUMS + Token-Scope + Lint-Step | §10 | Epic 8 (8.10) |

---

## Sign-Off-Anforderungen für Re-Review

Bevor die Architecture als **Approved** markiert werden darf, müssen folgende Punkte erfüllt sein:

1. **Alle 7 CRIT-Findings** (CC-01, CC-02, F-ARCH-01, F-AI-01, F-AI-02, F-HARMONY-02, F-STAB-01/02) revidiert in architecture.md.
2. **Alle 9 HIGH-Findings** revidiert oder explizit als Story-Task deferred (mit Decision-Log-Eintrag D-XX pro Deferral).
3. **TQ-02 aufgelöst** (HarmonyPriority-Matrix + Mod-Konflikt-Test-Gate).
4. **PRD-Konsistenz-Check:** D-11 (Zeit keine Voraussetzung) korrekt in Architecture reflektiert — besonders in §2.2 `EndingFeasibility` Reevaluation-Kadenz und in §3 Tick-Flow.
5. **Re-Review** durch mindestens 2 der 4 Personen-Lenses (RimWorld-Specialist + Stability sind die kritischen, da dort die meisten CRITs lagen).
6. **MED-Findings** dürfen als Story-Tasks übergehen, ABER müssen in `epics.md` den entsprechenden Stories als Sub-Tasks angehängt werden (sonst Cherry-Picking-Finding nach Guardian-Rule 4).

---

## Meta: Party-Mode-Methodik

- **Reviewer-Auswahl:** 4 komplementäre Perspektiven, keine Persona-Überlappung
- **Kontext-Isolation:** Jeder Reviewer spawned als eigener Subagent ohne Sicht auf die anderen drei Reviews — unabhängige Urteile
- **Severity-Skala konsistent:** CRIT (Lade-/Daten-Bruch), HIGH (Feature-Bruch / Re-Design eines Layers), MED (Wartbarkeit/Perf), LOW (Kosmetik)
- **Output-Limit:** 600 Wörter pro Reviewer (erzwingt Finding-Auswahl statt Komplett-Enumeration)
- **Verdict-Vokabular:** `APPROVE | APPROVE-WITH-CHANGES | REJECT` — binär interpretierbar
- **Konsolidierung:** Hier in diesem Dokument durch den Main-Agent. Cross-Cutting-Findings (≥ 2 Reviewer identifizieren dasselbe) haben Vorrang vor Einzel-Findings als Signal.

Das Format ist wiederverwendbar — für Re-Review nach Revision derselbe 4-Reviewer-Spawn, neues Review-Dokument (z. B. `architecture-party-mode-review-round-2-YYYY-MM-DD.md`).
