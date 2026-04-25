# Epics — RimWorld Bot Mod

**Version:** 1.0 (PLANNING)
**Letztes Update:** 2026-04-24
**Status:** **Approved (2026-04-24)** — User-Sign-Off nach Guardian-Validierung

---

## Critical Path

```
Epic 1 (Mod-Skeleton) → Epic 2 (Map-Analyzer) → Epic 3 (Invariants + Phase 0+1)
                                                     ↓
                                               MVP-Release
                                                     ↓
                                  Epic 4 (Phase-State-Machine 2-4)
                                                     ↓
                                     Epic 5 (Combat & Raids) ← parallel zu Epic 4 möglich
                                                     ↓
                                                 Alpha-Release
                                                     ↓
                                  Epic 6 (Industrialization Phase 5-6)
                                                     ↓
                                                  Beta-Release
                                                     ↓
                                   Epic 7 (Endings)
                                        ↓
                            Epic 8 (Polish, Learning, DLC)
                                        ↓
                                  Release (v1.0.0)
```

**Parallel-Fenster:**
- Epic 5 kann parallel zu Epic 4 laufen, sobald Epic 3 fertig
- Epic 8 kann teilweise parallel zu Epic 7 (Localization bereits ab Beta)

---

## Epic 1: Mod-Skeleton & Toggle

### Goal
Ein lauffähiger, leerer RimWorld-Mod mit Harmony-Init, Toggle-Button in der Top-Bar, Per-Pawn-Toggle, Savegame-Persistenz und Settings-Panel. Infrastruktur-Fundament ohne Bot-Logik.

### Acceptance Criteria
1. Mod lädt ohne Fehler in RimWorld mit Vanilla + allen DLCs
2. Button in Top-Bar klickbar, wechselt State OFF → ADVISORY → ON → OFF
3. Ctrl+K toggelt identisch zum Button
4. Per-Pawn-Checkbox „Player Use" persistiert
5. Savegame-Roundtrip: State bleibt erhalten
6. Settings-Panel öffnet, Werte persistieren
7. UI in Deutsch UND Englisch korrekt
8. Log-Einträge bei State-Wechsel sichtbar
9. Keine Exceptions im Output-Log

### Stories
- Story 1.1: Mod-Projekt anlegen (Ordnerstruktur, About.xml, leere DLL)
- Story 1.2: Harmony-Bootstrap (RimWorldBotMod.cs Entry-Point)
- Story 1.3: BotGameComponent (ExposeData für State)
- Story 1.4: Master-Toggle-Button (MainButtonDef, Top-Bar)
- Story 1.5: Keybinding Ctrl+K
- Story 1.6: Per-Pawn-Toggle (ITab-Patch)
- Story 1.7: Settings-Window
- Story 1.8: Localization-Skeleton (DE + EN)
- Story 1.9: Schema-Version-Registry (neu D-31, CC-STORIES-01)
- Story 1.10: Exception-Wrapper-Pattern (neu D-31, CC-STORIES-02)
- Story 1.11: Plan-Arbiter (AI-7 Layer-Präzedenz) (neu D-31, CC-STORIES-04)
- Story 1.12: QuestManager-Polling-Infrastruktur (neu D-31, CC-STORIES-09)
- Story 1.13: Test-Infrastructure (FakeSnapshotProvider, Testable Seams) (neu D-31, CC-STORIES-13)
- Story 1.14: Test-Runtime-Infrastructure-Refactor (neu D-37 2026-04-25) — fixt Krafs-mscorlib-vs-Microsoft-mscorlib Type-Identity-Mismatch der Production-DLL im xUnit-Runner; ermöglicht BotSafe + QuestManagerPoller + BoundedEventQueue Tests die in 1.13 deferred wurden

### Dependencies
Keine (erstes Epic). Story 1.14 hängt nur von 1.13 (Test-Framework-Setup) + 1.10 + 1.12 (zu testende Module) ab.

### Launch-critical: yes

### Risiken
- R-01: MainButtonDef-Position kollidiert mit anderen Mods
- R-02: ITab-Patch kollidiert mit Character-Editor-Mods

---

## Epic 2: Map-Analyzer

