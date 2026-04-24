# Re-Review Round 2 — RimWorld-Persona

**Datum:** 2026-04-24
**Scope:** Pass-2-Revision nach Round 1 APPROVE-WITH-CHANGES. Review von `api-reference.md`, Story 1.12, 3.4, 7.5–7.18.
**Verdict:** APPROVE-WITH-CHANGES

## Round-1-Findings-Status

- **CC-STORIES-08 (API-Reality):** PARTIAL
  - RESOLVED: `JobDefOf.TendPatient` korrekt in 3.4 AC5 + api-reference.md Zeile 18. `Tend`-Falle explizit adressiert. Stichprobe: `TendPatient`, `BeatFire`, `Rescue`, `HarvestDesignated`, `LayDown`, `DoBill` existieren tatsächlich in `Data/Core/Defs/JobDefs/Jobs_Misc.xml` und `Jobs_Work.xml`.
  - NEW ISSUE (HIGH, siehe Finding 1): IncidentDefs-Block in api-reference.md Zeile 73–81 ist **kategorisch falsch** — `GameEnded_ShipEscape`, `GameEnded_StellarchSafe`, `GameEnded_ArchonexusVictory` existieren nicht. Korrekt sind **QuestScriptDefs** `EndGame_ShipEscape`, `EndGame_StellarchSafe`, `EndGame_ArchonexusVictory` (verifiziert in `Core/Defs/QuestScriptDefs/Script_EndGame_ShipEscape.xml` mit `defName=EndGame_ShipEscape`, `isRootSpecial=true`, `autoAccept=true`).
  - NEW ISSUE (MED, siehe Finding 2): Quelle-Spalte der JobDef-Tabelle nennt Dateien wie `Jobs_Firefight.xml`, `Jobs_Medical.xml`, `Jobs_Carry.xml`, `Jobs_Plants.xml`, `Jobs_Rest.xml`, `Jobs_Movement.xml`, `Jobs_Fighting.xml`, `Jobs_Construct.xml` — **keine davon existiert**. Tatsächliche Jobs_*.xml: nur `Jobs_Animal`, `Jobs_Caravan`, `Jobs_Gatherings`, `Jobs_Joy`, `Jobs_Misc`, `Jobs_Work`. Die defNames sind korrekt, die Datei-Zuordnung ist erfunden.

- **CC-STORIES-09 (Quest-API):** RESOLVED
  - Story 1.12 sauber strukturiert: `Find.QuestManager.QuestsListForReading` + `QuestOfferEvent(int questId, …)` + `lastSeenQuestIds` persistent.
  - 7.9 AC1 referenziert QuestOfferEvent-Consumption, 7.9 AC3 `int questId` statt `Quest`-Ref (D-23-konform).
  - 7.7 Vorausgesetzt-Liste enthält 1.12. 7.12 dito. 7.15 dito.
  - H6 bleibt korrekt als Fallback-Only für Finale-Dialog-Events dokumentiert.

- **CC-STORIES-05 (DLC-Guards):** PARTIAL
  - RESOLVED: 7.5 (Ship), 7.6 (Ship), 7.7 (Ship), 7.8 (Ship), 7.13 (Royal), 7.14 (Ideology), 7.17 (Void), 7.18 (Void) haben explizite DLC-Guard-AC am Plan-Anfang.
  - UNFIXED (MED, Finding 3): **7.12 (Royal Honor-Farm) hat KEINEN AC-gekennzeichneten DLC-Guard-Entry** — lediglich AC2 „Nur aktiv wenn `DlcCapabilities.HasRoyalty`" als Durchschnitts-Constraint, aber nicht in der CC-STORIES-05-Konvention („am Plan-Anfang"/Early-Return). Inkonsistent mit 7.13.
  - UNFIXED (MED, Finding 4): **7.16 (Monolith)** hat ebenfalls keinen strukturierten Guard mit Early-Return — nur AC1 „aktiv wenn HasAnomaly + Monolith vorhanden". Keine `if (!EndingAvailable(Ending.Void)) return;`-AC.
  - 7.11 (Weltkarten-Reise) und 7.10 (Caravan-Prep) haben keinen DLC-Guard — Journey ist Vanilla, akzeptabel; aber keine explizite AC-Begründung.

## Neue Findings (Round 2)

