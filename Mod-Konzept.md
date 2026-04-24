# RimWorld Bot — Mod-Konzept

Architektur-Übersicht und Design-Entscheidungen für die autonome Siedlungs-KI.

---

## 1. Zielbestimmung

**Was die Mod ist:**
Eine autonome Entscheidungs-KI, die eine RimWorld-Kolonie eigenständig durch alle Phasen bis zum Ending führt.

**Was sie NICHT ist:**
- Kein Gameplay-Overhaul (keine neuen Items, Fraktionen, Karten)
- Keine Difficulty-Mod (rührt Vanilla-Balancing nicht an)
- Kein Auto-Clicker / Cheat (trifft Entscheidungen mit denselben Regeln, die ein Spieler hätte)

**Kernprinzip:** Der Bot ist ein **zusätzlicher Spieler**, der Pawn-Befehle, Work-Priorities, Bau-Orders und Karawanen steuert. Alles bleibt im Vanilla-Regelwerk.

---

## 2. User-Facing-Features

### 2.1 Toggle-Button (Hauptanforderung)

- **Position:** Oben rechts in der Top-Bar, neben Geschwindigkeitskontrolle
- **States:**
  - `AI_OFF` — Bot passiv, Spieler hat volle Kontrolle. Keine Eingriffe.
  - `AI_ON` — Bot aktiv, trifft alle Entscheidungen
  - `AI_ADVISORY` (optional, Stretch Goal) — Bot zeigt nur Empfehlungen, Spieler führt aus
- **Shortcut:** `Ctrl + K` (konfigurierbar in Keybindings)
- **Persistenz:** State wird im Savegame gespeichert; Bot nimmt Arbeit beim Reload an gleicher Stelle auf.

### 2.2 Map-Analysis bei Spielstart

Nach Karten-Generierung scannt der Bot die Karte und schlägt einen **Basis-Standort** vor (oder platziert ihn direkt, wenn AI_ON beim Start).

**Bewertete Faktoren:**
- Wilde Beeren-Population (Menge, Verteilung)
- Fruchtbarer Boden (soil_fertility ≥ 0.8) — Fläche und Zusammenhang
- Verteidigungs-Engstellen (Durchgänge ≤ 3 Felder breit, flankiert von Impassable)
- Berg-Kanten (Cliff-backed Base möglich)
- Wasser-Nähe (für Watermill später)
- Ressourcen-Outcrops (Stahl, Komponenten, Plasteel)
- Baum-Dichte (WoodLog-Verfügbarkeit)
- Durchschnitts-Temperatur und Wachstumsperiode

### 2.3 Status-Overlay

Wenn AI_ON:
- Kleine UI oben-mittig: aktuelle Phase, nächstes Goal, blockierende Skill-Gates
- Klickbar → Debug-Panel mit Invariants-Status, Ressourcen-Ziele, Pawn-Zuweisungen
- Color-Coding: Grün = alles OK, Gelb = Warning, Rot = Emergency

---

## 3. Technische Architektur

### 3.1 Mod-Typ
**Harmony-gepatchter C#-Mod**, weil:
- Vanilla-kompatibel, DLC-kompatibel (Royalty, Ideology, Biotech, Anomaly, Odyssey)
- Kann an Tick-Loop andocken ohne eigenes Main-Loop
- Kann mit Settings-Menü via `Mod`/`ModSettings`

### 3.2 Komponenten-Struktur

```
RimWorldBot.dll
├── Core/
│   ├── BotController.cs          (Master-Loop, Phase-State-Machine)
│   ├── Invariants.cs             (I1-I12 Checks)
│   ├── EmergencyHandler.cs       (P0 Emergencies)
│   └── PhaseDefinition.cs        (Phase-Goals, Exit-Conditions)
├── Analysis/
│   ├── MapAnalyzer.cs            (Scan-Routines, Site-Scoring)
│   ├── ThreatAssessment.cs       (Raid-Strength-Evaluation)
│   ├── ResourceTracker.cs        (Stock-Monitoring)
│   └── PawnAssessment.cs         (Skill-Evaluation, Best-for-Job)
├── Decision/
│   ├── BuildPlanner.cs           (Blueprint-Placement, Base-Layout)
│   ├── WorkAssigner.cs           (Work-Priority-Rebalance)
│   ├── CombatCommander.cs        (Draft, Kill-Box-Tactics)
│   └── CaravanManager.cs         (Journey-Offer, Trading)
├── UI/
│   ├── ToggleButton.cs           (Top-Bar-Gizmo)
│   ├── StatusOverlay.cs          (Phase-Display)
│   ├── DebugPanel.cs             (Invariants/Goals-Inspector)
│   └── SettingsWindow.cs         (Mod-Settings)
├── Data/
│   ├── BotGameComponent.cs       (Save-Data: current phase, flags)
│   └── Configuration.cs          (ModSettings: weights, thresholds)
└── RimWorldBotMod.cs             (Entry-Point, Harmony-Init)
```