### Goal
Vollständiger Map-Scan nach Karten-Generation mit Scoring-Formel, Cluster-Analyse, Top-3-Site-Output und Overlay-Rendering bei AI_ADVISORY. Analyse im BotGameComponent gecached.

### Acceptance Criteria
1. Scan läuft nach `Map.FinalizeInit`
2. Top-3-Sites werden produziert mit Score-Breakdown
3. AI_ADVISORY-Overlay zeigt Kreise korrekt
4. Lava/Pollution-Zellen per Hard-Filter ausgeschlossen
5. Scan-Zeit bleibt unter 500ms (Coroutine-verteilt)
6. Re-Analyse-Trigger funktioniert bei Map-Wechsel
7. Cache persistiert im Savegame

### Stories
- Story 2.1: MapCellData-Struct + Basis-Scan
- Story 2.2: Wilde-Pflanzen-Erkennung (Berries, Healroot)
- Story 2.3: Hazard-Scanner (Lava, Pollution, Toxic)
- Story 2.4: Defensibility-Score (Choke-Points, Cliffs)
- Story 2.5: Scoring-Formel (alle Gewichte)
- Story 2.6: Cluster-Analyse (Top-3-Regions)
- Story 2.7: Overlay-Rendering
- Story 2.8: Caching im BotGameComponent
- Story 2.9: Coroutine-Split für Performance

### Dependencies
Epic 1 (BotGameComponent für Caching).

### Launch-critical: yes

### Risiken
- R-01: TerrainDef-Namen für Hazards variieren zwischen DLCs — muss zur Impl-Zeit aus XML extrahiert werden
- R-02: Performance bei 350×350-Maps (Coroutine + Subsampling)
- R-03: Wild-Plant-Detection-Timing nach FinalizeInit

---

## Epic 3: Invariants & Phase 0+1 (MVP-Core)

### Goal
Bot kann nackte Kolonie von Spielstart bis Phase-1-Exit autonom durchspielen. Invariants I1-I5 überwacht, Emergencies gehandhabt, Bau-Orders + Bills automatisch.

### Acceptance Criteria
1. Bot startet Naked-Brutality und führt autonom bis Phase-1-Exit
2. Invariants I1-I5 blockieren bei Verletzung alle anderen Work
3. `allow_raw_eating = false` ab Campfire-Build
4. Top-3-Site aus Epic 02 wird als Basis-Mittelpunkt verwendet
5. Mid-Game-Aktivierung detektiert korrekte Phase
6. Pawn stirbt nicht in 100-Tick-Testlauf
7. Savegame-Roundtrip bleibt konsistent
8. Tick-Budget eingehalten (<5ms durchschnittlich)

### Stories
- Story 3.1: Invariant-Framework (Abstract-Klasse)
- Story 3.2: Invariants I1-I5 (Shelter, Food, Bleed, Fire, Temp)
- Story 3.3: Emergency-Handler E-FIRE
- Story 3.4: Emergency-Handler E-BLEED
- Story 3.5: Emergency-Handler E-FOOD
- Story 3.6: Emergency-Handler E-SHELTER + E-TEMP
- Story 3.7: Phase 0 Goals + Exit
- Story 3.8: BuildPlanner MVP (Wall, Door, Bed, Campfire)
- Story 3.9: BillManager MVP (Cook Simple Meal, Craft Tribal Wear)
- Story 3.10: WorkAssigner Basic (Skill-Matching)
- Story 3.11: Phase 1 Goals + Exit (Hütte, Kleidung, Anbau)
- Story 3.12: Phase-Detection (Mid-Game-Aktivierung)
- Story 3.13: Handler-Staleness-Pattern (neu D-31, CC-STORIES-07)

### Dependencies
Epic 1, Epic 2.

### Launch-critical: yes (MVP-Kern)

### Risiken
- R-01: Harmony-Patch H1 auf Game.UpdatePlay performance-kritisch
- R-02: Job-Queue-Konflikte mit Vanilla-Work-System
- R-03: Edge-Cases bei Phase-Detection (halbe Basis, fehlende Defs)

---

## Epic 4: Full Phase-State-Machine (Phase 2-4)

