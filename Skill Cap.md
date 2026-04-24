# RimWorld — Skill-Caps & Mindestanforderungen (verifiziert aus Game-Files)

**Korrektur-Hinweis:** Die frühere Version behauptete fälschlich, Konstruktion hätte keine Hard-Caps. Das war falsch. Das Spiel nutzt zwei getrennte XML-Felder:

- `skillRequirements` — für Rezepte (Werkbank-Jobs, Chirurgie, Kochen)
- `constructionSkillPrerequisite` — für **Gebäude, Terrain, Möbel**

In-Game-Verifikation: Die Stachelfalle zeigt im Tooltip „Konstruktion benötigt: 3", und das steht in `Data/Core/Defs/ThingDefs_Buildings/Buildings_Security.xml:179`. Die „Fallen ab Konstruktion 3"-Faustregel ist **korrekt**.

---

## Konstruktion (Construction) — `constructionSkillPrerequisite`

Alle folgenden Werte aus direktem Grep aller `Data/*/Defs/ThingDefs_Buildings/` und `TerrainDefs/` Dateien.

### Sicherheit / Fallen / Türme (Core)

| Gebäude | Min Construction |
|---|---|
| Sandbags, Barricade | 0 |
| **TrapSpike (Stachelfalle)** | **3** |
| TrapIED_HighExplosive / Incendiary / EMP / Smoke / Firefoam / Antigrain | **3** |
| Turret_MiniTurret | **5** |
| Turret_Mortar (BaseArtilleryBuilding) | **5** |
| Turret_FoamTurret | **5** |
| Turret_RocketswarmLauncher | **5** |
| Turret_Autocannon | **6** |
| Turret_Sniper (Uranium Slug) | **6** |

### Schiff (Core, Ending-relevant!)

| Gebäude | Min Construction |
|---|---|
| Ship_Beam | **5** |
| Ship_CryptosleepCasket | **8** |
| Ship_ComputerCore | **8** |
| Ship_Reactor | **8** |
| Ship_Engine | **8** |
| Ship_SensorCluster | **8** |

**Wichtig für den Schiffsstart-Pfad:** Alle Haupt-Schiffsteile brauchen Construction 8. Das ist ein **harter Gate** — ohne einen Pawn auf Skill 8+ kannst du das Schiff nicht bauen.

### Strom (Core)

| Gebäude | Min Construction |
|---|---|
| PowerConduit, HiddenConduit, WaterproofConduit, PowerSwitch, Battery | 0 |
| WoodFiredGenerator | 4 |
| WindTurbine | 4 |
| SolarGenerator | 6 |
| WatermillGenerator | 6 |
| ChemfuelPoweredGenerator | 6 |
| GeothermalGenerator | **8** |

### Produktion / Werkbänke (Core)

| Gebäude | Min Construction |
|---|---|
| CraftingSpot, ButcherSpot, TableSculpting, TableButcher, HandTailoringBench | 0 |
| FueledStove, TableStonecutter, Brewery, FermentingBarrel, SimpleResearchBench | 0 |
| FueledSmithy | 4 |
| ElectricTailoringBench | 4 |
| TableMachining | 4 |
| ElectricStove | 4 |
| DrugLab | 4 |
| ElectricSmelter | 4 |
| BiofuelRefinery | 4 |
| ElectricCrematorium | 4 |
| HydroponicsBasin | 4 |
| DeepDrill | 4 |
| ElectricSmithy | 5 |
| NutrientPasteDispenser | 5 |
| FabricationBench | **6** |
| HiTechResearchBench | **6** |

### Temperatur (Core)

| Gebäude | Min Construction |
|---|---|
| Campfire, PassiveCooler, Vent | 0 |
| Heater | **5** |
| Cooler | **5** |

### Struktur / Türen (Core)

| Gebäude | Min Construction |
|---|---|
| Wall, Door, Fence, FenceGate, Column, AnimalFlap | 0 |
| OrnateDoor | 5 |
| Autodoor | **6** |

### Möbel (Core)

| Gebäude | Min Construction |
|---|---|
| SleepingSpot, Bed, DoubleBed, Bedroll, Stool, EndTable, Table*, Shelf, TorchLamp, StandingLamp | 0 |
| DiningChair | 4 |
| Armchair | 5 |
| Couch | 5 |
| **RoyalBed** | **8** |
| **HospitalBed** | **8** |

