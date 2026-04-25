# Decision Log — RimWorld Bot Mod

Chronologisches Log aller Design-Entscheidungen. Neue Entscheidungen oben einfügen.

Format pro Eintrag:
```
## D-XX: Titel
Datum: YYYY-MM-DD
Status: accepted | superseded | rejected
Kontext: ...
Entscheidung: ...
Begründung: ...
Konsequenzen: ...
```

---

## D-36: Story 1.12 AC-Anpassungen (Test-Defer + Producer-Scope) und Schema-Slot-Reservierung
Datum: 2026-04-24
Status: accepted
Kontext: Code-Review Story 1.12 (QuestManager-Polling) hat drei AC-Drift-Findings (MED-1, MED-2, MED-3) gemeldet:
- File-List der Story spezifizierte separate Dateien `QuestOfferEvent.cs` und `QuestRemovedEvent.cs`, Implementierung hat Records aber konsolidiert in `BotEvent.cs` (konsistent mit allen anderen Event-Records).
- AC-6 fordert Unit-Tests, aber das Projekt hat noch keine Test-Infrastruktur — die wird in Story 1.13 (Test-Infrastructure) etabliert.
- AC-5 fordert Consumer-Integration (7.7/7.9), die strukturell nicht in 1.12-Scope gehört (separate Epic-7-Stories).
Zusätzlich nahm 1.12 den Schema-Slot v3→v4 für `lastSeenQuestIds` ein, der zuvor in `SchemaRegistry` als "Planned" für Story 4.3 (botManagedGuests) reserviert war.
Entscheidung:
1. **Story 1.12 File-List** updated auf konsolidierte Variante (Records in `BotEvent.cs`). Pattern-Konsistenz mit `MapFinalizedEvent`/`RaidEvent`/etc. ist wichtiger als die ursprüngliche separate-File-Spec.
2. **AC-6 (Unit-Tests)** explizit deferred zu Story 1.13. Story 1.13 trägt damit Carry-Over aus 1.9 (Schema-Tests), 1.10 (BotSafe-Tests) und 1.12 (Quest-Poller-Tests).
3. **AC-5 (Consumer)** umformuliert auf Producer-Scope: 1.12 stellt nur die Event-Records + Poller bereit; Consumer-Integration wandert vollständig in 7.7 + 7.9.
4. **SchemaRegistry-Slot-Verschiebung**: BotGame v3→v4 = Story 1.12 (Applied); botManagedGuests (4.3) auf v4→v5 verschoben; pawnSpecializations (6.5) auf v5→v6; journeyQuest (7.9) auf v6→v7.
5. **CR HIGH-1 Fix**: PollIntervalTicks 2500 → 1250 (halbe StalenessThreshold). Verhindert dass Critical-QuestOfferEvents zwischen Enqueue und Dequeue stale werden.
6. **CR HIGH-2 Fix**: Defensive `??=` für `lastSeenQuestIds` in GameComponentTick (vor Poll-Aufruf), Null-Check für `eventQueue` — Schutz gegen Reflection-Construction-Pfad ohne Ctor-Init.
Begründung: Strict BMAD heißt: Story-File ist Spec, aber Spec-Drift wird erkannt + dokumentiert + gefixt — nicht stillschweigend implementiert. Schema-Slot-Reorganisation ist deterministisch (Story-Order-Push), keine Konflikte mit anderen geplanten Stories. Test-Defer ist legitim weil 1.13 explizit als Test-Infrastructure-Story angelegt wurde.
Konsequenzen:
- Story 1.13 erweitert sein Carry-Over-Set um QuestManagerPoller-Tests.
- Stories 4.3, 6.5, 7.9 müssen ihre Schema-Bump-References auf die neuen Versionen updaten wenn implementiert (TODO in jeweiligen Story-Files).
- Quest-Detection-Latenz halbiert von ~42s auf ~21s — geringe CPU-Cost (1 HashSet-Diff alle 21s), Nutzen: Critical-Events zuverlässig dequeue-bar.

---

## D-35: Phase-Transition PLANNING → DEV, Sprint 2 mit Story 1.1 gestartet
Datum: 2026-04-24
Status: accepted
Kontext: Nach D-34 (Round-3-APPROVE) hat User am 2026-04-24 „Ja, kann starten" freigegeben. Commit `9c14b25 feat(planning): complete 3 review passes` pushed zu `https://github.com/shokiteufel/rimworld-ai.git`. Planning-Snapshot ist öffentlich archiviert; Dev-Loop kann starten.
Entscheidung:
1. **Phase-Transition** in `sprint-status.yaml`: `phase: PLANNING` → `phase: DEV`, `sub_phase: STORY_DRAFTING` → `sub_phase: EPIC_1_DEV`.
2. **Sprint 2 gestartet** (2026-04-24): Goal „Epic 1 (Mod-Skeleton & Toggle) — 13 Stories 1.1-1.13 durchentwickeln mit CR+VR-Loops". Sprint-1-Commit (`9c14b25`) archiviert als `previous_sprint`.
3. **Epic-1-Status** auf `in-progress` (≥1 Story in-progress-Regel).
4. **Story 1.1 `mod-project-skeleton`** auf `in-progress` gesetzt — Dev-Loop beginnt.
5. **Dev-Workflow** pro Story: dev-Implementation → code-review → fix → re-review → (bei UI-Story) visual-review → fix → re-review → done. Kein Status-Sprung ready-for-dev→done (Guardian Rule 3).
6. **Sprint-Deadline** weiterhin offen (kein Release-Datum definiert; hängt von Dev-Velocity ab).
Begründung: Strict BMAD-Workflow verlangt Dev-Start an Epic-1-Story-1 nach Review-APPROVE. User-Freigabe eingeholt (Commit+Push = legitimes User-Territory per CLAUDE.md).
Konsequenzen:
- Guardian wechselt intern die Watchdog-Konfiguration (PLANNING-Watchdogs wie epic-plan/sprint-plan bleiben always-on; DEV-spezifische Watchdogs design-critic + progress-check werden bei nächstem `/guardian`-Call aktiviert).
- `/guardian` sollte neu getriggert werden damit Phase-Watchdog-Set für DEV aufgebaut wird.
- Story 1.1 Review-Gate erwartet: Code-Review gegen AI-1, F-ARCH-01 (About.xml packageId); kein Visual-Review (Skeleton-Story ohne UI).

---

## D-34: Re-Review Round 3 APPROVE — Dev-Gate Epic 1 offen
Datum: 2026-04-24
Status: accepted
Kontext: Pass 3 (D-33) hat alle Round-2-Findings adressiert. Round 3 Re-Review (RimWorld + Stability) liefert zweifach **APPROVE**. Reports: `stories-re-review-round-3-rimworld-2026-04-24.md` + `stories-re-review-round-3-stability-2026-04-24.md`.
Entscheidung: Beide Personas bestätigen alle 6 Stability-Round-2-Findings + alle 5 RimWorld-Round-2-Findings RESOLVED. Nur 2 redaktionelle LOWs (7.16 AC-Nummer-Dupe, 1.3 AC 17 Test-Wording-Drift) — per Guardian Regel 4 auch gefixt:
1. **Epic-7 AC-Nummerierung harmonisiert** (7.6, 7.7, 7.8, 7.13, 7.16, 7.17, 7.18): Pass-2-DLC-Guard-Insert hatte „2." doppelt belassen — alle sechs Stories jetzt durchnummeriert.
2. **Story 1.3 AC 17 Test-Fälle aktualisiert**: drei Szenarien (TC-10a/b/c) gegen die neue Kombi-Threshold-Formel aus AC 15 statt veraltetem 25%-Wording.
Begründung: Planning-Phase für RimWorld Bot Mod ist **formell abgeschlossen**. 99 Stories ready-for-dev, Architecture v2.3 approved, PRD approved, epics.md approved, api-reference.md mit verifizierten Vanilla-Defs, 3 Review-Pässe durchlaufen mit APPROVE in Round 3. Sign-Off-Anforderungen aus D-31 erfüllt.
Konsequenzen:
- **Phase-Transition** `PLANNING` → `DEV`-Ready (Sub-Phase `STORY_DRAFTING` → Dev-Gate geöffnet).
- **Sprint 2** kann mit Story 1.1 (Mod-Projekt-Skeleton) gestartet werden.
- Sprint-Goal Sprint-2: Epic 1 komplett durchentwickelt (13 Stories mit Dev-Review-Loops).
- `sprint-status.yaml` muss bei Dev-Start auf `phase: DEV` + `sub_phase: EPIC_1_DEV` updated werden.
- Commit+Push der kompletten Planning-Artefakte wartet auf User-Freigabe (CLAUDE.md-Regel).

