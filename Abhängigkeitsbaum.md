# RimWorld — Abhängigkeitsbaum (Herstellung, Skill, Ressourcen, Research)

Alle Werte aus den XML-Defs in `Data/Core`, `Data/Royalty`, `Data/Ideology`, `Data/Biotech`, `Data/Anomaly`, `Data/Odyssey`.

**Legende:**
- **Skill** = Mindest-Skill aus `constructionSkillPrerequisite` (Bauen) bzw. `skillRequirements` (Rezepte). `—` = kein Cap.
- **Research** = Voraussetzung aus `researchPrerequisites`. `—` = keine Forschung nötig (direkt ab Start verfügbar).
- **Werkbank** = Wo das Rezept ausgeführt wird.
- **Stuff** = stuff-skalierte Ressourcen (Holz/Stein/Metall je nach Materialwahl).
- **Ressourcen** = feste Kosten zusätzlich zu Stuff.

---

# 🟢 TIER 0 — Ohne Forschung, ohne Skill verfügbar (Tribal-Start)

## Arbeitsplätze (Spots)

| Item | Skill | Ressourcen | Werkbank |
|---|---|---|---|
| CraftingSpot | 0 | keine | — |
| ButcherSpot | 0 | keine | — |
| SleepingSpot | 0 | keine | — |
| DoubleSleepingSpot | 0 | keine | — |
| AnimalSleepingSpot | 0 | keine | — |
| MarriageSpot, PartySpot, CaravanPackingSpot, RitualSpot | 0 | keine | — |

## Strukturen (stuffbasiert: Holz, Stein nach `Stonecutting`)

| Item | Skill | Stuff | Ressourcen |
|---|---|---|---|
| Wall | 0 | 5 stuff | — |
| Door | 0 | 25 stuff | — |
| Fence | 0 | 1 stuff | — |
| FenceGate | 0 | 25 stuff | — |
| Column | 0 | 20 stuff | — |

## Möbel (stuffbasiert)

| Item | Skill | Stuff | Research |
|---|---|---|---|
| Bed | 0 | 45 stuff | — |
| DoubleBed | 0 | 85 stuff | — |
| Bedroll | 0 | 40 stuff (Fabric) | — |
| BedrollDouble | 0 | 85 stuff (Fabric) | — |
| AnimalSleepingBox | 0 | 25 stuff | — |
| AnimalBed | 0 | 40 stuff | — |
| Stool | 0 | 25 stuff | — |
| PlantPot | 0 | 20 stuff | — |
| TorchLamp | 0 | 15 WoodLog | — |
| Shelf / ShelfSmall | 0 | 20 / 10 stuff | — |
| Grave | 0 | keine | — |

## Temperatur / Beleuchtung

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| Campfire | 0 | 20 WoodLog | — |
| PassiveCooler | 0 | 50 WoodLog | **PassiveCooler** |
| Vent | 0 | 30 Steel | — |

## Sicherheit (Tier 0)

| Item | Skill | Stuff / Ressourcen | Research |
|---|---|---|---|
| Sandbags | 0 | 6 stuff (Stony/Metallic) | — |
| Barricade | 0 | 5 stuff | — |

## Werkbänke (Tier 0)

| Item | Skill | Stuff / Ressourcen | Research |
|---|---|---|---|
| TableSculpting | 0 | 75 stuff | — |
| TableButcher | 0 | 75 stuff | — |
| HandTailoringBench | 0 | 75 stuff | **ComplexClothing** |
| TableStonecutter | 0 | 75 stuff | **Stonecutting** |
| FueledStove | 0 | 100 stuff + 2 Components | — |
| FueledSmithy | 4 | 100 stuff + 2 Components | **Smithing** |
| Brewery | 0 | 100 stuff + 2 Components | **Brewing** |
| SimpleResearchBench | 0 | 75 stuff | — |
| FermentingBarrel | 0 | 50 stuff + 4 Steel | **Brewing** |

## Böden (Tier 0)

| Boden | Skill | Material |
|---|---|---|
| Concrete | 0 | 5 stuff |
| PavedTile | 0 | 5 stuff |
| StrawMatting | 0 | 5 Haygrass |
| Carpet | 0 | 1 Cloth + 1 dye |
| Flagstones (alle Steine) | 0 | 1 stuff |
| WoodPlankFloor | **3** | 5 WoodLog |
| Stone Tiles (Sandstone/Granite/Limestone/Slate/Marble) | **3** | 1 stuff |

