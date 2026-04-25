# Story 1.11 Code-Review

**Verdict:** APPROVE-WITH-CHANGES
**Review-Datum:** 2026-04-25
**Reviewer:** Code-Review-Subagent
**Files reviewed:** `Source/Decision/PlanLayer.cs`, `Source/Decision/PlanRecords.cs`, `Source/Decision/PlanArbiter.cs`, `Source/RimWorldBot.csproj`

## AC-Coverage (7 ACs)

| AC | Status | Evidenz |
|---|---|---|
| 1. PlanArbiter mit 4 Arbitrate-Methoden | OK | `PlanArbiter.cs` Z. 31, 114, 160, 202 — alle vier vorhanden mit korrekten Signaturen `IEnumerable<(T plan, PlanLayer layer)>` → `T` |
| 2. PlanLayer enum mit 5 Werten | OK | `PlanLayer.cs` Z. 9-17 — Emergency=4 als höchster, Default=0. Explizite int-Werte → robust gegen Reorder |
| 3. Präzedenz-Order | OK | `SortByLayerDescending` Z. 237-242 nutzt `OrderByDescending((int)p.layer)`. Per-Pawn-Merge via `layer > existing.Layer` Z. 59, 66, 94, 136, 175, 217 |
| 4. Merge-Semantik (höchster Layer gewinnt, same-layer-Konflikte → Log + erster gewinnt) | TEILWEISE | Korrekt für WorkPriority/Build/Bill. **DraftOrder Cross-Set-Bug** siehe HIGH-1. Pawn-Exclusivity-Lock-Comment Z. 22-23 vorhanden |
| 5. DecisionLog-Eintrag bei Conflict | OK | Optional `RecentDecisionsBuffer`-Param mit `?.Add()`-Null-safe-Calls. Kind `"plan-arbitration-conflict"` konsistent. `DecisionLogEntry`-Konstruktor in `Data/DecisionLogEntry.cs` Z. 20-26 verifiziert |
| 6. Unit-Tests verschoben auf 1.13 | OK | Story-AC 6 sagt „Unit-Tests" — Tests fehlen, aber laut Brief auf 1.13 verschoben (Test-Infra). Akzeptabel |
| 7. BotController-Tick-Integration als TODO | TEILWEISE | Comment Z. 14 dokumentiert „Story 2.x Integration" — kein expliziter `TODO`-Marker für grep-Findability. Siehe LOW-1 |

## Findings

### HIGH-1 — DraftOrder same-layer Cross-Set-Konflikt wird nicht geloggt

**File:** `Source/Decision/PlanArbiter.cs` Z. 85-110 (`MergeDraftSet`)
**Problem:** `MergeDraftSet` wird zweimal aufgerufen (Z. 48 mit `draft:true`, Z. 49 mit `draft:false`). Innerhalb **einer** Producer-Iteration sieht der zweite Call den Eintrag des ersten. Stehen für denselben Pawn-ID *im selben Producer-Plan* sowohl in `Draft` als auch in `Undraft` (Producer-Bug), greift Z. 98-103 korrekt mit Log. Aber: Producer-A-Layer=PhaseGoal sagt `Draft={Pawn1}`, Producer-B-Layer=PhaseGoal sagt `Undraft={Pawn1}`. Beide Layer gleich, aber:

- Producer-A iteriert: schreibt `(PhaseGoal, true)` für Pawn1 ins Dict (Z. 107)
- Producer-B iteriert in `MergeDraftSet(Undraft)`: existing=(PhaseGoal,true), new=(PhaseGoal,false) → Z. 98 trifft → **Log + erster gewinnt**

Das funktioniert. **Aber:** Wenn Producer-A `Draft` durchläuft *und* Producer-A's `Undraft` Pawn1 enthält (selber Producer, defensiv-Bug), gewinnt Z. 98 ebenfalls — same-Producer-Bug wird geloggt. OK.

**Real Bug:** Z. 73-76 erzeugt `draftSet`/`undraftSet` aus dem `decision`-Dict per Where-Filter. Wenn `decision[pawn1] = (Emergency, false)` gewonnen hat, landet pawn1 in `undraftSet`. Falls ein Producer mit niedrigerem Layer Pawn1 in `Draft` hatte: Eintrag wurde silent verworfen (Z. 92 `if (layer > existing.Layer)`), KEIN Log-Eintrag. Cross-Layer-Override **ohne** Same-Action ist erwartet (das ist Präzedenz). Aber: Cross-Layer-Override mit **gegensätzlicher Action** (z.B. PhaseGoal sagt Draft, Emergency sagt Undraft) wird ebenfalls nicht geloggt — nur same-layer. Das ist mit AC4 konform, aber für Debug-Visibility wertvoll. Nicht-blocking, aber empfehle in `MergeDraftSet` vor Z. 96 Optionalfall ergänzen: wenn `existing.Draft != draft` und höher → DEBUG-Log (kind: `plan-arbitration-override`).

**Severity:** HIGH (Beobachtbarkeit für Debug-Panel 8.7) — **nicht blocking** für Story-Approval, aber vor Story 8.7 fixen.

### MED-1 — `existing.Intent` Null-Check in BuildPlan

