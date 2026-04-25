# Story 1.7 Code-Review

**Verdict:** APPROVE-WITH-CHANGES

**Reviewer:** Code-Reviewer (Claude)
**Datum:** 2026-04-25
**Files:** Configuration.cs (neu), SettingsRenderer.cs (neu), RimWorldBotMod.cs (modify), Settings.xml DE+EN (neu)

---

## AC-Coverage (9 ACs)

| AC | Status | Notiz |
|---|---|---|
| 1 — 4 Felder in Configuration | PASS | Defaults korrekt (`Opportunistic`, null, false, false) |
| 2 — ExposeData persistiert alles | PASS-mit-Anmerkung | siehe HIGH-1 (Nullable-Workaround Roundtrip) |
| 3 — GetSettings via Settings-Property | PASS | Lazy-Init in `RimWorldBotMod.Settings` |
| 4 — DoSettingsWindowContents 5 Sections | PASS | General/Ending/Learning/Telemetry/Info alle implementiert |
| 5 — WriteSettings ruft Invalidate-Hook | PARTIAL | siehe HIGH-2: Hook ist nur Kommentar, kein Code-Stub |
| 6 — SettingsCategory "RimWorld Bot" | PASS | Zeile 42 RimWorldBotMod.cs |
| 7 — Reset mit Confirm + Delete + Toast | PASS | Dialog_MessageBox + try/catch + Messages.Message |
| 8 — Telemetry Restart-Hint | PASS | prevValue-Diff in DrawSectionTelemetry |
| 9 — Persistenz via Vanilla-ModSettings | PASS | base.WriteSettings() korrekt aufgerufen |

---

## Decisions

- **AI-1 (kein Patch für Settings):** PASS — DoSettingsWindowContents ist Vanilla-Override, keine Harmony-Patch-Annotation auf Settings-Methoden.
- **AI-4 (kein Singleton außer RimWorldBotMod):** PASS — `SettingsRenderer` ist `static class` mit reinen statischen Methoden (kein `Instance`-Field, kein Zustand). Configuration wird als Parameter durchgereicht. Akzeptabel.
- **D-13 (Mod-Subclass-Override):** PASS — alle 4 Overrides (`SettingsCategory`, `DoSettingsWindowContents`, `WriteSettings`, GetSettings via Property) auf `Mod`-Subclass.
- **F-STAB-07 (Telemetry default OFF):** PASS — `telemetryEnabled` ohne Initializer (= false), Scribe-Default ebenfalls false.
- **D-06 / D-07 / D-17 / TQ-S7-01..04:** alle umgesetzt.

---

## Findings

### HIGH-1: Scribe-Roundtrip für `Ending? forcedEndingNullable` ist defekt
`Scribe_Values.Look(ref forcedEndingNullable, "forcedEnding", default(Ending))` mit `Ending?`-Ref auf einen non-nullable Default — RimWorlds Scribe schreibt `null` als fehlendes Element, beim Load bleibt der Wert dann auf `default(Ending)` (= Ship o.ä.) statt `null`. Der `forcedEndingHasValue`-Bool fängt das ab, aber: wenn jemand `forcedEndingHasValue=true` + Wert=Ship persistiert und beim Reload Scribe das Nullable nicht korrekt rehydriert, ist der Wert weg.

**Sauberer:** Statt zwei Felder: nur `Ending forcedEnding` + `bool forcedEndingHasValue` als Backing-Fields, beide mit `Scribe_Values.Look<Ending>` und `Scribe_Values.Look<bool>` (non-nullable). Property-Wrapper bleibt. Das ist exakt das RimWorld-Vanilla-Pattern (siehe `Pawn.relations.directRelations` o.ä.). Workaround mit zwei Feldern ist OK in Substanz, aber der Scribe-Call mit `Ending?`-Ref ist riskant — bitte auf `Ending` (non-nullable) umstellen.

