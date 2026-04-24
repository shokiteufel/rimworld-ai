# Architecture — Re-Review Round 4 (Full Panel)

**Dokument:** `_bmad-output/planning-artifacts/architecture.md` v2.2
**Review-Datum:** 2026-04-24
**Review-Modus:** Full-Panel (alle 4 Personas, unabhängige Subagent-Kontexte)
**Gesamtverdict:** **APPROVE-WITH-MINOR-CHANGES** × 4 — aber substanziellere Findings als Round 3, siehe Analyse unten.

---

## Executive Summary

Alle vier Reviewer geben erneut **APPROVE-WITH-MINOR-CHANGES**. Keine REJECTs, keine neuen CRITs. **Aber** die Finding-Anzahl ist höher als in Round 3 und enthält 1 Regression, 4 Status-Downgrades (RESOLVED→PARTIAL) und 5 neue HIGHs — keine Diminishing-Returns-Kurve.

### Reviewer-Verdicts
| Reviewer | v2.2 Verdict | Round-3 RESOLVED | Round-3 PARTIAL | REGRESSED | Neu HIGH | Neu MED | Neu LOW |
|---|---|---|---|---|---|---|---|
| RimWorld-Specialist | APPROVE-W-MINOR | 5/5 + F-AI-09 | 0 | 0 | 0 | 1 | 3 |
| C#-Architect | APPROVE-W-MINOR | 4/6 | 2 (F-ARCH-08, F-ARCH-09) | 1 (F-ARCH-11) | 2 | 3 | 1 |
| Game-AI-Expert | APPROVE-W-MINOR | 5/7 | 2 (F-AI-11, F-AI-12) | 0 | 1 | 2 | 1 |
| Stability-Engineer | APPROVE-W-MINOR | 5/6 (F-STAB-10 wieder PARTIAL) | 1 | 0 | 2 | 3 | 2 |
| **Summe** | — | 19/24 | **5** | **1** | **5** | **9** | **7** |

### Die wichtigste Erkenntnis
Round 4 hat **5 neue HIGHs** gefunden. Round 3 hatte 5 HIGHs. Round 2 hatte 2 HIGHs. Round 1 hatte 9 HIGHs. Die Review-Funde konvergieren **nicht monoton gegen Null** — sie pendeln. Das ist kein Zufall: jede Architektur-Revision führt neue Oberflächen ein, und jede Persona findet auf diesen Oberflächen neue Probleme. Round 5 würde vermutlich ähnlich viele Findings produzieren.

**Das ist ein struktureller Hinweis**, kein Signal dass v2.2 schlecht ist — v2.2 ist substanziell besser als v1.0. Aber **perfekte Architecture vor Story-Drafting** ist nicht erreichbar. Irgendwann muss der Sprung zum Code-Level gemacht werden.

---

## Status-Korrekturen

### F-STAB-10 (HIGH) — RESOLVED → PARTIAL (erneut)
In Round 2 war F-STAB-10 RESOLVED. In Round 3 wurde es zu PARTIAL korrigiert (orphaned Goals). In v2.2 wurde `ReconcilePhaseGoalOrphans()` und `CancelOrphaned*()` spezifiziert — Stability-Reviewer stellt jetzt fest: **die Goal-Phase-Assoziation wird nirgends spezifiziert.** Ohne Tag-Schema auf Blueprint/Designation/Job ist die Methode ein No-op. F-STAB-21 (neu, HIGH) fordert explizites Tag-Schema.

### F-ARCH-11 (LOW) — RESOLVED → REGRESSED
Round 3 forderte EventQueue-Init vor Harmony-Patch-Möglichkeit. v2.2 initialisiert EventQueue in `FinalizeInit()` **nach** `BuildController()` (§5 Code Zeile 425). Wenn ein Factory-erzeugter Kollaborateur EventQueue im Konstruktor referenziert → NullRef. F-ARCH-15 (HIGH, neu) eskaliert das zusammen mit fehlender Feld-Deklaration.

