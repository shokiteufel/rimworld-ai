# Story 1.4 Code-Review

**Datum:** 2026-04-24
**Reviewer:** BMAD Code-Review-Subagent
**Scope:** Statische Verifikation gegen 10 ACs + D-03/D-08/D-13 + AI-1/AI-4. Build ist laut Dev clean (0W/0E). Laufzeit-Test TC-04-TOGGLE-BUTTON erfolgt im Visual-Review.
**Verdict:** APPROVE-WITH-CHANGES

## AC-Coverage

- **AC 1 `Defs/MainButtonDefs.xml` — PASS (mit Namespace-Diff, siehe HIGH-1).** Alle Pflichtfelder aus Architecture §10.2 vorhanden: `defName=BotControl`, `label=Bot`, `description`, `tabWindowClass`, `order=99`, `defaultHidden=false`, `iconPath=UI/Buttons/BotIcon`, `minimized=false`. UTF-8-Deklaration, wohlgeformt.
- **AC 2 `MainTabWindow_BotControl : MainTabWindow` + `DoWindowContents` — PASS.** Source L13 erbt `MainTabWindow` (aus `RimWorld`). `DoWindowContents(Rect inRect)` L22–54 rendert: zentriertes State-Label via `Text.Anchor = MiddleCenter` (mit Anchor-Restore L38/41), drei `Widgets.ButtonText` via `DrawStateButton`-Helper, Aktiv-Indikator via gelbe `Widgets.DrawBoxSolid`-Outline bei `current == target` (L58–64). Drei Buttons setzen `masterState` direkt — keine Cycle-Logik, entspricht TQ-S4-01 resolution.
- **AC 3 `BotIcon.png` 32×32 — PASS.** Datei existent unter `Textures/UI/Buttons/BotIcon.png`, 140 Bytes (plausibel für 32×32 RGBA mit hohem Transparenz-Anteil, PNG-komprimiert). Visuelle Inspektion zeigt erwarteten weißen-auf-transparent-Charakter.
- **AC 4 State-Read/Write via `Current.Game.GetComponent<BotGameComponent>` — PASS.** L24–25 Read-Path, Write-Path via `gameComp.SetMasterState(target)` L68. Kein direkter Feld-Zugriff von außen auf `masterState` — sauber.
- **AC 5 Click-Cycle über Top-Bar-Button — PASS.** MainButtonDef mit `tabWindowClass` hat Vanilla-Open/Close-Toggle auf den Top-Bar-Button (RimWorld `MainButtonWorker_ToggleTab`). TabWindow selbst enthält zusätzlich die drei State-Buttons für den eigentlichen State-Wechsel — entspricht Story-AC „alternative Zyklus-Klick nur im TabWindow selbst".
- **AC 6 State-Persistenz via BotGameComponent — PASS.** `masterState` ist in BotGameComponent L30 als Feld deklariert, ExposeData L79 serialisiert via `Scribe_Values.Look(ref masterState, "masterState", ToggleState.Off)`. Kein Persistenz-Neucode in 1.4 — vererbt aus 1.3, bleibt intakt.
- **AC 7 Log + DecisionLog bei state-change — PASS.** `SetMasterState` L49–59: no-op bei `newState == masterState` (L51), sonst `Log.Message($"[RimWorldBot] state changed: {oldState} → {newState}")` L54 + `recentDecisions.Add(new DecisionLogEntry(kind: "state_change", reason: …, tick: GenTicks.TicksGame))` L55–58. Format konsistent mit Story-1.2-Konvention `[RimWorldBot] <verb> …`.
- **AC 8 Kein Harmony-Patch — PASS.** Grep über `Source/` zeigt keinen neuen `[HarmonyPatch]` in 1.4-Files. Button läuft rein über MainButtonDef-XML + TabWindow-Subclass. D-13 eingehalten.
- **AC 9 Button in Top-Bar (order=99) — PASS.** XML `<order>99</order>` exakt TQ-S4-02 resolution. Vanilla rendert hohe order-Werte am rechten Rand.
- **AC 10 Kein Crash bei raschem Klicken — PASS (statisch).** `DoWindowContents` ist idempotent, keine Allokation im Hot-Path außer Rects (stack-cheap). `SetMasterState` ist no-op bei identischem State → keine State-Thrashing-Races. Laufzeit-Verifikation beim Visual-Review.

## Decisions + Invarianten

- **D-03 Zwei-Ebenen-Toggle:** PASS. 1.4 adressiert nur Master-Ebene; Per-Pawn bleibt Story 3.x. Kein Crossover.
- **D-08 Off/Advisory/On-Semantik:** PASS. Drei Buttons entsprechen exakt den Enum-Werten aus `Source/Core/Enums.cs`. Kein Paused-Exposure — das bleibt interner State (Emergency-Handler-Hook später).
- **D-13 MainButtonDef via XML, kein H7:** PASS. MainButtonDef ist die alleinige Registrierungsmethode. `Source/Core/RimWorldBotMod.cs` `PatchAll` findet keine neuen Patches.
- **AI-1 kein Harmony-Tick-Patch:** PASS. TabWindow ist kein Tick-Hook.
- **AI-4 kein neues Singleton:** PASS. `MainTabWindow_BotControl` hält keinen statischen State. Game-Lookup via `Current.Game` ist Vanilla-Pattern.

