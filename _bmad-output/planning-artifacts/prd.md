# Product Requirements Document — RimWorld Bot Mod

**Version:** 1.0 (PLANNING)
**Letztes Update:** 2026-04-24
**Status:** Approved (2026-04-24)

---

## 1. Executive Summary

Autonome Entscheidungs-KI als RimWorld-Mod, die Kolonien eigenständig vom nackten Start bis zum gewählten Ending führt. Per Toggle jederzeit aktivierbar/deaktivierbar, pro Pawn konfigurierbar, mit Cross-Game-Lernsystem. Vollständig Vanilla-kompatibel, respektiert alle Spiel-Regeln.

---

## 2. Problem Statement

RimWorld-Spieler investieren viel Zeit in repetitive Mikro-Management-Entscheidungen:
- Work-Priorities rebalancen
- Bau-Orders platzieren
- Raid-Abwehr taktisch
- Essensversorgung überwachen
- Medizin-Behandlungen koordinieren

Es existieren nur Assistenz-Mods (Common Sense, AllowTool, Better Pawn Control), die einzelne Entscheidungen erleichtern. Keine Mod bietet **vollständige autonome Spielführung** mit Ziel-gerichteter Strategie bis zum Ending.

---

## 3. Target Users

| Gruppe | Anteil | Motivation |
|---|---|---|
| Erfahrene RimWorld-Spieler | 60% | Runs beobachten, Ergebnisse optimieren, Strategien evaluieren |
| Anfänger / Lernende | 25% | Funktionierende Strategie beobachten, Gameplay-Mechaniken erlernen |
| Content-Creator | 15% | „AI plays RimWorld"-Streams/Videos für Twitch/YouTube |

---

## 4. Functional Requirements

### FR-01: Master-Toggle
**Als Spieler** will ich den Bot jederzeit per Top-Bar-Button oder Shortcut global an- oder ausschalten.

**Acceptance Criteria:**
- Button sichtbar in Top-Bar neben Geschwindigkeitssteuerung
- Keyboard-Shortcut (Default: Ctrl+K) aktiviert/deaktiviert
- State (AI_OFF / AI_ADVISORY / AI_ON) persistiert im Savegame via BotGameComponent
- Zustandswechsel sofort wirksam (< 1 Tick)
- Visueller Indikator zeigt aktuellen State

### FR-02: Per-Pawn-AI-Toggle
**Als Spieler** will ich pro Pawn entscheiden, ob die KI ihn steuert oder ich.

**Acceptance Criteria:**
- Jeder Pawn zeigt im Inspector (Info-Tab) eine Checkbox „Player Use"
- Default beim Pawn-Spawn: unchecked (Bot steuert)
- Master OFF → alle Pawns manuell unabhängig von Checkbox
- Master ON + Pawn.playerUse = true → Pawn bleibt manuell, Bot beobachtet nur
- Flag persistiert im Savegame

### FR-03: Map-Analyse bei Spielstart
**Als Bot** soll ich die generierte Karte scannen und bewerten, welche Positionen für eine Basis geeignet sind.

**Acceptance Criteria:**
- Scan läuft nach `Map.FinalizeInit`-Hook
- Bewertete Faktoren: fruchtbarer Boden, Wilde Beeren, Verteidigungs-Engstellen, Baumbestand, Stein-/Erz-Outcrops, Wassernähe, Kartenrand-Abstand, Hazards (Lava/Gift/Pollution)
- Output: Top-3-Regionen mit Score-Breakdown
- Hard-Filter: Lava-Tiles und Zellen mit hazardProximity<3 komplett ausgeschlossen
- Scan-Budget: max 500ms (auf Ticks verteilbar)

### FR-04: Overlay je nach Toggle-State
**Als Spieler** will ich je nach Toggle-State unterschiedlich informiert werden.

**Acceptance Criteria:**
- AI_OFF → keine UI-Störung, stumm
- AI_ADVISORY → bis zu 3 grüne Kreise mit Score-Labels auf Top-Sites
- AI_ON → Overlay mit aktueller Site-Wahl, Phase-Name, nächstem Goal
- Landepod-Spawn bleibt in allen States Vanilla-unbeeinflusst

### FR-05: Invariants-Überwachung
**Als Bot** soll ich jederzeit 12 Invarianten überwachen und bei Verstoß den passenden Emergency-Handler ausführen.

