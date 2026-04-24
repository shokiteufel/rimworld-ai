# Stories (Epic 1-8) — Party-Mode-Review Round 1

**Dokumente:** 85 Story-Dateien in `_bmad-output/implementation-artifacts/stories/` (1-1 bis 8-10)
**Review-Datum:** 2026-04-24
**Review-Modus:** Full-Panel (alle 4 Personas parallel, je 1500-Wörter-Budget, alle 85 Stories gelesen)
**Gesamtverdict:** **APPROVE-WITH-CHANGES** × 4 — substanzielle Revision vor Dev-Start nötig

---

## Executive Summary

Alle vier Reviewer geben **APPROVE-WITH-CHANGES** — eine Stufe stärker als die `APPROVE-WITH-MINOR-CHANGES`-Konvergenz aus den Architecture-Reviews. Stories sind strukturell solide (konsistentes Format, Decision-References, Aufgelöste TBDs), aber **systemische Muster-Lücken** quer über mehrere Epics treiben den Verdict.

### Reviewer-Verdicts
| Reviewer | Verdict | HIGHs | MEDs | LOWs | Cross-Cutting |
|---|---|---|---|---|---|
| RimWorld-Specialist | APPROVE-WITH-CHANGES | 3 | 6 | 8 | 10 (CC-R1-01..10) |
| C#-Architect | APPROVE-WITH-CHANGES | 0 | 6 | 3 | 7 (CC-ARCH-P01..07) |
| Game-AI-Expert | APPROVE-WITH-CHANGES | 6 | 7 | 3 | 7 (CC-AI-P01..07) |
| Stability-Engineer | APPROVE-WITH-CHANGES | 6 | 10 | 8 | 8 (CC-STAB-P01..08) |
| **Summe** | — | **15** | **29** | **22** | **32** (teils überlappend) |

### Kontext-Interpretation
Round-1-Architecture hatte 7 CRIT + 9 HIGH für ein Artefakt. Stories sind 85 Artefakte und haben 15 HIGH + 29 MED + 22 LOW — **dichte-bereinigt deutlich weniger pro Artefakt** als Architecture-v1. Die Cross-Cutting-Findings sind das stärkere Signal: **viele Stories teilen dieselben Lücken** (Schema-Bumps, Exception-Wrapper, DLC-Guards).

---

## Cross-Cutting-Themen (mehrfach unabhängig gefunden)

Die folgenden Themen wurden von **≥ 2 Reviewern** aus unterschiedlichen Perspektiven gefunden. Sie haben Vorrang vor Einzel-Findings.

### CC-STORIES-01 (HIGH) — Schema-Versioning-Registry fehlt
**Gefunden von:** Stability (CC-STAB-P01), RimWorld (CC-R1-10)
**Betroffene Stories:** 2.3 (`excludedCells`), 3.9 (`botManagedBills`), 4.3 (`botManagedGuests` wenn Plan/Apply-Split), 6.5 (`pawnSpecializations`), 2.7 (`overlayVisible`), 7.9 (`journeyQuest`), 2.8 (`MapAnalysisSummary`-Scope), plus BotMapComponent hat in 1.3 überhaupt kein explizites Schema-Version-Feld.
**Kern:** Mehrere Stories fügen persistente Felder zu BotGameComponent/BotMapComponent hinzu. Keine einzige erwähnt Schema-Bump. Save-Load bei Mod-Update → neue Felder sind null/empty → Silent Data Loss.
**Fix konsolidiert:** Neue **Story 1.9 „Schema-Version-Registry"** die (a) BotMapComponent explizit `schemaVersion`-Feld bekommt, (b) zentrale Migration-Pfade dokumentiert (v1→v2→v3→v4), (c) Pflicht-AC-Template für alle Feature-Stories die Schema erweitern („Schema-Bump + Migration-Pfad + TC"). Alle betroffenen Stories kreuz-referenzieren.