### Goal
Phasen 2-4 (Food Security + Research, Winter Readiness, Stone Fortress) werden autonom durchspielbar. Research-Logik, Rekrutierung, vollständige 12 Invariants.

### Acceptance Criteria
1. Naked-Start durchläuft Phase 0 → 4 autonom
2. 2.-4. Pawn rekrutiert bis Ende Phase 4
3. Winter ohne Todesfall überstanden
4. Steinfestung mit Traps gebaut
5. Alle 12 Invariants aktiv
6. Mood im akzeptablen Bereich
7. 10-Tage-Testlauf ohne Crash

### Stories
- Story 4.1: Research-Scheduler
- Story 4.2: Phase 2 Goals + Exit
- Story 4.3: Rekrutierungs-Logik
- Story 4.4: Gefangenenzelle-Layout
- Story 4.5: Phase 3 Goals + Exit (Winter)
- Story 4.6: Phase 4 Goals + Exit (Stone)
- Story 4.7: Killpoint-Layout
- Story 4.8: Invariants I6-I12
- Story 4.9: Emergency-Handler für I6-I12 (retired D-31, gesplittet in 4.9a-g)
- Story 4.9a: Emergency-Handler E-MOOD (neu D-31-Split)
- Story 4.9b: Emergency-Handler E-HEALTH (neu D-31-Split)
- Story 4.9c: Emergency-Handler E-MENTALBREAK (neu D-31-Split)
- Story 4.9d: Emergency-Handler E-RAID (neu D-31-Split)
- Story 4.9e: Emergency-Handler E-FOODDAYS (pro Pawn) (neu D-31-Split)
- Story 4.9f: Emergency-Handler E-MEDICINE (neu D-31-Split)
- Story 4.9g: Emergency-Handler E-PAWNSLEEP (neu D-31-Split)
- Story 4.10: Skill-Grinding-Strategie

### Dependencies
Epic 3.

### Launch-critical: no (Alpha)

---

## Epic 5: Combat-Commander & Raid-Handling

### Goal
Raid-Events vollständig gemanaged: Bedrohungs-Einschätzung, Draft/Retreat, Post-Raid-Triage. Autonome Kampf-Entscheidungen.

### Acceptance Criteria
1. Raid mit 3 Gegnern ohne Verluste abgewehrt (Phase 4+)
2. Raid mit 10 Gegnern → Flee-Szenario korrekt ausgeführt
3. Post-Raid: alle Wounded behandelt, Leichen beseitigt, Walls repariert
4. Prisoner bei Raid mit 1 Gegner captured

### Stories
- Story 5.1: ThreatAssessment
- Story 5.2: Raid-Announce-Hook (H4)
- Story 5.3: CombatCommander-Entscheidungstree
- Story 5.4: DraftController
- Story 5.5: Flee-Option (Karawane)
- Story 5.6: Post-Raid-Handler
- Story 5.7: Focused-Fire-Tactics
- Story 5.8: Retreat-Fallback

### Dependencies
Epic 4 (Phase 4+ Killpoint existiert), Epic 3 (Emergency-Handler).

### Launch-critical: no (Alpha), **parallel zu Epic 4 möglich** ab Stories 5.1–5.3

---

## Epic 6: Industrialization (Phase 5+6)

### Goal
Phase 5 (Electrification) + Phase 6 (Industrialization) autonom. Strom, Hi-Tech, FabricationBench, Skill-Grinding auf 8, Components, optional Bionik.

### Acceptance Criteria
1. Phase 5+6 werden sicher erreicht (Phase-Exit-Conditions erfüllt, keine Pawn-Tode beim Übergang, keine Crash-Loops) — **keine Zeit-Voraussetzung** (siehe D-11)
2. Mind. 1 Pawn auf Crafting 8 + 1 Pawn auf Construction 8
3. Components autarke Produktion
4. Flak-Rüstung komplett
5. Turrets an Killpoints
6. 10+ Runs ohne Crash