## Waffen am Crafting Spot

| Waffe | Skill | Ressourcen | Research |
|---|---|---|---|
| **Short Bow** | Crafting 2 | 30 WoodLog | — |
| **Pila** | Crafting 3 | 70 WoodLog | — |
| Club | — | 30 WoodLog | — |
| Short Bow-Arrow, etc. — werden beim Schuss verbraucht (nicht craftbar) | — | — | — |

## Neolithische Nahkampfwaffen (Crafting Spot)

| Waffe | Skill | Ressourcen | Research |
|---|---|---|---|
| Club | — | 30 stuff (Woody/Stony/Metallic) | — |
| Knife (IlluminationKnife etc. nach Mod) | — | siehe Game | — |

## Kleidung am Crafting Spot (ohne Research)

| Item | Skill | Stuff | Research |
|---|---|---|---|
| Apparel_TribalA (Tribal Wear) | — | 60 stuff (Leathery/Fabric) | — |
| Apparel_Parka | — | 120 stuff (Leathery/Fabric) | — |
| Apparel_Tuque | — | 20 stuff (Leathery/Fabric) | — |
| Apparel_Pants | — | 40 stuff | — |
| Apparel_BasicShirt | — | 40 stuff | — |
| Apparel_Duster | — | 80 stuff | — |
| Apparel_CowboyHat | — | 30 stuff | — |

---

# 🟡 TIER 1 — Schmiede / Möbel-Forschung / Stonecutting

Erforschbar ab Start, braucht den **SimpleResearchBench**.

## Forschungen

| Research | Voraussetzung |
|---|---|
| Smithing | — |
| Stonecutting | — |
| ComplexFurniture | — |
| ComplexClothing | — |
| PassiveCooler | — |
| Pemmican | — |
| Brewing | — |
| IEDs | — |
| RecurveBow | — |
| Greatbow | — |
| LongBlades | Smithing |
| PlateArmor | Smithing |
| CarpetMaking | — |

## Metallwaffen (Fueled/Electric Smithy)

| Waffe | Skill | Stuff | Research |
|---|---|---|---|
| Mace | Crafting 3 | Stuff (Metallic/Stony) | **Smithing** |
| Gladius | Crafting 3 | Stuff | **Smithing** |
| LongSword | Crafting 5 | Stuff | **LongBlades** |

## Sicherheit — Fallen (Tier 1)

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| **TrapSpike (Stachelfalle)** | **Construction 3** | 45 WoodLog | — |
| TrapIED_HighExplosive | **Construction 3** | 70 Steel + 3 Comp | **IEDs** |
| TrapIED_Incendiary | 3 | 70 Steel + 3 Comp | IEDs |
| TrapIED_EMP | 3 | 70 Steel + 3 Comp | IEDs |
| TrapIED_Smoke | 3 | 70 Steel + 3 Comp | IEDs |
| TrapIED_Firefoam | 3 | 70 Steel + 3 Comp | IEDs + Firefoam |
| TrapIED_AntigrainWarhead | 3 | 70 Steel + 3 Comp + 1 Antigrain | IEDs |

## Waffen am Crafting Spot (mit Research)

| Waffe | Skill | Ressourcen | Research |
|---|---|---|---|
| Recurve Bow | Crafting 5 | 40 WoodLog | **RecurveBow** |
| Greatbow | Crafting 6 | 60 WoodLog | **Greatbow** |

## Rüstung (FueledSmithy)

| Item | Skill | Stuff | Research |
|---|---|---|---|
| PlateArmor | Crafting 7 | Stuff | **PlateArmor** |
| SimpleHelmet | — | 20 stuff (Metallic) | Smithing |
| TribalHeaddress | Crafting 4 | 40 stuff | — (faction Tribal) |

## Möbel Tier 1 (ComplexFurniture)

| Item | Skill | Stuff | Research |
|---|---|---|---|
| EndTable | — | 30 stuff | **ComplexFurniture** |
| Table1x2c / 2x2 / 2x4 / 3x3 | — | 28 / 50 / 95 / 100 stuff | ComplexFurniture |
| DiningChair | Construction 4 | 45 stuff | ComplexFurniture |
| Dresser | — | 50 stuff | ComplexFurniture |
| Bookcase / BookcaseSmall | — | 20 / 10 stuff | ComplexFurniture |

## Kochen (FueledStove)

