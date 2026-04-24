# RimWorld-API-Reference (verifizierte Vanilla-Defs)

**Status:** ready-for-dev
**Version:** 1.0
**Erstellt:** 2026-04-24 (D-31 Pass-2-Revision, CC-STORIES-08)
**Scope:** Kein Story, sondern **Pflicht-Artefakt** vor Dev-Start. Liste aller referenzierten Vanilla-Defs mit exakten `defName`s aus `D:/SteamLibrary/steamapps/common/RimWorld/Data/Core|Royalty|Ideology|Biotech|Anomaly|Odyssey/Defs/`.

**Zweck:** Stories referenzieren Jobs/Things mit generischen Namen (z. B. „Rescue", „Tend", „Cooler"). Ohne verifizierten Def-Name kompiliert der Code nicht (z. B. `Tend` ist `TendPatient`). Diese Datei ist die Autorität.

---

## JobDefs (Core)

**JobDef-Dateien in `Core/Defs/JobDefs/` (verifiziert 2026-04-24 gegen Installation):**
- `Jobs_Animal.xml`, `Jobs_Caravan.xml`, `Jobs_Gatherings.xml`, `Jobs_Joy.xml`, `Jobs_Misc.xml`, `Jobs_Work.xml`
- Alte Split-Namen wie `Jobs_Firefight.xml`, `Jobs_Medical.xml`, `Jobs_Movement.xml` existieren NICHT — Vanilla RimWorld hat alle Non-Joy/Non-Caravan/Non-Gathering/Non-Animal/Non-Work-Jobs in `Jobs_Misc.xml` konsolidiert. `DefOf`-Member kommen unabhängig vom Dateinamen.

| Story-Referenz | Generic-Name | Verifizierter `JobDefOf` | Notizen |
|---|---|---|---|
| 3.3 E-FIRE | BeatFire | `JobDefOf.BeatFire` | Drafted-Fighter-Mode; defName in `Jobs_Misc.xml` |
| 3.4 E-BLEED | Rescue | `JobDefOf.Rescue` | Downed-Pawn-Transport zu Bed; `Jobs_Misc.xml` |
| 3.4 E-BLEED | Tend | **`JobDefOf.TendPatient`** | NICHT `Tend` (das gibt Compile-Error); Doctor auf Patient; `Jobs_Work.xml` |
| 3.5 E-FOOD | Ingest | `JobDefOf.Ingest` | Eat-Job; `Jobs_Misc.xml` |
| 3.5 E-FOOD | Hunt | `JobDefOf.Hunt` | Jäger auf Wild; `Jobs_Work.xml` |
| 3.5 E-FOOD | Harvest | `JobDefOf.Harvest` | Wilde-Pflanzen-Ernte; `Jobs_Work.xml` |
| 3.5 E-FOOD | HarvestDesignated | `JobDefOf.HarvestDesignated` | Manuell-Designated; `Jobs_Work.xml` |
| 3.6 E-SHELTER | LayDown | `JobDefOf.LayDown` | Schlafen im Bed; `Jobs_Misc.xml` |
| 3.6 E-TEMP | GotoWander | `JobDefOf.GotoWander` | Movement-zu-Warm-Area; `Jobs_Misc.xml` |
| 4.3 Recruiting | — | `PrisonerInteractionModeDefOf.AttemptRecruit` | NICHT `JobDef`; Mode-Assignment |
| 4.3 Recruiting | Warden-Work | `WorkTypeDefOf.Warden` | WorkType (Priority), kein Job |
| 5.4 Draft | — | `pawn.drafter.Drafted = true/false` | `Verse/Pawn_DraftController` API — kein JobDef |
| 5.4 Draft | Goto | `JobDefOf.Goto` | Move-zu-Cell für Retreat-Point; `Jobs_Misc.xml` |
| 5.5 Flee | FormCaravan | — | `CaravanFormingUtility.StartFormingCaravan(…)` — Utility-Call |
| 5.7 Focused-Fire | Attack (Melee) | `JobDefOf.AttackMelee` | Melee-only; `Jobs_Misc.xml` |
| 5.7 Focused-Fire | Attack (Static) | `JobDefOf.AttackStatic` | Ranged-with-target für Focused-Fire; `Jobs_Misc.xml` |
| 4.9a E-MOOD | SocialRelax | `JobDefOf.SocialRelax` | Joy-Activity; `Jobs_Joy.xml` |
| 4.9b E-HEALTH | TendPatient | `JobDefOf.TendPatient` | siehe 3.4 |
| 4.9g E-PAWNSLEEP | LayDown | `JobDefOf.LayDown` | siehe 3.6 |
| 3.9 BillManager | DoBill | `JobDefOf.DoBill` | Workbench-Bill-Execution; `Jobs_Work.xml` |
| 3.8 BuildPlanner | PlaceBlueprint | — | `GenConstruct.PlaceBlueprintForBuild(…)` Utility-Call |
| 3.8 BuildPlanner | ConstructStructure | `JobDefOf.PlaceNoCostFrame`/`FinishFrame` | Vom Vanilla-Constructor-Workgiver getriggert; `Jobs_Work.xml` |

## ThingDefs (Core)

**WICHTIG — `ThingDefOf`-Membership TBV:** Ob ein ThingDef als statisches Member auf `ThingDefOf` existiert oder nur per `ThingDef.Named("…")` erreichbar ist, ist **RimWorld-Version-abhängig** und NICHT stabil (Ludeon ergänzt/entfernt Members zwischen 1.4→1.5→1.6). Die folgende Tabelle nennt den defName (stabil via XML) + **bekannten Stand** der `ThingDefOf`-Membership. **Dev MUSS bei Story-1.2/3.8 vor erstem Build verifizieren**: `typeof(RimWorld.ThingDefOf).GetField("<Name>") != null`. Bei Miss: `ThingDef.Named("<defName>")` als Fallback (defName ist stabil, ThingDefOf-Member nicht).

| Story-Referenz | Generic-Name | Bevorzugt + Fallback | `defName` (verifiziert via XML) | Quelle | DLC-Check |
|---|---|---|---|---|---|
| 3.6 E-TEMP | Heater | `ThingDefOf.Heater` **TBV** / `ThingDef.Named("Heater")` | `Heater` | `Core/Defs/ThingDefs_Buildings/Buildings_Temperature.xml` | Vanilla |
| 3.6 E-TEMP | Cooler | `ThingDefOf.Cooler` **TBV** / `ThingDef.Named("Cooler")` | `Cooler` | `Core/Defs/ThingDefs_Buildings/Buildings_Temperature.xml` | Vanilla |
| 3.6 Wall | Wall | `ThingDefOf.Wall` **TBV** / `ThingDef.Named("Wall")` | `Wall` | `Core/Defs/ThingDefs_Buildings/Buildings_Structure.xml` | Vanilla (stuff-based) |
| 3.8 BuildPlanner | Campfire | `ThingDefOf.Campfire` **TBV** / `ThingDef.Named("Campfire")` | `Campfire` | `Core/Defs/ThingDefs_Buildings/Buildings_Temperature.xml` (korrigiert) | Vanilla |
| 3.8 BuildPlanner | Door | `ThingDefOf.Door` **TBV** / `ThingDef.Named("Door")` | `Door` | `Core/Defs/ThingDefs_Buildings/Buildings_Structure.xml` | Vanilla |
| 3.8 BuildPlanner | Bed | `ThingDefOf.Bed` **TBV** / `ThingDef.Named("Bed")` | `Bed` | `Core/Defs/ThingDefs_Buildings/Buildings_Furniture_Beds.xml` | Vanilla |
| 6.1 Power | PowerConduit | `ThingDefOf.PowerConduit` **TBV** / `ThingDef.Named("PowerConduit")` | `PowerConduit` | `Core/Defs/ThingDefs_Buildings/Buildings_Power.xml` | Vanilla |
| 6.1 Power | WoodFiredGenerator | `ThingDef.Named("WoodFiredGenerator")` | `Core/Defs/ThingDefs_Buildings/Buildings_Power.xml` | Vanilla; nicht in `ThingDefOf` — Named-Lookup |
| 6.1 Power | SolarGenerator | `ThingDef.Named("SolarGenerator")` | `Core/Defs/ThingDefs_Buildings/Buildings_Power.xml` | Vanilla |
| 6.1 Power | WindTurbine | `ThingDef.Named("WindTurbine")` | `Core/Defs/ThingDefs_Buildings/Buildings_Power.xml` | Vanilla |
| 6.1 Power | GeothermalGenerator | `ThingDef.Named("GeothermalGenerator")` | `Core/Defs/ThingDefs_Buildings/Buildings_Power.xml` | Vanilla |
| 6.9 Turret | Turret_Mini | `ThingDef.Named("Turret_Mini")` | `Core/Defs/ThingDefs_Buildings/Buildings_Security.xml` | Vanilla |
| 6.3 HiTech | HiTechResearchBench | `ThingDef.Named("HiTechResearchBench")` | `Core/Defs/ThingDefs_Buildings/Buildings_Production.xml` | Vanilla |
| 6.3 HiTech | MultiAnalyzer | `ThingDef.Named("MultiAnalyzer")` | `Core/Defs/ThingDefs_Buildings/Buildings_Production.xml` | Vanilla |
| 7.5 Ship | Ship_Reactor | `ThingDef.Named("Ship_Reactor")` | `Core/Defs/ThingDefs_Buildings/Buildings_Ship.xml` | Vanilla; Royalty entfernt |
| 7.5 Ship | Ship_ComputerCore | `ThingDef.Named("Ship_ComputerCore")` | `Core/Defs/ThingDefs_Buildings/Buildings_Ship.xml` | Vanilla |
| 7.5 Ship | Ship_Engine | `ThingDef.Named("Ship_Engine")` | `Core/Defs/ThingDefs_Buildings/Buildings_Ship.xml` | Vanilla |
| 7.5 Ship | Ship_CryptosleepCasket | `ThingDef.Named("Ship_CryptosleepCasket")` | `Core/Defs/ThingDefs_Buildings/Buildings_Ship.xml` | Vanilla |
| 7.7 AIPersonaCore | AIPersonaCore | `ThingDefOf.AIPersonaCore` **TBV** / `ThingDef.Named("AIPersonaCore")` | `AIPersonaCore` | `Core/Defs/ThingDefs_Items/Items_Special.xml` | Vanilla (auch wenn Ending-Route verschieden) |

## ResearchProjectDefs (Ship-Chain, 7.5)

| Name | `defName` | Prerequisite | DLC |
|---|---|---|---|
| Ship Basics | `ShipBasics` | Electricity | Vanilla |
| Ship Computer | `ShipComputerCore` | ShipBasics + Microelectronics | Vanilla |
| Ship Reactor | `ShipReactor` | ShipBasics + Nuclear | Vanilla |
| Ship Cryptosleep | `Cryptosleep` | Microelectronics | Vanilla |
| Ship Structural | `ShipStructural` | ShipReactor | Vanilla |

## QuestScriptDefs (Endings — KORRIGIERT Round-2-Review)

**Wichtig** (HIGH-Fix Round-2 RimWorld-Persona): Endings sind NICHT `IncidentDef`-Instanzen, sondern **`QuestScriptDef`** in `Core/Defs/QuestScriptDefs/Script_EndGame_*.xml`. Verifiziert 2026-04-24 gegen Installation.

| Story | `QuestScriptDef.defName` | Datei | DLC |
|---|---|---|---|
| 7.1/7.8 Ship-Ending | **`EndGame_ShipEscape`** | `Core/Defs/QuestScriptDefs/Script_EndGame_ShipEscape.xml` | Vanilla (Ship-Reactor-Aktivierung als Trigger) |
| 7.1/7.15 Archonexus | **`EndGame_ArchonexusVictory`** | `Core/Defs/QuestScriptDefs/Script_EndGame_ArchonexusVictory.xml` | Ideology |
| 7.1/7.18 Void | **`EndGame_VoidAwakening`** | `Core/Defs/QuestScriptDefs/Script_EndGame_VoidAwakening.xml` | Anomaly |
| 7.1 Journey | — (Quest-Offer-basiert via `Find.QuestManager`, kein EndGame-Script) | — | Vanilla +Royalty |
| 7.1/7.13 Royal | **TBV** (evtl. via `QuestNode_RoyalEnding` o.ä., nicht als eigenes `EndGame_*`-Script gefunden) | `Royalty/Defs/QuestScriptDefs/…` | Royalty |

**Feasibility-Engine (Story 7.1) konsumiert diese via:** `DefDatabase<QuestScriptDef>.GetNamedSilentFail("EndGame_ShipEscape")` — NICHT `IncidentDefOf.GameEnded_*`. `IncidentDef.GameEnded` ist ein allgemeiner Credits-Roll-Trigger, nicht Ending-spezifisch.

## QuestManager-API (1.12, 7.9)

| Use-Case | API | Notizen |
|---|---|---|
| Quest-Offers iterieren | `Find.QuestManager.QuestsListForReading` | Primär-Pfad (RimWorld 1.3+) |
| Quest per ID | `Find.QuestManager.QuestsListForReading.FirstOrDefault(q => q.id == id)` | Resolve after Persistence |
| Quest-State | `Quest.State` enum: `NotYetAccepted`, `Ongoing`, `EndedSuccess`, `EndedFailed`, `EndedOfferExpired`, `EndedUnknownOutcome`, `EndedInvalid` | |
| Quest akzeptieren | `Quest.Accept(QuestPart_Accept-related)` — genaue Zeichenfolge je Quest-Type | |
| Dialog-Events (legacy) | H6 Harmony-Postfix auf `WindowStack.Add` gefiltert auf `Dialog_NodeTree` | Nur Fallback für Finale-Dialog-Events (z. B. Ship-Start) |

## DlcCapabilities (zentrale Guard-API)

| Check | Implementation (Story 1.2-Gelegenheit) |
|---|---|
| `HasRoyalty` | `ModsConfig.RoyaltyActive` |
| `HasIdeology` | `ModsConfig.IdeologyActive` |
| `HasBiotech` | `ModsConfig.BiotechActive` |
| `HasAnomaly` | `ModsConfig.AnomalyActive` |
| `HasOdyssey` | `ModsConfig.IsActive("Ludeon.RimWorld.Odyssey")` — **verifiziert 2026-04-24** gegen `Data/Odyssey/About/About.xml` (packageId-Feld) |
| `EndingAvailable(Ending.Ship)` | `!(ModsConfig.RoyaltyActive && …)` + Vanilla-Condition |
| `EndingAvailable(Ending.Royal)` | `HasRoyalty` |
| `EndingAvailable(Ending.Archonexus)` | `HasIdeology` |
| `EndingAvailable(Ending.Void)` | `HasAnomaly` |
| `EndingAvailable(Ending.Journey)` | Vanilla (kein DLC needed) |

## Scribe-API (Persistenz, 1.3)

| Use-Case | API |
|---|---|
| Primitive persistieren | `Scribe_Values.Look(ref field, "label", default)` |
| Collection persistieren | `Scribe_Collections.Look(ref dict, "label", LookMode.Value, LookMode.Value)` |
| Reference auf Thing | `Scribe_References.Look(ref thing, "label")` — **vermeiden** (D-23: identifier-only) |
| Deep-Serialize IExposable | `Scribe_Deep.Look(ref obj, "label")` |
| Mode-Check | `Scribe.mode == LoadSaveMode.Saving/LoadingVars/PostLoadInit` |

## MainButtonDef (Top-Bar-Buttons, Story 1.4)

**Wichtig** (Round-2-Fix 2026-04-24): Vanilla-MainButtonDef 1.6 hat KEIN Feld `defaultHidden` — frühere Architecture-Doku war falsch. Runtime-XML-Parse-Error bei Verwendung.

Tatsächliche Felder aus `Data/Core/Defs/Misc/MainButtonDefs/MainButtons.xml` (verifiziert 2026-04-24):

| Feld | Typ | Pflicht | Notiz |
|---|---|---|---|
| `defName` | string | ja | Vendor-Prefix empfohlen |
| `label` | string | ja | Lokalisierbar über Language-Keys |
| `description` | string | nein | Tooltip |
| `tabWindowClass` | Type-FullName | für Tab-Buttons | `RimWorldBot.UI.MainTabWindow_BotControl` (Namespace!) |
| `workerClass` | Type-FullName | optional | Alternativer Click-Handler statt TabWindow |
| `order` | int | nein | Vanilla: 1-500. Höhere Werte = weiter rechts. Menu=500, History=80, Factions=90 |
| `defaultHotKey` | KeyCode | nein | z. B. `Tab`, `F1`..`F9` |
| `closesWorldView` | bool | nein | Click schließt World-View |
| `validWithoutMap` | bool | nein | Button auch ohne aktive Map funktional |
| `canBeTutorDenied` | bool | nein | Vanilla: Menu=false, sonst default true |
| `minimized` | bool | nein | Bei true wird Button als Icon-only ohne Label gerendert |
| `buttonVisible` | bool | nein | default true; bei false komplett ausgeblendet |
| `iconPath` | string | nein | relativ zu `Textures/` (z. B. `UI/Buttons/BotIcon` → `Textures/UI/Buttons/BotIcon.png`) |

**Nicht existent** (Architecture-Drift-Findings):
- `defaultHidden` — **FALSCH**, verursacht XML-Parse-Error in 1.6. Verwende `buttonVisible` (default true).

## Persistence-Pattern (D-14, F-STAB-06)

- Pawn-Persistence via `pawn.GetUniqueLoadID()` (string) statt `thingIDNumber` (int; kollidiert zwischen Save-Sessions)
- Thing-Persistence via `thing.ThingID` wenn UniqueLoadID nicht verfügbar
- Resolve bei Use-Site: `Find.Maps.SelectMany(m => m.mapPawns.AllPawns).FirstOrDefault(p => p.GetUniqueLoadID() == id)` — bei null: F-STAB-06 Poisoned-Set

---

## Offene TBVs (To Be Verified bei Dev-Start)

- **ThingDefOf-Member-Stabilität** (NEW Round-2): pro `ThingDefOf.X` Referenz in oben gelisteter Tabelle prüfen `typeof(RimWorld.ThingDefOf).GetField(name) != null`; bei Miss → `ThingDef.Named(defName)` Fallback. defNames sind XML-verifiziert, Membership nicht.
- `ModsConfig.AnomalyActive` Existenz (verifiziert bei RimWorld 1.5 Release 2024; bei 1.6+ erneut prüfen)
- `PrisonerInteractionModeDefOf.AttemptRecruit` Namespace-Stabilität 1.5→1.6
- **Royal-EndGame-QuestScriptDef** (NEW Round-2): es existiert kein `EndGame_RoyalStellarch` o.ä. in Vanilla-Core-QuestScriptDefs. Royal-Ending-Trigger via `QuestNode_RoyalAscent`-Chain aus Royalty-DLC — dev muss beim Story-7.13-Implementation in `Data/Royalty/Defs/QuestScriptDefs/` nachschauen.

## Verifizierungs-Methode

Die oben gelisteten Defs sind aus folgenden Dateien **per Datei-Lookup** in `D:/SteamLibrary/steamapps/common/RimWorld/Data/Core/Defs/` verifiziert (Stand RimWorld 1.5 + 1.6 Released 2024). Bei Dev-Start erneut verifizieren gegen aktuelle Installation; XML-Format ist stabil zwischen 1.3 und 1.6.

**Bei fehlenden Def-Namen:** `DefDatabase<XDef>.AllDefs` im Dev-Mode iterieren und Name-Suche.

## Cross-Cutting-Rule

**Stories verbieten hardcoded-String-Lookups**, wenn `DefOf`-Pattern existiert:
- Richtig: `JobDefOf.TendPatient`
- Falsch: `DefDatabase<JobDef>.GetNamed("TendPatient")`
- Ausnahme: `ThingDef.Named("WoodFiredGenerator")` — weil nicht in `ThingDefOf`

Für DLC-conditional Defs immer DLC-Guard vor Lookup (siehe DlcCapabilities oben).