**Acceptance Criteria:**
- I1: Pawn-Shelter bei Nacht
- I2: Pawn-Essensverfügbarkeit
- I3: Bleeding-Pawn wird behandelt
- I4: Feuer im Home-Area wird gelöscht
- I5: Pawn-Körpertemperatur in Range
- I6: Raum-Temperatur (0°C-30°C)
- I7: Mindestens ein Doctor verfügbar
- I8: Kleidung oder Wärme
- I9: Hostile im Home-Area → Verteidigung
- I10: Food-Stock ≥ 3 Tage pro Pawn
- I11: Medicine-Stock ≥ 3 pro Pawn
- I12: Kein Pawn in extremem Mental Break
- Jede Invariant hat konkreten Checkpunkt und Pseudo-Code im Leitfaden

### FR-06: Phase-State-Machine
**Als Bot** soll ich linear durch 8 Phasen (0a Map → 0 Survival → 1 Shelter → 2 Research → 3 Winter → 4 Stone → 5 Power → 6 Industry → 7 Ending) laufen.

**Acceptance Criteria:**
- Exit-Conditions pro Phase als Boolean-Checkliste messbar
- Phase-Übergang nur wenn alle Exit-Conditions erfüllt
- Phase-Detection bei Mid-Game-Aktivierung heuristisch
- Phase-State persistiert im Savegame

### FR-07: Ending-Feasibility und Opportunistic Switch
**Als Bot** soll ich fortlaufend alle 5 möglichen Endings bewerten und das realistischste anstreben.

**Acceptance Criteria:**
- Feasibility-Score berechnet tägliche (in-game) aus: DLC-Status, Ressourcen, Pawn-Skills, Fraktion, Tech
- Primary-Ending = max-Feasibility
- Bot meidet Ending-exklusive Invests vor Phase 6
- Switch-Trigger bei Game-Breaking-Situations (Reaktor zerstört, Imperium feindlich, Monolith weg, AIPersonaCore unerreichbar)
- Force-Option in ModSettings (Dropdown)

### FR-08: Cross-Game-Lernsystem
**Als Mod** soll ich Erfahrungen zwischen Spielen persistieren, damit der Bot mit jedem Run besser wird.

**Acceptance Criteria:**
- Lern-Datei in `%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld\Config\RimWorldBot_LearnedConfig.xml`
- Bayesian-Update der Gewichte nach Emergencies (`new = 0.95*old + 0.05*observed`)
- Spieler-Override wird nur gespeichert, wenn nach 60s Outcome messbar besser als Bot-Alternative
- Reset-Button in ModSettings
- Out-of-Scope: neuronale Netze / Deep Learning

### FR-09: Nachträgliche Aktivierung
**Als Spieler** will ich die Mod in einem bestehenden Savegame aktivieren können.

**Acceptance Criteria:**
- Volle Map-Re-Analyse
- Basis-Layout-Audit: feuerfest? defensibel? ausreichend Betten? Kühlung? Strom? Medizin?
- Basis-Score ≥ 0.6 → übernehmen
- Basis-Score < 0.6 → Umbau-Plan im Status-Overlay vorschlagen, Bot handelt bei AI_ON
- Phase-Detection heuristisch aus Ist-Zustand

### FR-10: Work-Priority-Assignment
**Als Bot** soll ich Work-Priorities der Pawns anhand ihrer Skills/Passions rebalancen.

**Acceptance Criteria:**
- Default-Priority-Matrix aus Leitfaden §9
- Rebalance alle 2500 ticks oder bei Pawn-State-Wechsel
- Skill-Grinding-Strategie wenn Phase-Goal Skill-Cap erfordert
- Respektiert `pawn.playerUse` (keine Änderung an Player-Use-Pawns)

### FR-11: Kampf-Kommando
**Als Bot** soll ich bei Raids die Verteidigung koordinieren.

**Acceptance Criteria:**
- Raid-Strength vs Colony-Strength Ratio-Berechnung
- threat_ratio > 2.0 → FLEE-Option evaluieren (Karawane)
- threat_ratio > 1.0 → DEFEND mit Verlusten akzeptiert
- Sonst → normale Killbox-Taktik
- Post-Raid: Triage, Reparatur, Prisoner-Capture