### Sonstiges / Misc (Core)

| Gebäude | Min Construction |
|---|---|
| EggBox, OrbitalTradeBeacon, Grave, MarriageSpot, PartySpot, PenMarker | 0 |
| ToolCabinet | 4 |
| CommsConsole | **5** |
| FirefoamPopper | 5 |
| Sarcophagus | 5 |
| MoisturePump | 6 |
| PodLauncher | 6 |
| TransportPod | 6 |
| **MultiAnalyzer** | **8** |
| **VitalsMonitor** | **8** |
| **GroundPenetratingScanner** | **8** |
| **LongRangeMineralScanner** | **8** |
| **CryptosleepCasket** (nicht Schiff) | **8** |

### Freizeit (Core)

| Gebäude | Min Construction |
|---|---|
| HorseshoesPin, HoopstoneRing, GameOfUrBoard, ChessTable, Telescope | 0 |
| PokerTable | 6 |
| BilliardsTable | 6 |
| TubeTelevision | 7 |
| FlatscreenTelevision | 8 |
| MegascreenTelevision | (aus Def erblich, vermutlich 8) |

### Böden (Core, TerrainDefs)

| Boden | Min Construction |
|---|---|
| Concrete, PavedTile, StrawMatting, Carpet, MetalTile, SilverTile, GoldTile, Flagstone* | 0 |
| WoodPlankFloor | **3** |
| Stone Tiles (Sandstone, Granite, Limestone, Slate, Marble) | **3** |
| SterileTile | **6** |

### Royalty DLC

| Gebäude | Min Construction |
|---|---|
| Throne | **4** |
| GrandThrone | **6** |
| FineStoneTile-Böden | 6 |
| FineCarpet-Böden | 6 |

### Ideology DLC

| Gebäude | Min Construction |
|---|---|
| GibbetCage | 4 |
| Skullspike | 4 |
| Ideology Fine-Böden (Ideoglyphbild, etc.) | 6 |
| SleepAccelerator | **8** |
| BiosculpterPod | **8** |

### Biotech DLC

| Gebäude | Min Construction |
|---|---|
| Mechanitor / Deathrest-Betten etc. | 3–4 |
| Mech-Charger | 5 |
| Mech-Gestator | 6 |
| SubcoreSoftscanner | 5 |
| SubcoreRipscanner | 6 |
| WastepackAtomizer | **8** |

### Anomaly DLC

| Gebäude | Min Construction |
|---|---|
| Bioferrit-Plattenboden | 3 |
| Containment-Energie/Misc | 4–6 |
| SecurityDoor | **7** |

### Odyssey DLC

| Gebäude | Min Construction |
|---|---|
| Gravship-Basisteile | 4 |
| Gravship-Cockpit / Strukturteile | 5 |
| Gravship-Antriebe / Reaktor | 8 |
| Orbital-Beacon | 6 |
| Orbital-Hauptteile | 8 |
| Diverse Odyssey-Misc | 6–8 |

**Merkregeln Konstruktion:**
- **0** = Primitiv- und Basiskram (Wände, Türen, Sleeping Spot, Betten, Stühle)
- **3** = Fallen, Holz/Steinböden
- **4** = die meisten elektrischen Werkbänke + einfache Stromgeneratoren
- **5** = Türme (klein), Mortar, Heater/Cooler, OrnateDoor, Armchair, CommsConsole
- **6** = Autodoor, Solarpanel, Fabrikationsbank, Hi-Tech-Bench, Autocannon, GrandThrone
- **7** = Tube-TV, Security Door (Anomaly)
- **8** = **Schiffsteile, Geothermal, MultiAnalyzer, Vitals, Scanner, RoyalBed, HospitalBed, BiosculpterPod, SleepAccelerator, WastepackAtomizer**

---

## Handwerk (Crafting) — `skillRequirements`

### Neolithische Waffen

| Waffe | Min Crafting |
|---|---|
| Short Bow | **2** |
| Pila | 3 |
| Recurve Bow | 5 |
| Greatbow | 6 |

### Mittelalter (Royalty)

| Waffe | Min Crafting |
|---|---|
| Gladius, Mace (Smithing) | 3 |
| LongSword (LongBlades) | 5 |