---

## D-33: Story-Revision Pass 3 nach Re-Review Round 2
Datum: 2026-04-24
Status: accepted
Kontext: Re-Review Round 2 (min. RimWorld + Stability Personas) gegen Pass-2-Ergebnis lieferte zweifach **APPROVE-WITH-CHANGES** mit 4 neuen HIGHs + 7 neuen MEDs + LOWs. Reports: `stories-re-review-round-2-rimworld-2026-04-24.md` + `stories-re-review-round-2-stability-2026-04-24.md`. Zusätzlich Adversarial-Watchdog-Finding: DraftOrder-Record-Backwards-Compat-Bruch + api-reference.md Odyssey-packageId-Spekulation. Per Guardian Regel 4 (ALLE Findings werden gefixt) wird Pass 3 ausgeführt.
Entscheidung: Pass 3 Korrekturen:
1. **api-reference.md Faktenkorrekturen** (HIGH-Fix Round-2 RimWorld):
   - IncidentDef-Sektion entfernt, durch QuestScriptDef-Sektion ersetzt: `EndGame_ShipEscape`, `EndGame_ArchonexusVictory`, `EndGame_VoidAwakening` (defNames verifiziert gegen `Core/Defs/QuestScriptDefs/Script_EndGame_*.xml`).
   - JobDef-Source-File-Spalten korrigiert: nur 6 tatsächliche Dateien (`Jobs_Animal/Caravan/Gatherings/Joy/Misc/Work.xml`); falsche Split-Namen wie `Jobs_Firefight.xml` entfernt.
   - ThingDefOf-Einträge mit expliziter **TBV**-Markierung + `ThingDef.Named(defName)`-Fallback (RimWorld-Version-abhängige Membership).
   - `Campfire` Source-Correction: `Buildings_Temperature.xml` (nicht `Buildings_Production.xml`).
   - Odyssey-packageId `Ludeon.RimWorld.Odyssey` verifiziert gegen `Data/Odyssey/About/About.xml` — TBV entfernt.
2. **DLC-Early-Return-Guards** in 7.12 (Royal) + 7.16 (Void): Plan() beginnt mit `DlcCapabilities.EndingAvailable(...)`-Check (HIGH-Fix Round-2 RimWorld CC-STORIES-05).
3. **Exception-Wrapper-AC** in 9 Execution-Stories via Story 1.10 (HIGH-Fix Round-2 Stability NEW-STAB-01): 2.7 (MapComponentOnGUI), 2.9 (MapComponentTick-Iterator), 3.8 (BlueprintPlacer), 3.9 (BillManager), 3.10 (WorkPriorityWriter), 4.3 (RecruitingWriter), 4.7 (via 3.8 BuildWriter-Inherit), 5.4 (DraftController), 5.7 (via 5.4 Inherit), 6.8 (OutfitWriter).
4. **Schema-Bump-AC** in 4 Stories via Story 1.9 (HIGH-Fix Round-2 Stability NEW-STAB-02): 2.7 (`overlayVisible`), 3.9 (`botManagedBills`), 6.5 (`pawnSpecializations`), 7.9 (`journeyQuest: JourneyQuestRef?`).
5. **MED-Fixes Round-2:**
   - 3.8 + 3.9 Read-After-Write für BlueprintPlacer + BillManager (NEW-STAB-03) inline in AC erweitert.
   - 3.7 Phase-Guard auf CC-STORIES-12-Standard-Formulierung harmonisiert (NEW-STAB-04).
   - 1.3 AC 15 Data-Loss-Toast-Threshold: Kombi-Formel `dropped >= 1 AND (>10% OR >=2)` statt starrer 25% (NEW-STAB-05).
   - 3.1 Transient/Persistent-Sektion erweitert um `pawnClaims` + `unlockTimers` + `stalenessCounter` expliziter (NEW-STAB-06 war Scan-Miss, Klassifikation existierte bereits).
6. **Adversarial-Fixes:**
   - Architecture §2.3a DraftOrder: `FocusedFireTargets` + `AssignedPositions` von positional-args auf `init`-Properties mit `ImmutableDictionary.Empty`-Defaults umgestellt — existierende 3-arg-Konstruktor-Calls bleiben valid.
   - api-reference.md Offene-TBVs-Sektion erweitert um ThingDefOf-Membership-Check + Royal-EndGame-QuestScriptDef-Existenz.
Begründung: Option A strict (D-31) + Guardian Regel 4: keine Cherry-Picks. Pass 3 ist schnell weil alle Fixes AC-Template-Appends oder api-reference.md-Edits sind; kein struktureller Umbau nötig.
Konsequenzen:
- Kein Story-Count-Wechsel (99 bleibt).
- Re-Review Round 3 kann direkt starten (nur Stability-Persona reicht, da RimWorld-Konzerne alle resolved; oder beide für Completeness).
- Nach Approve in Round 3: Dev-Gate für Epic 1 geöffnet (Story 1.1).

---

## D-32: Story-Revision Pass 2 Turn 2 abgeschlossen
Datum: 2026-04-24
Status: accepted
Kontext: D-31 Pass-2-Revision-Plan wurde in 2 Turns durchgeführt. Turn 1 (vor-Compact): 7 neue Cross-Cutting-Stories + Story 3.1 Pawn-Lock-Framework + Story 4.9-Split in 4-9a..g. Turn 2 (nach-Compact): Epic-7-Naming-Refactor + alle HIGH/MED/LOW-Fixes + Architecture §2.3a DraftOrder-Erweiterung + api-reference.md.
Entscheidung: Pass 2 ist inhaltlich abgeschlossen. Dokumentierte Änderungen:
1. **epics.md** Stories-Listen erweitert um alle 14 neuen Stories (Epic 1: +5, Epic 3: +1, Epic 4: +7 Split, Epic 7: +1). Story-Count jetzt 99.
2. **Epic-7-Naming-Refactor** (13 Stories 7.5-7.18): 6 Manager→PhaseRunner (7.8, 7.11, 7.13, 7.15, 7.16, 7.18), 4 Decision-Consistency-Renames (7.7, 7.9, 7.12, 7.14). Alle 14 Stories 7.5-7.18 haben jetzt Sub-Phase-Referenz zu Story 7.0 EndingSubPhaseStateMachine.
3. **13 HIGH-Fixes** in einzelnen Stories: 1.3 (Migrate-Details), 1.5 (Exception-Wrapper), 2.3 (Schema-Bump + HashSet D-23), 3.4 (JobDefOf.TendPatient + Eligibility=false statt Penalty), 3.7 (Per-Goal-Staleness + Substitute-Goal), 4.3 (Plan/Apply-Split), 4.7 (D-25-Tag-Write-AC), 5.3 (CombatCommander als Sub-Planner für E-RAID), 5.4 (Read-After-Write Drafter), 5.8 (RetreatPlanner als Sub-Tree), 7.9 (QuestManager-Polling + int questId), 7.14 (Ideology-DLC-Guard), 8.1 (XXE/ReDoS-Schutz).
4. **MED-Fixes** via Cross-Cutting-AC-Templates: CC-10 Read-After-Write in 5.7 + 6.8; CC-11 Transient/Persistent in 3.10, 5.1, 5.5; CC-12 Phase-Transition-Guard (`EmergencyResolver.ActiveEmergencies.Count == 0`) in 7 Phase-Stories (3.7, 3.11, 4.2, 4.5, 4.6, 6.2, 6.4).
5. **DLC-Guards** (CC-05) in Epic-7-Stories 7.5, 7.6, 7.7, 7.8 (Ship=Vanilla+no-Royalty-Remove), 7.13 (Royal=Royalty), 7.17, 7.18 (Void=Anomaly).
6. **Architecture §2.3a DraftOrder erweitert** um `FocusedFireTargets: ImmutableDictionary<string, string>` + `AssignedPositions: ImmutableDictionary<string, (int x, int z)>` (CC-14).
7. **`api-reference.md`** angelegt mit verifizierten JobDefOf/ThingDefOf/ResearchProjectDef/IncidentDef/QuestManager/DlcCapabilities/Scribe-APIs (CC-08).
Begründung: Option-A-strict aus D-31 erfordert alle Findings gefixt, kein Cherry-Picking. Pass 2 ist jetzt vollständig für Re-Review Round 2 ready.
Konsequenzen:
- `sprint-status.yaml` Story-Count steigt auf 99, alle Stories ready-for-dev.
- Nächster Schritt: Commit + Push + Re-Review Round 2 (min. RimWorld + Stability Personas).
- Bei Approve in Round 2: Dev-Gate für Epic 1 geöffnet.