### FR-12: Karawanen-Management
**Als Bot** soll ich Karawanen packen und führen (Handel, Journey-Offer, Fraktions-Quests).

**Acceptance Criteria:**
- Auto-Accept bei Journey-Offer-Quest
- Packtier-Auswahl basierend auf verfügbaren gezähmten Tieren
- Proviant-Berechnung: 60 Pemmikan × Reisetage × Pawn
- Weltkarten-Route automatisch
- Ambush-Ausweichen wenn threat_ratio zu hoch

### FR-13: ModSettings-Panel
**Als Spieler** will ich die Bot-Parameter konfigurieren können.

**Acceptance Criteria:**
- Auto-Activate beim Spielstart (Bool)
- Analyse-Overlay anzeigen (Bool)
- Takeover-Delay nach manuellem Eingriff (Int 0-60s)
- Scoring-Gewichte (Slider W_FOOD, W_DEFENSE, …)
- Preset-Buttons: Balanced / Defense-Priority / Resource-Priority
- Ending-Präferenz-Dropdown
- Mood/Food/Medicine Warn-Thresholds
- Log-Level
- Reset-Lern-Daten-Button

### FR-14: Localization
**Als Spieler** will ich die Mod in Deutsch oder Englisch nutzen.

**Acceptance Criteria:**
- `Languages/Deutsch/Keyed/*.xml`
- `Languages/English/Keyed/*.xml`
- Dropdown in ModSettings: System-Default oder explicit
- Alle UI-Texte lokalisiert (keine Hardcoded-Strings)

---

## 5. Non-Functional Requirements

### NFR-01: Performance
- Tick-Budget durchschnittlich < 5 ms, max 20 ms
- Map-Analyse max 500 ms (Coroutine-verteilt)
- Kein spürbarer FPS-Drop (≥95% der Vanilla-Framerate)

### NFR-02: Kompatibilität
- **RimWorld:** aktuelle stabile Version + letzte 2 Minor-Versions
- **DLCs:** Vanilla, Royalty, Ideology, Biotech, Anomaly, Odyssey — einzeln und in allen Kombinationen getestet
- **Andere Mods:** Harmony-Patches minimal, nicht-kollidierend mit populären Mods (Common Sense, AllowTool, Work Tab)
- **Load-Order:** nach allen Content-Mods

### NFR-03: Robustheit
- Kein Game-Crash durch Mod-Fehler
- Exceptions werden gefangen, als Log geschrieben, Bot fällt auf AI_OFF zurück
- Savegame ohne Mod weiter ladbar (falls Spieler Mod deinstalliert)
- BotGameComponent entkoppelt von Vanilla-Game-State

### NFR-04: Wartbarkeit
- C# mit XML-Documentation
- Harmony-Patches dokumentiert und minimal
- Unit-Tests für pure Logic (Scoring, Phase-Detection, Feasibility)
- Code-Convention: Microsoft C# Coding Guidelines

### NFR-05: Speicherverhalten
- Savegame-Größe darf um max 100 KB wachsen
- Lern-Datei wächst mit kontrollierter Rate, max 5 MB
- Keine Ressourcen-Leaks (Event-Subscriptions sauber entfernt bei Game-Ende)

### NFR-06: Observability
- Log-Levels: Silent / Warnings / Info / Verbose
- Debug-Panel mit Invariants, Goals, Resources, Path-Planning
- Telemetry als JSONL in `RimWorldBot_Telemetry.jsonl` (opt-in)

---

## 6. Success Criteria (MVP-DoD)

- [ ] Mod lädt ohne Fehler mit Vanilla + allen DLCs
- [ ] Master-Toggle-Button funktioniert
- [ ] Per-Pawn-Flag funktioniert
- [ ] Map-Analyse produziert Top-3-Sites
- [ ] Phase 0 + Phase 1 werden autonom abgeschlossen (Nackter Start → Holzhütte + Kleidung + Reisfeld + 10 Simple Meals Vorrat)
- [ ] Invariants I1-I5 aktiv und testbar
- [ ] Keine Game-Crashes in 100 Ticks Testlauf
- [ ] Savegame-Persistenz funktioniert

---

## 7. Out-of-Scope

