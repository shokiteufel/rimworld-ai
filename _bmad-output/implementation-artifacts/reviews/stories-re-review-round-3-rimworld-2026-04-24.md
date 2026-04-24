# Re-Review Round 3 — RimWorld-Persona

**Datum:** 2026-04-24
**Scope:** Pass-3-Verifikation (D-33) gegen Round-2-Findings
**Verdict:** APPROVE

## Round-2-Findings-Status (nach Pass 3)

- **QuestScriptDef-Korrektur:** **RESOLVED**
  - `api-reference.md` Section "QuestScriptDefs (Endings)" existiert (Z. 79-91) mit `EndGame_ShipEscape`, `EndGame_ArchonexusVictory`, `EndGame_VoidAwakening` und korrekten Datei-Pfaden `Core/Defs/QuestScriptDefs/Script_EndGame_*.xml`.
  - Alte `IncidentDef GameEnded_*`-Referenzen entfernt. Explicit-Note Z. 91: `DefDatabase<QuestScriptDef>.GetNamedSilentFail(...)` statt `IncidentDefOf.GameEnded_*`. `IncidentDef.GameEnded` korrekt als allgemeiner Credits-Trigger markiert, nicht Ending-spezifisch.
  - Royal-Ending sauber als **TBV** markiert (Z. 89) mit Hinweis auf `QuestNode_RoyalAscent`-Chain in Royalty-DLC.

- **ThingDefOf-TBV:** **RESOLVED**
  - ThingDefOf-Tabelle (Z. 47-67) hat **TBV**-Marker an jedem relevanten `ThingDefOf.X`-Eintrag (Heater, Cooler, Wall, Campfire, Door, Bed, PowerConduit, AIPersonaCore).
  - `ThingDef.Named("<defName>")`-Fallback explizit pro Zeile gelistet.
  - Reflection-Check-Instruktion (Z. 45): `typeof(RimWorld.ThingDefOf).GetField("<Name>") != null`.
  - "Offene TBVs"-Section (Z. 138) hat NEW-Round-2-Eintrag zur ThingDefOf-Member-Stabilität + Royal-QuestScriptDef-Hinweis (Z. 141).

- **JobDef-Source-Files:** **RESOLVED**
  - Z. 15 listet korrekt 6 Dateien: `Jobs_Animal.xml`, `Jobs_Caravan.xml`, `Jobs_Gatherings.xml`, `Jobs_Joy.xml`, `Jobs_Misc.xml`, `Jobs_Work.xml`.
  - Z. 16 explizit-Note: Alte fabrizierte Dateien (`Jobs_Firefight.xml`, `Jobs_Medical.xml`, `Jobs_Movement.xml`) existieren NICHT.
  - Alle JobDef-Tabellenzeilen (20-41) ordnen Jobs den 6 realen Dateien korrekt zu.

- **7.12/7.16 DLC-Guards:** **RESOLVED**
  - **7.12 AC 1** (korrekt formuliert): `if (!DlcCapabilities.EndingAvailable(Ending.Royal)) return HonorFarmPlan.Empty;` als allererste Zeile von `Plan()`. Begründung mitgegeben.
  - **7.16 AC 1** (korrekt formuliert): `if (!DlcCapabilities.EndingAvailable(Ending.Void)) { inactive = true; return; }` als allererste Zeile von `Tick()`/`Evaluate()`. Begründung mitgegeben (Monolith-Entity + Void-QuestScriptDef fehlen ohne Anomaly).
  - Beide ACs tragen "HIGH-Fix Round-2 RimWorld, CC-STORIES-05"-Traceability.

- **Odyssey-packageId:** **RESOLVED**
  - Z. 111: `ModsConfig.IsActive("Ludeon.RimWorld.Odyssey")` mit Note "verifiziert 2026-04-24 gegen `Data/Odyssey/About/About.xml` (packageId-Feld)". TBV entfernt.

## Neue Findings (Round 3)

Keine neuen CRIT/HIGH. Eine kosmetische Beobachtung:

- **LOW (nicht blockierend):** Story 7.16 hat doppelte AC-Nummer `2` (Z. 12 "MonolithPhaseRunner aktiv" + Z. 13 "Stage-1-Activation"). Nummerierung rutscht danach; schadet Funktionalität/Traceability nicht, aber sollte bei nächstem Story-Touch begradigt werden. Reine Formatsache — KEIN Revert.

- **Zusatz-Bestätigung (positiv):** Campfire-Location ist in api-reference.md Z. 52 korrekt als `Buildings_Temperature.xml` mit "(korrigiert)"-Marker dokumentiert.

## Recommendation

**APPROVE.** Alle 5 Round-2-Findings sind RESOLVED. Der LOW-Finding zur AC-Nummerierung in 7.16 kann im nächsten Story-Touch mitgenommen werden und blockiert Dev-Start nicht. Stories + api-reference.md sind jetzt ready-for-dev mit stabilen, verifizierten Def-Referenzen und korrekten DLC-Guards.