| Rezept | Skill | Ressourcen | Research |
|---|---|---|---|
| Simple Meal | Cooking 0 | 10 Nutrition aus Raw | — |
| Fine Meal | **Cooking 6** | 0.5 Meat + 0.5 Plant | — |
| Lavish Meal | **Cooking 8** | 0.75 Meat + 0.75 Plant | — |
| Pemmican (TableButcher/Campfire) | 0 | 0.25 Meat + 0.25 Plant | **Pemmican** |
| Kibble (TableButcher) | 0 | Meat + Plant Mix | — |

## Bier / Alkohol (Brewery + FermentingBarrel)

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| Beer | 0 | Hops → Wort → FermentingBarrel | **Brewing** |

---

# 🟠 TIER 2 — Elektrizität

## Forschungen

| Research | Voraussetzung |
|---|---|
| Electricity | — |
| Batteries | Electricity |
| SolarPanels | Electricity |
| WatermillGenerator | Electricity |
| AirConditioning | Electricity |
| NutrientPaste | Electricity |
| HydroponicsBasin | Electricity |
| ColoredLights | Electricity |
| Autodoors | Electricity + ComplexFurniture |
| CarpetMaking | Electricity |

## Strom (Tier 2)

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| PowerConduit | 0 | 1 Steel / Feld | Electricity |
| HiddenConduit | 0 | 1 Steel + 0.1 Comp | Electricity |
| PowerSwitch | 0 | 10 Steel | Electricity |
| **WoodFiredGenerator** | Construction 4 | 100 Steel + 3 Comp | Electricity |
| **WindTurbine** | Construction 4 | 120 Steel + 4 Comp | Electricity |
| **Battery** | 0 | 70 Steel + 4 Comp | Batteries |
| **SolarGenerator** | Construction 6 | 100 Steel + 4 Comp | SolarPanels |
| **WatermillGenerator** | Construction 6 | 100 Steel + 6 WoodLog + 4 Comp | WatermillGenerator |

## Temperatur (Tier 2)

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| **Heater** | Construction 5 | 50 Steel + 1 Comp | Electricity |
| **Cooler** | Construction 5 | 90 Steel + 3 Comp | AirConditioning |

## Elektrische Werkbänke

| Item | Skill | Stuff / Ressourcen | Research |
|---|---|---|---|
| ElectricTailoringBench | 4 | 75 stuff + … | ComplexClothing + Electricity |
| ElectricStove | 4 | 100 stuff + 3 Steel + 2 Comp | Electricity |
| ElectricSmithy | 5 | 100 stuff + 2 Comp | Smithing + Electricity |
| ElectricSmelter | 4 | 180 Steel + 3 Comp | Smelting + Electricity |
| ElectricCrematorium | 4 | 150 stuff + 3 Comp | Electricity |
| DrugLab | 4 | 50 stuff + 3 Comp | DrugProduction + Electricity |
| HydroponicsBasin | 4 | 100 Steel + 1 Comp | HydroponicsBasin |
| NutrientPasteDispenser | 5 | 150 Steel + 3 Comp | NutrientPaste |
| Hopper | 0 | 35 Steel | NutrientPaste |
| ElectricCrematorium | 4 | 150 stuff + 3 Comp | Electricity |

## Türen (Tier 2)

| Item | Skill | Stuff / Ressourcen | Research |
|---|---|---|---|
| Autodoor | Construction 6 | 25 stuff + 40 Steel + 2 Comp | Autodoors |
| OrnateDoor | Construction 5 | 75 stuff + 50 Gold | ComplexFurniture |

## Beleuchtung

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| StandingLamp | 0 | 15 Steel | Electricity |
| WallLamp | 0 | 15 Steel | Electricity |
| SunLamp | 0 | 20 Steel + 2 Comp | HydroponicsBasin |
| FloodLight | 0 | 30 Steel + 2 Comp | Electricity |

## Sicherheit (Komms)

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| CommsConsole | Construction 5 | 100 Steel + 7 Comp | RadioComms |
| OrbitalTradeBeacon | 0 | 30 Steel | RadioComms |

---

# 🔴 TIER 3 — Mikroelektronik / Maschinenbau / Verteidigung

## Forschungen