**File:** `PlanArbiter.cs` Z. 177
**Code:** `if (existing.Intent != null && existing.Intent.DefName != intent.DefName)`
**Problem:** `byCell.TryGetValue` liefert bei miss `default((PlanLayer, BlueprintIntent))` = `(Default, null)`. Erstmaliger Insert (Z. 175 "or"-Branch nimmt true wegen `!byCell.TryGetValue`) springt korrekt rein, dann `existing.Intent == null` → Cross-DefName-Log wird **übersprungen** beim First-Insert. Korrektes Verhalten (kein Konflikt beim ersten Eintrag), aber subtile Lesart. **Empfehlung:** Comment ergänzen `// existing.Intent==null bei First-Insert → kein Override-Log, das ist gewollt`.

**Severity:** MED — Code-Lesbarkeit.

### MED-2 — `ToImmutableDictionary` doppelte ImmutableDict-Konstruktion

**File:** `PlanArbiter.cs` Z. 150-153
**Problem:** Pro Pawn wird ein `Dictionary<string,(Layer,Priority)>` aufgebaut, dann zweimal `.ToImmutableDictionary()` aufgerufen (outer + nested via Lambda). Bei 50 Pawns × 30 WorkTypes = 1500 Allocations + 51 ImmutableDict-Builders pro Tick. Ist akzeptabel (PlanArbiter läuft per Brief-Aussage selten — pro Phase-Recompute, nicht jeden Tick), aber für Hot-Path würde `ImmutableDictionary.CreateBuilder` mit explizitem `ToImmutable()` GC-Pressure halbieren.

**Severity:** MED — Performance-OK für angegebene Plan-Größen (10-50 Pawns), Refactor-Kandidat falls Profile später Hotspot zeigt.

### LOW-1 — Fehlender `TODO`-Marker für BotController-Integration

**File:** `PlanArbiter.cs` Z. 14
**Problem:** Story-AC 7 fordert „TODO/Vorbereitung". Comment „Story 2.x Integration" ist menschenlesbar, aber `grep -r "TODO.*BotController"` findet nichts. **Empfehlung:** Z. 14 ergänzen: `// TODO(Story 2.x): BotController-Tick ruft PlanArbiter zwischen Planner.Plan() und Apply auf.`

**Severity:** LOW — Findability.

### LOW-2 — System.Collections.Immutable Bundling-Doku

**File:** `RimWorldBot.csproj` Z. 28-30
**Problem:** Comment erwähnt nicht das 5-DLL-Bundling (~545KB, +System.Buffers/Memory/Numerics.Vectors/Runtime.CompilerServices.Unsafe). Bei Konflikt mit anderen Mods wäre Issue-Triage einfacher mit explizitem Hinweis.

**Empfehlung:** Comment erweitern um „Bundlt 5 DLLs in Assemblies/ (Community-Standard, vgl. HugsLib). Konflikt-Risiko nur bei Mods mit divergenten Versionen."

**Severity:** LOW — Doku.

### LOW-3 — `PlanRecords.cs` D-23-Identifier-Pattern

**File:** `PlanRecords.cs` — alle Records nutzen `string`/`int`/`byte`/Tuple, keine Verse-Types. **Konform zu D-23.** Kein Finding, nur Bestätigung.

## Spezifische Checks (Brief-Antworten)

- **MergeDraftSet-Helper:** Same-layer Draft+Undraft-Konflikt für gleichen Pawn wird korrekt geloggt + erster gewinnt (Z. 98-103). **Korrekt.** Cross-Layer Cross-Action-Override wird stillschweigend (siehe HIGH-1).
- **ImmutableHashSet/Dictionary.CreateRange + ToImmutableDictionary:** Performance-OK für 10-50 Pawns. Bei 200+ Pawns sollte CreateBuilder erwogen werden (MED-2).
- **DecisionLog-Parameter optional:** **Sauber.** Test-Friendly + Production-Code übergibt `botGameComponent.RecentDecisions`. Pattern bewährt.
- **Pawn-Exclusivity-Lock:** Comment Z. 22-23 dokumentiert „Story 3.1 Integration". **Klar dokumentiert.**
- **Stateless static class:** **AI-4-OK** — pure Functions, kein Mod-Singleton, keine Side-Effects außer optionalem DecisionLog-Append.
- **Init-only Properties (DraftOrder):** CC-STORIES-14-Pattern korrekt umgesetzt — Empty-Defaults, 3-arg-Konstruktor bleibt aufrufbar (Z. 244-247 nutzt ihn). **Korrekt.**
- **System.Collections.Immutable-Bundling:** Mod-Conflict-Risiko niedrig (Community-Standard). Doku-Lücke siehe LOW-2.

## Recommendation

**APPROVE-WITH-CHANGES.** Code ist solide, AI-7-Präzedenz korrekt implementiert, D-23 Identifier-Pattern eingehalten. Vor Merge: **LOW-1 (TODO-Marker)** und **LOW-2 (csproj-Doku)** fixen — Trivial-Edits. **HIGH-1** und **MED-1/2** als follow-up-Findings ins Sprint-Backlog (nicht blocking für 1.11-Done, aber vor Story 8.7 Debug-Panel adressieren).

**Build:** 0 Errors / 0 Warnings (laut Brief verifiziert).
**Story-Fortschritt:** Bereit für `done`-Übergang nach LOW-1+LOW-2-Fix.