### F-ARCH-08 (MED) PARTIAL + F-ARCH-09 (MED) PARTIAL
Cache-Spec vorhanden aber `Dictionary<ConfigKey, object>`-Typ-Erasure ist Cast-Fehler-Risiko. Plan-Records enthalten RimWorld-Runtime-Typen (`ThingDef`, `Building_WorkTable`, `IntVec3`, `Rot4`) — widerspricht §2.2 Testbarkeits-Invariante. F-ARCH-13 (HIGH, neu) fordert Umstellung auf Identifikator-only-Pattern (analog D-21-Regel für Pawns).

### F-AI-11 (MED) PARTIAL + F-AI-12 (MED) PARTIAL
Auto-Escape-Timer in §6.3 hat F-AI-08-isomorphes Reset-Problem (Timer zählt absolute Zeit seit Phase-7-Entry, nicht konsekutive Erfüllung) → F-AI-15 (HIGH). `GoalHealthScore`-Aggregation `min(across all completed goals)` löst Priority-Flush-Kaskaden bei Rand-Werten orthogonaler Exit-Conds aus → F-AI-16 (MED).

---

## Cross-Cutting-Themen Round 4

### CC-4-01 (HIGH) — Counter-Reset-Semantik wiederholt sich
**Beteiligt:** F-AI-15 (neu, Reviewer: Game-AI)
**Pattern:** Das gleiche Class-of-Problem wie F-AI-08 (Round 3 HIGH) tritt jetzt auf Ending-Ebene in §6.3 Auto-Escape auf. Timer zählt absolute Zeit statt konsekutive Bedingungserfüllung. Architektonisch: Jeder Stable-Counter braucht Reset-Semantik.
**Fix:** Generisches `StableConsecutiveCounter`-Pattern mit explizitem Vertrag (Reset bei Bedingung-Bruch, zählt monoton sonst), angewendet auf: Phase-Transition (F-AI-08, bereits OK), Auto-Escape (F-AI-15, neu), ggf. zukünftige.

### CC-4-02 (HIGH) — Plan-Schema-Vertrag unvollständig durchgehalten
**Beteiligt:** F-ARCH-13 (neu, HIGH), F-ARCH-09 (PARTIAL)
**Pattern:** D-21 führt `UniqueLoadID`-Pattern für Pawns ein. Aber Plan-Records nutzen Runtime-Typen für alles andere (ThingDef, Building, IntVec3, Rot4). Test-Invariante bricht am Plan-Schema.
**Fix:** Konsistente Identifikator-only-Semantik in allen Plan-Records: `ThingDef` → `string defName`, `Building_WorkTable` → `int thingIDNumber`, `IntVec3` → `(int x, int z)`, `Rot4` → `byte rotation`. Resolve im Apply-Layer, nicht in Plans.

### CC-4-03 (HIGH) — Event-Queue-Lifecycle hat zwei Einstiegs-Löcher
**Beteiligt:** F-ARCH-15 (neu, HIGH), F-STAB-20 (neu, HIGH), F-ARCH-11 (REGRESSED)
**Pattern:**
1. **Init-Timing:** EventQueue wird in `FinalizeInit()` nach `BuildController()` erzeugt. Zu spät, wenn Factory-Kollaborateur Queue-Referenz im Konstruktor braucht. Außerdem: Harmony-Patches (H2-H6) können zwischen `BotGameComponent`-ctor und `FinalizeInit` feuern und auf null-Queue stoßen.
2. **Save-Load:** `LoadedGame()` baut EventQueue **nicht neu** und **cleared sie nicht**. Load-Path-H2-Postfix-Events bleiben in (möglicherweise alter) Queue mit alten `EnqueueTick`-Werten.
**Fix:** EventQueue im `BotGameComponent`-**Konstruktor** initialisieren (nicht `FinalizeInit`). `LoadedGame()` beginnt mit `eventQueue.Clear()`. Alternative: Load-Baseline-Tick setzen, Events mit niedrigerem Tick silent-drop.