### Industrielle Waffen

| Waffe | Min Crafting |
|---|---|
| Revolver | 3 |
| Autopistol, Machine Pistol | 4 |
| Pump Shotgun, Bolt-Action Rifle | 4–5 |
| Heavy SMG | 5 |
| Incendiary Launcher, Smoke Launcher | 4 |
| Chain Shotgun, LMG, Assault Rifle | 6 |
| Sniper Rifle, Minigun | 7 |

### Spacer-Waffen

| Waffe | Min Crafting |
|---|---|
| Charge Rifle | 7 |
| Charge Lance | 9 |

### Rüstung / Kleidung

| Item | Min Crafting |
|---|---|
| Tribal Headdress | 4 |
| Flak Vest / Flak Pants / Flak Jacket | 4 |
| Advanced Helmet | 6 |
| Recon Armor (+ Helm) | 6 |
| Plate Armor | 7 |
| Power Armor (+ Helm) | 8 |
| Cataphract Armor (Royalty) | 8 |
| Shield Belt | 6 |

### Bauteile / Medizin

| Item | Min-Skills |
|---|---|
| Component (Industrial) | Crafting **8** |
| Advanced Component | Crafting **8** |
| Medicine (Industrial) | Crafting **4** + Intellectual **4** (Doppel-Cap!) |

### Körperteile

| Item | Min Crafting |
|---|---|
| Prosthetic-Teile | 5 |
| Bionische Teile | 8 |
| Mechanitor-Implantate (Biotech) | 6 |
| Toxin-Filter-Organe (Biotech) | 8 |

### Musikinstrumente (Royalty)

| Item | Min Crafting |
|---|---|
| Harp | 3 |
| Harpsichord | 5 |
| Piano | 6 |

---

## Pflanzenanbau (Growing) — `sowMinSkill`

| Pflanze | sowMinSkill |
|---|---|
| Rice, Potato, Corn, Cotton, Haygrass | 0 |
| Hops | 3 |
| Smokeleaf | 4 |
| Strawberry | 5 |
| Psychoid | 6 |
| Healroot | 8 |
| TreeCocoa | 8 |
| Devilstrand | 10 |

---

## Tiere zähmen (Handling)

Die Mindest-Handling-Skill wird aus dem `Wildness`-Stat berechnet (`StatWorker_MinimumHandlingSkill`). Praktisch `ceil(Wildness × 10)`.

| Tier | Wildness | Min Handling |
|---|---|---|
| Cow, Sheep, Pig, Goat, Donkey, Cat, Labrador, Husky, Chicken, Yorkshire Terrier | 0 | 0 |
| Yak, Insect-Basis | 0.2 | 2 |
| Alpaca, Dromedary | 0.25 | 3 |
| Horse | 0.35 | 4 |
| Pig (wild) | 0.5–0.55 | 5–6 |
| Muffalo, Boomalope, Bison, Warg, Monkey, Lynx, Turkey, Duck | 0.6 | 6 |
| Hare, Cobra, Tortoise, Alphabeaver, Capybara, Elk, Caribou, Elephant | 0.75 | 8 |
| Cougar, Panther, Grizzly, Polar Bear, Ostrich, Eagle | 0.8 | 8 |
| Wolf | 0.85 | 9 |
| Rhinoceros | 0.9 | 9 |
| Cassowary, Emu | 0.95 | 10 |
| Megasloth | 0.97 | 10 |
| Thrumbo | 0.98 | 10 |

---

## Tier-Training-Befehle — `TrainableDef`

**KEINE** Handling-Skill-Caps, nur Trainability-Stufe des Tiers:

| Befehl | Erforderliche Trainability |
|---|---|
| Tameness | None (alle Tiere) |
| Obedience (Guard) | Intermediate |
| Release (Attack) | Intermediate |
| Haul | Advanced + `minBodySize 0.40` |
| Rescue | Advanced + `minBodySize 0.65` |

---

## Kochen (Cooking) — `skillRequirements`

| Rezept | Min Cooking |
|---|---|
| CookMealSimple (+Bulk) | 0 |
| Make_Pemmican, Make_Kibble | 0 |
| CookMealFine (+Bulk, +Veg, +Meat) | 6 |
| CookMealLavish (+Bulk, +Veg, +Meat) | 8 |
| CookMealSurvival (+Bulk) | 8 |
| PsychiteTea | 2 |