---

## D-31: Story-Revision nach Party-Mode-Review Round 1 (Option A strict BMAD)
Datum: 2026-04-24
Status: accepted
Kontext: Party-Mode-Review Round 1 über alle 85 Stories ergab APPROVE-WITH-CHANGES × 4 mit 15 HIGHs + 29 MEDs + 22 LOWs + 16 Cross-Cutting-Themen. User-Entscheidung: Option A = strict Revision aller Findings ohne Cherry-Picks (Guardian-Regel 4 konform). Report persistiert in `_bmad-output/implementation-artifacts/reviews/stories-party-mode-review-2026-04-24.md`.
Entscheidung: Revision-Pass 2 umfasst:
1. **7 neue Cross-Cutting-Stories:** 1.9 Schema-Version-Registry, 1.10 Exception-Wrapper-Pattern, 1.11 Plan-Arbiter, 1.12 QuestManager-Polling-Hook, 1.13 Test-Infrastructure, 3.13 Handler-Staleness-Pattern, 7.0 EndingSubPhaseStateMachine.
2. **Story 3.1 erweitert** um Pawn-Exclusivity-Lock-Framework-Kontrakt (CC-STORIES-06).
3. **Story 4.9 gesplittet** in 4-9a (E-MOOD), 4-9b (E-HEALTH), 4-9c (E-MENTALBREAK), 4-9d (E-RAID), 4-9e (E-FOODDAYS), 4-9f (E-MEDICINE), 4-9g (E-SLEEP) — jeweils S-Size.
4. **Epic 7 Naming refaktoriert:** 13 Stories von „…Manager" → „…Planner"/"…PhaseRunner" gemäß D-15-Konvention.
5. **Alle 15 HIGHs + 29 MEDs + 22 LOWs** in Einzelstory-AC-Erweiterungen adressiert.
6. **Architecture §2.3a erweitert** um DraftOrder-Schema (FocusedFireTarget + AssignedPosition) für CC-STORIES-14.
7. **`_bmad-output/implementation-artifacts/api-reference.md`** neu angelegt mit verifizierten JobDefOf/ThingDefOf-Namen für CC-STORIES-08.
8. **Re-Review Round 2** (min. RimWorld + Stability) nach Pass 2.
Begründung: Guardian-Regel 4 verlangt ALLE Findings gefixt, keine Cherry-Picks. Regel-5-Selbst-Finding aus vorherigem Turn (drei-Optionen-Präsentation) korrigiert — Option A ist Default, nicht „eine von mehreren Alternativen". Party-Mode-Report hat Cross-Cutting-Themen hinreichend dokumentiert, dass eine systematische Revision möglich ist.
Konsequenzen: Story-Count: 85 → 85 + 7 (neu) + 6 (4-9-Split: -1 + 7) = **98 Stories**. Sprint 1 Goal bleibt offen bis Pass 2 durch ist. Dev-Start verschiebt sich — akzeptiert.

---

## D-30: All-Upfront-Story-Drafting vor Epic-1-Dev-Start
Datum: 2026-04-24
Status: accepted (User-Override auf BMAD-Default)
Kontext: BMAD-Standard ist Per-Epic-Drafting — nur die aktuelle Epic-Batch draften, nach Epic-Done die nächste. Begründung dort: Story-Details ändern sich durch Erkenntnisse aus früheren Implementierungen. User hat explizit „Option 2" gewählt: alle 85 Stories (Epic 1-8) vor Epic-1-Dev-Start draften, dann einmaligen Party-Mode-Review statt 8 iterativer Review-Runden.
Entscheidung: Sprint 1 Goal erweitert: „Alle 85 Stories (Epic 1-8) gedraftet + Party-Mode-Review → Dev-Start-Gate für Epic 1". Reihenfolge:
1. Stories 1-1..1-8 bereits gedraftet (Epic 1)
2. Stories 2-1..2-9, 3-1..3-12, 4-1..4-10, 5-1..5-8, 6-1..6-10, 7-1..7-18, 8-1..8-10 werden nachgezogen
3. Party-Mode-Review über alle 85 Stories (4 Personas parallel, scoped-by-Epic)
4. Findings eingearbeitet
5. Sprint 1 Goal erfüllt → Dev-Phase für Story 1.1 startet
Begründung: (a) User will einmaligen Review-Aufwand statt 8× Iteration. (b) Story-Drafting ist zu diesem Zeitpunkt bereits informiert durch die durchlaufenen 4 Architecture-Review-Runden — die Unsicherheit die Per-Epic-Drafting ursprünglich adressieren soll, ist durch die intensive Architecture-Phase stark reduziert. (c) Party-Mode-Review über alle 85 gibt Cross-Epic-Konsistenzchecks (Naming, Pattern-Anwendung, Dependency-Grafiken), die bei Per-Epic nicht entstehen würden.
Konsequenzen: Circa 67 weitere Story-Dateien zu schreiben (Epic 2-8). Story-Format kompakter als Epic-1-Stories (Decisions-Kontext ist geteilt, weniger Inline-TBD-Erklärungen nötig). Nach Draft: Party-Mode über alle 85. Danach evtl. Revision-Runde. Das erhöht Zeit-Investment in Planning vs. Dev-Progress; User-Override akzeptiert das Tradeoff.

---