### CC-4-04 (HIGH) — Goal-Phase-Assoziation fehlt vollständig
**Beteiligt:** F-STAB-21 (neu, HIGH), F-STAB-10 (PARTIAL)
**Pattern:** `ReconcilePhaseGoalOrphans()` in §5 ruft `CancelOrphanedDesignations()` + `CancelOrphanedJobs()` — setzt aber voraus dass Blueprints/Designations/Jobs ein Bot-eigenes Phase-Tag tragen. Vanilla-Klassen haben kein solches Feld. Ohne Tag-Schema ist die Methode No-op und F-STAB-10 bleibt PARTIAL.
**Fix:** Neue Struktur in `BotMapComponent`: `Dictionary<int thingIDNumber, PhaseGoalTag> botPlacedThings` (persistiert). Execution-Layer (`BlueprintPlacer.Apply`, `BillManager.Apply`, etc.) trägt Tags beim Placement ein. `CancelOrphaned*` liest daraus, cancelt was `tag.phaseIndex > currentPhaseIndex`.

---

## Liste aller neuen Findings

### HIGHs (5)
| ID | Reviewer | Kern | Fix |
|---|---|---|---|
| F-STAB-20 | Stability | LoadedGame cleared EventQueue nicht; TicksGame-Save-Reset-Race | `eventQueue.Clear()` als erste Aktion in `LoadedGame()` + `StartedNewGame()`. |
| F-STAB-21 | Stability | Goal-Phase-Tag-Schema fehlt → ReconcilePhaseGoalOrphans No-op | `botPlacedThings` Dict in BotMapComponent + Tag-Schreiben in Execution-Layer (siehe CC-4-04). |
| F-ARCH-13 | Architect | Plan-Records mit RimWorld-Runtime-Typen brechen Test-Invariante | Identifikator-only-Rewrite aller Plan-Records (siehe CC-4-02). |
| F-ARCH-15 | Architect | EventQueue-Init nach BuildController (REGRESSION von F-ARCH-11) + controller-Feld-Deklaration fehlt | EventQueue im BotGameComponent-Konstruktor; explizite Feld-Deklarationen `BotController controller; BoundedEventQueue<BotEvent> eventQueue; ConfigResolver configResolver;`. |
| F-AI-15 | Game-AI | Auto-Escape-Timer zählt absolute Zeit, nicht konsekutive Erfüllung (F-AI-08-isomorph) | Separater `auto_escape_stable_counter`, Reset bei Bedingung-Bruch. Escape-Trigger: 2 konsekutive Reevals (= 5000 Ticks). |

### MEDs (9)
| ID | Reviewer | Kern | Fix |
|---|---|---|---|
| F-RW4-02 | RimWorld | `InitializeCompat()`-Aufrufzeitpunkt unspezifiziert, `DefDatabase`-Population-Race | `[StaticConstructorOnStartup]`-Klasse statt `Mod`-ctor. `Compile(TimeSpan)` als C#-Def-Methode klarstellen. |
| F-STAB-22 | Stability | Atomic-Rename-Race + Orphan-tmp-Cleanup | `File.Exists(tmp) → Delete` nach Mutex-Acquire; `finally`-Cleanup bei Exception. |
| F-STAB-23 | Stability | BotErrorBudget Reset-Cycle bei deterministischer Load-Exception | `consecutiveSessionBudgetExhausts` persistiert; nach ≥3: User-Toast. |
| F-STAB-24 | Stability | `Take(50)` bei CompatPatternDef silent-drops | WARN-Log wenn `totalCount > 50`. |
| F-ARCH-12 | Architect | Factory-Signatur `...`-Platzhalter, Assembly-Verantwortung unklar | Builder-Pattern oder vollständige Parameter-Liste in §2.1. `NewTestBuilder(FakeSnapshotProvider)` für Tests. |
| F-ARCH-14 | Architect | `Dictionary<ConfigKey, object>` Typ-Erasure | Typisierte Keys `ConfigKey<T>`, Signatur `Get<T>(ConfigKey<T> key)`. |
| F-ARCH-16 | Architect | RecentDecisionsBuffer Add-API: Caller-Decision fragil | Single `Add(entry)` mit Pin-Regel basierend auf `entry.Kind`. |
| F-AI-16 | Game-AI | `GoalHealthScore` min-Aggregation Priority-Flush-Kaskaden | Launch-Critical-Klassifikation pro Exit-Cond; Aggregation nur über launch-critical Goals. |
| F-AI-17 | Game-AI | `EndingCommitment=Locked` kein Auto-Release bei Phase-Regression | `PhaseStateMachine.TransitionBackward(7→6)` released `endingCommitment = None` + Log-Entry. |

