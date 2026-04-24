# Re-Review Round 2 — Stability-Persona

**Datum:** 2026-04-24
**Scope:** Pass-2-Revision nach Round 1 APPROVE-WITH-CHANGES (Stability-Engineer-Perspektive)
**Verdict:** APPROVE-WITH-CHANGES

---

## Round-1-Findings-Status

### Cross-Cutting (CC-STORIES-01 … CC-STORIES-11, nur Stability-relevante)

| ID | Kern | Status | Begründung |
|---|---|---|---|
| **CC-STORIES-01** | Schema-Versioning-Registry | **PARTIAL** | Story 1.9 existiert und listet Retroaktiv-Bumps (v3→v9). ABER: Stories 3.9 (`botManagedBills`), 6.5 (`pawnSpecializations`), 2.7 (`overlayVisible`), 7.9 (`journeyQuest`) haben KEINE Rückverweis-AC auf 1.9. Nur Story 2.3 + 1.3 haben explizite Schema-Bump-AC. Drift-Risiko: Dev liest 3.9 isoliert, setzt Feld ohne Bump. |
| **CC-STORIES-02** | Exception-Wrapper für Tick-Host+Execution | **PARTIAL** | Story 1.10 liefert `BotSafe.SafeTick/SafeApply`-Helper. Rückverweis in Einzelstories: **nur 1.5 referenziert 1.10 explizit**. Stories 2.7 (MapComponentOnGUI), 2.9 (Tick-Iterator), 3.8/3.9/3.10 (Apply), 4.3/4.7 (Writer), 5.4/5.7 (DraftController), 6.8 (OutfitWriter), 8.8 (TelemetryLogger Tick-Flush?) haben KEINE `SafeTick/SafeApply`-AC. Story 1.10 AC 5 erklärt Retroaktiv-Pflicht, aber Einzelstories sind nicht synchronisiert. |
| **CC-STORIES-10** | Read-After-Write-Check | **RESOLVED** | 3.10, 4.3, 5.4, 5.7, 6.8 haben explizite AC. Siehe aber „Neue Findings" für **zwei fehlende Writer**. |
| **CC-STORIES-11** | Transient/Persistent-Klassifikation | **RESOLVED** | 18 Stories haben `## Transient/Persistent`-Section, inkl. 3.10, 5.1, 5.5, 4-9a…g, 7.0, 1.9/1.10/1.11/1.12/1.13/3.13. Pattern etabliert. |

### Individual HIGH-Findings aus Round 1

| Story | Finding | Status |
|---|---|---|
| 1.3 | Migrate v1→v2→v3 + Toast-Threshold | **RESOLVED** — AC 15-17 vorhanden, 25%-Schwelle für User-Toast plus auto-pinned DecisionLog. Siehe Finding NEW-STAB-05 für Threshold-Kritik. |
| 1.5 | GameComponentUpdate ohne Exception-Wrapper | **RESOLVED** — AC 8 referenziert 1.10 `BotSafe` + BotErrorBudget + FallbackToOff. |
| 2.3 | `excludedCells` Schema-Bump + HashSet-Scribe | **RESOLVED** — AC 9 + AC 10 (HashSet-Flatten via `List<int>` D-23-konform). |
| 5.4 | Read-After-Write auf Drafter-Mutation | **RESOLVED** — AC 6 Retry + Poisoned-Set-Eskalation. |
| 8.1 | XXE/ReDoS-Schutz | **RESOLVED** — AC 8 komplett: `DtdProcessing.Prohibit`, `XmlResolver=null`, `MaxCharactersFromEntities=1024`, `MaxCharactersInDocument=2M`, 15s-Timeout, Corruption-Pfad. AC 9 liefert XXE+Billion-Laughs+Timeout-Tests. |

### Phase-Transition-Guard (CC-STORIES-12, MED)

| Phase-Story | Guard vorhanden? |
|---|---|
| 3.7 (Phase 0) | **UNFIXED** — AC 3 nutzt Altformulierung „Kein Emergency aktiv für 2 konsekutive Eval-Ticks" statt `EmergencyResolver.ActiveEmergencies.Count == 0`. |
| 3.11, 4.2, 4.5, 4.6, 6.2, 6.4 | **RESOLVED** — alle 6 haben standardisierte Guard-AC. |

---

## Neue Findings (Round 2)

### HIGH