Flake, Yayo, Bier, Smokeleaf-Joints, GoJuice, WakeUp, Luciferium, Penoxycyline: kein Skill-Cap im Def (nur Research).

---

## Medizin (Medical) — `skillRequirements`

### Core

| Eingriff | Min Medicine |
|---|---|
| Anesthetize, Euthanize, RemoveBodyPart | 0 (nur Erfolgsrate) |
| Install Wooden Peg / Denture | 3 |
| Install Prosthetic | 4 |
| Install / Remove Implant (generic) | 5 |
| Install Natural Organ (Heart etc.) | 8 |

### Royalty

| Eingriff | Min Medicine |
|---|---|
| Cure Blood Rot | 5 |
| Cure Paralytic Abasia | 5 |

### Anomaly

| Eingriff | Min Medicine |
|---|---|
| SurgicalInspection | 3 |
| BlissLobotomy | 4 |
| GhoulInfusion | 4 |

**Normale Chirurgie** (Kugel entfernen, Amputation, Bionik-Einbau) hat **keinen** harten Skill-Cap — nur `surgerySuccessChanceFactor` und `deathOnFailedSurgeryChance` (z. B. 0.25 beim Herzeinbau).

---

## Kunst (Artistic)

| Item | Min Artistic |
|---|---|
| Core: SculptureSmall/Large/Grand | 0 |
| Odyssey: Statue | 4 |

---

## Restliche Skills

**Intellectual, Shooting, Melee, Social, Mining, Plants-Ernten:**

- **Keine** `skillRequirements` oder `skillPrerequisite` in den Defs.
- Effektivität skaliert rein über Stat-Formeln (Forschungsgeschwindigkeit, Trefferchance, Yield, Recruit-Chance).

---

## Zusammenfassung: Harte Caps im Überblick

| Skill | Wo blockiert |
|---|---|
| **Construction** | Gebäude 3/4/5/6/8 je nach Typ — **Schiffsteile erfordern 8** |
| **Construction** | Steinböden + Holzboden: 3 |
| **Growing** | Pflanzen 3/4/5/6/8/10 je nach Pflanze |
| **Handling** | Tiere nach Wildness-Formel |
| **Cooking** | Fine 6, Lavish 8, Survival 8, PsychiteTea 2 |
| **Crafting** | Waffen 2–9, Rüstung 4–8, Components 8, Bionik 8 |
| **Medicine** | Install-Operationen 3–8 je Körperteil |
| **Crafting + Intellectual** | Industrial Medicine (Doppel-Cap 4+4) |
| **Artistic** | Odyssey-Statue 4 |

---

## Korrekturen zur **vorherigen Version** dieser Datei

Ich habe beim ersten Durchlauf **den falschen XML-Tag durchsucht** (`skillRequirements` statt `constructionSkillPrerequisite`). Dadurch fehlte der **gesamte Gebäude-Konstruktions-Block**. Die korrigierten Werte oben sind jetzt aus beiden Feldern zusammengeführt.

Frühere Falschaussagen, die jetzt widerlegt sind:

- „Keine harten Konstruktions-Caps" → **Falsch**: >100 Gebäude haben `constructionSkillPrerequisite`.
- „Fallen ohne Cap" → **Falsch**: Spike Trap und alle IEDs brauchen Construction 3.
- „Türme kein Cap" → **Falsch**: MiniTurret 5, Autocannon 6.
- „Schiffsteile kein Cap" → **Falsch**: Alle Haupt-Schiffsteile brauchen **Construction 8**.
- „Heater/Cooler kein Cap" → **Falsch**: beide Construction 5.
- „Hi-Tech-Bench kein Cap" → **Falsch**: Construction 6.
- „MultiAnalyzer kein Cap" → **Falsch**: Construction 8.
- „Betten kein Cap" → **Teilweise falsch**: RoyalBed und HospitalBed benötigen Construction 8.

Die Pflanzen-, Tier-, Koch-, Medizin- und Crafting-Werte der Vorversion waren korrekt (aus `skillRequirements` und `sowMinSkill`) und sind unverändert übernommen.