### LOWs (7)
| ID | Reviewer | Kern | Fix |
|---|---|---|---|
| F-RW4-01 | RimWorld | „Scribe-Loader" falsche Terminologie für Defs | Ersetzen durch „DirectXmlLoader" in §4.2. |
| F-RW4-03 | RimWorld | `Find.WorldPawns.AllPawnsAliveOrDead.Concat(...)` Performance bei Pawn-Event-Dispatch | Map-first-short-circuit: Maps zuerst durchsuchen, WorldPawns als Fallback. |
| F-RW4-04 | RimWorld | `MainButtonDef`-Pflichtfelder unspezifiziert | §10.2 Code-Snippet für `MainButtonDefs.xml` ergänzen. |
| F-STAB-25 | Stability | `CompatMode.IsReduced` Persistenz-Verhalten undokumentiert | §4.2-Absatz: „Nicht persistiert — bei jedem Mod-Load neu evaluiert." |
| F-STAB-26 | Stability | `BotControllerFactory.Create()` Fehler-Pfad unspezifiziert | try/catch in `BuildController()`, `LoadedGame()` ruft `BuildController()` retry bei `controller == null`. |
| F-ARCH-17 | Architect | `IReadOnlyList<T>` ist View, nicht Immutabilität | `ImmutableList<T>` etc. aus `System.Collections.Immutable`. |
| F-AI-18 | Game-AI | `stableCounter` Multi-Map-Inkonsistenz | Counter-Quelle: Home-Map. Explizit in §5 dokumentieren. |

### Status-Downgrades (2)
- **F-STAB-10** RESOLVED (R2) → PARTIAL (R3) → PARTIAL (R4, weiterhin) — adressiert durch F-STAB-21-Fix
- **F-ARCH-11** RESOLVED (R3) → REGRESSED (R4) — adressiert durch F-ARCH-15-Fix

---

## Ehrliche Bewertung der Review-Trajektorie

| Runde | HIGHs gefunden | CRITs | Verdict |
|---|---|---|---|
| Round 1 | 9 | 7 | REJECT |
| Round 2 | 2 | 0 | APPROVE-W-MINOR |
| Round 3 | 5 | 0 | APPROVE-W-MINOR |
| Round 4 | 5 (+1 Regression, +4 Status-Downgrades) | 0 | APPROVE-W-MINOR |

**Die Kurve konvergiert nicht gegen Null.** Jede Revision führt neue Oberflächen ein (Plan-Records in v2.0, Event-Queue in v2.1, CompatPatternDef in v2.2), und jede Persona findet auf jeder neuen Oberfläche 2–4 Findings. Das ist eine **strukturelle Eigenschaft** des Review-Prozesses, keine Eigenschaft der Architektur.

Gleichzeitig: die Findings sind **nicht trivial**. F-ARCH-13 (Plan-Schema-Identifikator-Pattern) ist eine substantielle Refactoring-Arbeit. F-STAB-21 (Goal-Phase-Tag) erfordert ein neues Schema in BotMapComponent. F-ARCH-15 + F-STAB-20 sind echte Lifecycle-Bugs.

---

## Drei Optionen für User-Entscheidung

### Option A: v2.3 einarbeiten, dann Approval ohne Round 5
Guardian-Regel 4 einhalten, alle 21 neuen Findings fixen, PARTIAL → RESOLVED promoviert, keine weitere Review-Runde. Realistisch: 4–5 neue Decisions, eine Iteration Edits ähnlich v2.1 → v2.2.
**Pro:** Regel 4 strikt erfüllt. Alle bekannten Findings adressiert.
**Con:** Wir schließen mit dem Wissen ab, dass Round 5 vermutlich wieder 3–5 HIGHs finden würde. Diese verstecken sich nicht, sondern sind real — nur auf noch-nicht-reviewten Detail-Flächen.