**NEW-STAB-01 (HIGH)** — Exception-Wrapper-Coverage unvollständig
Story 1.10 erklärt Retroaktiv-Pflicht, aber die betroffenen Stories (2.7 `MapComponentOnGUI`, 2.9 `FullScanIterator`-Tick, 3.8–3.10 Apply, 4.3/4.7 Writer, 5.4/5.7/6.8) erwähnen `SafeTick`/`SafeApply` nicht. Ein Dev der isoliert eine Story implementiert wird den Wrapper vergessen. Fix: in jeder betroffenen Story eine AC „Haupt-Apply/Tick-Körper wrapped via `BotSafe.SafeApply(\"DraftController.Apply\", …)`" + Unit-Test-AC „wirft Exception in Mock → Poisoned-Context registriert".

**NEW-STAB-02 (HIGH)** — Schema-Bump-Rückverweis fehlt in 4 State-erweiternden Stories
3.9 (`botManagedBills` v4→v5), 6.5 (`pawnSpecializations` v5→v6 bzw. v6→v7), 2.7 (`overlayVisible` v7→v8), 7.9 (`journeyQuest` v8→v9) haben NUR Feld-Deklaration, keine AC „SchemaVersion auf N+1 bumpen + Migrate-Pfad per Story 1.9 Registry + TC Savegame-Roundtrip v(N)→v(N+1)". Ohne Rückverweis wird Registry-Eintrag verpasst wenn Stories geshuffled oder Sub-Story gesplittet wird. Fix: je eine Schema-Bump-AC pro Story, identisch zu 2.3 AC 9.

### MED

**NEW-STAB-03 (MED)** — Fehlende Read-After-Write-Coverage
Zwei Writer-ähnliche Mutationen wurden nicht mit Read-After-Write-AC ausgestattet:
1. **`BlueprintPlacer.Apply` (Story 3.8)** — setzt Designations + `botPlacedThings`-Tag. Dritt-Mods (z. B. Replace Stuff, Blueprints) patchen `GenPlace` / `Designator.DesignateSingleCell`. Ohne Read-Back ist der Tag-Eintrag theoretisch inkonsistent zum tatsächlichen Blueprint.
2. **`BillManager.Apply` (Story 3.9)** — `BillStack.AddBill`. Mods wie RimFactory, Better Workbench Management patchen `BillStack`. Ohne Read-Back (`map.listerBills.AllBillsInOrder.Contains(…)` oder LoadID-Lookup) kann der `botManagedBills`-Tag auf nicht-existente Bill-IDs zeigen → Orphan-Cleanup-Panik.
Fix: AC „Read-Back via `map.listerThings`/`BillStack` + WARN-Log + Poisoned-Set" in 3.8 und 3.9.

**NEW-STAB-04 (MED)** — Story 3.7 Phase-Transition-Guard nicht standardisiert
AC 3 nutzt „Kein Emergency aktiv für 2 konsekutive Eval-Ticks" — semantisch ähnlich, aber nicht identisch zu `EmergencyResolver.ActiveEmergencies.Count == 0`. Zwei-Tick-Counter ist strenger als sofortige Prüfung — kann Phase-0-Exit um 2 Ticks verzögern, was bei Instant-Resolve-Emergencies (E-FOOD akut) Auto-Escape (D-26) triggern kann. Fix: Sprache an 3.11/4.2/4.5/4.6/6.2/6.4 angleichen ODER in 3.11 Grund-Explicit machen warum 3.7 strenger ist.

**NEW-STAB-05 (MED)** — Story 1.3 AC 15 Toast-Threshold (25%) ohne empirische Rationale
Der 25%-Threshold für User-Toast bei Pawn-thingIDNumber→UniqueLoadID-Mapping-Verlust ist willkürlich. Für eine 3-Pawn-Colony bedeutet 25% = „0.75 Pawns verloren → kein Toast bei 1 Verlust" (gerundet 33%). Bei einer 10-Pawn-Colony ist 25% = 2.5 Pawns, was dramatisch ist. Vorschlag: **absolute-OR-prozentuale Kombi**: Toast wenn `dropped >= 1 AND (dropped/total > 0.1 OR dropped >= 2)`. Alternativ: jeder Drop triggert einen zusammengefassten Toast am Ende der Migrate-Phase (nicht pro Entry).

