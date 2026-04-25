# Story 1.5 Code-Review Round 2

**Verdict:** APPROVE

## Findings-Status

- **HIGH-01 (Event.current → Input.GetKey):** RESOLVED
  `BotGameComponent.cs:297` nutzt `Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)` als Control-Gate. `Event.current.control` und `Event.current.Use()` sind entfernt. Kommentar bei Zeile 283-285 dokumentiert explizit, warum Polling statt Event-basiertem Check (`GameComponentUpdate` kommt aus Unity Update(), nicht OnGUI — `Event.current` ist dort null). Root-Cause sauber adressiert.

- **MED-01 (Interim-Vermerk AC 8):** RESOLVED
  Story AC 8 (Zeile 26) enthält den expliziten Interim-Block: „**Story-1.5-Interim (retroaktiv 2026-04-24):** `BotErrorBudget`/`FallbackToOff` existieren noch nicht (Story 1.10 pending). Inline `try/catch + Log.Error` als Interim, markiert mit `TODO(Story 1.10)` im Code." Code-seitig findet sich der `TODO(Story 1.10)`-Marker bei `BotGameComponent.cs:314` mit konkretem Ersetzungs-Target (`ExceptionWrapper.TickHost(...)`). Retroaktive CR-Verifikation in 1.10 ist vermerkt.

- **MED-02 (GetNamedSilentFail):** RESOLVED
  Beide Call-Sites umgestellt: `BotGameComponent.cs:292` und `RimWorldBotMod.cs:36` nutzen `DefDatabase<KeyBindingDef>.GetNamedSilentFail("RimWorldBot_ToggleMaster")` mit anschließendem `if (… == null) return;`-Guard. Kommentar bei Zeile 291 nennt den Grund (Def-Load-Order-Resilienz). AC 10 dokumentiert die Änderung retroaktiv.

- **LOW-01 (Collision-Check implementiert):** RESOLVED
  `RimWorldBotMod.LogKeybindingCollisions` (Zeile 34-47) iteriert `DefDatabase<KeyBindingDef>.AllDefsListForReading`, prüft sowohl `defaultKeyCodeA` als auch `defaultKeyCodeB`, loggt Kollisionen mit Default-Key-Begründung + Rebind-Hinweis. Aufruf via `LongEventHandler.QueueLongEvent` in Zeile 29 — korrektes Timing nach DefDatabase-Populate. Task in Story-Zeile 38 mit `[x]` abgehakt inkl. statischem Befund (Vanilla `Misc8`).

## Neue Findings (Round 2)

Keine.

## Recommendation

Story 1.5 ist review-ready. Alle Round-1-Findings sauber und konsistent adressiert (Code + Story + Tasks). Build-Clean (0 W / 0 E) bestätigt. Story-Status kann auf `ready-for-visual-review` bzw. bei fehlender UI-Komponente direkt auf `done` gesetzt werden (AC 7 + Review-Gate-Notiz „Visual-Review: nicht relevant"). Follow-up in Story 1.10: Interim-try/catch durch `ExceptionWrapper.TickHost(...)` ersetzen und re-verifizieren.