## D-28: Repository-Name `rimworld-ai`
Datum: 2026-04-24
Status: accepted
Kontext: GitHub-Repo wurde initialisiert, Default-Vorschlag war `rimworld-bot` (Match zu packageId `mediainvita.rimworldbot`). User entschied sich für `rimworld-ai` — eigenständiger Name, nicht strikt am `packageId` gebunden.
Entscheidung: **Repo-Name = `rimworld-ai`**, Owner = `shokiteufel`, Public, Topics: `rimworld`, `rimworld-mod`, `ai`, `csharp`, `harmony`. URL: https://github.com/shokiteufel/rimworld-ai. `packageId` bleibt `mediainvita.rimworldbot` (Architektur-Decision aus Story 1.1, TQ-S1-02 resolved) — beide Identifier müssen nicht identisch sein, `packageId` ist für RimWorld-Ingame-Mod-Resolution, Repo-Name ist für Distribution.
Begründung: „AI" beschreibt das Kern-Feature präziser als „Bot" (Bot ist generischer, „AI" signalisiert Entscheidungs-System). User-Präferenz für eigenständige Namenswahl respektiert Scope-Autonomie. Kein Konflikt mit `packageId` — Ingame-Referenz + Distribution-URL können divergieren.
Konsequenzen: Story 1.1 `About/About.xml` AC 2: `url`-Feld = `https://github.com/shokiteufel/rimworld-ai`. README.md bekommt Repo-URL. Keine anderen Artefakte betroffen.

---

## D-27: Sub-Phase-Transition PM_ARCHITECT → STORY_DRAFTING
Datum: 2026-04-24
Status: accepted
Kontext: Alle drei Planning-Autoritäten (PRD, Architecture v2.3, epics.md) sind vom User signed-off. Architecture durchlief 4 Review-Runden (REJECT → 3× APPROVE-WITH-MINOR-CHANGES) mit 24 Decisions (D-11 bis D-26) als Beleg. epics.md hat Guardian-Epic-Plan-Check über 11 Runs bestanden.
Entscheidung: sub_phase in sprint-status.yaml wechselt PM_ARCHITECT → STORY_DRAFTING. Sprint 0 (Planning) endet, Sprint 1 (Story-Drafting Epic 1) startet. Ziel Sprint 1: alle 8 Stories von Epic 1 (1-1-mod-project-skeleton bis 1-8-localization-skeleton) als Dateien in `_bmad-output/implementation-artifacts/stories/` gedraftet, Status backlog → ready-for-dev.
Begründung: BMAD-Workflow-Gate erfüllt (PRD + Architecture + Epic-Liste approved). Weiteres Planungs-Polieren wäre Diminishing-Returns (siehe Round-4-Report Trajektorien-Analyse). Story-Drafting bringt die Arch-Annahmen an die Realität.
Konsequenzen: sprint-status.yaml sub_phase + current_sprint aktualisiert. epics.md Status auf Approved. Dev-Loop (dev → code-review → fix → re-review → visual-review → fix → re-review → done) ist ab Story 1.1 aktiv.

---

## D-26: StableConsecutiveCounter-Pattern (generisch)
Datum: 2026-04-24
Status: accepted
Kontext: Round-4 Review F-AI-15. Das Counter-Reset-Problem aus F-AI-08 (Phase-Transition `stableCounter`) wiederholte sich exakt in §6.3 Auto-Escape-Timer (v2.2): `ticks_since_phase7_entry >= 5000` zählt absolute Zeit statt konsekutive Bedingungserfüllung. Fix musste zweimal gemacht werden. Generisches Pattern verhindert dritte Wiederholung bei zukünftigen „N konsekutive Erfüllungen"-Regeln.
Entscheidung: Gemeinsames `StableConsecutiveCounter`-Pattern mit explizitem Vertrag:
- Zählt **konsekutive Eval-Zyklen** (Ticks oder Reevals) in denen Bedingung erfüllt ist.
- **Reset auf 0** sobald Bedingung in einem Eval-Zyklus nicht erfüllt ist.
- Zählt **monoton** sonst.
- Persistiert im passenden Component (Save-Load-stabil).
- Dokumentiert explizit: Trigger-Schwelle in Reeval-Einheiten (nicht absolute Zeit).
Aktuelle Instanzen: `BotMapComponent.stableCounter` (Phase-Transition, F-AI-08) und `BotGameComponent.autoEscapeStableCounter` (Ending-Auto-Escape, F-AI-15). Zukünftige „N stable Reevals"-Regeln MÜSSEN diesem Pattern folgen.
Begründung: Generische Spec verhindert Copy-Paste-Varianten mit unterschiedlichen Semantiken; macht Unit-Tests ausknobelbar (§9.1 `StableCounterReset`); Reviewer können Pattern-Anwendung direkt prüfen.
Konsequenzen: Architecture §3 Stable-Counter-Block + §6.3 auto_escape_stable_counter mit identischem Kontrakt. Beide persistiert, beide Reset-Regel ausgeschrieben.

---

## D-25: Goal-Phase-Tag-Schema für Orphan-Cleanup
Datum: 2026-04-24
Status: accepted
Kontext: Round-4 Review F-STAB-21. `ReconcilePhaseGoalOrphans()` in v2.2 §5 rief `CancelOrphanedDesignations()` + `CancelOrphanedJobs()` auf, aber die Methoden waren ohne Tag-Schema No-op — Vanilla-`Blueprint`/`Designation`/`Job`-Klassen haben kein Bot-eigenes Phase-Feld. F-STAB-10 bleibt dadurch PARTIAL trotz v2.2-Code.
Entscheidung: `BotMapComponent` hält zwei Dicts:
- `Dictionary<int /*thing.thingIDNumber*/, PhaseGoalTag> botPlacedThings`
- `Dictionary<int /*job.loadID*/, PhaseGoalTag> botAssignedJobs`

`PhaseGoalTag(int PhaseIndex, string GoalId)` als immutable record. Execution-Klassen (`BlueprintPlacer.Apply`, `BillManager.Apply`, `DraftController.Apply`, `WorkPriorityWriter.Apply`) tragen nach jedem Placement-Step einen Tag ein. `ReconcilePhaseGoalOrphans()` iteriert Dicts und cancelt wo `tag.PhaseIndex > currentPhaseIndex`, entfernt Entries. Cleanup von destroyed/discarded Things alle 60000 Ticks (analog `perPawnPlayerUse`).
Begründung: (a) Macht Orphan-Cleanup nach Phase-Rollback tatsächlich wirksam — F-STAB-10 → RESOLVED. (b) Minimal-invasive Tag-Persistenz in `BotMapComponent` ohne Vanilla-Klassen zu patchen. (c) `thingIDNumber`/`loadID` sind save-stabile Vanilla-Identifier.
Konsequenzen: `BotMapComponent.ExposeData` um beide Dicts erweitert. Execution-Layer-Stories (Epic 3, Stories 3.8–3.10) müssen Tag-Schreiben im Apply-Pfad implementieren. Test-Fälle für Tag-basierten Cleanup.

---

## D-24: Event-Queue-Lifecycle — Init im Konstruktor + Clear bei LoadedGame
Datum: 2026-04-24
Status: accepted, refines D-18
Kontext: Round-4 Review F-ARCH-15 (REGRESSION von F-ARCH-11) + F-STAB-20. v2.2 initialisierte EventQueue in `FinalizeInit()` nach `BuildController()` — zu spät, wenn Factory-Kollaborateur Queue-Referenz im Konstruktor braucht, außerdem können Harmony-Patches (H2–H6) zwischen `BotGameComponent`-ctor und `FinalizeInit` feuern. Zweitens: `LoadedGame()` cleared Queue nicht — Load-Path-Events (H2-Postfix während Save-Load) bleiben mit alten `EnqueueTick`-Werten in Queue.
Entscheidung:
- EventQueue wird in `BotGameComponent(Game)`-Konstruktor initialisiert, **vor** jeder Harmony-Patch-Möglichkeit.
- `LoadedGame()` und `StartedNewGame()` rufen beide als erste Aktion `eventQueue.Clear()`.
- Explizite Feld-Deklarationen `BotController controller; BoundedEventQueue<BotEvent> eventQueue; ConfigResolver configResolver;` im Klassenkopf.
Begründung: (a) Konstruktor-Init eliminiert Race mit Pre-FinalizeInit-Events. (b) `Clear()` bei Load/NewGame verhindert Dispatch auf recycelte Map/Pawn-IDs mit alten Tick-Stamps. (c) Explizite Felder beseitigen den impliziten Local-Var-Geschmack aus v2.2-Code.
Konsequenzen: §5 BotGameComponent-Code-Snippet vollständig überarbeitet. §2.6 EventQueue-Eintrag ergänzt um Init-Timing.

---

## D-23: Plan-Schema-Identifier-Pattern (verschärft D-21)
Datum: 2026-04-24
Status: accepted, refines D-21
Kontext: Round-4 Review F-ARCH-13. D-21 führte immutable Records + `UniqueLoadID`-Pattern für Pawns ein, aber v2.2 §2.3a verwendete weiterhin `ThingDef`, `Building_WorkTable`, `IntVec3`, `Rot4`, `WorkTypeDef` in Plan-Records. Record-Value-Equality über RimWorld-Runtime-Types → Reference-Equality → Test-Asserts inkorrekt. Widerspricht AI-2 (Decision-Klassen pure) und §2.2 (Unit-testbar ohne RimWorld-Runtime). Inkonsistent zu D-21-Regel für Pawns.
Entscheidung: **Strikt konsistent** — keine RimWorld-Runtime-Typen in Plan-Records. Nur:
- `string` für Def-Namen (`ThingDef.defName`, `WorkTypeDef.defName`, `RecipeDef.defName`)
- `int` für Thing-Identifier (`thing.thingIDNumber`)
- `string` für Pawn-Identifier (`pawn.GetUniqueLoadID()`)
- `(int x, int z)` Tuple für Positionen (statt `IntVec3`)
- `byte` für Rotationen (`Rot4.AsByte`)
- `enum` für Discriminated-Unions (`BillIntentKind`)
Apply-Klassen resolven Identifier zur Laufzeit via `DefDatabase<T>.GetNamed()`, `Map.thingGrid.ThingAt()`, etc. Bei Resolve-Failure (Mod-Unload, Thing destroyed) Skip + Log, keine Exception.
Zusätzlich: `ImmutableList/Dictionary/HashSet` aus `System.Collections.Immutable` statt `IReadOnly*` — echte Immutability (F-ARCH-17).
Begründung: (a) Plans sind jetzt vollständig unit-testbar ohne RimWorld-Harness. (b) Value-Equality arbeitet auf Content (Strings, Ints, Enums) — Test-Asserts via `planA == planB` korrekt. (c) Plans überleben Mod-Re-Load (keine dangling Defs/Things-Referenzen). (d) Konsistent mit D-21-Pattern durchgehend.
Konsequenzen: §2.3a alle 5 Plan-Records komplett umgeschrieben. §9.1 Unit-Tests einfacher (keine Runtime-Mocks nötig). Execution-Layer in Epic 3/5 muss Resolve-Pattern implementieren.

---

## D-22: OverrideLibrary-Layer-Kontrakt
Datum: 2026-04-24
Status: accepted
Kontext: Round-3 Review Finding F-AI-13. OverrideLibrary-Einträge können mit hoher Confidence (z. B. 0.95) vom gelernten Player-Override stammen. Ohne expliziten Layer-Kontrakt unklar, ob Override EmergencyResolver überstimmen darf. Risiko: Player-Override „Draft all combat pawns to killbox" lernt sich ein, bei späterem Raid-Typ wo Killbox falsch ist feuert Override trotzdem dank Confidence → statt EmergencyResolver-Kontext-Analyse.
Entscheidung: Neue Architektur-Invariante **AI-7: Emergency > Override > PhaseGoal**. OverrideLibrary greift NUR wenn (a) EmergencyResolver keine Emergency aktiv hat UND (b) aktueller `SituationHash` exakt einem Eintrag matcht. Override-Confidence beeinflusst nur Tie-Break zwischen match-enden Overrides, niemals gegen Emergency-Score.
Begründung: (a) Emergency-Resolver ist kontext-sensitiv (D-16 Utility-Scoring), Override ist situational-hash-basiert ohne Kontext — Emergency ist immer kontextreicher. (b) Player-Override-Lernen soll PhaseGoal-Wahl verfeinern, nicht Safety-Decisions überstimmen. (c) Klare Semantik für Debug-Panel-Transparenz.
Konsequenzen: Architecture §1 AI-7 + §6.2a Layer-Interaktion-Block. OverrideEvaluator (§2.6) wird angepasst damit es nie in Emergency-aktiven Kontexten ausgewertet wird.

---

## D-21: Plan-Schema-Vertrag
Datum: 2026-04-24
Status: accepted
Kontext: Round-3 Review Finding F-ARCH-09. Plan/Apply-Trennung (D-15) führte Plan-Klassen ein (`BillPlan`, `WorkPriorityPlan`, `DraftOrder`, `BuildPlan`, `CaravanPlan`) — aber ohne spezifizierte Schemata. Test-Strategie (§9.1) und Unit-Test-Assertions nicht möglich ohne Vertrag.
Entscheidung: Alle Plan-Objekte sind **immutable `record`-Typen** mit Value-Equality (C# 9+, .NET Framework 4.7.2 kompatibel via Nuget-Polyfill wenn nötig, oder via manueller `record`-Semantik implementiert). Vertrag: Planner erzeugen immer valide Plans (keine Nulls, keine paradoxen Empty-Sets). Apply-Klassen dürfen Plans refusal (z. B. Pawn nicht mehr spawned) ohne Exception — Skip + Log. Plan-Schemata explizit in Architecture §2.3a als Code-Snippets.
Begründung: (a) records ermöglichen Value-Equality-Tests ohne Mock-Overhead. (b) Immutabilität verhindert versehentliche Mutation zwischen Plan-Erzeugung und Apply. (c) Pawn-Referenzen in Plans via `UniqueLoadID` (string) statt `Pawn`-Object — Plan bleibt gültig auch nach Pawn-Referenz-Staleness.
Konsequenzen: §2.3a dokumentiert 5 Plan-Records. `UniqueLoadID`-basierte Pawn-Referenzen in DraftOrder, WorkPriorityPlan, CaravanPlan. Test-Helper `FakeSnapshotProvider` + `TestSnapshotBuilder` in §9.1.

---

## D-20: Composition-Root via Factory (test-konstruierbar)
Datum: 2026-04-24
Status: accepted
Kontext: Round-3 Review Finding F-ARCH-06. `BotGameComponent` als Composition-Root hart gekoppelt an RimWorld `GameComponent`-Lifecycle → Unit-Tests müssten `Game`-Mock bauen, was §9.1-Strategie (RimWorld-freie Unit-Tests) unterläuft.
Entscheidung: Neue `BotControllerFactory.Create(ConfigResolver, ISnapshotProvider, ...)` als reine Factory ohne GameComponent-Bezug. `BotGameComponent.FinalizeInit()` ruft Factory mit RimWorld-Dependencies. Tests instanzieren direkt via Factory mit `FakeSnapshotProvider` ohne jegliches RimWorld-API. `BotMapComponent` bekommt Controller-Referenz via Konstruktor-Injection statt `Current.Game`-Lookup im Hot-Path.
Begründung: (a) Macht `BotController` samt allen Dependencies Unit-testbar ohne RimWorld-Runtime. (b) Entkoppelt Komposition vom Framework-Lifecycle — Tests + `BotGameComponent` verwenden beide dieselbe Factory, keine duplizierte Wiring-Logik. (c) Vermeidet versteckte Globals via `Current.Game`.
Konsequenzen: `BotControllerFactory`-Klasse neu in §2.1. `BotGameComponent.FinalizeInit()` delegiert an Factory. Architektur-Invariante AI-4 präzisiert: „Konstruktor-Injection über `BotControllerFactory`".

---

## D-19: Event-Identifier-Generation-Tick gegen ID-Recycling
Datum: 2026-04-24
Status: accepted, refines D-18
Kontext: Round-3 Review Finding F-STAB-15. `Map.uniqueID` und `Pawn.GetUniqueLoadID()` können nach Save-Edit oder Mod-Versions-Wechsel recycelt werden. `MapFinalizedEvent(uniqueID=42)` für gedroppte alte Quest-Map könnte nach 200 Ticks in Queue beim Dispatch auf eine neue Map mit zufällig identischer ID treffen → `MapAnalyzer.FullScan` läuft auf falscher Map.
Entscheidung: Jedes `BotEvent` trägt `EnqueueTick = Find.TickManager.TicksGame` als Feld. Stale-Check vergleicht `target.generationTick <= event.EnqueueTick`. Maps/Pawns die NACH dem Enqueue erzeugt wurden sind kategorisch nicht der Referent des Events → silent drop. Analoges Pattern für Pawn-Events via `pawn.ageTracker.BirthAbsTicks` oder `Pawn.thingIDNumber`-Generation (falls BirthTick nicht verfügbar für non-Human Things).
Zudem: EventQueue ist **transient** — NICHT über Save-Load persistiert. Nach Load triggert RimWorld `Map.FinalizeInit` ohnehin neu (H2-Patch-Postfix), andere Events sind Vanilla-Replay-fähig (Raid wird vom `IncidentWorker` neu announced).
Begründung: (a) `EnqueueTick`-Stamping ist kostengünstig (int) und bindet Event eindeutig an die Identitätsgeneration seines Referenten. (b) Transient-Queue-Design spart Save-Size und vermeidet Schema-Migration bei Queue-Struktur-Änderungen.
Konsequenzen: `BotEvent`-Records bekommen `EnqueueTick` als Pflichtfeld. §3 Event-Dispatch-Block dokumentiert Stale-Check mit `target.generationTick <= EnqueueTick`. §2.6 EventQueue-Eintrag markiert transient.

---

## D-18: Event-Queue-Spezifikation
Datum: 2026-04-24
Status: accepted
Kontext: Re-Review Round 2 Findings F-EVENT-01 (RimWorld-Specialist) + F-STAB-09 (Stability). Beide identifizierten unabhängig: Architecture v2.0 referenzierte `EnqueueEvent` / Event-Queue ohne Spezifikation von Kapazität, Drop-Policy, Ordering, Stale-Check, Event-Klassen-Hierarchie. Bei Event-Spam (Raid-Kaskade, Quest-Flood) drohte unbegrenztes Queue-Wachstum → GC-Pressure → Tick-Miss.
Entscheidung:
- `BoundedEventQueue<BotEvent>` mit Kapazität **256**, Drop-Oldest-Policy bei Overflow, WARN-Log rate-limited (einmal pro 100 Drops).
- **FIFO per Frame**, zwischen Frames Ordering per Enqueue-Timestamp.
- **Stale-Check** vor Dispatch: `Map.uniqueID` lookup (Map disposed?), `Pawn.GetUniqueLoadID()` lookup (Pawn destroyed?). Stale Events werden silent gedropt (kein Log — kann bei normaler Map-Dispose häufig sein).
- **Event-Klassen-Hierarchie:** `BotEvent` abstract; konkret `MapFinalizedEvent`, `RaidEvent`, `DraftedEvent`, `QuestWindowEvent`, `PawnExitMapEvent`.
- **Events enthalten nur Identifikatoren** (UniqueLoadID, Map-uniqueID, Window-Typ-Name), NIE Live-Referenzen auf `Pawn`/`Map`/`Window`. Auflösung der Referenzen passiert im Dispatch im Tick-Host — dort ist auch der Stale-Check billig.
Begründung: (a) Bounded-Queue verhindert Memory-Blowup bei Event-Floods. (b) Identifikator-only-Events vermeiden Referenz-Staleness, die im Patch-Body nicht detektierbar wäre. (c) FIFO + Stamp macht Ordering deterministisch für Tests. (d) Drop-Oldest ist die richtige Policy: neue Events sind meist relevanter als alte (Raids, Quests).
Konsequenzen: `EventQueue` als neues Service-Objekt in §2.6. `BotEvent`-Subklassen. Patch-Body-Skelette in §4.1 ziehen nur Identifikatoren (Map.uniqueID, window.GetType().Name). Test-Fall für Queue-Overflow + Stale-Dispatch im Epic-1-Story-Drafting.

---

## D-17: Ending-Hysterese + Phase-7-Commitment
Datum: 2026-04-24
Status: accepted
Kontext: Party-Mode-Review Finding F-AI-03. Opportunistisches Ending-Switching (D-06) kann bei zwei fast-gleichen Feasibility-Scores zu Flip-Flopping und Ressourcen-Verlust führen, besonders mit D-11 (keine Zeit-Voraussetzung, also kein Druck „bald entscheiden").
Entscheidung:
- `HYSTERESIS_MARGIN = 0.15`: Switch nur wenn `new_feasibility > current + margin`
- `sunk_cost_penalty`: ergänzend `resources_invested * 0.7` wird abgezogen vom new_feasibility
- `endingCommitment`-Feld im BotGameComponent: nach Phase-7-Entry auf `Locked`; danach nur Switch bei Game-Breaking-Events (Reaktor zerstört, Monolith weg, Imperium feindlich), nicht über Feasibility-Flip
Begründung: Sunk-Cost-Awareness verhindert Flip-Loops; Phase 7 ist der natürliche Commitment-Punkt (alle Endings brauchen ab dort dedizierte Ressourcen). Alternative Phase 6 wäre früher, aber würde Opportunismus in Phase 6 unnötig einschränken.
Konsequenzen: EndingFeasibility bekommt Hysterese-Gate; `EndingCommitment` enum; Phase-7-Entry setzt Commitment; Event-basierte Switches nur bei explizit-definierten Game-Breaking-Triggern.

---

## D-16: Emergency-Utility-Scoring statt fixer Priorität
Datum: 2026-04-24
Status: accepted, supersedes Mod-Leitfaden §2 fixe Emergency-Reihenfolge
Kontext: Party-Mode-Review Finding F-AI-02. Fixe Prio (E-FIRE > E-BLEED > E-INTRUSION > …) führt zu falschen Entscheidungen bei Konflikten (z. B. Doctor rennt unter Raid-Feuer zum bleedenden Pawn, beide sterben).
Entscheidung: `EmergencyResolver` berechnet pro aktiver Emergency einen `Score = base_prio + context_modifiers(colony_state)`. Modifier-Beispiele: `+100 wenn E-BLEED-Pawn unter aktiver Intrusion unreachable`, `+50 wenn E-FIRE in Kühlraum und food_stock_days < 10`. Die höchste-scorende Emergency gewinnt. Handler deklarieren Pawn-Exclusivity (Draft-Lock). E-BLEED bei blockierten Pawns landet auf Rescue-Later-Queue, wird nach Intrusion-Clear erneut evaluiert.
Begründung: Real-World-Emergency-Kollisionen sind kontext-abhängig; fixe Prio ist zu starr. Utility-Maximizer bleibt deterministisch (gleicher State → gleicher Score), aber reagiert auf Situations-Kombis.
Konsequenzen: `EmergencyHandler.Score(state)` wird verpflichtend. Mod-Leitfaden §2 bekommt Update-Hinweis (v1.0 Prio-Liste wird zum `base_prio`, Modifier-Liste neu).

---

## D-15: Plan/Apply-Trennung Decision ↔ Execution
Datum: 2026-04-24
Status: accepted
Kontext: Party-Mode-Review Finding F-ARCH-01. Ursprüngliche Architecture hatte Schicht-Leck: `BillScheduler` in Decision mutierte via `Ensure()` direkt RimWorld-Bills; `WorkAssigner` in Decision schrieb `pawn.workSettings.priorities`. Das bricht die Observation → Decision → Execution-Kontur.
Entscheidung: Strikte Trennung — Decision-Klassen produzieren Plan-Objekte (`BillPlan`, `WorkPriorityPlan`, `DraftOrder`, `BuildPlan`, `CaravanPlan`), Execution-Klassen appliziern. Namens-Konvention: Decision = `XxxPlanner` (returns Plan), Execution = `XxxManager` oder `XxxWriter` (takes Plan, mutates). `BotController` orchestriert: `var plan = planner.Plan(state); applier.Apply(plan);`.
Begründung: (a) Testbarkeit — Planners sind pure, in Unit-Tests ohne Mocking testbar. (b) Debug-Transparenz — Plan-Objekte sind inspizierbar vor Apply. (c) Rollback/Dry-Run-Fähigkeit frei implementierbar (AI_ADVISORY-Modus zeigt Plans ohne Apply).
Konsequenzen: Umbenennung `BillScheduler` → `BillPlanner`. Split `WorkAssigner` → `WorkPlanner` + `WorkPriorityWriter`. Neue Plan-Klassen. Controller-Tick-Schritte 11-14 im Daten-Fluss-Diagramm.

---

## D-14: Persistence-Scoping + Schema-Versioning
Datum: 2026-04-24
Status: accepted
Kontext: Party-Mode-Review Findings CC-02 / F-SAVE-01 / F-STAB-01. Ursprüngliche Architecture hatte `Scribe_Deep` auf `MapAnalysisResult` im game-globalen `BotGameComponent` ohne Schema-Version, und `perPawnPlayerUse` mit `thingIDNumber`-Keying (leakt bei Tod/Banish).
Entscheidung:
- `BotMapComponent : MapComponent` für Per-Map-Daten (`MapAnalysisSummary`-Cache). Per-Cell-Array wird nicht persistiert, nur Top-3-Sites + Scores; rekomputierbar on Load.
- `BotGameComponent : GameComponent` bleibt für Game-global (ToggleState, primaryEnding, endingCommitment, perPawnPlayerUse, recentDecisions FIFO-100).
- `schemaVersion`-Feld als erste Scribe-Zeile in beiden Components. Bei Mismatch: `Migrate()` oder `ResetToDefaults()` mit User-Toast.
- `perPawnPlayerUse` neu als `Dictionary<string, bool>` mit `pawn.GetUniqueLoadID()` (stabil über Saves). Cleanup alle 60000 Ticks (Destroyed/Discarded-Pawns entfernen).
- Alle `ExposeData`-Methoden in try/catch mit `ResetToDefaults()`-Fallback bei Corruption.
Begründung: (a) Per-Map-Scoping entspricht RimWorld-Konvention; Multi-Map-Szenarien (Caravan-Map, Quest-Map) waren vorher korrupt. (b) Schema-Versioning erlaubt forward-compatible Mod-Updates ohne Save-Break. (c) `UniqueLoadID` ist der offizielle Save-stabile Identifier.
Konsequenzen: Zwei-Komponenten-Architektur für Persistenz. `MapAnalysisSummary` als schlanker `IExposable`-POCO. Migration-Tests (TC-08).

---

## D-13: Minimal-Harmony-Strategie
Datum: 2026-04-24
Status: accepted, supersedes v1.0 Patch-Inventar
Kontext: Party-Mode-Review Findings CC-01 / F-HARMONY-01 (H1 patcht nicht-existente `Game.UpdatePlay`), F-HARMONY-02 (H7 widerspricht `MainButtonDefs.xml`), F-HARMONY-03 (H8 falscher ITab), CC-03 / F-ARCH-04 / F-STAB-02 (Exceptions im Patch-Body nicht gefangen), F-STAB-04 (Mod-Konflikte).
Entscheidung:
- H1, H7, H8 entfernt.
- H7 ersetzt durch `MainButtonDef`-XML + `MainTabWindow_BotControl`-Subklasse (kein Patch).
- H8 ersetzt durch eigenen `ITab_Pawn_BotControl : ITab`, registriert via XML-Patch auf `Human.inspectorTabs`.
- Alle verbleibenden Patches (H2–H6, H9) bekommen:
  - `[HarmonyPriority(Priority.Low)]` (lässt fremde Mods zuerst laufen)
  - Verpflichtendes Exception-Skelett: `try { … } catch (Exception ex) { if (!BotErrorBudget.CanLog()) return; Log.Error(…); BotSafe.Get(() => FallbackToOff()); }`
  - Patch-Body nur Enqueue-Logik (keine Analyse/Entscheidung — passiert im Tick-Host)
- Startup-Mod-Konflikt-Scan: `RimWorldBotMod.InitializeCompat()` prüft `Harmony.GetAllPatchedMethods()` und erkennt Überschneidung mit RocketMan / Performance Fish / Dubs Performance Analyzer → `CompatMode.EnableReducedPerformance()` (Tick-Intervall ×2) + Warning-Log.
Begründung: (a) H1 war Lade-verhindernder CRIT. (b) H7/H8 waren unnötig Vanilla-invasive. (c) Exception-Wrapper-Skelett schließt die CRIT-Lücke aus CC-03. (d) `Priority.Low` + Konflikt-Scan macht Mod koexistenz-fähig mit den wichtigsten Performance-Mods.
Konsequenzen: `Patches/HumanInspectTabs.xml` neue Datei. `MainTabWindow_BotControl`-Klasse. `BotErrorBudget` + `BotSafe` Utility-Klassen. `CompatMode` Service. Story-Impact dokumentiert in Action-Matrix des Party-Mode-Reports.

---

## D-12: Tick-Host = GameComponentTick, kein Patch auf Tick-Loop
Datum: 2026-04-24
Status: accepted, supersedes v1.0 H1-Design
Kontext: Party-Mode-Review Finding CC-01 / F-HARMONY-01. `Game.UpdatePlay` existiert in RimWorld 1.5/1.6 nicht als patchbare Methode. Harmony würde bei Load mit `patched method not found` crashen — Mod läuft gar nicht erst an.
Entscheidung: Haupt-Tick-Arbeit läuft in `BotGameComponent.GameComponentTick()` (Game-global) und `BotMapComponent.MapComponentTick()` (per-Map). Harmony-Patches werden ausschließlich für Event-Hooks verwendet (Map-Init, Raid-Announce, Draft-Event, Quest-Window, Pawn-Exit) — jeder Patch enqueued nur ein Event, das im nächsten Tick-Host-Tick verarbeitet wird.
Begründung: (a) `GameComponentTick` ist der dokumentierte, stabile RimWorld-API-Einstiegspunkt für Per-Tick-Arbeit. (b) Keine Harmony-Abhängigkeit auf Tick-Schleife → robust gegen RimWorld-Version-Updates. (c) Saubere Trennung Event-Empfang (Patch) vs. Verarbeitung (Tick-Host) macht Patches minimal und testbar.
Konsequenzen: H1 raus (architektur.md §4). Tick-Host muss Budget-aware arbeiten (nur alle 60 Ticks volle Evaluation). Event-Queue (`BotGameComponent.EnqueueEvent`) als Kommunikationspfad zwischen Patches und Tick-Host.

---

## D-11: Keine Zeit-Voraussetzungen für Phase-Erreichung
Datum: 2026-04-24
Status: accepted
Kontext: Epic 6 AC #1 forderte „Phase 5+6 in <3 Spieljahren autonom". Der Bot soll Phasen sicher erreichen, nicht in einem Zeitfenster.
Entscheidung: Projektweite Regel — die einzige Voraussetzung für das Abschließen einer Phase ist, dass die Phase-Exit-Conditions erfüllt sind und die Kolonie dabei nicht crashed / Pawns stirbt. Spielzeit ist keine Success-Metric. Das gilt für alle Epics (3, 4, 6, 7). Zeit-Angaben in Invariants (Food-Stock-Tage, 15-Tage-Belagerung, 90 Tage Winter-Essensvorrat etc.) sind Vanilla-Spielmechanik-Puffer bzw. Vanilla-Events — NICHT Erreichungs-Voraussetzungen und bleiben bestehen. Test-Dauer-Kriterien (z. B. „10-Tage-Testlauf ohne Crash" in Epic 4) sind Stabilitäts-Benchmarks, keine Gameplay-Ziele, und bleiben ebenfalls bestehen.
Begründung: Eine Zeit-Vorgabe für Phase-Erreichung würde den Bot zu Risk-Taking zwingen (suboptimale Builds, zu früh skalieren) und damit die Überlebens-Priorität unterlaufen. Opportunistisches, sicheres Vorgehen > Speedrun.
Konsequenzen: Epic 6 AC #1 angepasst. Keine weiteren Epic-Änderungen nötig. Ending-Feasibility-Engine (Epic 7) darf „Zeit-bis-Phase" intern als Hilfs-Metrik nutzen (z. B. „AIPersonaCore seit N Jahren nicht erreicht → Ending-Wechsel erwägen"), aber nicht als Abbruch-Kriterium für eine laufende Strategie.

---

## D-10: BMAD-Struktur einziehen
Datum: 2026-04-24
Status: accepted
Kontext: Projekt wächst über lose Docs hinaus; vor Code-Beginn klare Artefakt-Struktur nötig.
Entscheidung: `_bmad/` und `_bmad-output/` anlegen, PRD und Architecture aus `Mod-Konzept.md` splitten, Milestones in 7 Epics überführen. Phase = PLANNING, Sub-Phase = PM_ARCHITECT.
Begründung: Saubere Mod in Größenordnung erfordert DoD-getrennte Artefakte, Story-drafted Work-Packages, Guardian-Enforcement gegen Scope-Creep.
Konsequenzen: Guardian-Rules gelten jetzt. Keine Code-Arbeit bis MVP-Epics (01–03) mit Stories approved.

---

## D-09: Hazard-Score (Lava/Gift/Pollution/Strahlung)
Datum: 2026-04-24
Status: accepted
Kontext: Map-Analyse berücksichtigte keine negativen Terrain-Features. Odyssey-Vulkane, Biotech-Pollution, Anomaly-Tox-Zonen machen Sites unbewohnbar.
Entscheidung: Neuer Gewichts-Term W_HAZARD=0.30 (negativ) mit Unter-Kategorien Lava/Pollution/Toxic. Hard-Filter: Lava-Tiles und hazardProximity<3 sind komplett aus Site-Pool ausgeschlossen.
Begründung: Ein „nur negativ gewichtet" reicht nicht, Lava ist schlicht unbebaubar. Hard-Filter verhindert, dass Bot absurde Kompromisse eingeht.
Konsequenzen: MapCellData-Struct um 5 Hazard-Felder erweitert. Implementation muss TerrainDef-Scans um Pollution-/Lava-Tags ergänzen.

---

## D-08: AI_OFF = stumm, AI_ADVISORY = Top-3-Overlay
Datum: 2026-04-24
Status: accepted
Kontext: Offene Frage aus Design-Runde: was tut die Mod im OFF-State mit der Map-Analyse?
Entscheidung:
- AI_OFF → Analyse läuft, Cache-only, kein Overlay
- AI_ADVISORY → Top-3-Markierungen als Overlay, Bot führt nicht aus
- AI_ON → Top-1 als Basis-Mittelpunkt + Blueprints
- Landepod-Spawn bleibt Vanilla-unberührt in allen States
Begründung: Minimale UI-Störung wenn Mod aus; klare Eskalationsstufe über Advisory; Landepod-Vanilla respektiert Spieler-Autonomie.
Konsequenzen: Drei separate Render-Pfade im UI-Layer nötig.

---

## D-07: Cross-Game-Lernsystem
Datum: 2026-04-24
Status: accepted
Kontext: User-Wunsch „KI soll immer weiter lernen, mit jedem Spiel besser werden".
Entscheidung:
- Lern-Daten außerhalb Savegame in `%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld\Config\RimWorldBot_LearnedConfig.xml`
- Bayesian-Update von Gewichten nach Emergencies: `new = 0.95*old + 0.05*observed`
- Spieler-Override wird nur gelernt wenn nach 60s Outcome messbar besser als Bot-Alternative
- Expliziter Reset-Button in ModSettings
- Out-of-Scope: neuronale Netze / deep ML
Begründung: Regelbasiertes Learning ist wartbar und debugbar, ML-Modelle wären nicht im Mod-Scope und schwer getestet. Cross-Game-Persistenz macht Bot progressiv besser.
Konsequenzen: Eigenes Learning-Layer mit Outcome-Evaluator-Komponente nötig.

---

## D-06: Opportunistisches Ending + Force-Option
Datum: 2026-04-24
Status: accepted
Kontext: Welches Ending verfolgt der Bot?
Entscheidung:
- Fortlaufende Feasibility-Bewertung aller 5 Endings
- Bot hält Infrastruktur bis Phase 6 ending-agnostisch
- Switch-Trigger bei Game-Breaking-Situations (Reaktor zerstört, Imperium feindlich etc.)
- Force-Option per Dropdown in ModSettings
Begründung: Maximiert Erfolgsrate über unterschiedliche Map-/Raid-Szenarien; Force erlaubt gezielte Experimente.
Konsequenzen: Ending-Feasibility-Score-Komponente; Phase 7 ist dynamisch, nicht linear.

---

## D-05: Volle Re-Analyse bei nachträglicher Aktivierung
Datum: 2026-04-24
Status: accepted
Kontext: Was passiert, wenn Spieler Bot mitten in bestehender Kolonie aktiviert?
Entscheidung: Komplette Map- + Basis-Analyse. Layout-Score ≥ 0.6 → übernehmen, sonst Umbau-Plan vorschlagen.
Begründung: Halbherziges „nur ab jetzt" würde Bot mit suboptimaler Basis scheitern lassen.
Konsequenzen: Re-Analyse-Routine muss Phase-Detection + Basis-Audit leisten.

---

## D-04: Start-Pawn-Auswahl via „Player Use"-Flag
Datum: 2026-04-24
Status: accepted
Entscheidung: Im Character-Creation-Screen Checkbox pro Pawn. Bot steuert alle nicht-markierten.
Begründung: Einfach, explicit, kein Cherry-Picking von Skills durch Bot.
Konsequenzen: UI-Patch für Character-Creation nötig.

---

## D-03: Zwei-Ebenen-Toggle
Datum: 2026-04-24
Status: accepted
Entscheidung: Master-Button (alle Pawns) + Per-Pawn-Checkbox im Inspector. Master OFF überschreibt alles.
Begründung: User-Request für granulare Kontrolle.
Konsequenzen: Toggle-State-Machine ist zweistufig.

---

## D-02: Raw-Eating default-off nach Campfire
Datum: 2026-04-24
Status: accepted
Kontext: Simple Meal ist 1.8× effizienter als Raw, kein Mood-Malus, kein Food Poisoning.
Entscheidung: `allow_raw_eating = false` als Default ab Campfire-Build. Nur bei echter Last-Resort (keine Kochstelle erreichbar) Override.
Begründung: Ressourcen-Effizienz ist Pflicht, nicht Optimierung.
Konsequenzen: Cooking-Bills haben höchste Bill-Priorität in Phase 0 bis erste Mahlzeiten gestapelt.

---

## D-01: Harmony-gepatchter C#-Mod (statt XML-only)
Datum: 2026-04-24
Status: accepted
Entscheidung: Vollständiger C#-Mod mit Harmony-Patches für Vanilla-Hooks.
Begründung: Laufzeit-Entscheidungen (Tick-Loop, Emergency-Handler) sind in reinem XML nicht umsetzbar.
Konsequenzen: Build-System, DLL-Packaging, CI nötig.
