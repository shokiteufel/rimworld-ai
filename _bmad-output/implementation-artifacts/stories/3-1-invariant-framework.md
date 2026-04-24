# Story 3.1: Invariant-Framework (Abstract-Klasse + Resolver)

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** M
**Decisions referenced:** D-16 (Emergency-Utility-Scoring), AI-2 (pure Decision-Klassen), Mod-Leitfaden §2

## Story
Als Mod-Entwickler möchte ich das **abstrakte `Invariant`-Basis-Framework + `InvariantSet` + `EmergencyResolver`** implementieren, damit ab Story 3.2 die 12 konkreten Invariants (I1–I12) im gleichen Kontrakt implementiert werden können.

## Acceptance Criteria
1. `Source/Invariants/Invariant.cs` abstract: `CheckResult Check(ColonySnapshot state)`, `string Id`, `int BasePriority`
2. `InvariantResult` record: `(string InvariantId, bool Violated, double Severity, string Reason)`
3. `InvariantSet`-Klasse in `Source/Invariants/InvariantSet.cs` mit Liste `Invariant[]`, `CheckAll(snapshot) → InvariantResult[]`
4. **Idempotenz-Kontrakt** (AI-2): gleicher Snapshot → gleicher InvariantResult; keine Side-Effects
5. `EmergencyResolver` wie in Architecture §2.1: `Resolve(InvariantResult[], ColonySnapshot) → EmergencyChoice?` — wählt höchste-scorende aktive Emergency via `BasePriority + context_modifiers`
6. `EmergencyHandler` abstract: `Score(state)`, `Eligibility(state)`, `Claim(pawns) → DraftOrder?`, `Apply(controller)` + **`LockPriority: int`-Property** (CC-STORIES-06, default 50)
7. **Pawn-Exclusivity-Lock-Framework** (CC-STORIES-06 aus Review Round 1):
   - `EmergencyResolver` hat `pawnClaims: Dictionary<string UniqueLoadID, (string EmergencyId, int UnlockTick, int LockPriority)>`
   - `Claim(pawns)` registriert für jeden claimten Pawn einen Eintrag: `UnlockTick = now + handler.LockDurationTicks` (default 3600 = 60s bei 60 TPS)
   - **Re-Claim während aktiver Lock-Period nur erlaubt bei höherer LockPriority**
   - Lock-Priority-Matrix (dokumentiert, nicht hardcoded): E-RAID=100, E-BLEED=90, E-HEALTH=80, E-FOODDAYS=70, E-MEDICINE=50, E-FIRE=85, E-FOOD=65, E-SHELTER=55, E-TEMP=55, E-MOOD=20, E-PAWNSLEEP=25, E-MENTALBREAK=30
   - `ReleasePawn(string UniqueLoadID)` — explizit, z. B. bei Handler-Resolve-Complete
   - Auto-Release bei `UnlockTick <= now`
   - DecisionLog-Eintrag bei Lock-Konflikt (niedrigere Priority gewinnt nicht)
8. `I0_ColonyExtinct` konkret: `colonist_count == 0 → Violated=true, Severity=∞`, Handler `E_EXTINCT` setzt Bot auf OFF
9. Unit-Tests: Invariant-Idempotenz, EmergencyResolver-Utility-Scoring mit 2 konkurrierenden Emergencies, **Pawn-Lock-Konflikt** (LockPriority-basiert)

## Tasks
- [ ] `Source/Invariants/Invariant.cs` abstract
- [ ] `Source/Invariants/InvariantResult.cs` record
- [ ] `Source/Invariants/InvariantSet.cs`
- [ ] `Source/Emergency/EmergencyHandler.cs` abstract
- [ ] `Source/Emergency/EmergencyResolver.cs` Utility-Scoring
- [ ] `Source/Invariants/I0_ColonyExtinct.cs` + `Source/Emergency/E_Extinct.cs`
- [ ] Unit-Tests für Framework + I0

## Dev Notes
**Architektur-Kontext:** §2.1 + D-16 + D-22 (AI-7 Emergency > Override > PhaseGoal).
**Nehme an, dass:** `ColonySnapshot` (aus Story 2.1 erweitert in Story 3.x) enthält alle Felder die Invariants prüfen.
**Vorausgesetzt:** Story 1.3 (Snapshot-Types), Story 2.1 (Snapshot-Provider).

## File List
| Pfad | Op |
|---|---|
| `Source/Invariants/Invariant.cs` | create |
| `Source/Invariants/InvariantResult.cs` | create |
| `Source/Invariants/InvariantSet.cs` | create |
| `Source/Emergency/EmergencyHandler.cs` | create |
| `Source/Emergency/EmergencyResolver.cs` | create |
| `Source/Invariants/I0_ColonyExtinct.cs` | create |
| `Source/Emergency/E_Extinct.cs` | create |

## Testing
- Unit: Framework-Idempotenz, Utility-Scoring mit Context-Modifiers
- Integration: I0 mit ColonySnapshot.ColonistCount=0 → E_Extinct gewählt

## Review-Gate
Code-Review gegen §2.1, D-16 (Utility statt fixe Prio), AI-2 (pure), **CC-STORIES-06 Lock-Framework korrekt implementiert**.

## Transient/Persistent
- `pawnClaims: Dictionary<string UniqueLoadID, (string EmergencyId, int UnlockTick, int LockPriority)>` ist **transient** — re-initialisiert bei `LoadedGame`/`StartedNewGame`. Begründung: Emergency-State überlebt Save-Load nicht; neue Emergencies werden nach Load fresh detektiert. Explizit KEIN Eintrag in `SchemaVersionRegistry` (CC-STORIES-01), KEIN `Scribe_Collections.Look`-Call in `ExposeData` des EmergencyResolver.
- `stalenessCounter` pro Handler (aus Story 3.13) ebenfalls **transient**.