**NEW-STAB-06 (MED)** — Story 3.4 `EmergencyResolver`-Modify ohne Schema-Impact-Check
AC Dev-Notes: `EmergencyResolver.cs | modify (Pawn-Exclusivity-Lock)`. Das Framework-Dict `pawnClaims: Dictionary<string, (string, int, int)>` — ist das transient oder persistent? Wenn persistent (Save-mitten-im-Raid mit aktivem Claim): Schema-Bump nötig. Wenn transient: Save-mitten-im-Raid verliert Claim → Dup-Claim in derselben Tick nach Load möglich. Keine der beteiligten Stories (3.1, 3.4, 4-9d) klärt das. Fix: Classification-AC in 3.1 ergänzen — empfohlen **transient + bei LoadedGame EmergencyResolver führt frische Evaluation** (pawnClaims werden in der ersten Eval-Tick nach Load neu vergeben).

### LOW

**NEW-STAB-07 (LOW)** — `BotSafe.SafeApply<T>(Func<T, bool> body, T plan, …)` — Return-Value-Semantik unklar
Story 1.10 AC 1 spezifiziert Signature, aber nicht was das `bool` bedeutet (Success? PlanApplied?) und was bei Exception returnt wird (false? Thrown? Plan-Retry?). Dev wird eigene Konvention erfinden. Fix: AC erweitern um „Rückgabe false bei Exception; caller entscheidet über Retry-Semantik".

**NEW-STAB-08 (LOW)** — Poisoned-Set-Unlock-Semantik unterschiedlich in 3.10 vs. 5.4 vs. 4.3
3.10 nutzt 10-min-Unlock-Timer (`unlockTimers`), 5.4 hat keinen expliziten Unlock (nur Eintrag), 4.3 referenziert „analog 3.10". Inkonsistent. Fix: zentrale Pattern-Doku in 3.10 referenzieren und sicherstellen dass 5.4, 4.3, 5.7, 6.8 die Unlock-Semantik übernehmen (10min → empfohlen) ODER Handler-Staleness-Pattern (3.13) als Unlock-Trigger nutzen.

---

## Unchanged-Risks

1. **Scribe-API-Backward-Compat-Annahme (1.9 Dev Notes)** — Annahme „Scribe-API funktioniert zwischen 1.5 und 1.6". Wenn Ludeon in 1.7 `Scribe_Values`-Interface ändert, bricht das gesamte Migrations-Modell. Kein Fallback-Plan. Aber: außerhalb unserer Kontrolle, Risiko-Akzeptanz dokumentiert wäre sauber.
2. **`BotSafe.SafeTick` reentrancy** — Wenn Exception in Exception-Log (z. B. `Log.Error` selbst wirft aus Stack-Overflow), entsteht Infinite-Loop. Story 1.10 spezifiziert nicht ob SafeTick re-entrant-safe ist.
3. **HashSet-Scribe-Reconstruction-Order (2.3 AC 10)** — Wenn `_excludedCellsFlat`-List beim Scribe partiell corrupted (z. B. ungerade Länge), entsteht ArgumentException. Kein Schutz-AC.

---

## Recommendation

**APPROVE-WITH-CHANGES.**

Pass-2 hat die **strukturellen HIGH-Findings** (Schema-Registry, Exception-Wrapper-Helper, RAW für Drafter, XXE) erfolgreich addressiert. Die verbleibenden Lücken sind **Integrations-Drift**: Helper-Infrastruktur existiert, aber die Einzelstories synchronisieren nicht konsequent per AC-Rückverweis.

**Blocker für Dev-Start:**
1. NEW-STAB-01 (Exception-Wrapper-AC in 9 Stories)
2. NEW-STAB-02 (Schema-Bump-AC in 3.9, 6.5, 2.7, 7.9)
3. NEW-STAB-03 (RAW-AC in 3.8, 3.9)

Diese sind schnell: 1-2 AC-Zeilen pro betroffener Story, Pattern ist klar. Nach diesem Pass-3 erwarte ich **APPROVE** — solange Guardian-Regel-4-konform alle MED/LOW ebenfalls gefixt werden (NEW-STAB-04 … NEW-STAB-08).

**Nicht-Blocker, aber Guardian-Regel-4:** Toast-Threshold-Rationale (NEW-STAB-05) und Poisoned-Set-Unlock-Konsistenz (NEW-STAB-08) sollten im gleichen Pass nachgezogen werden.

Word-Count: ~590.