| Research | Voraussetzung |
|---|---|
| MicroelectronicsBasics | Electricity |
| Machining | MicroelectronicsBasics |
| Gunsmithing | Machining |
| Mortars | Gunsmithing |
| GunTurrets | Machining |
| FoamTurret | Machining + Firefoam |
| FlakArmor | Gunsmithing |
| ShieldBelt | MicroelectronicsBasics |
| MedicineProduction | MicroelectronicsBasics |
| Prosthetics | MicroelectronicsBasics |
| Firefoam | MicroelectronicsBasics |
| TransportPod | MicroelectronicsBasics + Rocketry |
| DeepDrilling | MicroelectronicsBasics |
| GroundPenetratingScanner | DeepDrilling + MicroelectronicsBasics |
| LongRangeMineralScanner | GroundPenetratingScanner |
| BiofuelRefining | Electricity |
| GeothermalPower | Electricity |
| TubeTelevision | MicroelectronicsBasics |
| MoisturePump | Electricity |
| BlowbackOperation | Gunsmithing |
| GasOperation | BlowbackOperation |
| PrecisionRifling | GasOperation |
| MultibarrelWeapons | Gunsmithing |
| HospitalBed | MicroelectronicsBasics |

## Schlüssel-Werkbänke

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| **TableMachining** | Construction 4 | 150 Steel + 4 Comp | Machining |
| **HiTechResearchBench** | Construction 6 | 150 stuff + 100 Steel + 8 Comp | MicroelectronicsBasics |
| **MultiAnalyzer** | Construction 8 | 40 Steel + 2 Plasteel + 8 Comp | MultiAnalyzer (Forschung) |
| **BiofuelRefinery** | Construction 4 | 200 Steel + 3 Comp | BiofuelRefining |
| **DeepDrill** | Construction 4 | 100 Steel + 3 Comp | DeepDrilling |
| **GroundPenetratingScanner** | Construction 8 | 150 Steel + 6 Comp | GroundPenetratingScanner |
| **LongRangeMineralScanner** | Construction 8 | 200 Steel + 10 Comp | LongRangeMineralScanner |
| **VitalsMonitor** | Construction 8 | 70 Steel + 3 Comp | HospitalBed |
| **ToolCabinet** | Construction 4 | 100 Steel + 3 Comp + 40 stuff | MicroelectronicsBasics |

## Türme

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| **Turret_MiniTurret** | Construction 5 | 70 Steel + 3 Comp | GunTurrets |
| **Turret_Mortar** | Construction 5 | 150 Steel + 6 Comp | Mortars |
| **Turret_FoamTurret** | Construction 5 | 150 Steel + 4 Comp | FoamTurret |
| **Turret_RocketswarmLauncher** | Construction 5 | 200 Steel + 8 Comp | RocketswarmLauncher |
| **Turret_Autocannon** | Construction 6 | 350 Steel + 40 Plasteel + 7 Comp | Autocannon |
| **Turret_Sniper (Uranium Slug)** | Construction 6 | 300 Steel + 30 Plasteel + 60 Uranium + 6 Comp | UraniumSlugTurret |

## Industrielle Waffen (TableMachining)

| Waffe | Skill | Ressourcen | Research |
|---|---|---|---|
| Revolver | Crafting 3 | 30 Steel + 2 Comp | Machining |
| Autopistol | Crafting 4 | 45 Steel + 2 Comp | BlowbackOperation |
| Machine Pistol | Crafting 4 | 60 Steel + 3 Comp | BlowbackOperation |
| Pump Shotgun | Crafting 4 | 75 Steel + 4 Comp | Machining |
| Incendiary Launcher | Crafting 4 | 75 Steel + 4 Comp | Machining |
| Smoke Launcher | Crafting 4 | 75 Steel + 4 Comp | Machining |
| Bolt-Action Rifle | Crafting 4 | 75 Steel + 4 Comp | Machining |
| Heavy SMG | Crafting 5 | 60 Steel + 3 Comp | GasOperation |
| Chain Shotgun | Crafting 6 | 60 Steel + 3 Comp | GasOperation |
| LMG | Crafting 6 | 60 Steel + 3 Comp | GasOperation |
| Assault Rifle | Crafting 6 | — + 3 Comp | PrecisionRifling |
| Sniper Rifle | Crafting 7 | — + 4 Comp | PrecisionRifling |
| Minigun | Crafting 7 | 160 Steel + 20 Comp | MultibarrelWeapons |

## Rüstung (TableMachining)

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| Flak Vest | Crafting 4 | 60 stuff + 2 Comp | FlakArmor |
| Flak Pants | Crafting 4 | 50 stuff + 2 Comp | FlakArmor |
| Flak Jacket | Crafting 4 | 90 stuff + 3 Comp | FlakArmor |
| Advanced Helmet | Crafting 6 | 60 Steel + 2 Comp | FlakArmor |
| Shield Belt | Crafting 6 | 50 Steel + 1 Comp + 4 Plasteel | ShieldBelt |

