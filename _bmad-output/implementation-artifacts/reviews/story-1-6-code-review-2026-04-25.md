# Story 1.6 Code-Review

**Verdict:** APPROVE-WITH-CHANGES
**Reviewer:** Code-Reviewer (Claude)
**Datum:** 2026-04-25
**Files reviewed:** `Source/UI/ITab_Pawn_BotControl.cs`, `Patches/HumanInspectTabs.xml`, `Languages/Deutsch/Keyed/PerPawnToggle.xml`, `Languages/English/Keyed/PerPawnToggle.xml`

## AC-Coverage (11 ACs + Decisions)

| AC | Status | Beleg |
|---|---|---|
| 1 ITab_Pawn_BotControl : ITab | PASS | `ITab_Pawn_BotControl.cs:11` erbt `ITab`, kein `ITab_Pawn_Character`-Patch |
| 2 ~~ITabDefs.xml~~ retroaktiv | PASS | Datei nicht mehr in File-List, Story korrekt aktualisiert |
| 3 PatchOperationAdd auf Human.inspectorTabs | PASS | `HumanInspectTabs.xml:14-19`, Vanilla `Human ParentName="BasePawn"` ohne lokale `inspectorTabs`-Override verifiziert (Races_Humanlike.xml:4) |
| 4 Tab-Label „Bot" lokalisiert | PASS | `labelKey = "RimWorldBot.PerPawn.TabLabel"` + DE/EN-Keys vorhanden |
| 5 Checkbox + Hilfstext | PASS | FillTab rendert beide |
| 6 Persistenz via UniqueLoadID | PASS | `pawn.GetUniqueLoadID()` als Key, `BotGameComponent.perPawnPlayerUse` Scribe verifiziert |
| 7 Default false | PASS | `TryGetValue ... && v` liefert false für fehlenden Key |
| 8 Cleanup aus 1.3 | PASS | Vorhanden in `BotGameComponent.cs:321-333`, keine Story-1.6-Änderung nötig |
| 9 Savegame-Roundtrip | PASS | Scribe-Pfad bereits aus 1.3 abgedeckt; Remove-on-false-Strategie ist roundtrip-safe (false bleibt Default beim Load) |
| 10 Kein Konflikt CE/PC | PASS | Eigener Tab statt Patch auf existierende Tabs (D-13) |
| 11 Sichtbar Colonists+Slaves | PASS | `Faction.IsPlayer` deckt beide ab; Slaves haben `Faction == OfPlayer` in 1.6 |

**Decisions:**
- AI-1 (kein Harmony): PASS — nur XML-PatchOperation
- AI-4 (kein Singleton): PASS — `Current.Game.GetComponent<BotGameComponent>()`
- D-13 (kein H8): PASS — eigener ITab
- D-14 (UniqueLoadID): PASS — `pawn.GetUniqueLoadID()`

## Antworten auf Reviewer-Fragen

- **IsVisible-Filter** korrekt: Slaves sind in 1.6 `Faction.IsPlayer == true`, Quest-Lodgers nicht. `Faction == null` per Short-Circuit (`||`) sauber abgefangen — kein Type-Error, weil `||` left-to-right evaluiert und nach `null` direkt false returned wird.
- **Remove-on-false** korrekt für Roundtrip: Default-false-Semantik bleibt erhalten, Dict bleibt klein, Cleanup wird nicht erschwert.
- **PatchOperationAdd auf Human**: läuft korrekt nach Inheritance-Flattening (RimWorld 1.6 Standard-Reihenfolge: Inheritance → Patches → Cross-References). `Human` definiert `inspectorTabs` nicht lokal, erbt komplette Liste von `BasePawn`; Add ergänzt zur effektiv-vorhandenen Liste. **Nicht** auf BasePawn patchen (würde Animals den Tab geben — IsVisible-Filter würde greifen, aber unsauber).
- **Tab-Größe 360×180**: vergleichbar mit Vanilla `ITab_Pawn_Social` (~480×510) etwas knapp aber für 1 Checkbox + 60 px Hilfstext ausreichend.
- **HelpText-Color**: prevColor-Pattern korrekt restored.

## Findings

### MED-1: Log.Message-Spam bei häufigem Toggle
`Log.Message` schreibt unkonditional in Standard-Log (sichtbar in Dev-Mode-Log und Standard-Log). Bei schnellem Klick durch viele Pawns entsteht Log-Rauschen.
**Fix:** Bedingen mit `Prefs.DevMode` oder zu `Log.Dev`/eigenem Bot-Log-Channel umleiten. Vergleich SetMasterState (Story 1.5): falls dort `Prefs.DevMode`-Guard fehlt, dort ebenfalls Finding (cross-story konsistent halten).

### LOW-1: HelpText könnte über HelpTextHeight=60 hinauslaufen
DE-Text 167 Zeichen, EN 138 Zeichen — bei 336 px breite (rect.width nach 12 px Padding *2) und Standard-Font passt knapp in 3 Zeilen. Bei längeren Übersetzungen (z. B. zukünftig RU/CN) könnte clipping auftreten.
**Fix optional:** `Text.CalcHeight(text, helpRect.width)` für dynamische Höhe oder `Widgets.LabelScrollable`.

### LOW-2: `Current.Game?.GetComponent<BotGameComponent>()` ohne Null-Log
Stilles return wenn gameComp null. Sollte nie passieren (ITab nur in Spiel sichtbar), aber ein einmaliges `Log.WarningOnce` würde Diagnose erleichtern wenn doch.

## Recommendation

**APPROVE-WITH-CHANGES** — die Implementierung ist sauber, AC-Coverage vollständig, alle Decisions eingehalten. Vor Done-Move:

1. **MED-1 fixen** — `if (Prefs.DevMode)` Guard um `Log.Message` (1 Zeile, deterministisch).
2. LOW-1/LOW-2 nach Ermessen — kein Blocker.
3. Visual-Review (Design-Critic) für Tab-Layout-Validation noch ausstehend (ITab in-Game mit Pawn-Selection rendern).
4. Kompat-Test TC-06-COMPAT-MATRIX (CE + EdB PC + Yayo) noch zu fahren — gehört aber in Test-Phase, nicht in dieses Review.

Nach MED-1-Fix: Re-Review nicht nötig, direkt zu Visual-Review.