### 3.3 Hook-Points (Harmony-Patches)

| Vanilla-Klasse | Methode | Patch-Typ | Zweck |
|---|---|---|---|
| `Game` | `UpdatePlay` | Postfix | Main-Tick für Bot-Loop |
| `MapInterface` | `Notify_SwitchedMap` | Postfix | Re-Init Analyse bei Map-Wechsel |
| `Map` | `FinalizeInit` | Postfix | Map-Analyse nach Generation |
| `Pawn` | `ExitMap` | Prefix | Pawn verlässt Karte → Karawanen-Logik |
| `WorkGiver` (diverse) | `PotentialWorkThingsGlobal` | Prefix | Eigene Work-Priorisierung |
| `IncidentWorker_RaidEnemy` | `TryExecuteWorker` | Postfix | Raid-Announce abfangen |
| `WindowStack` | `Add` | Postfix | Quest-Windows automatisch akzeptieren/ablehnen |

### 3.4 Tick-Budget

Performance ist kritisch. Zeitbudget pro Tick:

| Operation | Frequenz | Max ms/Tick |
|---|---|---|
| Invariant-Check | alle 60 ticks | < 1 |
| Emergency-Handler-Aufruf | event-driven | < 5 |
| Work-Priority-Rebalance | alle 2500 ticks | < 10 |
| Resource-Stock-Update | alle 2500 ticks | < 2 |
| Phase-Goal-Assignment | alle 1000 ticks | < 3 |
| Map-Analysis (full scan) | einmalig + bei Bedarf | < 500ms (gepuffert) |

Bei Budget-Überschreitung: Analyse auf mehrere Ticks verteilen (Coroutine-ähnlich).

---

## 4. Map-Analyzer — Detail-Design

### 4.1 Scan-Phasen

**Phase A — Global Scan (einmalig beim MapGeneration-Hook):**
Durchläuft jede Zelle der Karte genau einmal und schreibt ein Grid von `MapCellData`.

```csharp
struct MapCellData {
    IntVec3 position;
    float fertility;           // von TerrainDef
    bool isImpassable;         // von RoofType/Building/Rock
    bool isPassableByPawn;
    int wildPlantBerriesCount; // in 5-cell-Radius
    int wildHealrootCount;
    int treeCount;             // in 10-cell-Radius
    int stoneOutcrops;
    int oreDeposits;           // Steel/Component/Uranium
    float coverValue;          // aus Nachbar-Impassable
    bool isChokePointCandidate; // wenn in Durchgang ≤ 3 breit
    float waterProximity;      // Manhattan-Distanz zu nächstem Water-Tile
    
    // Hazard-Faktoren (negativ gewichtet)
    bool isLava;               // Odyssey: TerrainDef.defName contains "Lava"
    bool isPolluted;           // Biotech: PollutedDirt, PollutedGravel, PollutedWater
    bool isToxicSpawn;         // Anomaly / Tox-Fallout-prone terrain
    bool isRadioactive;        // falls vorhanden (Mods/DLCs)
    float hazardProximity;     // Min-Distanz zu nächstem Hazard-Tile
}
```

**Phase B — Scoring:**
Für jede Zelle Kolonie-Eignung berechnen:

```
siteScore(cell) =
  // POSITIV
    W_FOOD     * min(1.0, fertileArea_within_20_tiles / 100)
  + W_BERRIES  * min(1.0, berryCount_within_30_tiles / 20)
  + W_DEFENSE  * defensibilityScore(cell)
  + W_WOOD     * min(1.0, treeCount_within_15_tiles / 50)
  + W_STONE    * min(1.0, stoneOutcrops_within_20_tiles / 10)
  + W_ORE      * min(1.0, oreCount_within_40_tiles / 5)
  + W_WATER    * max(0, 1.0 - waterProximity / 30)
  
  // NEGATIV
  - W_THREAT   * threatProximity(cell)
  - W_HAZARD   * hazardScore(cell)
```

