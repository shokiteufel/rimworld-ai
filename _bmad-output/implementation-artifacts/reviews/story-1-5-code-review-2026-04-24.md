# Story 1.5 Code-Review

**Verdict:** REJECT

**Datum:** 2026-04-24
**Reviewer:** Code-Reviewer (Claude Opus 4.7)
**Files reviewed:** `Defs/KeyBindingDefs.xml`, `Languages/{Deutsch,English}/Keyed/KeyBindings.xml`, `Source/Data/BotGameComponent.cs`

---

## AC-Coverage (8 ACs + Invarianten)

| AC | Status | Notiz |
|---|---|---|
| 1 KeyBindingDef ohne modifierA | PASS | `defaultKeyCodeA: K`, kein modifierA-Feld, Misc8-Collision dokumentiert im XML-Kommentar. 1.6-Drift sauber erklärt. |
| 2 GameComponentUpdate + KeyDownEvent + Control-Gate | **FAIL** | Control-Gate via `Event.current.control` — siehe HIGH-01. |
| 3 Cycle Off→Advisory→On→Off | PASS | Switch-Expression korrekt, Default-Branch `_ => Off` fängt Paused o. ä. |
| 4 Options-konfigurierbar | PASS | Vanilla-KeyBindingDef-Verhalten, kein Custom-Gate nötig. |
| 5 Log via SetMasterState | PASS | Aufruf unverändert aus Story 1.4. |
| 6 Collision-Check | PARTIAL | Misc8-Kollision dokumentiert, aber Task „Vanilla-KeyBindingDefs auflisten" in Story nicht abgehakt. LOW. |
| 7 Pause-kompatibel | PASS | Update-Hook statt Tick, `doDesignatorInterrupt` nicht gesetzt (default false). |
| 8 Exception-Wrapper Interim | PASS-CONDITIONAL | Inline try/catch vorhanden, Kommentar verweist auf 1.10. AC fordert `BotErrorBudget.Report` + `FallbackToOff` — Interim dokumentiert, aber siehe MED-01. |
| AI-1 kein Harmony | PASS | Nur Vanilla-API. |
| AI-4 kein Singleton | PASS | `BotGameComponent` per-Game, nichts Statisches neu. |

---

## Findings

### HIGH-01 — `Event.current` ist in `GameComponentUpdate()` typischerweise null

`GameComponent.GameComponentUpdate()` wird aus Unity's `Update()`-Phase gerufen, **nicht** aus `OnGUI()`. `Event.current` ist ausschließlich innerhalb des IMGUI-OnGUI-Dispatch-Pfads gesetzt. In `Update()` ist `Event.current == null` der Normalfall.

**Konsequenz:** Der Early-Return `if (Event.current == null || !Event.current.control) return;` greift praktisch **immer**. Ctrl+K wird NIE ausgelöst. Plain-K löst nichts aus (KeyDownEvent true, Control-Gate fängt). AC 2 + AC 3 sind de facto nicht erfüllt — Feature ist tot.

**Zusätzlich:** `Event.current.Use()` nach `SetMasterState()` ist in `Update()`-Context no-op (kein IMGUI-Event zu konsumieren). Misc8-Doppelfeuer wird dadurch nicht verhindert.

**Fix-Vorschläge (wähle einen):**

A. **Hook auf `MapComponentOnGUI` / `WindowStack.WindowsOnGUI`-Pfad umziehen** — dort lebt `Event.current`. RimWorld-Pattern: Unity-Hook im Mod-Root (`Mod.DoSettingsWindowContents`-Äquivalent) oder separater `MainTabWindow.DoWindowContents`. Nicht trivial für ein globales Keybinding.