## Findings

### HIGH

**H-1: Namespace-Drift zwischen Architecture §10.2 und XML/Code — Architecture muss gefixt werden.**
`Defs/MainButtonDefs.xml` L7 spezifiziert `<tabWindowClass>RimWorldBot.UI.MainTabWindow_BotControl</tabWindowClass>`, Code L8 deklariert `namespace RimWorldBot.UI`. Architecture-§10.2-Snippet (L1085) schreibt dagegen `RimWorldBot.MainTabWindow_BotControl` ohne Folder-Segment. Laufzeit-Fakt: RimWorld löst `tabWindowClass` via `Type.GetType(name, ignoreCase: true)` — der exakte Namespace-String muss matchen. Der Dev-Namespace ist konsistent mit allen anderen Folder-basierten Namespaces im Projekt (`RimWorldBot.Core`, `RimWorldBot.Data`, `RimWorldBot.Events`, `RimWorldBot.Phases`, `RimWorldBot.Analysis`), d.h. Code/XML ist richtig, das Architecture-Snippet ist veraltet. **Fix-Action:** Architecture §10.2 Snippet auf `RimWorldBot.UI.MainTabWindow_BotControl` korrigieren (1-Zeilen-Doku-Edit, kein Code-Change). Decision-Log-Eintrag optional. **Keine Cherry-Picks:** Diese Doku-Drift muss jetzt behoben werden, nicht „später in Polish".

### MED

Keine.

### LOW

**L-1: Redundanter Aktiv-Button-Klick spielt Sound obwohl SetMasterState no-op ist.**
`DrawStateButton` L66–70: `if (Widgets.ButtonText) { SetMasterState(target); SoundDefOf.Click.PlayOneShotOnCamera(); }`. Wenn State schon `target` ist, macht `SetMasterState` korrekt nichts (AC 7 L51), aber `SoundDefOf.Click` spielt trotzdem. Das ist vertretbar (User-Feedback dass Klick registriert wurde), aber kein echtes State-Change-Audio. Alternative: Sound nur bei tatsächlichem Wechsel abspielen via Rückgabe-bool aus `SetMasterState` oder Comparison vor Aufruf. Nicht release-blockierend; akzeptables UX-Verhalten. **Empfehlung:** Entweder Comment im Code dokumentieren („Click-Feedback auch bei no-op — bewusst") oder `bool SetMasterState` mit Return-Value. Keine Pflicht.

**L-2: `DrawBoxSolid` als Aktiv-Indikator ist visuell grob.**
L63 malt eine solid-gelbe Fläche hinter dem Button (`ExpandedBy(2f)` → 2px-Rahmen, aber DrawBoxSolid füllt die gesamte expanded Rect, nicht nur den Border). Da `Widgets.ButtonText` anschließend die Button-Fläche selbst überdeckt (L66), sieht das Ergebnis wie ein 2px-Rahmen aus — funktioniert optisch, ist aber implizit. Saubererer Ansatz: `Widgets.DrawBox(outline, thickness: 2)` (vanilla-ish) oder explizit 4 Kanten-Rects. Visual-Review entscheidet Finalität. Kein Release-Blocker.

**L-3: `RequestedTabSize` 360×140 ist minimal bemessen.**
Drei Buttons à 90px + 2×8px Spacing = 286px; das passt bei 360px Container-Breite (`Padding` wirkt nicht explizit auf Button-Row, nur via manuellem `buttonsStartX`-Centering L45). Höhe 140px: Label-Row 32px + Padding 10+10+4 + Button-Row 32px = ~88px — Luft vorhanden, aber keine Reserve für Future-Additions (z.B. Phase-Info). Akzeptabel für 1.4-Scope; wenn Story 2.x/4.x weitere Felder braucht, wird die Size angehoben. **Empfehlung:** Kommentar an `RequestedTabSize` warum die Werte so gewählt sind.

**L-4: XML hat keinen `<modExtensions>`-Platzhalter, aber auch keine Story-Anforderung.**
`MainButtonDef` hat kein `<modExtensions>`-Tag. Aktuell nicht nötig. Notiert für Story 1.7 falls Settings-gebundene Button-Visibility kommt.

## Recommendation

**APPROVE-WITH-CHANGES.** Alle 10 ACs + D-03/D-08/D-13 + AI-1/AI-4 sind code-seitig korrekt umgesetzt. Story kann zu `ready-for-visual-review` weitergehen, sobald H-1 adressiert ist:

1. **Architecture §10.2 Snippet updaten** von `RimWorldBot.MainTabWindow_BotControl` auf `RimWorldBot.UI.MainTabWindow_BotControl`. Das ist eine Doku-Korrektur, kein Code-Change — der Code ist projekt-konsistent, die Architektur-Doku muss folgen. Guardian-Regel 5 („Doku ist heilig"): der Widerspruch zwischen Primär-Artefakt und Realität wird jetzt gefixt.
2. L-1/L-2/L-3 sind optional; Entscheidung liegt beim Visual-Review.

Nach H-1-Fix: direkt zu Visual-Review, kein Re-Review nötig (reine Doku-Änderung). Re-Review wäre nur bei Code-Änderungen Pflicht.