**Hazard-Score** (Gift/Lava/Strahlung/Pollution):
```
hazardScore(cell) =
    0.5 * lavaProximity(cell)        // 1.0 wenn Lava < 5 Felder, 0 wenn > 25
  + 0.3 * pollutedTileRatio_within_10 // Anteil polluted Tiles in 10er-Radius
  + 0.2 * toxicHazardCount_within_20  // Summe toxischer Spawns + Tox-Fallout-Quellen
```

Gewichte default (anpassbar in ModSettings):

```
// Positiv
W_FOOD    = 0.25
W_BERRIES = 0.10   // kurzfristig wichtig, langfristig weniger
W_DEFENSE = 0.25
W_WOOD    = 0.15
W_STONE   = 0.05
W_ORE     = 0.10
W_WATER   = 0.05

// Negativ
W_THREAT  = 0.15   // Distanz zu Karten-Rändern (Raid-Spawns)
W_HAZARD  = 0.30   // stark negativ: Lava, Gift, Pollution, Strahlung
```

**Hard-Filter:** Zellen mit `isLava == true` oder `isImpassable == true` werden **komplett aus der Site-Wahl ausgeschlossen**, nicht nur negativ gewichtet. Auf Lava kann man nicht bauen. Gleiches für `hazardProximity < 3` — zu nah an Lava / Giftsee kriegt Hard-Filter.

**Phase C — Site-Selection:**
- Top-50-Zellen nach Score
- Cluster-Analyse: nahe beieinanderliegende Punkte zu „Regions" gruppieren
- Pro Region: Bewertung des Zentrums als potenzieller Basis-Mittelpunkt
- Output: **Top 3 Site-Vorschläge** mit Score-Breakdown

### 4.2 Defensibility-Heuristik

```
defensibilityScore(cell) =
    0.4 * chokePointProximity(cell)   // Distanz zu Durchgang < 3 Felder breit
  + 0.3 * cliffBackedScore(cell)      // Rock/Mountain-Felder in 3er-Radius
  + 0.2 * openFieldRatio(cell)        // umgeliegende offene Felder für Schussfelder
  + 0.1 * distanceFromMapEdge(cell)   // Raid-Spawn-Abstand
```

### 4.3 Site-Decision bei AI_OFF-Start

Wenn Bot beim Start auf OFF steht: Ergebnis der Analyse als **Hint-Overlay** einblenden (grüne Kreise auf den Top-3-Sites), aber Spieler entscheidet selbst.

Wenn AI_ON: Bot platziert die erste Bau-Blueprint (Hütte oder CraftingSpot) automatisch am Top-1-Site und startet Phase 0.

---

## 5. Toggle-Mechanik — Detail-Design

### 5.1 State-Transitions

```
  Spieler klickt Button:
  AI_OFF ─────────────────▶ AI_ON
       ◀─────────────────
  
  Automatische Übergänge:
  AI_ON ─on_player_draft──▶ AI_PAUSED  (Spieler nimmt Kontrolle, Bot wartet)
  AI_PAUSED ─on_undraft──▶ AI_ON
  
  AI_ON ─on_emergency_end─▶ AI_ON  (nach E-Handler zurück zum Phase-Goal)
```

### 5.2 Soft-Takeover-Regeln (wenn Spieler eingreift)

Wenn Spieler Pawn draftet, eine Blueprint setzt, oder eine Work-Priority manuell ändert:
- Bot pausiert für diesen Pawn / dieses Bill für 10 Sekunden (Spielzeit)
- Nach 10 Sekunden Idle: Bot übernimmt wieder
- Logging: Spieler-Override wird aufgezeichnet → „User prefers manual X in situation Y"

Optional (Stretch): **Lern-Modus** — Overrides werden statistisch gesammelt und passen Gewichte an.

### 5.3 Beim Einschalten während laufendem Spiel

`transition(AI_OFF → AI_ON)` triggert:
1. Volle Map-Analyse (falls noch nicht gecached)
2. Current-State-Assessment: Welche Phase entspricht dem Bestand?
3. Goals re-queuen
4. Status-Overlay einblenden