B. **`Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)`** in `GameComponentUpdate()` nutzen — das ist Unity-Polling, funktioniert in `Update()`. `KeyDownEvent` auf der Def selbst ist das Flank-Signal.
```csharp
if (!def.KeyDownEvent) return;
bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
if (!ctrl) return;
```
Keine `Event.current.Use()` nötig — Misc8-Doppelfeuer bleibt theoretisch möglich, ist aber nur relevant wenn User Misc8 aktiv rebindet.

C. **Override auf OnGUI-Hook verlegen** — z. B. zweiter `MapComponent` oder `Mod.DoSettingsWindowContents` ist falscher Ort. Richtig wäre ein UIRoot-Patch, aber das bricht AI-1 (kein Harmony).

**Empfehlung:** Option B. Minimale Diff, Unity-idiomatic, passt zum `GameComponentUpdate`-Hook-Kommentar. AC 8 Exception-Wrapper bleibt intakt.

### MED-01 — AC 8 fordert `BotErrorBudget.Report` + `FallbackToOff`, Interim loggt nur

AC 8 nennt konkret `BotErrorBudget.Report("GameComponentUpdate", ex)` + Fallback-Pfad bei ≥ 2 Exceptions/min. Interim macht nur `Log.Error`. Guardian-Regel „alle Findings werden gefixt" → Story 1.10 muss ein expliziter Follow-up-Task in dieser Story referenziert sein. Aktuell steht nur ein Inline-Kommentar. **Fix:** Story-File um „Open follow-up: Story 1.10 ersetzt Inline-Log durch ExceptionWrapper" ergänzen und `sprint-status.yaml` markiert Story 1.5 als „done-pending-1.10".

### MED-02 — `KeyBindingDef.Named()` wirft bei Missing-Def, `def == null` fängt das nicht

Verse's `KeyBindingDef.Named(string)` ruft intern `DefDatabase<KeyBindingDef>.GetNamed(name, errorOnFail: true)` und wirft eine Exception wenn die Def fehlt (statt null zurückzugeben). Der `if (def == null) return;`-Check greift nie. Praktisch irrelevant solange die XML lädt; wenn die XML lädt **nicht** lädt (Def-Load-Error), fängt der try/catch die Exception — aber dann feuert jeder Frame einen Log.Error-Spam. **Fix:** `DefDatabase<KeyBindingDef>.GetNamedSilentFail("RimWorldBot_ToggleMaster")` nutzen, dann greift `def == null`.

### LOW-01 — Collision-Check-Task nicht ausgeführt

Story-Task „alle Vanilla KeyBindingDefs auflisten via `DefDatabase<KeyBindingDef>.AllDefs` → prüfen dass keine `Ctrl+K` hat" ist nicht abgehakt. Ergebnis nicht dokumentiert. Misc8 ist bekannt, aber systematische Verifikation fehlt. **Fix:** Einmal-Log in `FinalizeInit()`: alle KBDefs mit `defaultKeyCodeA == K` auflisten, Ergebnis im Review-Followup notieren.

### LOW-02 — Sprach-Keys nutzen Label-Override-Pattern, aber Label im XML ist schon englisch

`KeyBindingDefs.xml` hat `<label>Toggle RimWorldBot master state</label>` — Vanilla nutzt normalerweise Lowercase + Language-Override. Funktioniert so, aber in EN-Keyed.xml ist das Label identisch dupliziert. Konsistent mit Vanilla-Mod-Praxis, kein Bug. Deutsch-Override greift korrekt. Nur kosmetisch.

---

## Recommendation

**REJECT** wegen HIGH-01: Das Feature ist in der aktuellen Form funktional tot. `Event.current`-Gate in `GameComponentUpdate()` blockt jeden KeyDown. Fix via `Input.GetKey(KeyCode.*Control)` (Option B) ist ~2 Zeilen, macht das Feature funktionsfähig und ändert nichts an der AC-Architektur. MED-01 (Story-File-Referenz auf 1.10), MED-02 (`GetNamedSilentFail`), LOW-01 (Collision-Log) in derselben Runde fixen. Nach Re-Review: APPROVE möglich.