### CC-STORIES-02 (HIGH) — Exception-Wrapper-Pattern nur bei Harmony-Patches fixiert
**Gefunden von:** Stability (CC-STAB-P02), RimWorld (implizit in 3.3-3.6-Findings)
**Betroffene Stories:** 1.4 (SetMasterState), 1.5 (GameComponentUpdate), 2.7 (MapComponentOnGUI), 2.9 (Tick-Iterator), 5.4 (DraftController.Apply), 5.7 (Focused-Fire-Apply), alle Execution-Apply-Stories (3.8, 3.9, 3.10, 4.3, 4.7, 6.1, 6.8 …).
**Kern:** D-13 Exception-Skelett ist nur für Harmony-Postfixes mandatiert. Tick-Host-Code + Execution-Apply-Code haben keinen mandatorischen try/catch + BotErrorBudget + FallbackToOff. Architecture §3 Top-Level-Try fängt nur, wenn die Exception bis dahin propagiert — Apply-Methoden in eigenen Call-Stacks (Event-Dispatch, MapComponentOnGUI) sind nicht abgedeckt.
**Fix konsolidiert:** Neue **Story 1.10 „Exception-Wrapper-Pattern für Tick-Host + Execution"** oder Pflicht-AC-Template: „Jede Methode die RimWorld-State mutiert wrapt Hauptkörper in try/catch + BotErrorBudget. Bei 2 Exceptions/min → Pawn-/Map-Poisoned-Set analog Story 3.10."