- Machine Learning / Neural Networks (Scope: regelbasiertes Learning)
- Multiplayer-Support
- Custom-Scenario-Design
- Eigene DLCs / Content-Erweiterungen
- Storyteller-Modifikation (Vanilla bleibt)
- Steam Workshop Publish (Milestone 4+ Stretch)
- UI-Overhaul (bestehende RimWorld-UI wird genutzt)

---

## 8. Constraints

- **C-01:** RimWorld ist Single-Threaded für Mods, keine Thread-Safety-Annahmen
- **C-02:** Harmony ist abhängig von RimWorld-API-Stabilität; Updates können Patches brechen
- **C-03:** Bot folgt Vanilla-Regeln — kein Cheating, kein Ressourcen-Spawn, kein Time-Skip
- **C-04:** Keine Trennung Bot/Vanilla-Gameplay auf Save-Level (Mod muss entfernbar sein ohne Game-Korruption)
- **C-05:** Build-System: .NET Framework 4.7.2 (RimWorld-Standard)

---

## 9. Assumptions

- **A-01:** Spieler hat Core-RimWorld installiert
- **A-02:** Spieler versteht Grund-Gameplay (Tutorial durchlaufen)
- **A-03:** Map-Generation ist deterministisch per Seed
- **A-04:** XML-Defs sind zur Laufzeit via DefDatabase zugreifbar
- **A-05:** Harmony ist als Abhängigkeit verfügbar (RimWorld-Standard)

---

## 10. Risks

| ID | Risk | Impact | Likelihood | Mitigation |
|---|---|---|---|---|
| R-01 | RimWorld-Update bricht Harmony-Patches | Hoch | Mittel | Minimal-Patches, API-Version-Pinning, schnelle Update-Response |
| R-02 | Performance-Degradation in Late-Game | Mittel | Mittel | Tick-Budget-Monitoring, Profiling-Hooks, Coroutine-Verteilung |
| R-03 | User-Erwartungen überschätzt (echte AI vs regelbasiert) | Niedrig | Hoch | Klare Doku „was die Mod ist / nicht ist" |
| R-04 | Savegame-Corruption durch Bot-Fehler | Hoch | Niedrig | BotGameComponent strikt entkoppelt, Defensive ExposeData, automatische Savegame-Backups |
| R-05 | DLC-Inkompatibilität | Mittel | Mittel | DLC-Detection via ModsConfig, per-DLC-Test-Matrix |
| R-06 | Andere Mods kollidieren | Mittel | Hoch | Harmony-Priorität nutzen, keine Destructive-Patches, Transpiler nur wenn unvermeidbar |
| R-07 | Lern-Datei-Korruption | Niedrig | Niedrig | Strict-XML-Schema, Fallback auf Defaults bei Parse-Error |
| R-08 | AIPersonaCore nicht beschaffbar (Ship-Block) | Mittel | Mittel | Feasibility-Check, Ending-Switch, Force-Option als Override |

---

## 11. Glossary

- **Pawn** — Kolonist oder humanoides Wesen
- **Invariant** — Bedingung, die jederzeit wahr sein muss
- **Ending** — Eins von 5 Siegbedingungen: Ship, Journey, Royal, Archonexus, Void
- **Phase** — Diskreter Abschnitt im Bot-Workflow (Phase 0 - 7)
- **Emergency-Handler** — Funktion, die eine Invariant-Verletzung behebt
- **Feasibility-Score** — Berechnung, wie erreichbar ein Ending aktuell ist
- **Player-Use-Flag** — Per-Pawn-Checkbox; markiert Pawn als manuell gesteuert
- **Site-Score** — Bewertung einer Map-Zelle als Basis-Standort
- **Hazard** — Negativer Terrain-Faktor (Lava, Gift, Pollution, Strahlung)
- **Stuff** — stuff-skalierte Ressourcen in RimWorld (Holz/Stein/Metall je nach Wahl)
- **Def** — XML-Definition im RimWorld-Spiel (z. B. ThingDef, RecipeDef)

---

## 12. Acceptance / Sign-Off

**Reviewer:** Projekt-Eigentümer (User)
**Status:** Draft — Approval ausstehend

Vor Approval: Review offener Punkte, Scope-Confirm, NFR-Validierung.
Nach Approval: Story-Drafting für Epic 01-03 starten, Update sprint-status.yaml zu sub_phase: STORY_DRAFT.