### Stories
- Story 6.1: Strom-Netz-Planer
- Story 6.2: Phase 5 Goals + Exit
- Story 6.3: Hi-Tech-Werkbänke (HiTech-Bench, MultiAnalyzer)
- Story 6.4: Phase 6 Goals + Exit
- Story 6.5: Spezialisten-Rollen
- Story 6.6: Components-Fabrik
- Story 6.7: Bionics-Option (optional)
- Story 6.8: Flak-Rüstung-Verteilung
- Story 6.9: Turret-Placement
- Story 6.10: Mech-Cluster-Handler

### Dependencies
Epic 4, Epic 5.

### Launch-critical: no (Beta)

---

## Epic 7: Endings (Feasibility + Finish-Phases)

### Goal
Alle 5 Endings autonom erreichbar. Feasibility-Score wählt primary-Ending dynamisch. Force-Option übersteuert. Ending-Sequenzen bis zum Credits-Roll.

### Acceptance Criteria
1. Bot erreicht mind. 1 Ending in 100% der Test-Runs (mit DLC)
2. Feasibility-Score sinnvoll
3. Force-Option funktioniert für alle 5 Endings
4. Switch-Trigger greift bei absichtlich zerstörtem Reaktor
5. AIPersonaCore-Beschaffung findet Quelle in 80% der Fälle
6. Journey-Quest wird zuverlässig erkannt und akzeptiert

### Stories
- Story 7.0: Ending-Sub-Phase-State-Machine (neu D-31, CC-STORIES-03)
- Story 7.1: EndingFeasibility-Engine
- Story 7.2: Feasibility-Daily-Recompute
- Story 7.3: Switch-Trigger-Logik
- Story 7.4: Force-Option
- Story 7.5: PHASE_SHIP Research-Chain
- Story 7.6: PHASE_SHIP Bauplatz-Planung
- Story 7.7: PHASE_SHIP AIPersonaCore-Beschaffung
- Story 7.8: PHASE_SHIP Finale-Belagerung
- Story 7.9: PHASE_JOURNEY Quest-Watcher
- Story 7.10: PHASE_JOURNEY Karawanen-Vorbereitung
- Story 7.11: PHASE_JOURNEY Weltkarten-Reise
- Story 7.12: PHASE_ROYAL Imperium-Honor-Farm
- Story 7.13: PHASE_ROYAL Stellarch-Belagerung
- Story 7.14: PHASE_ARCHONEXUS Wealth-Farm
- Story 7.15: PHASE_ARCHONEXUS Transition-Sequenz
- Story 7.16: PHASE_VOID Monolith-Aktivierung
- Story 7.17: PHASE_VOID Dark-Study-Grind
- Story 7.18: PHASE_VOID Void-Provocation-Ritual

### Dependencies
Epic 6.

### Launch-critical: yes (Release)

---

## Epic 8: Polish, Learning-System, Localization, DLC-Matrix

### Goal
Release-Qualität: Cross-Game-Lernsystem, Full-Localization, DLC-Matrix getestet, Telemetry + Debug-Panel, GitHub-Release.

### Acceptance Criteria
1. Lern-Datei wächst über 5 Runs, Gewichte bewegen sich messbar
2. Override in min. 1 Test als „besser" klassifiziert
3. Keine Hardcoded-Strings im Code
4. Alle 63 DLC-Kombinationen laden ohne Fehler
5. Debug-Panel zeigt korrekte Live-Daten
6. GitHub-Release mit `v1.0.0` ZIP produziert

### Stories
- Story 8.1: LearnedConfig-Struktur (XML)
- Story 8.2: Bayesian-Weight-Update
- Story 8.3: OverrideEvaluator
- Story 8.4: Override-Library
- Story 8.5: Reset-Button
- Story 8.6: Localization-Full-Coverage
- Story 8.7: Debug-Panel
- Story 8.8: Telemetry-Logger
- Story 8.9: DLC-Matrix-Testing
- Story 8.10: GitHub-Release-Pipeline

### Dependencies
Epics 1-7 (alle Features funktional).

### Launch-critical: yes (Release)

---

## Verweise

- Detaillierte Epic-Dokumente (Fallback-Sicht, nicht Autorität): `../../Epics/epic-0N-*.md`
- PRD: `prd.md`
- Architecture: `architecture.md`
- Runtime-Spec: `../../Mod-Leitfaden.md`
