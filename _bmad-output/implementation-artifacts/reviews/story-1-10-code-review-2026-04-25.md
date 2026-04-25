# Story 1.10 Code-Review

**Verdict:** APPROVE-WITH-CHANGES

## AC-Coverage (6 ACs)

1. **SafeTick** (`BotSafe.cs:44`): Signature `(Action body, string context)` — passt. Null-Guard + IsPoisoned-Gate korrekt. ✓
2. **SafeApply<T>** (`BotSafe.cs:62`): Signature `(Func<T,bool> body, T plan, string context)` — passt. Kein Generic-Constraint nötig (T frei). Returnt `false` bei poisoned/exception, `true/false` von body sonst. ✓
3. **Exception-Path** (`BotSafe.cs:107-130`): `Log.Error` + Sliding-Window-Add + Threshold-Poison vorhanden. **FallbackToOff** ist als `TODO(Story 2.x)` dokumentiert (`BotSafe.cs:128`) — AC 2 sagt aber „FallbackToOff analog D-13", also Teil-Erfüllung. Akzeptabel weil BotController-Wiring noch nicht existiert (Story 2.x), aber muss in Story 2.x explizit retroaktiv getriggert werden. ✓ (mit TODO)
4. **Sliding-Window 60s / Threshold 2 / Cooldown 10min** (`BotSafe.cs:29-31`): Konstanten korrekt (3600/36000 Ticks @ 60 TPS). Logik in `Report` + `PruneOldExceptions` sauber. ⚠ Siehe HIGH-1.
5. **AC-Template als XML-Doc** (`BotSafe.cs:7-26`): File-Header dokumentiert das Verwendungs-Pattern. Story-AC 4 verlangt aber „Pflicht-AC-Template **für Stories**" — gehört eigentlich in eine zentrale Story-Template-Doc, nicht nur in Code-Kommentar. Code-seitig ausreichend, Story-seitig offen. ⚠
6. **Retroaktiv Story 1.5** (`BotGameComponent.cs:298-320`): `GameComponentUpdate` ist auf `BotSafe.SafeTick` migriert. Inline-try/catch entfernt. `LoadedGame`/`StartedNewGame` rufen `BotSafe.Clear()` (Z. 240/251). ✓

## Findings

### HIGH-1: GenTicks.TicksGame friert in Pause-State ein — Sliding-Window unbrauchbar für UI-Hot-Pfade

`GenTicks.TicksGame` ist Sim-Time und stoppt bei Pause/Speed=0. `GameComponentUpdate` läuft aber pro Unity-Frame weiter (~60 FPS) — das ist der ganze Grund warum Story 1.5 dort lebt (siehe `BotGameComponent.cs:289-294`). Konsequenz: Wenn der User pausiert und dabei eine fehlerhafte Code-Pfad-Schleife aktiv bleibt (z.B. Ctrl+K-Spam mit Bug), bekommen alle Exceptions denselben TicksGame-Stempel → Sliding-Window erkennt sie als „1 Tick alt" und prunt nie. Threshold=2 wird trotzdem getriggert (Liste füllt sich), aber die 10-min-Cooldown ist effektiv ∞ in Pause. Umgekehrt: Wenn Pause endet, sind alle alten Stempel plötzlich „aktuell" → false-positive Poison.

**Fix:** Für `SafeTick`-Aufrufe aus `GameComponentUpdate`/`MapComponentOnGUI` (Frame-basiert) muss eine echte Real-Time-Quelle her: `Time.realtimeSinceStartup` oder `DateTime.UtcNow`. Empfehlung: BotSafe nimmt Real-Time intern (frame-rate-unabhängig), Konstanten in Sekunden. Tick-basierte Hosts laufen weiter sauber, weil Real-Time auch da monoton steigt.

### HIGH-2: AC 6 (Unit-Tests) nicht erfüllt — Test-Infrastructure existiert noch nicht