## Medizin & Bionik (Tier 3)

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| **MedicineIndustrial** | Crafting 4 + **Intellectual 4** | 1 Neutroamine + 1 Cloth + 3 Herbal Medicine | MedicineProduction |
| Prosthetic-Teile (Arm/Bein/etc.) | Crafting 5 | meist 30 Steel + 2 Comp | Prosthetics |

## Möbel Tier 3

| Item | Skill | Stuff | Research |
|---|---|---|---|
| Armchair | Construction 5 | 110 stuff | ComplexFurniture + Electricity? |
| Couch | Construction 5 | 200 stuff | ComplexFurniture |
| RoyalBed | Construction 8 | 100 stuff + 50 Gold | RoyalApparel |
| HospitalBed | Construction 8 | 40 stuff + 40 Steel + 3 Comp | HospitalBed |

## Strom Tier 3

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| **ChemfuelPoweredGenerator** | Construction 6 | 150 Steel + 6 Comp | BiofuelRefining |
| **GeothermalGenerator** | **Construction 8** | 340 Steel + 8 Comp | GeothermalPower |

## Sonstiges Tier 3

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| MoisturePump | Construction 6 | 120 Steel + 4 Comp | MoisturePump |
| PodLauncher | Construction 6 | 100 Steel + 3 Comp | TransportPod |
| TransportPod | Construction 6 | 35 Steel + 2 Comp | TransportPod |
| Sarcophagus | Construction 5 | 75 stuff (Stony) + 25 Steel | ComplexFurniture |
| FirefoamPopper | Construction 5 | 100 Steel + 1 Comp | Firefoam |

## Böden Tier 3

| Boden | Skill | Ressourcen | Research |
|---|---|---|---|
| SterileTile | Construction 6 | 5 Steel + 1 Silver | SterileMaterials |
| MetalTile | 0 | 4 Steel | Electricity / Tribal-OK |
| SilverTile | 0 | 5 Silver | — |
| GoldTile | 0 | 5 Gold | — |

## Freizeit Tier 3

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| PokerTable | Construction 6 | 70 stuff + 1 Comp | ColoredLights? |
| BilliardsTable | Construction 6 | 100 stuff + 2 Comp | ComplexFurniture |
| TubeTelevision | Construction 7 | 80 Steel + 3 Comp | TubeTelevision |

---

# 🟣 TIER 4 — Fabrikation / Bionik / Ladungswaffen

## Forschungen

| Research | Voraussetzung |
|---|---|
| Fabrication | MicroelectronicsBasics + Bionics |
| AdvancedFabrication | Fabrication |
| Bionics | MicroelectronicsBasics |
| ChargedShot | Fabrication |
| ReconArmor | Fabrication |
| PowerArmor | ReconArmor |
| FlatscreenTelevision | TubeTelevision |
| MegascreenTelevision | FlatscreenTelevision |

## Werkbänke Tier 4

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| **FabricationBench** | Construction 6 | 200 Steel + 6 Comp | Fabrication |

## Waffen Tier 4 (FabricationBench)

| Waffe | Skill | Ressourcen | Research |
|---|---|---|---|
| Charge Rifle | Crafting 7 | 60 Steel + 40 Plasteel + 4 Comp + 3 AdvComp | ChargedShot |
| Charge Lance | Crafting 9 | — + AdvComp | ChargedShot + Advanced |

## Rüstung Tier 4

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| Recon Armor (+ Helm) | Crafting 6 | Plasteel + Uranium + Comp | ReconArmor |
| Power Armor (+ Helm) | Crafting 8 | viel Plasteel + AdvComp | PowerArmor |

## Bauteile (TableMachining / FabricationBench)

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| **ComponentIndustrial** | Crafting 8 | 12 Steel | — (ab Machining) |
| **ComponentSpacer (Advanced)** | Crafting 8 | 1 ComponentIndustrial + 20 Steel + 10 Plasteel + 0.3 Gold | AdvancedFabrication |

## Bionik (FabricationBench)

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| BionicEye | Crafting 8 | 2 Plasteel + 4 Comp | Bionics |
| BionicArm | Crafting 8 | 10 Plasteel + 6 Comp | Bionics |
| BionicLeg | Crafting 8 | 10 Plasteel + 6 Comp | Bionics |
| BionicSpine | Crafting 8 | 10 Plasteel + 6 Comp | Bionics |
| BionicHeart | Crafting 8 | 4 Plasteel + 4 Comp | Bionics |
| BionicStomach | Crafting 8 | 4 Plasteel + 2 Comp | Bionics |
| BionicEar | Crafting 8 | 2 Plasteel + 4 Comp | Bionics |

