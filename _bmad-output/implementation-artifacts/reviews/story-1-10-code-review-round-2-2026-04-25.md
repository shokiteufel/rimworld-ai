# Story 1.10 Code-Review Round 2

**Reviewer:** Code-Review-Subagent
**Datum:** 2026-04-25
**Scope:** Re-Verifikation der Round-1-Findings nach Pass-1-Fixes
**Verdict:** APPROVE

## Findings-Status (nach Pass 1)

- **HIGH-1 (Time.realtimeSinceStartup):** RESOLVED
  Verifiziert in `BotSafe.cs`:
  - L42-43: `ExceptionWindowSeconds = 60f`, `PoisonCooldownSeconds = 600f` (Sekunden, float).
  - L50: `Dictionary<string, List<float>>` — Timestamp-Typ jetzt `float`.
  - L53: `_poisonedUntil` jetzt `Dictionary<string, float>`.
  - L124, L160, L165, L176: alle Lese-/Schreibzugriffe nutzen `Time.realtimeSinceStartup`.
  - L30-32: Doku-Block erklärt explizit warum GenTicks.TicksGame in Pause friert und Update-Loop weiterläuft. Sliding-Window kollabiert nicht mehr.

- **HIGH-2 (AC 6 Story-Doc):** RESOLVED
  Story-File L21: AC 6 ist durchgestrichen (`~~...~~`) mit klarem Verweis „verschoben auf Story 1.13 (Test-Infrastructure)" plus Begründung (FakeSnapshotProvider/Test-Builder noch nicht etabliert). L23-26 enthält zusätzlichen „Retroaktive Drift-Fixes"-Block der beide HIGHs nachvollziehbar macht.

- **MED-1 (AI-4-Doku):** RESOLVED
  L36-39 expliziter AI-4-Hinweis-Block: BotSafe ist pure Helper-State, kein Mod-Singleton, Per-Game-Reset via `Clear()` in BotGameComponent garantiert. Argumentation sauber.

- **MED-2 (RemoveAt-Comment):** RESOLVED
  L48-49: Comment dokumentiert O(n)-Charakteristik plus Threshold=2-Argument („Liste praktisch winzig, max ~3 Einträge zwischen Reports").

- **LOWs (Caller-Pattern, LogOnce, AC-4-Template):** RESOLVED
  - Caller-Pattern: L19-24 (Apply-Beispiel mit `if (!BotSafe.SafeApply(...)) return; // skip downstream`) plus L86 Inline-Kommentar an SafeApply.
  - LogOnce: `LogOnceWarning` Helper L143-149 mit `_warnedOnceMessages`-HashSet, in Clear() L117 mitresettet, eingesetzt für null/empty body+context (L63, L68, L91, L95).
  - AC-4-Pflicht-Template: weiterhin im Story-File (AC 4) und implizit im Header-Beispielblock dokumentiert.

## Neue Findings (Round 2)

Keine. Build laut Pass-1-Report 0 Warnungen / 0 Fehler. Code-Walkthrough zeigt konsistente Time-Reference-Nutzung über alle 5 Touchpoints (Add/Prune/Cutoff/PoisonSet/PoisonCheck), Clear() resettet jetzt zusätzlich `_warnedOnceMessages` was ein potentielles Per-Game-Leak verhindert hätte. Saubere Arbeit.

## Recommendation

Status auf **done** setzen. Story 1.13 muss AC 6 (Unit-Tests SafeTick/SafeApply mit Mock-Exception) als Pflicht-Carry-Over aufnehmen — empfehle expliziten Eintrag in 1.13-Tasks beim Drafting.