Story-AC 6 fordert Unit-Tests für SafeTick + SafeApply mit Mock-Exception. Test-Infrastructure ist auf Story 1.13 verschoben (Reviewer-Hinweis). Empfehlung: AC 6 in Story 1.10 retroaktiv auf „verschoben → 1.13" markieren mit Decision-Log-Eintrag, **nicht** silent ignorieren. Sonst wandert Story 1.10 mit unerfüllter AC nach „done" — Guardian-Verstoß (Regel 4: alle Findings/ACs werden adressiert).

### MED-1: Static state in BotSafe — AI-4 grenzwertig

`_exceptionTimestamps` + `_poisonedUntil` sind `private static`. `BotGameComponent.cs:16` deklariert explizit „einziger statischer Singleton ist RimWorldBotMod.Instance; BotGameComponent ist per-Game-Instanz". BotSafe bricht das. Pragmatisch akzeptabel weil reine Helper-State + `Clear()` in LoadedGame/StartedNewGame (Z. 240/251) den Cross-Game-Leak verhindert. Aber: wenn ein zweites Game ohne Quit geladen wird und Clear vergessen wird (in einem zukünftigen Hook), tragen wir Poison-Cooldowns rüber. **Mitigation:** Komponieren als Member auf `BotGameComponent` wäre sauberer; mindestens einen Test/Smoke in 1.13 der den Reset über Save→Load→Save bestätigt.

### MED-2: PruneOldExceptions O(n) RemoveAt(0)

`while (list.Count > 0 && list[0] < cutoff) list.RemoveAt(0)` ist O(n) pro Removal wegen Array-Shift. Bei Threshold=2 + Poison sofort danach ist die Liste praktisch nie >5 Einträge lang (poisoned context skippt die `Report`-Aufrufe komplett). Akzeptabel im Normalbetrieb. **Aber:** wenn ein Context manuell reaktiviert wird oder während der 60s vor Threshold Hunderte Exceptions reinkommen (Tick-Loop @ 60 TPS = max 3600 Einträge bevor erste pruned), lohnt sich Deque/RingBuffer. Empfehlung: Comment im Code dass Liste durch Threshold=2 effektiv kappt; bei Threshold-Erhöhung umstellen.

### LOW-1: SafeApply-Caller-Pattern nicht im Header dokumentiert

File-Header (`BotSafe.cs:18-20`) zeigt Apply-Beispiel, aber nicht das `if (!BotSafe.SafeApply(...)) { /* skip downstream */ }`-Pattern. Future-Caller könnten den Return-Value ignorieren. Ein Satz ergänzen.

### LOW-2: `string.IsNullOrEmpty(context)` als Silent-No-op

`BotSafe.cs:46` + `:64` returnen leise bei leerem Context. Besser: `Log.ErrorOnce` oder Debug-Assert — Caller mit Tippfehler im Context-String bekommen sonst unsichtbares no-op statt Error.

### LOW-3: Story-AC 4 Pflicht-AC-Template fehlt als zentrale Doc

XML-Doc in BotSafe.cs deckt nur Code-seitig. Für AC 4 sollte ein Snippet in `_bmad/decisions.md` oder einem Story-Template-Doc existieren, das künftige Story-Author:innen referenzieren. Folge-Aufgabe.

## Recommendation

**APPROVE-WITH-CHANGES.** Build clean, AC-Coverage 5/6 vollständig, AC 6 verschoben.

**Vor „done":**
1. **HIGH-1 fixen** (Real-Time-Source für Frame-basierte Hosts) — sonst ist Poison-Logik in Pause kaputt. Kleiner Edit (Switch auf `Time.realtimeSinceStartup` + Sekunden-Konstanten).
2. **HIGH-2 dokumentieren**: AC 6 in Story-Datei retroaktiv markieren als „verschoben auf 1.13" + Eintrag in `_bmad/decisions.md`.
3. **LOW-1 + LOW-2** als Polish im selben Round-2-Commit.

**Nach Round 2 → done.** MED-1/MED-2 sind Folge-Sprint-Beobachtungen, kein Blocker.

**Files:**
- `D:/SteamLibrary/steamapps/common/RimWorld/Mods/RimWorld Bot/Source/Core/BotSafe.cs`
- `D:/SteamLibrary/steamapps/common/RimWorld/Mods/RimWorld Bot/Source/Data/BotGameComponent.cs`