### Option B: v2.3 einarbeiten, Round 5 als Full-Panel
Transparent weiter-iterieren. Realistisch: Round 6, Round 7 mit ähnlicher Funde-Dichte.
**Pro:** Maximale Gründlichkeit.
**Con:** Diminishing Returns auf Architektur-Papier vs. Code-Realität. Das Planning-Artefakt wird paradox detaillierter als das noch-nicht-existierende Code-Repo.

### Option C: v2.2 Sign-Off, Findings als Epic-1/Epic-3-Story-Sub-Tasks
**Explizite User-Genehmigung** für controlled Cherry-Pick: v2.2 wird approved, die 21 Round-4-Findings werden nicht als Architecture-Edits, sondern als Sub-Tasks in die entsprechenden Stories getragen (F-ARCH-13 → Story 3.8 BuildPlanner MVP, F-STAB-21 → Story 3.1 Invariant-Framework, F-ARCH-15 → Story 1.2 Harmony-Bootstrap, etc.). Architecture bleibt v2.2, aber Stories-Phase startet mit bekannter Action-Matrix.
**Pro:** Findings gehen nicht verloren, Progress ist möglich. Genau das Pattern das Round-3-Report bereits im Sign-Off-Block erwog („MEDs dürfen als Story-Tasks übergehen, ABER müssen in epics.md den Stories angehängt werden").
**Con:** Erfordert explizite User-Genehmigung (Guardian-Regel 4 + 5 sagen: keine unauthorisierten Cherry-Picks). Verschiebt architektonische Entscheidungen in Dev-Zeit.

### Meine Empfehlung

**Option C** — mit der Modifikation dass die 5 **HIGHs** plus die zwei **Cross-Cutting-Cluster CC-4-02 und CC-4-03** direkt in v2.3 eingearbeitet werden (Plan-Schema-Rewrite + EventQueue-Lifecycle-Fixes + Auto-Escape-Counter + Goal-Phase-Tag-Schema), die 9 MEDs und 7 LOWs aber als Story-Sub-Tasks dokumentiert werden.

Begründung: HIGHs sind Architecture-Level-Probleme (Lifecycle, Schema-Vertrag, Counter-Semantik) — gehören in die Architektur-Datei. MEDs/LOWs sind häufig Dev-Level-Präzisierungen (Builder-Pattern, Immutable-Collections, Def-Loader-Terminologie, Pflichtfelder-Snippets) — sinnvoll beim Story-Drafting mitzutragen, wo der Code-Kontext ohnehin da ist.

Das ist formell ein Cherry-Pick (Guardian-Regel 4 verlangt User-Genehmigung), aber durch explizite Dokumentation als Epic-1/Epic-3-Sub-Tasks mit Line-References und Fix-Spec geht nichts verloren.

---

## Sign-Off-Anforderungen je nach Option

### Für Option A (v2.3 voll)
1. Alle 5 HIGHs + 1 Regression + 9 MEDs + 7 LOWs eingearbeitet
2. F-STAB-10 auf RESOLVED via F-STAB-21-Fix promoviert
3. F-ARCH-11 auf RESOLVED via F-ARCH-15-Fix promoviert
4. Neue Decisions: D-23 (Plan-Schema-Identifier-Pattern), D-24 (Event-Queue-Lifecycle-Regel), D-25 (Goal-Phase-Tag-Schema), D-26 (StableConsecutiveCounter-Pattern)
5. User-Sign-Off danach

### Für Option B (v2.3 + Round 5)
Wie A, plus nochmal Full-Panel-Review auf v2.3.

### Für Option C (empfohlen, HIGHs in v2.3 + MEDs/LOWs als Stories)
1. Alle 5 HIGHs + 1 Regression + F-STAB-10-Fix + F-AI-15-Fix in v2.3
2. Neue Decisions D-23 bis D-26
3. `architecture-review-round-4-2026-04-24.md` bleibt als Referenz
4. MEDs/LOWs werden in `epics.md` pro Epic als Sub-Tasks dokumentiert (bevor Story-Drafting startet)
5. **Explizite User-Genehmigung** dass MEDs/LOWs als Story-Sub-Tasks zählen (nicht als Cherry-Pick-Verstoß)
6. User-Sign-Off auf v2.3 + Genehmigung → Story-Drafting startet
