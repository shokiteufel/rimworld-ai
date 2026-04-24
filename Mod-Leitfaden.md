# RimWorld Bot — Leitfaden für autonome Siedlungsführung

Struktur: **Invariants → Emergency-Handler → Phasen-State-Machine → Ressourcen-Ziele → Entscheidungsbäume**.

Alle Regeln sind **priorisiert**. Höhere Priorität unterbricht niedrigere. Die Mod arbeitet die Liste von oben nach unten ab.

---

# 1. Invariants — dürfen niemals verletzt werden

Jede Invariant ist ein Boolean-Check. Wenn `false` → entsprechende Emergency-Rule ausführen, bis Invariant wieder `true`.

| ID | Invariant | Prüfintervall | Bei Verletzung |
|---|---|---|---|
| **I1** | `∀ Pawn: pawn.isLying == false OR pawn.hasBed == true` | 250 ticks | E-SHELTER |
| **I2** | `∀ Pawn: pawn.hungerLevel > 0.1 OR food_reachable(pawn)` | 250 ticks | E-FOOD |
| **I3** | `∀ Pawn: pawn.isBleeding == false OR tending_scheduled(pawn)` | 60 ticks | E-BLEED |
| **I4** | `∀ HomeArea Tile: tile.isOnFire == false` | 60 ticks | E-FIRE |
| **I5** | `∀ Pawn: pawn.bodyTemp ∈ [−5°C, +40°C]` | 250 ticks | E-TEMP |
| **I6** | `∀ Room.owned: room.temperature ∈ [0°C, 30°C]` | 1000 ticks | E-ROOMTEMP |
| **I7** | `∃ Pawn: pawn.canDoctor == true` | 2500 ticks | E-NODOCTOR |
| **I8** | `∀ Pawn: pawn.apparel.bodyCover > 0.5 OR room.temperature > 15°C` | 2500 ticks | E-NAKED |
| **I9** | `∀ hostilePawn.inHomeArea: defended == true` | 60 ticks | E-INTRUSION |
| **I10** | `food_stock_days ≥ 3 * colonist_count` | 2500 ticks | E-FOODLOW |
| **I11** | `medicine_stock ≥ 3 * colonist_count` | 5000 ticks | E-MEDLOW |
| **I12** | `∃ Pawn: pawn.mood > 0.35` (keiner in Extreme Break) | 500 ticks | E-MOOD |

---

# 2. Emergency Handlers (Priorität P0)