Phase-Detection-Heuristik:
```
if no_campfire AND no_bed:
    phase = 0
elif no_closed_shelter:
    phase = 1
elif colonist_count < 2:
    phase = 2
elif no_stone_fortress:
    phase = 3  // Winterfest
elif no_electricity:
    phase = 4  // Stone Fortress
elif no_multianalyzer:
    phase = 5  // Electrification
elif no_ending_chosen:
    phase = 6  // Industrialization
else:
    phase = 7  // Ending-specific
```

---

## 6. ModSettings (Konfigurations-Panel)

Alle Werte vom User einstellbar:

### Allgemein
- **Bot aktivieren beim Spielstart?** Bool (Default: false — Spieler entscheidet)
- **Map-Analyse-Overlay anzeigen?** Bool (Default: true)
- **Auto-Takeover nach manuellem Eingriff (Sekunden)** Int 0-60 (Default: 10)

### Analyse-Gewichte
- Schieberegler für W_FOOD, W_DEFENSE, W_WOOD etc. (0.0-1.0)
- Preset-Buttons: "Balanced", "Defense-Priority", "Resource-Priority"

### Ending-Präferenz
- Dropdown: "Opportunistic" (Default), "Ship", "Journey", "Royal", "Archonexus", "Void"
- Bei nicht-Opportunistic: Bot ignoriert andere Ending-Opportunities

### Emergency-Sensitivität
- **Mood-Warn-Threshold** (Default: 0.35, Range 0.2-0.5)
- **Food-Days-Warning** (Default: 10, Range 5-30)
- **Medicine-Critical** (Default: 3/pawn, Range 1-10)

### Debug
- **Log-Level:** Silent / Warnings / Info / Verbose
- **Overlay einblenden**: Invariants, Goals, Resources, Path-Planning
- **Dry-Run-Modus**: Bot plant, führt aber keine Jobs aus (nur Logging)

---

## 7. Save-Game-Integration

`BotGameComponent : GameComponent` speichert:
- Aktueller Phase-Index
- Toggle-State (AI_ON/OFF)
- Gewählte Ending-Strategie
- Cached Map-Analysis-Result
- Pending Goals und deren Progress
- Letzte Emergency-Logs (letzte 50)

Bei Savegame-Load: `BotGameComponent.ExposeData()` stellt den Zustand wieder her; Bot prüft, ob Phase-Goals noch gültig sind, sonst Re-Assessment.

---

## 8. Kompatibilitäts-Strategie

### 8.1 DLC-Detection
```csharp
ModsConfig.RoyaltyActive
ModsConfig.IdeologyActive
ModsConfig.BiotechActive
ModsConfig.AnomalyActive
ModsConfig.OdysseyActive
```

Jede Ending-Phase wird nur erlaubt, wenn das entsprechende DLC aktiv ist. Phase-Auswahl respektiert Available-Endings-Liste.

### 8.2 Andere Mods

- **Skill-Cap-ändernde Mods** (z. B. Vanilla Expanded Serie): Werte aus XML-Defs zur Laufzeit lesen (`DefDatabase<ThingDef>`), nicht hart-kodieren.
- **UI-Mods**: Toggle-Button via `RimWorld.MainButtonDef` registrieren, nicht statische Koordinaten nutzen.
- **Work-Tab-Mods**: Via `WorkTypeDef.priorityInType` interagieren, nicht direkt eigene Priorities setzen.

### 8.3 Load-Order

Am Ende der Load-Order laden (nach allen Content-Mods), damit der Bot alle DefDatabase-Einträge sehen kann.

---

## 9. Entwicklungs-Roadmap (Vorschlag)

### MVP (Milestone 1)
- [ ] Mod-Skeleton mit Harmony-Init
- [ ] ToggleButton in Top-Bar
- [ ] BotGameComponent (Save/Load)
- [ ] Map-Analyzer: Phase A (Global Scan)
- [ ] Invariant-Checks I1-I5 (Fire, Bleed, Food, Shelter, Temp)
- [ ] Emergency-Handler E-FIRE, E-BLEED, E-FOOD
- [ ] Phase 0 + Phase 1 ausführbar (bis Holzhütte + Kleidung)