**HIGH-1 — Ending-Trigger falsch typisiert:** `api-reference.md` ordnet Ending-Trigger als `IncidentDef` ein. Tatsächlich sind `EndGame_ShipEscape` (Core), `EndGame_ArchonexusVictory` (Ideology) und `Script_EndGame_StellarchSafe` (Royalty, angenommen) **QuestScriptDefs**. Die Unterscheidung ist nicht kosmetisch: `Find.IncidentDefList.GetNamed("GameEnded_ShipEscape")` gibt `null`; korrekt ist `DefDatabase<QuestScriptDef>.GetNamed("EndGame_ShipEscape")`, gestartet via `QuestUtility.GenerateQuestAndMakeAvailable(scriptDef, points)`. Story 7.8 AC5 („Ship-Start-Event akzeptieren") und 7.1 Feasibility-Logic sind daraufhin auf falschem API-Verständnis. Fix: api-reference.md Zeile 73–81 korrigieren + 7.8/7.1/7.15 Dev-Notes Update.

**HIGH-2 — `ThingDefOf.Heater`/`Cooler`/`Wall`/`Campfire`/`Door`/`Bed`/`PowerConduit`/`AIPersonaCore` ungeprüft:** Tabelle behauptet `ThingDefOf`-Member für diese Things, aber einige sind Vanilla nur als `Named`-Lookup verfügbar (`AIPersonaCore` insbesondere ist historisch inkonstant). Nicht verifiziert in diesem Review-Zeitraum — der Dev sollte bei Story 3.6/3.8/6.1/7.7-Dev-Start im Dev-Mode `DefDatabase<ThingDef>.GetNamed(name)` probieren und bei Fehlschlag auf `Named`-Lookup umstellen. Risiko: Compile-Error wie bei `Tend`.

**MED-1 — JobDef-Datei-Quellen erfunden:** Siehe CC-STORIES-08 PARTIAL. Dev wird bei Debug verwirrt wenn er die genannten Dateien öffnen will.

**MED-2 — 7.12 + 7.16 DLC-Guard-Pattern nicht CC-STORIES-05-konform:** Siehe oben.

**LOW-1 — Odyssey-TBV trivial auflösbar:** `Ludeon.RimWorld.Odyssey` ist in 10s aus `Data/Odyssey/About/About.xml` verifizierbar. TBV-Markierung ist dokumentations-lax, akzeptabel als blocker nur dann wenn nicht bis Story-1.2-Dev-Start gefixt. **Nicht-Blocker für Round-2-Approve.**

**LOW-2 — api-reference.md doppelte AC-Nummerierung:** 7.5, 7.6, 7.7, 7.13, 7.14 etc. haben nach dem DLC-Guard-Add einen doppelten AC-Index („1. … 2. …" zweimal). Kein Technik-Risiko, aber auffallend beim Lesen — Review-Gate-Confusion möglich.

## Unchanged-Risks

- **Story 7.5 Research-Chain-defNames** (`ShipBasics`, `ShipReactor`, `ShipStructural`) sind **nicht gegen `ResearchProjects_5_Ship.xml` verifiziert** in diesem Review (Zeit). Dev muss bei 7.5-Start Gegenprüfen.
- **`Quest.Accept()`-Signatur in api-reference.md Zeile 90** ist vage („genaue Zeichenfolge je Quest-Type") — 7.9 Auto-Accept und 7.15 Archonexus-Quest-Accept brauchen die konkrete API-Signatur. Risiko für Dev-Overrun.
- **F-STAB-06-Analog für `poisonedQuestIds`** ist in 7.9 referenziert aber nicht in api-reference.md Scribe-Tabelle gelistet. Minor-Gap.

## Recommendation

**APPROVE-WITH-CHANGES.** Round-1-CC-STORIES-09 ist voll RESOLVED, aber CC-STORIES-08 und CC-STORIES-05 sind nur PARTIAL. Zwei neue HIGH-Findings (Ending-DefType-Verwechslung, ungeprüfte ThingDefOf-Member) blockieren Dev-Start von 7.1/7.8/3.6/3.8 bis Korrektur. Blocker-Fix-Scope ist eng: api-reference.md Zeile 41–61 und 73–81 korrigieren, 7.12/7.16 DLC-Guard-AC nachziehen, Odyssey-TBV trivial resolven. Keine Story-Struktur-Änderungen nötig. Ready-for-dev nach Pass-2-Sub-Revision.