> **⚠ Update 2026-04-24 (siehe `_bmad/decisions.md` D-16, `_bmad-output/planning-artifacts/architecture.md` §2.1):** Die folgende Reihenfolge ist seit Architecture v2.0 **`base_prio`** für `EmergencyHandler.Score(state) → double`, NICHT mehr strikte fixe Prio. Der `EmergencyResolver` addiert `context_modifiers` (z. B. „+100 wenn E-BLEED-Pawn unter aktiver Intrusion unreachable") und wählt die höchste-scorende Emergency per Utility-Maximization. **Implementation-Regel:** nicht als Switch-Statement oder if-else-Kette umsetzen — Utility-Score-basiert iterieren. Die Zahlen in der Prio-Spalte sind `base_prio`-Werte, nicht hartcodierte Reihenfolge.

Unterbrechen **alle** niedriger priorisierten Jobs. Nur eine Emergency gleichzeitig, `base_prio`-Werte:

```
Prio 1: E-FIRE      (Feuer breitet sich aus, kann alle anderen Invariants auslösen)
Prio 2: E-BLEED     (Pawn verblutet in ~30 Sekunden Spielzeit)
Prio 3: E-INTRUSION (Feind betritt Home Area)
Prio 4: E-TEMP      (Pawn erfriert/überhitzt)
Prio 5: E-FOOD      (Pawn ist akut am Verhungern)
Prio 6: E-SHELTER   (Pawn schläft draußen während Sturm)
Prio 7: E-MOOD      (Mental Break imminent)
Prio 8: E-ROOMTEMP  (Raum zu kalt/warm — nicht lebensbedrohlich, aber verschlechtert I5)
Prio 9: E-FOODLOW   (Vorrat reicht nicht mehr lange)
Prio 10: E-MEDLOW   (Medizin knapp)
Prio 11: E-NODOCTOR (keiner kann doktern)
Prio 12: E-NAKED    (Pawn läuft nackt)
```

## Handler-Pseudocode

### E-FIRE
```
for each burning_tile in home_area:
    assign nearest_idle_pawn to ExtinguishJob
if no firefoam_popper_available AND fire.intensity > threshold:
    draft all pawns, manual firefighting
```

### E-BLEED
```
for each bleeding_pawn sorted by blood_loss_rate DESC:
    if doctor_available:
        assign doctor to TreatJob(bleeding_pawn, use_best_medicine_available)
    else:
        assign nearest_pawn to RescueJob(bleeding_pawn → medical_bed)
```

### E-FOOD
```
if food_stock == 0:
    allow_raw_eating = true
    for each pawn: if pawn.hungerLevel < 0.05: assign to ForageBerries OR HuntSmallAnimal
if existing_meal in stockpile: 
    allow eating
```

### E-INTRUSION
```
1. Draft all combat-capable pawns
2. Route them to kill_box_position (pre-defined)
3. If kill_box_unbuilt: retreat inside building, block door
4. If raid_strength > colony_strength * 1.5: consider flight caravan
```

### E-SHELTER
```
if no_completed_house:
    build priority = MAX
    recipe: SimpleHut (6 wood walls + wood door + campfire)
else:
    assign sleep zones indoors
```

### E-MOOD (Pawn Mental Break imminent)
```
if pawn.mood < 0.35:
    isolate pawn from stressors
    trigger RecreationJob (joy-building, beer, meal)
    if < 0.15: restrain (prevent running amok)
```

---

# 3. Phasen-State-Machine

Die Siedlung durchläuft lineare Phasen. Übergang NUR wenn alle **Exit Conditions** erfüllt sind.

## Phase 0a — Map Analysis (pre-colony)

**Entry:** Map wurde generiert, Start-Pawn(s) noch nicht platziert ODER gerade gelandet.

**Goals:**
1. **Globaler Scan** jeder Zelle der Karte → `MapCellData`-Grid aufbauen (siehe Mod-Konzept §4.1)
2. **Scoring** pro Zelle mit Gewichten W_FOOD, W_BERRIES, W_DEFENSE, W_WOOD, W_STONE, W_ORE, W_WATER, W_THREAT
3. **Cluster-Analyse**: Top-50-Zellen zu Regionen gruppieren
4. **Site-Selection**: Top-3 Regionen als Basis-Kandidaten rausgeben
5. Pro Kandidat Score-Breakdown loggen

**Exit Conditions:**
- `map_analysis_complete == true`
- `top_3_sites != null`
- Wenn `ai_toggle_state == AI_ON`: `chosen_site = top_3_sites[0]`, Blueprint für erste Struktur am gewählten Ort platziert
- Wenn `ai_toggle_state == AI_OFF`: Overlay mit Top-3-Markierungen anzeigen, auf User-Decision warten

**Scoring-Formel:**
```
siteScore(cell) =
    W_FOOD     * min(1.0, fertileArea_within_20 / 100)
  + W_BERRIES  * min(1.0, berryCount_within_30 / 20)
  + W_DEFENSE  * defensibilityScore(cell)
  + W_WOOD     * min(1.0, treeCount_within_15 / 50)
  + W_STONE    * min(1.0, stoneOutcrops_within_20 / 10)
  + W_ORE      * min(1.0, oreCount_within_40 / 5)
  + W_WATER    * max(0, 1.0 - waterProximity / 30)
  - W_THREAT   * proximityToMapEdge(cell)
  - W_HAZARD   * hazardScore(cell)
```

**Hazard-Score** (Gift/Lava/Pollution/Strahlung):
```
hazardScore(cell) =
    0.5 * lavaProximity(cell)           // 1.0 wenn Lava < 5 Felder, 0 wenn > 25
  + 0.3 * pollutedTileRatio_within_10   // Biotech PollutedDirt/Gravel/Water
  + 0.2 * toxicHazardCount_within_20    // Anomaly-Spawns + Tox-Sources
```

**Hard-Filter** (Zelle wird komplett ausgeschlossen):
- `terrain.defName contains "Lava"` → nicht bebaubar
- `isImpassable == true` → nicht bebaubar
- `hazardProximity < 3` → zu nah an Lava/Gift

Default-Gewichte (konfigurierbar):
```
W_FOOD=0.25, W_BERRIES=0.10, W_DEFENSE=0.25, W_WOOD=0.15,
W_STONE=0.05, W_ORE=0.10, W_WATER=0.05, W_THREAT=0.15, W_HAZARD=0.30
```

**Verteidigungs-Score:**
```
defensibilityScore(cell) =
    0.4 * chokePointProximity(cell)
  + 0.3 * cliffBackedScore(cell)
  + 0.2 * openFieldRatio(cell)
  + 0.1 * distanceFromMapEdge(cell)
```

**Nach Phase 0a:**
- Analyse-Resultat in `BotGameComponent` cachen (für Savegame-Kompatibilität)
- Re-Analyse nur bei Map-Wechsel oder expliziter User-Anforderung

## Phase 0 — Initial Survival
**Entry:** Spielstart, nackter Pawn.

**Goals (in order):**
1. Forage 20 Beeren (als **Rohmaterial**, nicht zum direkten Verzehr)
2. Baue CraftingSpot (0 Ressourcen)
3. Harveste 50 WoodLog (mit Händen wenn nötig)
4. Baue Campfire (20 WoodLog) — **priorisiert vor Club**, weil ohne gekochte Nahrung Effizienzverlust einsetzt
5. Sobald Campfire fertig: **Bill `Cook Simple Meal` dauerhaft** queuen. Sammel-Ziel: mind. 5 Simple Meals auf Lager bevor Phase 1
6. Baue Club am CraftingSpot (30 WoodLog, kein Skill-Cap)
7. Baue SleepingSpot

**Nahrungs-Regel ab Campfire-Build:**
- `allow_raw_eating = false` global setzen, außer bei kritischer E-FOOD-Emergency
- Beeren/Fleisch/Pflanzen nur als **Cook-Input**, nicht direkt konsumieren
- Simple Meal hat 1.8× Hunger-Effizienz gegenüber Raw (0.5 nutrition Input → 0.9 nutrition Output)
- Vermeidet −7 Mood "ate raw food" + ~2% Food Poisoning pro Raw-Meal

**Exit Conditions:**
- `has_campfire == true`
- `has_weapon(primary_pawn) == true`
- `cooked_meal_stockpile ≥ 5 * colonist_count` (**NEU — statt nur `food_stock_days ≥ 1`**)
- `sleeping_spot_exists == true`
- `allow_raw_eating == false`

## Phase 1 — Shelter & Clothing
**Goals:**
1. Jage Kleintiere mit Club → Leder sammeln (Ziel: 60 Leder)
2. Craft Tribal Wear (60 Leather) + Tuque (20 Leather) am CraftingSpot
3. Baue HolzHütte:
   - 3×4 Wall (Construction 0, 5 WoodLog/Feld)
   - 1× Door (25 WoodLog)
   - Campfire innen (gleichzeitig Heat-Source + Kochstelle)
4. Baue Holzbett (Construction 0, 45 WoodLog)
5. Baue SimpleResearchBench (75 WoodLog)
6. Baue Anbauzone: Rice 6×6, Potato 4×4, Cotton 4×4
7. **Cooking-Bill dauerhaft aktiv**: `Cook Simple Meal until you have X` mit X = 10 × colonist_count. Kein Raw-Eating zulassen.

**Exit Conditions:**
- `pawn.apparel.hasTribalWear == true`
- `closed_room_with_door == true`
- `bed_count ≥ colonist_count`
- `research_bench_exists == true`
- `growing_zone_area ≥ 60`
- `cooked_meal_stockpile ≥ 10 * colonist_count`

## Phase 2 — Food Security & Research Start
**Goals:**
1. Forschung: `Pemmican` (Priorität 1)
2. Forschung: `PassiveCooler` (gegen 32°C Sommer)
3. Forschung: `ComplexFurniture` (bessere Mood-Möbel)
4. Produziere Pemmikan-Vorrat: Ziel `food_stock_days ≥ 60 * colonist_count`
5. Erste Rekrutierung:
   - Wanderer/Flüchtling-Quests: IMMER annehmen
   - Schwache Raider (< 3 Gegner): downen, nicht töten → gefangen nehmen
   - Gefangenenzelle bauen: 3×3 Raum, Gefangenenbett, Tür

**Exit Conditions:**
- `pemmican_researched == true`
- `food_stock_days ≥ 60 * colonist_count`
- `colonist_count ≥ 2`

## Phase 3 — Winter Readiness
**Goals:**
1. Parka + Tuque + Stiefel für alle Pawns
2. Winter-Essensvorrat: `pemmican + potato ≥ 90 * colonist_count` Tage
3. Berg-Lagerraum oder PassiveCooler für Sommer-Kühlung
4. Forschung: `Smithing → Stonecutting`
5. Skill-Grind: Crafting-Skill des dedizierten Crafters auf **2** (Short Bow freischalten)

**Exit Conditions:**
- `winter_clothing_complete == true`
- `food_stock_days ≥ 90 * colonist_count`
- `stonecutting_researched == true`
- `∃ pawn: pawn.Crafting ≥ 2`

## Phase 4 — Stone Fortress
**Goals:**
1. Basis mit **Steinmauern** umgeben (Wall, Construction 0 aber Stein feuerfest)
2. Steinböden innen (Construction 3 nötig — Grind Construction-Pawn hoch)
3. Skill-Grind: Construction auf **3** (TrapSpike + Steinböden)
4. **Killpoint** definieren: Engstelle + Holzspieße davor + Schussfeld
5. Forschung: `ComplexClothing → Electricity`
6. Erweiterung auf 3–4 Pawns

**Exit Conditions:**
- `home_area.fireproof_percent ≥ 0.8`
- `traps_at_killpoint ≥ 5`
- `electricity_researched == true`
- `colonist_count ≥ 3`
- `∃ pawn: pawn.Construction ≥ 3`

## Phase 5 — Electrification
**Goals:**
1. WoodFiredGenerator (Construction 4) — eine Einheit Basis-Strom
2. Zusätzlich WindTurbine (Construction 4) + Battery
3. ElectricStove ersetzt Campfire als Kochstelle
4. ElectricSmithy + TableMachining
5. Forschung: `MicroelectronicsBasics → Machining → Gunsmithing`
6. Heater (Construction 5) in allen Schlafräumen
7. Cooler (Construction 5) im Lagerraum

**Exit Conditions:**
- `power_grid_stable == true` (Batteries > 0 im Worst Case)
- `has_electric_stove == true`
- `has_heater_in_bedroom == true`
- `microelectronics_researched == true`

## Phase 6 — Industrialization
**Goals:**
1. HiTechResearchBench (Construction 6) + MultiAnalyzer (Construction 8)
2. Turret_MiniTurret an allen Killpoints (Construction 5)
3. Flak-Rüstung für alle Kämpfer (Crafting 4)
4. Pawns in dedizierten Spezialisten-Rollen:
   - Crafter → Skill 6+ priorisiert
   - Constructor → Skill 6+ priorisiert
   - Doctor → Skill 8+ priorisiert
   - Grower → Skill 8 (für Heilwurz)
5. Components selbst herstellen (wenn Crafter Skill 8 erreicht)

**Exit Conditions:**
- `has_hitech_research == true`
- `has_multianalyzer == true`
- `turret_count ≥ 3`
- `flak_armor_complete == true`
- `∃ pawn: pawn.Crafting ≥ 8`
- `∃ pawn: pawn.Construction ≥ 6`

## Phase 7 — Ending-Zielwahl
Entscheidet, welches Ending angestrebt wird basierend auf:

```
if journey_offer_quest_active:
    → goto PHASE_JOURNEY
elif colonist_count ≥ 4 AND wealth > 100000 AND no_blocked_skills:
    → goto PHASE_SHIP
elif royalty_dlc_active AND empire_allied:
    → goto PHASE_ROYAL
elif ideology_dlc_active AND wealth > 200000:
    → goto PHASE_ARCHONEXUS
elif anomaly_dlc_active AND monolith_on_map:
    → goto PHASE_VOID
else:
    wait / continue Phase 6 consolidation
```

Ending-spezifische Phasen (siehe Abschnitt 7).

---

# 4. Monitoring-Sensoren (Tick-basiert)

| Sensor | Update-Intervall | Trigger |
|---|---|---|
| `pawn.needs.*` | 60 ticks | Invariant-Check |
| `pawn.health.hediffs` | 60 ticks | I3, I7, I8 |
| `food_stock_days` | 2500 ticks | I10 |
| `medicine_count` | 2500 ticks | I11 |
| `room.temperature` | 1000 ticks | I6 |
| `wealth_total` | 15000 ticks | Phase-Transition, Raid-Scaling |
| `raid_announced` | event-driven | E-INTRUSION |
| `weather.temperature` | 250 ticks | Cold/Heat wave detection |
| `pawn.currentJob` | 60 ticks | Priority-Rebalance |

## Berechnungen

```
food_stock_days = total_nutrition_in_stockpile / (colonist_count * 1.6 nutrition/day)

colony_strength = Σ (pawn.combat_power * pawn.health_fraction * weapon_dps_modifier)

raid_strength = raid.combat_points

wealth_total = Σ (item.market_value) + colony.buildings_value + colony.pawns_value
```

---

# 5. Ressourcen-Ziele (pro Phase)

| Ressource | Phase 0 | Phase 1 | Phase 3 | Phase 5 | Phase 7 |
|---|---|---|---|---|---|
| **WoodLog** | ≥50 | ≥200 | ≥500 | ≥1.000 | ≥500 |
| **Steel** | — | ≥100 | ≥300 | ≥1.000 | ≥2.000 |
| **Stone Blocks** | — | — | ≥500 | ≥2.000 | ≥1.000 |
| **Components** | — | — | — | ≥20 | ≥100 |
| **Medicine (Herbal)** | ≥3/pawn | ≥5/pawn | ≥10/pawn | ≥15/pawn | ≥20/pawn |
| **Medicine (Industrial)** | — | — | — | ≥10/pawn | ≥30/pawn |
| **Pemmikan** | — | ≥30/pawn | ≥60/pawn | ≥90/pawn | ≥120/pawn |
| **Parka + Tuque** | — | 1/pawn | 1/pawn + Spare | 1/pawn + Spare | — |
| **Flak-Rüstung** | — | — | — | 1/Kämpfer | 1/Kämpfer + Spare |
| **Plasteel** | — | — | — | — | ≥500 |
| **Uranium** | — | — | — | — | ≥150 |
| **AdvancedComponents** | — | — | — | — | ≥30 |

**Puffer-Regel:** Jede Ressource auf 1.5× des Phasen-Ziels halten, damit Raids + Katastrophen den Vorrat nicht unter kritische Schwelle drücken.

---

# 6. Skill-Gates (Hard-Caps aus den Game-Files)

Diese Werte **blockieren** Aktionen. Die Mod darf entsprechende Jobs nicht queuen, wenn der Skill-Cap nicht erreicht ist.

## Construction

| Aktion | Min Skill |
|---|---|
| Wall, Door, Bed, Campfire, SleepingSpot | 0 |
| WoodPlankFloor, StoneTile | 3 |
| TrapSpike, TrapIED | 3 |
| FueledSmithy, TableMachining, DrugLab, WoodFiredGenerator, WindTurbine, ElectricStove, HydroponicsBasin, ElectricSmelter, BiofuelRefinery, DeepDrill | 4 |
| ElectricSmithy, Heater, Cooler, MiniTurret, Mortar, OrnateDoor, NutrientPasteDispenser, CommsConsole | 5 |
| FabricationBench, HiTechResearchBench, Autodoor, SolarGenerator, WatermillGenerator, ChemfuelPoweredGenerator, MoisturePump, PodLauncher, AutocannonTurret, UraniumSlugTurret, SterileTile | 6 |
| TubeTelevision | 7 |
| **Alle Schiffsteile (Reactor, Engine, Sensor, Core, Cryptosleep, außer Beam)** | **8** |
| GeothermalGenerator, MultiAnalyzer, VitalsMonitor, GroundPenetratingScanner, LongRangeMineralScanner, CryptosleepCasket, RoyalBed, HospitalBed, FlatscreenTelevision | 8 |

## Crafting

| Aktion | Min Skill |
|---|---|
| Club, Tribal Wear, Parka, Tuque, Cape, Duster, Pants, BasicShirt | 0 |
| Short Bow | 2 |
| Pila, Revolver, Gladius, Mace, Harp | 3 |
| Autopistol, Machine Pistol, Pump Shotgun, Bolt-Action Rifle, Flak Vest/Pants/Jacket, Incendiary/Smoke Launcher, Tribal Headdress, Toolcabinet (item) | 4 |
| Recurve Bow, LongSword, Heavy SMG, Harpsichord, Prosthetic Teile | 5 |
| Greatbow, Chain Shotgun, LMG, Assault Rifle, Advanced Helmet, Recon Armor, Shield Belt, Piano, Mechanitor-Implantate | 6 |
| Sniper Rifle, Minigun, Plate Armor, Charge Rifle | 7 |
| **Component, Advanced Component, Bionic Teile, Power Armor, Cataphract Armor, Toxin-Filter-Organe** | **8** |
| Charge Lance | 9 |

## Cooking

| Aktion | Min Skill |
|---|---|
| Simple Meal, Pemmican, Kibble, Nutrient Paste | 0 |
| Psychite Tea | 2 |
| Fine Meal | 6 |
| Lavish Meal, Survival Meal | 8 |

## Medicine

| Aktion | Min Skill |
|---|---|
| Anesthetize, Treat Wounds, Remove Body Part, basic care | 0 |
| Install Wooden Peg, Denture | 3 |
| Install Prosthetic | 4 |
| Install Implant / Remove Implant | 5 |
| Install Natural Organ | 8 |
| Anomaly: Bliss Lobotomy, Ghoul Infusion | 4 |

## Plants (Growing)

| Pflanze | Min Skill |
|---|---|
| Rice, Potato, Corn, Cotton, Haygrass | 0 |
| Hops | 3 |
| Smokeleaf | 4 |
| Strawberry | 5 |
| Psychoid | 6 |
| Healroot | 8 |
| Tree Cocoa | 8 |
| Devilstrand | 10 |

## Handling (Taming)

Min Handling = `⌈wildness × 10⌉`. Vorab prüfen mit `animal.wildness`.

---

# 7. Ending-spezifische Finish-Phasen

## PHASE_JOURNEY (Opportunistic)
- Watcher: `∃ quest: quest.def == "JourneyOffer"` → sofort akzeptieren
- Vorbereitung passiv:
    - `∃ pawn: pawn.Handling ≥ 7` (Muffalo zähmen)
    - `pemmican ≥ 60 * colonist_count` als Reise-Reserve
    - `parka + boots + weapon` für jeden Pawn bereit
- Bei Trigger: Karawane packen → alle Pawns + Packtiere + Ressourcen → Reiseroute berechnen → starten

## PHASE_SHIP
Pre-Requirements:
- `∃ pawn: pawn.Construction ≥ 8`
- `∃ pawn: pawn.Crafting ≥ 8`
- Ressourcen-Buffer:
    - Steel ≥ 2.000
    - Plasteel ≥ 700
    - Uranium ≥ 200
    - Components ≥ 30
    - AdvancedComponents ≥ 40
    - **AIPersonaCore ≥ 1**
- Defense: Turrets + Kill-Box + 30+ Days Food + 30+ Medicine

Sequence:
1. Forschung: `ShipBasics → ShipCryptosleep, ShipComputerCore, ShipReactor, ShipEngine, ShipSensorCluster` (alle brauchen MultiAnalyzer)
2. Schiff-Bauplatz außerhalb der Wohnbasis, mit eigenen Killpoints
3. Bauen: Beam (Construction 5) → Cryptosleep → ComputerCore → Engine → SensorCluster → **Reactor LETZT** (Reactor triggert Belagerung)
4. Belagerung: 15 Tage Mechs + Raider abwehren
5. Pawns in Kryokapseln laden
6. Launch

Abort-Condition:
- `colonist_count < 2` → pause Ship-Phase, recruit
- `wealth < minimum_threshold` → focus on wealth recovery

## PHASE_ROYAL (Royalty DLC only)
- Requires: Empire relationship ≥ Ally
- Sequence:
    - Honor-Farm via Imperium-Quests
    - Titel-Upgrade jedes Mal erst nachdem Raum/Kleidung-Requirements gebaut sind
    - Final: Quest „Invite the High Stellarch"
    - Gäste-Suite (luxuriöser als eigene Räume)
    - 7-Tage-Belagerung
    - Shuttle boarden

## PHASE_ARCHONEXUS (Ideology DLC only)
- Cycle 1: wealth ≥ 400k → kaufe klein-archonexus → transition mit 3 Pawns
- Cycle 2: wealth ≥ 500k (in neuer Basis) → mittleres archonexus → 4 Pawns
- Cycle 3: → Haupt-Archonexus → aktivieren → Ende

## PHASE_VOID (Anomaly DLC only)
- Pre-Req: Stahlwände-Containment, Bioferrit-Produktion, Crafting 5+ Pawn, Psychic-resistente Kämpfer
- Monolith-Aktivierung
- Entity-Containment + Dark-Study-Grind
- Stufen-Aufstieg: dormant → awakened → disrupted
- Final: Void Provocation Ritual

---

# 8. Entscheidungsbäume für häufige Situationen

## 8.1 Raid Announcement

```
on_event RaidAnnounced(raid):
    threat_ratio = raid.combat_points / colony.combat_power
    
    if threat_ratio > 2.0:
        if caravan_capable():
            → FLEE: pack caravan, escape via weltkarte
        else:
            → BUNKER: all pawns indoors, block doors, wait for raid to wander off
    
    elif threat_ratio > 1.0:
        → DEFEND with losses accepted
        tactics: killbox + traps + turrets + focused fire on leader
    
    else:
        → DEFEND easily
        tactics: pick off from cover, avoid close combat
    
    post_raid:
        heal wounded (prio by blood_loss)
        repair defenses
        bury/cremate bodies (wealth cleanup)
        recruit downed enemies if prison available
```

## 8.2 Pawn-Injury

```
on_event PawnInjured(pawn, injury):
    if injury.bleed_rate > critical_threshold:
        → immediate_rescue + treat_with_best_medicine
    elif injury.causes_infection_risk > 0.5:
        → treat_with_industrial_medicine
    elif injury.severity == "minor":
        → standard_doctor_queue
    
    if pawn.consciousness < 0.3:
        → rescue_to_medical_bed + vitals_monitor_if_available
```

## 8.3 Food-Shortage

```
// Default-Zustand nach Campfire-Build:
allow_raw_eating = false   // Raw-Konsum ist ineffizient (siehe Phase 0 Nahrungs-Regel)

if food_stock_days < 5:
    PAUSE: non-essential work (research, crafting, bau)
    PRIO_UP: cooking work to MAX
    ACTIONS_IN_ORDER:
        1. hunt: assign shooters to all huntable animals on map → cook results
        2. forage: designate berry bushes / wild healroot → cook berries into Simple Meals
        3. emergency_harvest: premature harvest of growing crops → cook
        4. trade: if trader present, buy food with silver
        5. cannibalism (eat downed enemies) — Mood-Trade — Fleisch immer erst kochen
    
    // Raw-Gate bleibt geschlossen, solange Campfire funktional ist
    // Effizienz-Gewinn 1.8×: 0.5 nutrition Raw → 0.9 nutrition Cooked
    
if food_stock_days < 1 AND no_cooking_possible:
    // Absolute Last-Resort: Campfire zerstört, Nacht, keine Kochstelle erreichbar
    TEMPORARY: allow_raw_eating = true
    Log("RAW_EATING_OVERRIDE reason=no_cookable_station")
    // Reset sobald Kochstelle wieder verfügbar
```

## 8.4 Disease/Plague

```
on_event Disease(pawn, disease_def):
    priority = 1 if disease.lethal else 3
    
    // Medicine-Selection
    if industrial_medicine_count > 10: use industrial
    elif herbal_medicine_count > 0: use herbal
    else: use no_medicine (bed rest only)
    
    // Bettruhe
    confine pawn to medical_bed
    
    // Immunitäts-Fortschritt
    if disease.immunity_progress - disease.severity < 0.1:
        → switch to industrial medicine immediately
        → remove distractions (drafted work, etc.)
```

## 8.5 Mental Break

```
on_event MentalBreakImminent(pawn):
    // Identify top mood stressor
    root_cause = top_negative_thought(pawn)
    
    match root_cause:
        "nackt" → craft_clothes
        "hunger" → priority_eat_best_available
        "müde" → send_to_bed
        "schmerz" → treat_injuries, give_painkillers if needed
        "gefangener_mord" → stop_euthanizing_prisoners
        "leichenberg" → bury/cremate bodies
        "schlechtes_essen" → cook_fine_meals
        "dunkelheit" → add_lamps
        "hässlicher_raum" → add_statue, change_floor
    
    if mood < 0.15:
        confine_to_room (prevent harm)
        provide_beer, psychite_tea (mood buffs)
```

## 8.6 Weather Event

```
on_event HeatWave(duration, max_temp):
    if max_temp > 35°C:
        retreat_colonists_to_cooler_rooms
        build_passive_cooler if not existing
        reduce_outdoor_work
    
on_event ColdSnap(duration, min_temp):
    if min_temp < -20°C:
        ensure_heater_in_every_room
        stockpile_wood x 2
        retreat_indoors_at_night

on_event ToxicFallout(duration):
    keep_all_pawns_indoors
    harvest_crops_asap (before they die)
    build_roof_over_growing_zones if possible
```

## 8.7 Raid Recovery

```
after_raid:
    1. medical_triage all wounded
    2. strip_corpses (armor, weapons, implants)
    3. repair_walls + replace_destroyed_turrets
    4. bury_corpses in graves OR cremate (wealth)
    5. recruit_downed_enemies if prisoner_capacity > 0
    6. clean_blood from floors (mood)
    7. re-stock medicine if < 5 per pawn
```

---

# 9. Pawn-Arbeitsverteilung (Work Priorities)

Die Mod muss Work-Assignments rebalancen basierend auf:
- Skill-Level pro Task
- Passion (für XP-Grind-Entscheidungen)
- Aktuelle Engpässe

## Default-Priority-Matrix

| Task | Pawn mit höchstem Skill | Fallback |
|---|---|---|
| Firefighting | 1 (alle) | — |
| Patient | 1 (alle) | — |
| Doctor | Pawn mit Medicine ≥ max | — |
| BasicWorker | 2 | — |
| Warden | Pawn mit Social ≥ max | — |
| Handling | Pawn mit Animals ≥ wildness_max | — |
| Cooking | Pawn mit Cooking ≥ 6 (für Fine Meals) | Cooking 0 für Simple |
| Hunting | Pawn mit Shooting ≥ 6 | ≥ 4 |
| Construction | Pawn mit Construction ≥ max | — |
| Growing | Pawn mit Plants ≥ max_crop_requirement | — |
| Mining | Pawn mit Mining ≥ max | — |
| PlantCutting | Pawn mit Plants ≥ 2 | — |
| Smithing | Pawn mit Crafting ≥ max | — |
| Tailoring | Pawn mit Crafting ≥ max | — |
| Art | Pawn mit Artistic ≥ 8 (für rentable Skulpturen) | — |
| Crafting | Pawn mit Crafting ≥ max | — |
| Hauling | 3 (alle) | — |
| Cleaning | 3 (alle) | — |
| Research | Pawn mit Intellectual ≥ max | — |

## Skill-Grinding-Strategie

Wenn kein Pawn das benötigte Skill-Level für den nächsten Phasen-Goal hat:

```
target_skill = next_phase.required_skill
best_pawn = pawn with (skill + passion) ≥ max

if best_pawn.skill < target_skill:
    assign_dedicated_grind_task(best_pawn, skill)
    // Crafting: Produktion von Tribal Wear am Crafting Spot
    // Construction: build+deconstruct cycle (aber Ressourcen-Verlust!)
    // Cooking: Simple Meals produzieren (auch über Bedarf)
    // Medicine: operate on colonists (nur non-risk tasks)
    // Intellectual: Research
    // Plants: Baumwolle/Reis-Felder maximieren
```

---

# 10. Layout-Regeln (Basis-Design)

## Minimal-Basis (Phase 1–2)

```
[Holzhütte 6×6]
    ┌──────────┐
    │  Bed  Bed│
    │          │
    │          │
    │  Campfire│
    │  Research│
    │    Door  │
    └────┬─────┘
         Stockpile (außen, überdacht)
```

## Killpoint-Regeln
- **Maximale Länge**: 10–15 Felder Korridor
- **Beidseitig Mauer** (Steinmauer, feuerfest)
- **Fallen**: alle 2 Felder (nicht benachbart sonst blockieren sich)
- **Schussfeld**: 1–2 Felder Freiraum zum Kämpfen aus Deckung
- **Tür am Ende**: Pawns können durch, Raider müssen brechen

## Wärme-Isolierung
- Türen immer im Airlock-Design (2 Türen hintereinander)
- Räume nie größer als 64 Felder (Heater packt das nicht in Extremkälte)
- Schlafräume einzeln pro Pawn (Mood-Bonus)

## Raum-Size-Tabelle (Mood-Relevant)

| Raum | Min Fläche | Empfohlene Möbel |
|---|---|---|
| Pawn-Schlafzimmer | 4×5 (20 Felder) | Holzbett, Endtable, Holzboden, 1 Lampe, 1 Dresser |
| Esszimmer | 6×6 | Table3x3, Dining Chairs, Sculpture |
| Krankenstation | 4×5 | Hospital Bed (oder Holzbett), Vitals Monitor, Sterile Tile |
| Thronraum (Royalty) | je nach Titel | Throne + Diener-NPCs |
| Gefangenenzelle | 3×3 | Gefangenenbett, Holztür, abschließbar |

---

# 11. Anti-Patterns (was die Mod NIEMALS tun darf)

1. **Nackte Pawns im Feld kämpfen lassen** — immer zuerst Kleidung sicherstellen.
2. **Ungesichertes Essen im Lager** — Verderben beachten, Kühlung oder Pemmikan.
3. **Holzbauten ohne Steinmauer-Umschließung ab Phase 4** — Molotows zerlegen Holz.
4. **Pawns 2+ Tage wach halten** — Mental Break unvermeidlich.
5. **Einzelperson-Combat gegen Raids ≥ 3 Gegner** — Rückzug ins Gebäude priorisieren.
6. **Raiderleichen offen liegen lassen** — Mood-Penalty + Infektionsrisiko + Tollwut.
7. **Ohne Lagerbuffer zur nächsten Phase** — wenn WoodLog = 0 und Hütte noch nicht steht, wird weitere Forschung nutzlos.
8. **Growing ohne Backup-Kühlung** — Sommer-Ernteverlust möglich, immer >50% Pemmikan-Reserve.
9. **Alle Pawns gleichzeitig krank** — nicht alle im selben Feld essen lassen, Seuchenrisiko.
10. **Reaktor vor fertiger Verteidigung aktivieren** — Belagerung nicht überlebbar.
11. **Raw-Food direkt konsumieren lassen, wenn Campfire verfügbar ist** — verschwendet ~45% der Raw-Ressource und triggert Mood-Malus + Food-Poisoning. Kochen ist nie „nicht lohnend", selbst bei Skill 0. Gate: `allow_raw_eating` darf nur in echten Last-Resort-Situationen auf `true`.

---

# 12. Toggle-State-Machine

Die Mod hat mehrere Betriebszustände, zwischen denen sie wechselt:

```
AI_OFF      — Stumm. Analyse läuft im Hintergrund, cached, zeigt aber NICHTS
              Landepod-Spawn bleibt Vanilla-unbeeinflusst
              Spieler hat volle Kontrolle ohne UI-Störung
              
AI_ADVISORY — Bot zeigt Empfehlungen, führt NICHT aus
              Map-Analyse wird als Overlay sichtbar (bis zu 3 grüne Kreise
              mit Score-Labels an Top-Sites)
              Status-Overlay zeigt Phase und nächste empfohlene Goals
              Spieler entscheidet und führt aus
              
AI_ON       — Bot aktiv, trifft und führt Entscheidungen aus
              Nutzt Top-1-Site als Basis-Mittelpunkt
              Overlay sichtbar mit aktueller Site-Wahl und Phase
              
AI_PAUSED   — Vorübergehend inaktiv wegen Spieler-Eingriff
              (draft, manuelle Blueprint)
              Auto-Recovery nach 10s Idle
```

**Per-Pawn-Override (unabhängig vom globalen State):**
- Jeder Pawn hat eine eigene Checkbox im Inspector: „Player Use" (Default: unchecked)
- Master OFF → alle Pawns manuell, unabhängig von Pawn-Flag
- Master ON/ADVISORY → nur Pawns mit `playerUse == false` werden bot-gesteuert (bzw. von Bot beobachtet im Advisory)
- Master ON + pawn.playerUse == true → dieser Pawn bleibt manuell, Bot ignoriert ihn aber rechnet ihn in Kolonie-Metriken ein

## Transitions

```
// Spieler-getriggert
click_toggle_button:            AI_OFF   ←→ AI_ON
manual_draft_pawn(p):           AI_ON    →  AI_PAUSED  (für p, 10s)
undraft_pawn(p):                AI_PAUSED→  AI_ON      (nach 10s Idle)
manual_blueprint_place:         AI_ON    →  AI_PAUSED  (für diese Blueprint-Area)

// Auto-Recovery
emergency_resolved:             AI_PAUSED→  AI_ON      (wenn durch Emergency gekommen)
load_savegame:                  any      →  saved_state (aus BotGameComponent)
```

## Beim Einschalten (AI_OFF → AI_ON)

1. Falls noch kein Analyse-Cache existiert: Phase 0a (Map-Analyse) ausführen
2. Current-State-Assessment: Welche Phase entspricht dem Ist-Zustand?
3. Phase-Goals re-queuen
4. Status-Overlay einblenden
5. Beginne im aktuellen Tick mit normaler Execution

## Phase-Detection bei Mid-Game-Aktivierung

```
if no_campfire AND no_bed:
    detected_phase = 0
elif no_closed_shelter OR no_clothing:
    detected_phase = 1
elif no_pemmican_researched OR colonist_count < 2:
    detected_phase = 2
elif no_winter_readiness:
    detected_phase = 3
elif no_stone_fortress OR traps_at_killpoint < 3:
    detected_phase = 4
elif no_electricity OR no_electric_stove:
    detected_phase = 5
elif no_multianalyzer OR construction_max_skill < 8:
    detected_phase = 6
else:
    detected_phase = 7  // Ending-Wahl
```

## Soft-Takeover-Regeln (AI_ON)

Wenn Spieler im AI_ON-Modus eingreift:
- **Pawn draften**: Dieser Pawn wird sofort aus Bot-Kontrolle entfernt. Andere Pawns laufen normal weiter. Rückkehr: Auto-nach 10s Idle nach Undraft.
- **Blueprint manuell setzen**: Bot respektiert die Blueprint, queued sie in seine Bau-Liste (keine Duplikate).
- **Bill manuell setzen**: Bot fügt die Bill zur Bill-Queue hinzu, überschreibt keine existierenden.
- **Work-Tab geändert**: Bot akzeptiert die neuen Prioritäten für 60 Minuten (Spielzeit), rebalanciert danach.

---

# 13. Tick-Loop (Haupt-Execution)

```
while game_running:
    // 0. Toggle-Check
    if bot_state == AI_OFF:
        continue                              // Bot tut nichts
    if bot_state == AI_PAUSED:
        check_unpause_conditions()
        continue
    
    // 1. Invariant-Checks
    for invariant in [I1..I12]:
        if not invariant.check():
            handler = invariant.emergency_handler
            if handler.priority > current_action.priority:
                pause current_action
                execute handler
    
    // 2. Phase-Transitions
    if current_phase.exit_conditions_met():
        current_phase = next_phase
        log("Phase transition: " + current_phase.name)
    
    // 3. Goal-Pursuit
    next_goal = current_phase.next_pending_goal()
    if next_goal != null:
        assign_workers_to(next_goal)
    
    // 4. Opportunistic Handlers
    check_events_queue()  // Raid, Quest, Trader, Event
    
    // 5. Work-Priority-Rebalance (alle 2500 ticks)
    if tick % 2500 == 0:
        rebalance_pawn_work_priorities()
    
    // 6. Resource-Monitoring
    update_resource_stocks()
    check_resource_thresholds()
    
    // 7. Ending-Trigger-Check
    if ending_condition_met():
        execute_ending_sequence()
    
    tick += 1
```

---

# 14. Debug / Telemetry (für Mod-Entwicklung)

Die Mod sollte loggen:
- Phase-Transitions (wann, warum)
- Invariant-Verletzungen (welche, Dauer bis Fix)
- Emergency-Handler-Aufrufe
- Resource-Thresholds-Hits
- Pawn-Deaths (Ursache, Vermeidbarkeit)
- Ending-Progress-Metriken
- Skill-Gate-Blocks (welche Aktion warum verzögert)

Format: JSONL, eine Zeile pro Event, für spätere Auswertung.

---

# 15. Quick-Reference: Todesursachen und Gegenmittel

| Todesursache | Häufigkeit | Gegenmittel |
|---|---|---|
| Verbluten | hoch | I3 aktiv halten; Medizin ≥ 3/pawn |
| Infektion / Sepsis | mittel | Heilwurz-Vorrat; Bettruhe; Industrial Medicine ab Phase 5 |
| Kälteschock | mittel (Winter) | I5 + I6 + Parka + Heater |
| Hitzschlag | niedrig (Sommer) | Passive Cooler / Cooler / Berg-Basis |
| Hunger | niedrig | I10 halten; Pemmikan-Buffer |
| Mental Break → Raserei | mittel | I12 halten; Mood-Maßnahmen frühzeitig |
| Raid-Kampf-Treffer | hoch | Killbox + Rüstung + Rückzug vor starken Raids |
| Feuer | niedrig-mittel | Steinmauern ab Phase 4; Firefoam ab Phase 5 |
| Organversagen (Gift/Toxic) | niedrig | Schutzrüstung; Detox-Lunge in Phase 6 |
| Schiff-Belagerung Mech | hoch | Turrets + EMP-Granaten + Kill-Box |

---

# 16. Minimale AIPersonaCore-Beschaffung

Da AIPersonaCore die einzige nicht-craftbare Schiffs-Ressource ist:

**Quellen (in Prio-Reihenfolge):**
1. **Ancient Danger Complex** auf der Karte — manchmal spawnt ein Persona Core drin. Öffnen im Late-Game mit Power Armor + Turrets.
2. **Quest "Aerodrone Strike" / "Ancient Complex"** — Imperium/AI-Fraktion belohnen mit Persona Core.
3. **Abgestürztes Schiff** (Event) — enthält häufig Persona Core.
4. **Händler** — orbitale Bulk Goods Trader verkaufen selten welche für ~3.000 Silber.
5. **Mechanoid Cluster** besiegen — enthält Persona Core als Loot.

**Mod-Regel:** Sobald ein Persona Core verfügbar ist (`on_map` oder `buyable`), **Max-Priorität** für dessen Beschaffung vergeben, auch wenn andere Phasen-Goals darunter leiden.

---

# Abschluss

Diese Datei ist die **executive decision layer** der Mod. Sie beschreibt das WAS (Ziel-Zustand) und das WANN (Trigger-Bedingungen), nicht das WIE (Pfad-Finding, konkrete Job-Assignments) — das bleibt der Mod-Logik überlassen.

Alle Zahlen und Skill-Gates sind aus den XML-Defs der installierten Spielversion verifiziert. Für neue DLCs oder Mods, die Skill-Gates oder Rezepte hinzufügen, muss die Tabelle in Abschnitt 6 + Ressourcen-Ziele in Abschnitt 5 entsprechend erweitert werden.