### Alpha (Milestone 2)
- [ ] Phase 2-4 (Research, Winter, Stone Fortress)
- [ ] Work-Assigner (Skill-basierte Zuweisung)
- [ ] Build-Planner (Basis-Layout automatisch)
- [ ] Site-Scoring + Site-Selection UI-Overlay
- [ ] Raid-Handler, Combat-Commander
- [ ] ModSettings-Panel

### Beta (Milestone 3)
- [ ] Phase 5-6 (Electrification, Industrialization)
- [ ] Alle Invariants I1-I12
- [ ] Alle Emergency-Handler
- [ ] Caravan-Manager (Trading, Journey)
- [ ] Status-Overlay
- [ ] Soft-Takeover-Logik

### Release (Milestone 4)
- [ ] Alle 5 Ending-Phases funktional
- [ ] DLC-Kompatibilität durchgetestet (Vanilla + alle 5 DLCs)
- [ ] Telemetry/Debug-System
- [ ] Preset-Konfigurationen
- [ ] Dokumentation + Tutorial-Video

---

## 10. Design-Entscheidungen (finalisiert)

### 10.1 Toggle-Scope — zwei Ebenen
- **Master-Toggle (global):** „AI für alle Pawns an/aus". Button in Top-Bar. Shortcut `Ctrl+K`.
- **Per-Pawn-Toggle:** Im Pawn-Inspector (Info-Tab oder als Gizmo) eigene AI-On/Off-Checkbox pro Pawn.
- **Regel-Hierarchie:** Master OFF überschreibt alle individuellen ON. Master ON respektiert individuelle OFF-Flags.
- **Pawns mit individuellem OFF** = „Player Use"-Pawns. Bot steuert sie nicht, aber bezieht sie in Kolonie-Metriken ein (z. B. Food-Stock-Berechnung zählt sie).

### 10.2 Start-Pawn-Auswahl
- Im Character-Creation-Screen (Scenario-Pawn-Edit) gibt es pro Pawn eine Checkbox **„Player Use"** (Default: unchecked).
- Alle **nicht markierten** Pawns fallen unter Bot-Kontrolle ab Spielstart.
- Alle **markierten** bleiben manuell.
- Bot beeinflusst die Pawn-Generierung NICHT — kein Cherry-Picking für Skills/Passions beim Start.

### 10.3 Site-Selection & Landepod-Verhalten
- **Landepods bleiben Vanilla-Verhalten:** Der Bot beeinflusst den Spawn-Punkt NICHT. Wo die Pods laut Spielauswahl landen, landen sie. Analyse läuft unabhängig davon.
- **Output je nach Toggle-State:**

| State | Verhalten |
|---|---|
| `AI_OFF` | **Stumm.** Analyse läuft im Hintergrund und cached das Ergebnis, zeigt aber nichts. Spieler hat volle Kontrolle ohne UI-Störung. |
| `AI_ADVISORY` | **Top-3-Markierung.** Auf der Karte werden bis zu 3 grüne Kreise mit Score-Labels eingeblendet. Bot führt nicht aus, Spieler entscheidet selbst. |
| `AI_ON` | Bot nutzt Top-1-Site als Basis-Mittelpunkt und platziert Blueprints. Overlay sichtbar mit aktueller Site-Wahl. |

- Wechsel zwischen States wendet das passende Overlay-Verhalten sofort an.

### 10.4 Lern-System (Cross-Game-Persistenz)
**Ziel:** Bot wird mit jedem Spiel besser, nicht jeder Run beginnt bei Null.

- **Speicherort:** `%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld\Config\RimWorldBot_LearnedConfig.xml` (außerhalb des Savegames)
- **Daten-Schema:**
  - Gewichte (W_FOOD, W_DEFENSE, ...) — initial Defaults, werden über Runs angepasst
  - Schwellenwerte (food_days_warning, mood_critical, ...) — angepasst nach Vorkommen von Emergencies
  - Override-Bibliothek: Liste von `{situation_hash → preferred_action}` Einträgen, die aus Spieler-Overrides **mit besserem Outcome als Bot-Plan** gelernt wurden
- **Lern-Trigger:**
  - Nach jeder Emergency: Outcome messen (Pawn tot? Ressourcen verloren? Mood?) → relevante Gewichte nachjustieren (Bayesian-Update mit kleiner Rate, z. B. `new = 0.95 * old + 0.05 * observed`)
  - Nach Spieler-Override: Bot berechnet nach 60s Spielzeit Outcome der Spieler-Aktion und vergleicht mit simulierter Bot-Alternative
    - Wenn Spieler-Outcome **messbar besser**: Override als Regel ablegen
    - Wenn Spieler-Outcome **gleich oder schlechter**: verwerfen
    - Wenn nicht messbar: verwerfen (kein Guessing)
  - Nach Run-Ende (Ending erreicht oder Wipe): Zusammenfassender Update über gesamten Run