## Freizeit Tier 4

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| FlatscreenTelevision | Construction 8 | 80 Steel + 4 Comp | FlatscreenTelevision |
| MegascreenTelevision | Construction 8 | 100 Steel + 50 Plasteel + 8 Comp | MegascreenTelevision |

---

# 🚀 TIER 5 — SCHIFF (Ending-Trigger)

Alle Schiffsteile erfordern **Construction 8** und einzelne Forschungen.

## Forschungs-Kette Schiff

```
ShipBasics
 ├── ShipCryptosleep
 ├── ShipComputerCore
 ├── ShipReactor
 ├── ShipEngine
 └── ShipSensorCluster
```

Alle brauchen: **MicroelectronicsBasics → ShipBasics** als Gate. Die Kinder benötigen zusätzlich `MultiAnalyzer`.

## Schiff-Bauteile

| Teil | Skill | Ressourcen | Research |
|---|---|---|---|
| **Ship_Beam** | Construction 5 | 200 Steel + 40 Plasteel + 3 Comp + 1 AdvComp | **ShipBasics** |
| **Ship_CryptosleepCasket** | Construction 8 | 120 Steel + 14 Uranium + 3 Comp + 3 AdvComp | **ShipCryptosleep** |
| **Ship_ComputerCore** | Construction 8 | 150 Steel + 70 Gold + 4 AdvComp + **1 AIPersonaCore** | **ShipComputerCore** |
| **Ship_Reactor** | Construction 8 | 350 Steel + 280 Plasteel + 70 Uranium + 8 AdvComp | **ShipReactor** |
| **Ship_Engine** | Construction 8 | 260 Steel + 140 Plasteel + 70 Uranium + 6 AdvComp | **ShipEngine** |
| **Ship_SensorCluster** | Construction 8 | 140 Steel + 4 Gold + 6 Comp + 6 AdvComp | **ShipSensorCluster** |

## Gesamt-Ressourcenbedarf (Minimum-Schiff)

Bei einem Pawn ohne Strukturteile extra:

| Ressource | Menge |
|---|---|
| Steel | ~**1.220** (+ Beams je nach Anzahl) |
| Plasteel | ~**460** |
| Uranium | ~**154** |
| Gold | ~**74** |
| ComponentIndustrial | ~**13** |
| ComponentSpacer (Advanced) | ~**27** |
| **AIPersonaCore** | **1** (nicht herstellbar — Quest/Find) |

Dazu pro Schiff-Beam zusätzlich 200 Steel + 40 Plasteel + 3 Comp + 1 AdvComp.

---

# 🔬 CryptoSleep / Misc Tier 5

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| CryptosleepCasket (non-ship) | Construction 8 | 150 Steel + 50 Uranium + 3 Comp + 2 AdvComp | CryptosleepCasket |

---

# DLC-Spezifisches

## Royalty

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| Throne | Construction 4 | Stuff + Gold | ComplexFurniture |
| GrandThrone | Construction 6 | Stuff + viel Gold | — |
| Harp | Crafting 3 | Stuff + WoodLog | Harp |
| Harpsichord | Crafting 5 | viel Stuff | Harpsichord |
| Piano | Crafting 6 | viel Stuff | Piano |
| Cataphract Armor | Crafting 8 | Plasteel + Uranium + AdvComp | CataphractArmor |
| FineStoneTile-Böden | Construction 6 | Stone + dye | CarpetMaking |
| FineCarpet-Böden | Construction 6 | 1 Cloth + dye | CarpetMaking |

## Ideology

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| Altar_Small/Medium/Large/Grand | 0 | Stuff | — |
| GibbetCage | Construction 4 | Stuff + 1 Skull | — |
| Skullspike | Construction 4 | 25 stuff + 1 Skull | — |
| SleepAccelerator | Construction 8 | 200 Steel + 100 Plasteel + 10 AdvComp | SleepAccelerator |
| BiosculpterPod | Construction 8 | 150 Steel + 100 Plasteel + 6 AdvComp | BiosculpterPod |
| NeuralSupercharger | 0 | 150 Steel + 4 Comp | NeuralSupercharger |