### CC-STORIES-03 (HIGH) — Epic 7 Naming-Drift + Ending-Sub-Phase-State-Machine fehlt
**Gefunden von:** C#-Architect (CC-ARCH-P01), Game-AI (CC-AI-P05), Stability (CC-STAB-P05-bezogen)
**Betroffene Stories:** 7.5–7.18 (13 Stories mit „Manager"-Naming + fehlender Sub-Phase-Exit-Kontrakt)
**Kern:** Epic 7 verwendet `Manager`-Naming für State-haltende Decision-Orchestrators. D-15 reserviert `Manager`/`Writer` für Execution-Schicht. Tiefere Problematik: Ship-Ending hat 4 Sub-Phasen (Research→Bauplatz→Core→Finale), Royal 2 (HonorFarm→Stellarch), Journey 3, Archonexus 2, Void 3. Ohne `EndingSubPhaseStateMachine` ist unklar wie Sub-Phase-Transitions + Rollback bei Game-Breaking-Events laufen. TransitionBackward (F-AI-17) ist nur für Phase 7→6 spezifiziert, nicht für Sub-Phase-Rollback.
**Fix konsolidiert:** Neue **Story 7.0 „EndingSubPhaseStateMachine"**: pro Ending Sub-Phase-Sequenz + Exit-Conditions + Rollback-Semantik + D-26-StableCounter pro Sub-Phase. Alle 13 „Manager"-Stories refaktorieren in `...PhaseRunner` oder `...Planner` (abhängig von State-Ownership). Architecture §2.3c neu für „Phase-Runner"-Kategorie.

### CC-STORIES-04 (HIGH) — Multiple Plan-Producer ohne Arbitrage-Regel
**Gefunden von:** Game-AI (CC-AI-P04), C#-Architect (CC-ARCH-P02 für DraftOrder), Stability (CC-STAB-P03 für Pawn-Mutation)
**Betroffene Stories:** Für DraftOrder: 5.3, 5.5, 5.7, 5.8, 7.8, 7.13. Für WorkPriorityPlan: 3.10, 3.5 (E-Food), 4.9 (E-Mood), 4.10. Für BuildPlan: 3.8, 4.7, 6.1, 6.3, 7.6. Für BillPlan: 3.9, 6.6, 6.8.
**Kern:** Mehrere Stories produzieren den gleichen Plan-Typ. Ohne Arbitrage-Regel zwischen konkurrierenden Produzenten entsteht Last-Write-Wins auf Pawn-State. AI-7 (Emergency > Override > PhaseGoal) ist etabliert aber nicht konsequent in Stories.
**Fix konsolidiert:** Neue **Story 1.11 „Plan-Arbiter"** (oder 3.0c): zentraler Arbiter pro Plan-Typ der AI-7-Layer-Präzedenz + Plan-Merge implementiert. Alle Planner-Stories kreuzen Arbiter-Referenz. Alternativ: Scheme-Change in Plan-Records um Producer-Layer explicit zu markieren, Apply-Seite sortiert nach Layer.

### CC-STORIES-05 (HIGH) — DLC-Guards inkonsistent in Epic 7
**Gefunden von:** RimWorld (CC-R1-02), Stability (CC-STAB-P05)
**Betroffene Stories:** **7.14 (Archonexus — Ideology-DLC-Guard fehlt KOMPLETT)**, 7.13, 7.17, 7.18 (Guards nur in Dev-Notes, nicht in AC), 7.5–7.8 (Ship — brauchen Royalty-Absence-Check, nicht DLC-Presence).
**Kern:** Archonexus-Ending ist Ideology-gated, Story 7.14 hat aber keinen Guard. Anomaly-Endings (7.17, 7.18) haben Guards nur in Dev-Notes. Ship-Ending ist Vanilla aber Royalty entfernt Ship — kein Check. F-DLC-01 wird dadurch systematisch verletzt.
**Fix konsolidiert:** Alle Epic-7-Stories AC 1 standardisieren: „`DlcCapabilities.EndingAvailable(X)` Guard zu Beginn jeder Plan-Methode; bei false → Plan returns Empty + Feasibility-Score undefined." 7.1 `EndingFeasibility` dokumentiert Matrix: Ship=Vanilla+no-Royalty-Remove, Journey=Vanilla+Royalty, Royal=Royalty, Archonexus=Ideology, Void=Anomaly.

### CC-STORIES-06 (HIGH) — Pawn-Exclusivity-Lock-Framework fehlt in Story 3.1
**Gefunden von:** Game-AI (Individual-Findings 3.1, 3.4, 4.9), Stability (CC-STAB-P02 indirekt)
**Betroffene Stories:** 3.1 (Framework-Spec-Lücke), 3.4 (deklariert Lock ohne Framework-Spec), 4.9 (7 Handler ohne Lock-Detail), plus 3.3, 3.5, 3.6.
**Kern:** Story 3.4 E-Bleed deklariert „Doctor 60s locked", aber im Framework-Story 3.1 gibt es kein Lock-Feld im EmergencyResolver. Second Emergency kann in parallelem Eval-Zyklus denselben Pawn re-claimen. Story 3.10 E-HEALTH + 3.4 E-BLEED + 4.9 E-RAID konkurrieren alle um denselben Doctor-Pawn.
**Fix konsolidiert:** Story 3.1 AC 6 erweitern: „`EmergencyResolver.pawnClaims: Dictionary<string UniqueLoadID, (string EmergencyId, int UnlockTick, int LockPriority)>`. `Claim` registriert + setzt Unlock-Tick. `LockPriority`-Matrix: E-RAID=100, E-BLEED=90, E-HEALTH=80, E-MOOD=20. Re-Claim während Lock-Period nur bei höherer LockPriority erlaubt. Alle Emergency-Handler-Stories referenzieren Framework-Lock-Kontrakt."

### CC-STORIES-07 (HIGH) — Stuck-State-Handling nicht systematisch
**Gefunden von:** Game-AI (CC-AI-P01), RimWorld (CC-R1-04)
**Betroffene Stories:** 3.3–3.6, 4.9, 7.7, 7.8, 7.13, 7.18. Story 3.7 hat „zahnlosen" Hook.
**Kern:** F-AI-04 fordert Fallback-Pfade bei unreachable targets, colony_extinct, leere top_3_sites. Emergency-Handler-Stories haben alle keine Staleness-Detection — wenn Handler 3× in 60s ohne Apply-Progress läuft, bleibt er Utility-Winner → Dead-Lock.
**Fix konsolidiert:** Story 3.1 erweitern um **Handler-Staleness-Pattern**: pro `EmergencyHandler` ein `stalenessCounter: int`. Resolver inkrementiert wenn `Apply` returned ohne Colony-State-Diff. Bei `stalenessCounter >= 3` → Handler `cooldown_until_tick = now + 5000`, Resolver überspringt. Stories 3.3–3.6, 4.9, 7.x referenzieren das.

### CC-STORIES-08 (MED) — API-Reality-Check für RimWorld-JobDefOf/ThingDefOf
**Gefunden von:** RimWorld (CC-R1-01)
**Betroffene Stories:** 3.3, 3.4, 3.5, 3.6, 4.3, 4.9, 5.4, 5.5, 5.7 (Job-Namen) + 3.6, 6.1 (Thing-Namen).
**Kern:** Stories beschreiben Vanilla-Jobs/Things mit generischen Namen (`BeatFire`, `Rescue`, `Tend`, `Attack`, `Hunt`, `LayDown`, `Heater`, `Cooler`, `Wall`). Ohne verifizierten `JobDefOf`-Identifier (z. B. `JobDefOf.BeatFire` korrekt, aber `Tend` ist `TendPatient`) gibt's Compile-Fehler zur Dev-Zeit.
**Fix konsolidiert:** Neue Datei `_bmad-output/implementation-artifacts/api-reference.md` (kein Story-Scope, aber Pflicht-Artefakt vor Dev): liste alle referenzierten Vanilla-Defs mit exakten defNames aus `Data/Core/Defs/JobDefs/*.xml`. Stories linken auf die Reference-Datei.

### CC-STORIES-09 (MED) — Quest-API-Hook-Pattern falsch (7.9 nutzt Dialog-Pattern)
**Gefunden von:** RimWorld (CC-R1-03)
**Betroffene Stories:** 7.9, 7.11, 7.14, 7.15, 7.7 (AIPersonaCore-Quest)
**Kern:** RimWorld 1.3+ nutzt `Find.QuestManager.QuestsListForReading` als primären Quest-Flow. Story 7.9 hookt via H6 WindowStack.Add (alter Dialog_NodeTree-Pattern) — das funktioniert für Event-Dialoge aber verpasst moderne Quest-Offers.
**Fix konsolidiert:** Neue **Story 1.12 oder 5.9 „QuestManager-Polling"**: GameComponentTick alle 2500 Ticks iteriert `Find.QuestManager.QuestsListForReading`, enqueued `QuestOfferEvent` in EventQueue. H6 bleibt für Finale-Dialog-Events (z. B. Ship-Start). 7.9/7.11/7.14/7.15/7.7 referenzieren neues Pattern.

### CC-STORIES-10 (MED) — Read-After-Write-Check nur bei WorkPriorityWriter
**Gefunden von:** Stability (CC-STAB-P03), Game-AI (implizit)
**Betroffene Stories:** 3.10 (hat), 4.3 (Recruiting), 5.4 (Draft), 5.7 (Focused-Fire), 6.8 (Outfit).
**Kern:** F-STAB-04 fordert Read-After-Write-Check bei Pawn-State-Mutation wegen Mod-Konflikten (Combat Extended, Simple Sidearms, CAI 5000 patchen Drafter). Nur Story 3.10 hat es explizit.
**Fix konsolidiert:** Pflicht-AC-Template für alle Execution-Stories die `pawn.drafter` / `pawn.guest` / `pawn.outfitTracker` mutieren: „Nach Mutation Read-Back. Bei Mismatch: WARN-Log, Retry 1× nach 60 Ticks, dann Pawn-Poisoned-Set analog 3.10."

### CC-STORIES-11 (MED) — Transient-vs-Persistent-State-Klassifikation fehlt
**Gefunden von:** Stability (CC-STAB-P04)
**Betroffene Stories:** 3.10 (Poisoned-Set), 5.4 (Retreat-Target), 5.5 (Flee-Destination), 5.1 (ThreatReport), plus 7.2 hat's korrekt markiert.
**Kern:** Stories führen Runtime-State-Felder ein ohne Pflicht-AC „persistent?/transient?". Save-mitten-in-Raid ist unklar.
**Fix konsolidiert:** Pflicht-AC in jeder Story die Runtime-State-Feld einführt: „Feld X ist [transient/persistent]. Bei transient: wird in LoadedGame/StartedNewGame re-initialisiert."

### CC-STORIES-12 (MED) — Phase-Transition-Guard nicht explizit in Phase-Stories
**Gefunden von:** Game-AI (CC-AI-P03)
**Betroffene Stories:** 3.7, 3.11, 4.2, 4.5, 4.6, 6.2, 6.4.
**Kern:** F-AI-01 „Emergency aktiv blockt Phase-Transition" — keine Phase-Story definiert explizit wo der Guard greift. Phase-Story-AC `stableCounter >= 2` ist Consecutive-Counter, aber: „Emergency aktiv, Counter hoch wegen Goals-done" → Transition würde ausgelöst.
**Fix konsolidiert:** Neue AC in jeder Phase-Goal-Story: „Exit-Condition-Check: alle Goals done UND `stableCounter >= 2` UND `EmergencyResolver.ActiveEmergencies.Count == 0` (F-AI-01)."

### CC-STORIES-13 (MED) — Test-Infrastructure-Story fehlt
**Gefunden von:** C#-Architect (CC-ARCH-P03)
**Betroffene Stories:** 3.1, 3.7, 3.11, 4.2, 4.5, 4.6, 5.1, 5.3, 6.2, 6.4, 7.1, 7.16.
**Kern:** Architecture §9.1 nennt `FakeSnapshotProvider` + `TestSnapshotBuilder` als Pflicht-Helper, aber keine Story liefert diese Test-Infrastruktur. Jeder Dev würde sein eigenes Mock bauen.
**Fix konsolidiert:** Story 1.3 AC erweitern oder neue **Story 1.13 „Test-Infrastructure"** die `FakeSnapshotProvider` + `TestSnapshotBuilder` + Architecture-§9.1-konforme Helpers liefert.

### CC-STORIES-14 (LOW) — DraftOrder-Schema-Lücken für Combat-Subtypen
**Gefunden von:** C#-Architect (CC-ARCH-P02)
**Betroffene Stories:** 5.4, 5.7, 5.8, 7.8, 7.13.
**Kern:** DraftOrder in §2.3a hat nur `Draft/Undraft/RetreatPoint`. Combat-Features brauchen Focused-Fire-Target, Siege-Turret-Crews, Ship-Defense-Positions. Stories mutieren `pawn.jobs.StartJob(Attack)` direkt in Apply — umgeht das Record-Schema.
**Fix:** DraftOrder in Architecture §2.3a erweitern um optionale Felder: `ImmutableDictionary<string, string? FocusedFireTargetUniqueLoadID>` und `ImmutableDictionary<string, (int x, int z)? AssignedPosition>`.

### CC-STORIES-15 (LOW) — Naming-Konvention Planner-vs-Manager in Epic 7 mixed
Siehe CC-STORIES-03, wird zusammen gelöst.

### CC-STORIES-16 (LOW) — ConfigKey<T>-Typ-Sicherheit nur in Story 2.5
**Gefunden von:** C#-Architect (CC-ARCH-P07)
**Fix:** Stories die Config lesen explizit typisieren: `cfg.Get<T>(ConfigKeys.X)`.

---

## Story-spezifische HIGH-Findings (Nicht-Cross-Cutting)

| Story | Reviewer | Kern | Fix |
|---|---|---|---|
| 1.3 | Stability | Migrate v1→v2→v3 Details fehlen, `thingIDNumber → UniqueLoadID`-Migration undokumentiert | AC 14 erweitern: Best-Effort Pawn-Lookup, User-Toast bei Data-Loss |
| 1.5 | Stability | GameComponentUpdate-KeyDownEvent ohne Exception-Wrapper | AC: KeyDownEvent-Handler in try/catch + BotErrorBudget |
| 2.3 | Stability | `excludedCells`-Feld ohne Schema-Bump + HashSet-Scribe-Format | Schema-Bump, `HashSet<(int,int)>` D-23-konform |
| 3.1 | Game-AI | Pawn-Exclusivity-Lock-Kontrakt fehlt im Framework | Siehe CC-STORIES-06 |
| 3.4 | RimWorld+Game-AI | `JobDefOf.TendPatient` (nicht `Tend`); Score-Penalty -50 zu weich bei E-INTRUSION | Eligibility=false statt Penalty; Job-Def präzise |
| 3.7 | Game-AI | Stuck-State-Hook zahnlos (nur Log) | Per-Goal-Staleness-Tracking + Substitute-Goal |
| 4.3 | Stability | AI-2-Verletzung: Planner mutiert `pawn.guest.interactionMode` | Plan/Apply-Split analog BillPlanner |
| 4.7 | Stability | D-25-Tag-Write nicht in AC | AC ergänzen: Tag-Write in Apply |
| 4.9 | RimWorld+Game-AI | 7 Handler in einer Story, Lock-Matrix fehlt | Split in 4-9a..g; Lock-Priority-Matrix |
| 5.3 | Game-AI | Combat-Lock vs. Emergency-Lock unkoordiniert | CombatCommander als Sub-Planner für E-RAID |
| 5.4 | Stability | Kein Read-After-Write-Check auf Drafter-Mutation | AC: Read-Back + Retry + Poisoned-Set |
| 5.8 | Game-AI | RetreatPlanner racet mit CombatCommander | RetreatPlanner als Sub-Phase von CombatCommander |
| 7.9 | RimWorld+Stability | Dialog_NodeTree-Pattern falsch; Quest-ID statt Quest-Ref persistieren | QuestManager-Polling (siehe CC-STORIES-09) + `int questId` |
| 7.14 | RimWorld | Ideology-DLC-Guard KOMPLETT fehlt | Siehe CC-STORIES-05 |
| 8.1 | Stability | XXE/ReDoS-Schutz fehlt im AC | XmlReaderSettings explizit: DtdProcessing=Prohibit + Timeout |

---

## Fehlende / neu einzuführende Stories

| Neue Story | Grund | Größe |
|---|---|---|
| **1.9 Schema-Version-Registry** | CC-STORIES-01 | S |
| **1.10 Exception-Wrapper-Pattern** (Tick-Host + Execution) | CC-STORIES-02 | S |
| **1.11 Plan-Arbiter** (AI-7-Arbitrage) | CC-STORIES-04 | M |
| **1.12 QuestManager-Polling-Hook** | CC-STORIES-09 | S |
| **1.13 Test-Infrastructure** (FakeSnapshotProvider + Builder) | CC-STORIES-13 | S |
| **3.0a Pawn-Exclusivity-Lock-Framework-Erweiterung** (oder in 3.1 eingearbeitet) | CC-STORIES-06 | S (oder Erweiterung 3.1) |
| **3.0b Handler-Staleness-Pattern** | CC-STORIES-07 | S |
| **7.0 EndingSubPhaseStateMachine** | CC-STORIES-03 | M |
| **4-9 Split** in 4-9a..4-9g (7 einzelne Handler-Stories) | Naming/Granularität | gleiche Gesamt-Size |

**Zusätzliche Anzahl:** ~8 neue Stories + 1 Split. Neuer Story-Count: 85 + 8 - 1 = **92 Stories** (plus 4-9 split in 7 Sub-Stories = 85 - 1 + 7 = 91, plus 8 neue = 99 Stories, aber einige verdichten sich innerhalb existierender).

Realistisch: ~8 neue Stories + inline Erweiterungen vieler bestehender.

---

## Konsolidierte Action-Matrix für Story-Revision

| Priorität | # Findings | Typ |
|---|---|---|
| **Neue Cross-Cutting-Stories** | 5 HIGH + 2 MED (CC-01 bis CC-13) | Story-Creation |
| **Story-AC-Erweiterungen** (HIGH) | ~15 Stories | Edit bestehend |
| **Story-AC-Erweiterungen** (MED) | ~29 Stories | Edit bestehend |
| **Story-AC-Erweiterungen** (LOW) | ~22 Stories | Edit bestehend |
| **Naming-Refaktorierung Epic 7** | 13 Stories | Rename + AC-Update |
| **api-reference.md Appendix** | Neu | Artefakt-Datei (kein Story-Scope) |
| **DraftOrder-Schema-Erweiterung** | Architecture §2.3a | Edit Architecture |

---

## Sign-Off-Anforderungen für Story-Revision-Pass 2

1. Alle 7 neuen Cross-Cutting-Stories geschrieben (1.9, 1.10, 1.11, 1.12, 1.13, 3.0b, 7.0)
2. Story 4.9 in 4-9a..4-9g gesplittet
3. Alle 15 HIGH-Findings in Einzelstories adressiert
4. Epic 7 Naming einheitlich (Planner vs. PhaseRunner)
5. Alle Epic-7-Stories haben DLC-Guard-AC (CC-STORIES-05)
6. Schema-Bump-AC-Template in betroffenen Stories
7. Read-After-Write-AC-Template in Execution-Stories
8. `api-reference.md` angelegt mit verifizierten Vanilla-Def-Namen
9. Re-Review (mindestens 2 Personas: RimWorld + Stability) nach Revision

---

## Empfehlung

**Diese Round-1-Review ist substantiell.** Direkte Revision aller 85 Stories + 7-8 neue Stories ist ein massiver Batch (~100 Story-Dateien insgesamt). Das kann 2-3 Arbeitssessions brauchen.

**Realistische nächste Schritte (User-Entscheidung):**

- **Option A: Voll-Revision** aller Findings in einer Pass 2, dann Re-Review (Round 2) über alle Stories. Guardian-Regel-4-konform.
- **Option B: Nur HIGHs + Cross-Cutting fixen** (die 5 HIGH-CCs + 15 Einzel-HIGHs + neue Stories). MEDs/LOWs gehen als Sub-Tasks in die bestehenden Stories (explizite User-Genehmigung nach Regel 4 nötig).
- **Option C: Epic-1-Only-First** — nur Epic-1-Stories revidieren, Rest bleibt Round-2-Gate. Dev von Epic 1 startet mit revidierten 8 Stories; Epic 2-8 werden pro Epic revidiert bevor jeweiliger Dev-Start. Das ist effektiv „Per-Epic-Revision" statt „All-Upfront-Revision" — die Inverse von D-30.

**Meine Empfehlung: Option B mit expliziter User-Genehmigung.** HIGHs + Cross-Cutting-Cluster sind die strukturellen Defekte — MED/LOW sind Präzisierungen die beim Story-Dev-Drafting (wo der Dev die Story liest) ohnehin aufgefangen werden können. Voll-Revision aller 85 Stories + Re-Review ist Planning-Overengineering-Risk (siehe Architecture-Round-4-Trajektorien-Analyse).

**Aber:** Nach Guardian-Regel 5 „BMAD ist absolut, keine Abkürzungen" darf ich Option B nicht selbst wählen — User muss explizit genehmigen.