- **Reset-Option in ModSettings:** Button „Lern-Daten zurücksetzen" für Debugging.
- **Out-of-Scope (Stretch Goal):** Neuronale Netze / ML-Modelle. Bleibt bei regelbasiertem Learning mit statistischer Anpassung.

### 10.5 Ending-Strategie — opportunistisch mit Force-Option
- **Fortlaufende Feasibility-Bewertung** aller Endings:
  ```
  feasibility(ending) = 
      dlc_available(ending) * weight(1.0)
    + resource_readiness(ending) * weight(0.3)
    + pawn_skill_readiness(ending) * weight(0.3)
    + faction_standing(ending) * weight(0.2)
    + tech_progress(ending) * weight(0.2)
  ```
- **Primary-Ending:** Max-Feasibility-Ending. Tägliche Re-Evaluation.
- **„Alle Endings offenhalten":** Bot meidet Ending-exklusive Investitionen vor Phase 6. Bis dahin nur Basis-Infrastruktur (nutzbar für alle 5 Pfade).
- **Switch-Trigger** („Game-Breaking Situations"):
  - Schiff-Reaktor >50% zerstört → prüfe Journey als Alternative
  - Archonexus-Quest verloren oder nicht erscheinend → Ship
  - Imperium → Hostile (nicht reparierbar) → Royal raus
  - Monolith zerstört oder nie gespawnt → Void raus
  - AIPersonaCore unerreichbar nach N Jahren → Ship nach hinten priorisieren
- **Force-Option in ModSettings:** Dropdown „Ending-Strategie" mit Werten:
  - `Opportunistic` (Default) — Bot wählt und wechselt dynamisch
  - `Force_Ship` — ignoriert Feasibility, baut stur Schiff
  - `Force_Journey` — nimmt nur Journey-Quests
  - `Force_Royal` — forciert Imperium-Aufstieg
  - `Force_Archonexus` — forciert Wealth-Stufen
  - `Force_Void` — forciert Anomaly-Pfad

### 10.6 Nachträgliche Aktivierung (Mod auf bestehendem Savegame)
- **Volle Re-Analyse:**
  - Map-Scan komplett neu (wie Phase 0a)
  - Basis-Layout-Bewertung: feuerfest? defensibel? ausreichend Betten? Kühlung? Stromversorgung redundant? Medizin-Vorrat?
  - Pawn-Skills-Audit: wer ist best-fit für welchen Job?
  - Ressourcen-Inventar
- **Basis-Übernahme-Entscheidung:**
  - Layout-Score ≥ 0.6 → Basis übernehmen, ab jetzt ausbauen
  - Layout-Score < 0.6 → im Status-Overlay Umbau-/Umzug-Plan vorschlagen, Spieler entscheidet bei AI_OFF, Bot handelt bei AI_ON
- **Phase-Detection wie in §13 Leitfaden** (Heuristik über Ist-Zustand).

### 10.7 Localization
- **Zwei Basis-Sprachen:** Deutsch + Englisch
- Struktur: `Languages/Deutsch/Keyed/*.xml`, `Languages/English/Keyed/*.xml`
- Key-Referenzen im C#-Code über `"KeyName".Translate()`
- In ModSettings: Sprachwahl-Dropdown (respektiert RimWorld-Systemsprache als Default)

### 10.8 Distribution
- **Primär: GitHub** — Public Repo mit Release-Tags
- Repo-Struktur:
  ```
  RimWorldBot/
  ├── About/About.xml
  ├── Assemblies/RimWorldBot.dll
  ├── Defs/
  ├── Languages/
  │   ├── Deutsch/Keyed/
  │   └── English/Keyed/
  ├── Source/                (C#-Code)
  ├── README.md
  ├── CHANGELOG.md
  └── LICENSE
  ```
- README: Install-Anleitung, Feature-Liste, Screenshots, Konfig-Doku
- **Stretch Goal:** Steam Workshop Publish (Publish-Script im Repo, aber nicht MVP)