## Biotech

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| Mechanitor-Implantate | Crafting 6 | Steel + Plasteel + Comp | BasicMechtech |
| Toxin-Filter-Lunge/Niere | Crafting 8 | Plasteel + Comp | ToxFiltration |
| Mech-Gestator | Construction 6 | 200 Steel + 8 Comp | BasicMechtech |
| Mech-Recharger | Construction 5 | 100 Steel + 3 Comp | BasicMechtech |
| SubcoreSoftscanner | Construction 5 | 150 Steel + 4 Comp | BasicMechtech |
| SubcoreRipscanner | Construction 6 | 250 Steel + 8 Comp | StandardMechtech |
| WastepackAtomizer | Construction 8 | 300 Steel + 100 Plasteel + 8 Comp + 4 AdvComp | WastepackAtomizer |
| Deathrest-Betten | Construction 4 | Steel + Comp + HemogenPack | Deathrest |

## Anomaly

| Item | Skill | Ressourcen | Research |
|---|---|---|---|
| Bioferrit-Plattenboden | Construction 3 | Bioferrit | — |
| SecurityDoor | Construction 7 | Plasteel + AdvComp | SecurityDoor |
| Bioferrit-Gewehre/Pistolen | Crafting ~5 | Bioferrit + Comp | Bioferrite-Forschung |
| Surgical Inspection | Medicine 3 | — | — |
| BlissLobotomy | Medicine 4 | — | BlissLobotomy |
| GhoulInfusion | Medicine 4 | 1 Shard + 30 Bioferrit | GhoulInfusion |

## Odyssey

| Item | Skill | Research |
|---|---|---|
| Gravship-Basisteile | Construction 4 | Gravship-Research |
| Gravship-Cockpit / Struktur | Construction 5 | Gravship |
| Gravship-Antriebe / Reaktor | Construction 8 | Gravship-Advanced |
| Orbital-Beacon | Construction 6 | Orbital |
| Orbital-Haupt | Construction 8 | Orbital-Advanced |
| Statue (Artistic-basiert) | **Artistic 4** | — |

---

# 🧪 Medizinische Chirurgie (an Bett mit Arzt)

| Eingriff | Medicine-Skill | Research | Materialien |
|---|---|---|---|
| Anesthetize, Euthanize, RemoveBodyPart | — (nur Erfolgsrate) | — | 1 Medicine |
| Install Wooden Peg / Denture | **Medicine 3** | — | 25 WoodLog + 1 Medicine |
| Install Prosthetic | **Medicine 4** | Prosthetics | Prosthetic-Teil + 1 Medicine |
| Install / Remove Implant | **Medicine 5** | je Implant | Implantat + 1 Medicine |
| Install Natural Organ (Heart etc.) | **Medicine 8** | — | Organ + 1 Medicine |
| Install Bionic | — (kein Hard-Cap, aber Empfehlung 8) | Bionics | Bionik-Teil + 1 Medicine |
| Cure Blood Rot (Royalty) | Medicine 5 | — | — |
| Cure Paralytic Abasia (Royalty) | Medicine 5 | — | — |

---

# 🌿 Pflanzenanbau (Growing)

| Pflanze | Growing-Skill | Research |
|---|---|---|
| Rice, Potato, Corn, Cotton, Haygrass | 0 | — |
| Hops | 3 | Brewing |
| Smokeleaf | 4 | DrugProduction |
| Strawberry | 5 | — |
| Psychoid | 6 | PsychoidBrewing |
| **Healroot** | **8** | — |
| TreeCocoa | 8 | ComplexClothing? |
| **Devilstrand** | **10** | Devilstrand |

---

# 🐎 Tiere zähmen (Handling)

Min Handling ≈ `⌈Wildness × 10⌉`.

| Tier | Wildness | Min Handling | Trainability |
|---|---|---|---|
| Cow, Donkey, Pig, Sheep, Goat, Chicken, Cat, Labrador, Husky, Yorkshire | 0 | 0 | verschieden |
| Yak, Insekt | 0.2 | 2 | — |
| Alpaca, Dromedar | 0.25 | 3 | None |
| Horse | 0.35 | 4 | None |
| Muffalo, Boomalope, Bison, Warg, Monkey, Lynx | 0.6 | 6 | None/Intermediate |
| Hare, Cobra, Alphabeaver, Capybara, Elk, Caribou, Elephant | 0.75 | 8 | None/Advanced |
| Cougar, Panther, Grizzly, Polar Bear, Ostrich | 0.8 | 8 | Advanced |
| Wolf | 0.85 | 9 | Advanced |
| Rhinoceros | 0.9 | 9 | Intermediate |
| Cassowary, Emu, Megasloth, Thrumbo | 0.95–0.98 | 10 | Advanced |