### HIGH-2: WriteSettings-Hook ist Dead-Code, nicht Stub
Zeilen 55-58 RimWorldBotMod.cs: `gameComp` wird geholt aber nirgends benutzt. Kommentar sagt „Vorbereitung für Story 2.x — aktuell no-op". Das ist Dead-Code, kein Stub. Entweder:
- (a) Methodenaufruf hinzufügen: `gameComp?.InvalidateConfigCache()` (Methode in BotGameComponent als TODO-throwing Stub anlegen) — dann ist es echter Hook.
- (b) `var gameComp = ...`-Zeile entfernen und nur den TODO-Kommentar lassen.

AC 5 verlangt explizit den Aufruf. Variante (a) ist sauberer.

### MED-1: `Application.OpenURL` ohne URL-Validation
URL ist hardcoded und gehört dem Mod-Author — Phishing-Risiko = 0 solange Konstante nicht aus User-Daten stammt. OK. **Aber:** GitHub-Repo-URL `shokiteufel/rimworld-ai` matcht nicht den Mod-Namen „RimWorld Bot" und nicht das `mediainvita.rimworldbot`-Harmony-ID. Bitte verifizieren ob URL korrekt ist (sonst Tote-Link in Settings).

### MED-2: Mod-Version aus Assembly — Build-Pipeline-Setup fehlt
`Assembly.GetExecutingAssembly().GetName().Version` zieht aus `[assembly: AssemblyVersion]`. csproj hat aktuell keinen `<Version>`-Tag (bitte verifizieren). Fallback `"0.0.0"` ist OK für jetzt, aber Story 8.x oder Release-Pipeline muss `<Version>` setzen. **TODO im File-Header von SettingsRenderer.cs ergänzen** damit es nicht vergessen wird.

### MED-3: Localization-Keys ohne Fallback bei Missing-Key
`"RimWorldBot.Settings.X".Translate()` wirft bei Missing-Key in dritter Sprache (z.B. wenn User auf Französisch spielt) eine rote Translation-Warning ins Log. Für DE+EN aktuell OK, aber Pattern für Story 1.8 (vollständiges Locale-Sweep) merken.

### LOW-1: `Confirm`/`Cancel` als Translation-Keys ohne RimWorldBot-Prefix
Zeilen 108/110 SettingsRenderer.cs: `"Confirm".Translate()` und `"Cancel".Translate()` greifen auf Vanilla-Keys zu. Funktioniert (Vanilla hat die Keys), aber inkonsistent zum Rest. OK so, da Vanilla-Style.

### LOW-2: GapLine nach letzter Section fehlt korrekterweise
Konsistenz-Check: zwischen Section 4 (Telemetry) und 5 (Info) ist GapLine, nach Info keine — korrekt. Kein Finding, nur Bestätigung.

### LOW-3: `Dialog_MessageBox` ohne `destructive:true`
RimWorld 1.6 hat den Parameter tatsächlich nicht im public Konstruktor. Confirm/Cancel + Title reicht — destructive ist Polish, Default-Style ist akzeptabel. Final-Version OK.

### LOW-4: `Listing_Standard.GetRect(28f)` + LeftPart/RightPart
Standard-Vanilla-Pattern, korrekt verwendet. `0.45f + 0.5f = 0.95f` lässt 5% Gap zwischen Label und Button — visuell sauber. OK.

---

## Recommendation

**APPROVE-WITH-CHANGES.** Bitte fixen vor Visual-Review:

1. **HIGH-1:** Scribe-Roundtrip auf non-nullable `Ending forcedEnding` + `bool forcedEndingHasValue` umstellen (Property-API bleibt unverändert).
2. **HIGH-2:** WriteSettings-Hook entweder zu echtem Stub-Aufruf machen (`gameComp?.InvalidateConfigCache()` mit TODO-throwing Methode) oder Dead-Code-Zeile entfernen und nur Kommentar lassen.
3. **MED-1:** GitHub-URL gegen About.xml/csproj-RepoUrl verifizieren.
4. **MED-2:** TODO für `<Version>` in csproj im SettingsRenderer-Header dokumentieren.

MED-3, LOW-1..4 sind Awareness-Items, kein Blocker.

Nach diesen Fixes → Re-Review Round 2 + Visual-Review TC-07-SETTINGS.
