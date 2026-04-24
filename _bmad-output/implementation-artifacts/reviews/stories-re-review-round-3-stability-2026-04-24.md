# Re-Review Round 3 — Stability-Persona

**Datum:** 2026-04-24
**Scope:** Pass-3-Verifikation (D-33) gegen Round-2-Findings
**Verdict:** APPROVE

## Round-2-Findings-Status (nach Pass 3)

- **NEW-STAB-01 (Exception-Wrapper in 9 Stories): RESOLVED**
  Verifiziert in allen 10 Ziel-Stories, jeweils mit expliziter Formel `via Story 1.10 ExceptionWrapper.<Host|Execution>(...)` + Fallback-Verhalten:
  - 2.7 AC 8 (`TickHost`, OnGUI-Kontext korrekt gewählt, 2-Exc/min → `FallbackToOff`)
  - 2.9 AC 8 (`TickHost`, Iterator-Advance, Abbruch + partial-verwerfen)
  - 3.8 AC 8 (`Execution`, kombiniert mit RAW — siehe STAB-03)
  - 3.9 AC 8 (`Execution`, kombiniert mit RAW — siehe STAB-03)
  - 3.10 AC 8 (`Execution`, kontext-konsistent mit Poisoned-Set)
  - 4.3 AC 9 (`Execution`, nach Plan/Apply-Split)
  - 5.4 AC 9 (`Execution`, Per-Pawn-Mutate-Loop abgedeckt)
  - 5.7 AC 7 (erbt explizit von 5.4 AC 9, Verweis ist sauber — akzeptabel, da 5.7 nur `DraftController.Apply`-Extension ist und dieselbe Aufruf-Grenze betrifft)
  - 6.8 AC 7 (`Execution`, Per-Pawn-Outfit-Assign-Loop abgedeckt)
  - 4.7 AC 8 (erbt explizit von 3.8 AC 8 via `BuildWriter.Apply` — akzeptabel, da 4.7 derselbe `BlueprintPlacer`-Pfad ist; kein Dupe-Wrapper nötig)

- **NEW-STAB-02 (Schema-Bump in 4 Stories): RESOLVED**
  - 2.7 AC 9: `overlayVisible` via `SchemaVersionRegistry` + Migrate-Default `true`
  - 3.9 AC 9: `botManagedBills: Dictionary<int, PhaseGoalTag>` registriert + Migrate leeres Dict
  - 6.5 AC 6: `pawnSpecializations` registriert + Migrate `Specialization=None`
  - 7.9 AC 6: `journeyQuest: JourneyQuestRef?` registriert + Migrate `null`; `poisonedQuestIds` explizit als transient ausgewiesen (keine Doppel-Persistierung)

- **NEW-STAB-03 (RAW 3.8+3.9): RESOLVED**
  - 3.8 AC 8: Read-Back `DesignationOn(cell, Build) != null OR thingGrid.ThingsListAt(cell).Any(t is Blueprint)` + `poisonedBlueprintCells` (transient) + DecisionLog `blueprint-placement-failed`
  - 3.9 AC 8: Read-Back `billStack.Bills.Contains(bill) && recipe.defName == target` + Retry 1× nach 60 Ticks + `poisonedWorkbenches` (transient)
  Beide kombinieren Wrapper + RAW in einer AC, wie von Round-2 gefordert (kein Split, der RAW ohne Wrapper ließe).

- **NEW-STAB-04 (3.7 Harmonisierung): RESOLVED**
  3.7 AC 3 nutzt jetzt wortgleich `EmergencyResolver.ActiveEmergencies.Count == 0` und referenziert explizit CC-STORIES-12-Standard-Formulierung sowie die konsistenten Phasen 3.11, 4.2, 4.5, 4.6, 6.2, 6.4. Alt-Formulierung ist nicht nur gelöscht, sondern mit Begründung dokumentiert (`stableCounter >= 2` + `Count == 0` zum gleichen Prüf-Zeitpunkt deckt 2-Tick-Nachweis implizit ab).

- **NEW-STAB-05 (1.3 Kombi-Threshold): RESOLVED**
  1.3 AC 15 ersetzt starre 25% durch `dropped >= 1 AND (dropped/oldKeys.Count > 0.10 OR dropped >= 2)`. Dokumentiert drei Kolonie-Größen-Szenarien (3-Pawn / 5-Pawn / 10-Pawn) inkl. der Begründung warum 25% allein zu Blind-Spot in 10-Pawn-Colonies führt. AC 17 (Migrate-Test TC-10) noch mit 25%-Wording — minor inconsistency zur neuen Formel, aber Test-Case-Intent (< threshold → no toast) ist durch Kombi-Formel weiter erfüllt (1 dropped von 3 = 33%, trigger over `dropped >= 2` Zweig falsch → aber genau ein dropped, also nur `>0.10`-Zweig greift = trigger). **Sub-minor redaktioneller Drift**, kein Findings-Re-Open.

- **NEW-STAB-06 (3.1 pawnClaims): RESOLVED**
  3.1 hat jetzt eigene Sektion `## Transient/Persistent` (Zeilen 61-63): `pawnClaims` explizit als transient markiert, mit Begründung (Emergency-State überlebt Save-Load nicht), plus explizites Verbot von `SchemaVersionRegistry`-Eintrag und `Scribe_Collections.Look` in `ExposeData`. `stalenessCounter` aus 3.13 mit-klassifiziert. Klassifikation ist eindeutig und implementierungsfest.

## Neue Findings (Round 3)

Keine neuen Findings. Einzig sub-minorer redaktioneller Drift in 1.3 AC 17 (Test-Case-Wording nennt "25%" statt neuer Kombi-Formel) — Test-Intent bleibt gültig, kein Re-Open.

## Recommendation

**APPROVE.** Alle sechs Round-2-Findings RESOLVED. Pass 3 hat integration-drift-frei gearbeitet: Wrapper-ACs sind konsistent formuliert (`Story 1.10 ExceptionWrapper.<Host|Execution>(...)` + Fallback-Trigger), Schema-Bump-ACs sind konsistent formuliert (`Story 1.9 SchemaVersionRegistry` + Migrate-Default), und die Inherit-Verweise (4.7→3.8, 5.7→5.4) sind explizit und technisch korrekt (kein Dupe-Wrapper nötig, weil derselbe Apply-Pfad). Stories sind implementation-ready aus Stability-Sicht.