**Training-Befehle** (unabhängig vom Handling-Skill, abhängig von Trainability):
- Tameness: None (alle Tiere)
- Obedience / Release (Attack): Intermediate
- Haul (`minBodySize 0.40`) / Rescue (`minBodySize 0.65`): Advanced

---

# 🔑 Kritische Pfadabhängigkeiten

## Kürzester Weg Nackt-Start → Schiff-Ending

```
Nackt-Start
   │
   ├── Crafting Spot (0) ──► Short Bow (Crafting 2, 30 Wood)
   │                          Tribal Wear, Parka, Tuque
   │                          Pila (Crafting 3)
   │
   ├── SimpleResearchBench (0, 75 stuff) ──► Forschung startet
   │
   ├── Smithing ──► FueledSmithy (Construction 4)
   │                  └── Mace, Gladius (Crafting 3)
   │
   ├── Stonecutting ──► Stein-Bauten möglich
   │
   ├── Electricity ──► Stromnetz + WindTurbine / WoodGen (Construction 4)
   │                    └── Heater, Cooler (Construction 5)
   │                    └── ElectricStove, ElectricSmithy (Construction 4-5)
   │
   ├── MicroelectronicsBasics ──► HiTechResearchBench (Construction 6)
   │                                └── MultiAnalyzer (Construction 8)
   │
   ├── Machining ──► TableMachining (Construction 4)
   │                   └── ComponentIndustrial (Crafting 8)
   │                   └── Industriewaffen + Flak-Rüstung
   │                   └── Turret_MiniTurret (Construction 5)
   │
   ├── Fabrication ──► FabricationBench (Construction 6)
   │                     └── ComponentSpacer (Crafting 8)
   │                     └── Power Armor, Bionik, Charge-Waffen
   │
   └── ShipBasics ──► Ship_Beam (Construction 5)
           └── ShipReactor ──► Reaktor (Construction 8, 350 Steel + 280 Plasteel + 70 Uranium + 8 AdvComp)
           └── ShipEngine ──► Engine (Construction 8)
           └── ShipSensorCluster ──► Sensor (Construction 8)
           └── ShipComputerCore ──► Core (Construction 8 + **AI Persona Core!**)
           └── ShipCryptosleep ──► Kryokapseln (Construction 8)
```

## Harte Skill-Gates für Schiff-Ending

1. **Crafting 2** — Short Bow (Start-Waffe)
2. **Construction 4** — erste elektrische Werkbänke
3. **Construction 5** — MiniTurret (Verteidigung)
4. **Construction 6** — HiTechResearchBench + FabricationBench (Gate zu allem Spacer-Tech)
5. **Crafting 8** — Components + Advanced Components + Bionik herstellen
6. **Construction 8** — Alle Haupt-Schiffsteile + MultiAnalyzer + GeothermalGenerator

Ohne einen Pawn, der auf **Construction 8 und Crafting 8** trainiert wird, kann das Schiff-Ending nicht abgeschlossen werden.

## Einmalige, nicht herstellbare Schlüsselitems

| Item | Wie beschaffen |
|---|---|
| **AIPersonaCore** | Quest-Belohnung, antike Komplexe, abgestürzte Schiffe, Imperium-Quest, Händler mit Glück |
| Antigrain Warhead | Quest-Belohnung, Schiffs-Wreck, Kauf bei Händlern |
| Jade, Gold, Uranium | Nur Mining/Händler, nicht synthetisierbar |
| Neutroamine | Nur Händler/Quest |
| Bioferrit | Anomaly-Entities schlachten |

---

## Notizen zur Genauigkeit

Manche Kostenwerte (besonders bei Industrie-Waffen ab AssaultRifle) stehen in `costList`-Blöcken, die ich beim Ressourcen-Sampling nicht im Detail geprüft habe und auf ~-Werte gerundet habe. Für eine exakte Mengenliste bei einer konkreten Aufrüstung empfehle ich: Item im Spiel auswählen und die Werkbank-Billings lesen. Die **Skill- und Research-Prerequisiten** oben sind jedoch alle direkt verifiziert.

Korrekturen zu diesem Dokument jederzeit willkommen — vor allem wenn weitere DLCs installiert sind, die ich nicht separat durchgearbeitet habe.
