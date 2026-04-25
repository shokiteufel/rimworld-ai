using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RimWorldBot.Data;

namespace RimWorldBot.Decision
{
    // Story 1.11 (CC-STORIES-04, AI-7) — zentrale Plan-Arbitrage für konkurrierende Producer.
    //
    // AI-7 Layer-Präzedenz: Emergency > Override > PhaseGoal > SkillGrind > Default.
    // Pro Pawn/Cell/Workbench: höhere Layer überschreibt niedrigere; gleiche Layer
    // → Konflikt-Log + erster Producer gewinnt (deterministisch durch Producer-Order).
    //
    // Stateless — pro Tick neu aufgerufen via BotController-Tick-Flow (Story 2.x Integration).
    //
    // Alle 4 Arbitrate-Methoden folgen dem Pattern:
    //   1. Producer nach Layer absteigend sortieren
    //   2. Plan-Items aus höherem Layer haben Priorität
    //   3. Bei Conflict: DecisionLog-Eintrag (über recentDecisions wenn verfügbar)
    //   4. Merge zu einem einzigen applyable Plan
    //
    // TODO(Story 3.1): Pawn-Exclusivity-Lock-Integration — sobald EmergencyResolver.pawnClaims
    // existiert, vor Layer-Vergleich Lock-Status prüfen damit gelockter Pawn nicht überstimmt wird.
    // TODO(Story 2.x): BotController-Tick-Flow-Integration — nach allen Planner.Plan()-Calls
    // PlanArbiter rufen, dann Apply-Layer mit gemergetem Plan.
    public static class PlanArbiter
    {
        // ----- DraftOrder -----

        // Merget multiple DraftOrder-Producer. Pawn-UniqueLoadID-Konflikte werden nach
        // Layer aufgelöst: höhere Layer gewinnt für Draft/Undraft/AssignedPosition/FocusedFire.
        // RetreatPoint: höchster Layer mit gesetztem Wert gewinnt (nullable).
        public static DraftOrder ArbitrateDraftOrder(
            IEnumerable<(DraftOrder plan, PlanLayer layer)> producers,
            RecentDecisionsBuffer decisionLog = null)
        {
            var sorted = SortByLayerDescending(producers);
            if (sorted.Count == 0) return EmptyDraftOrder();

            // Pawn → (Layer, Action: Draft|Undraft) — höchster Layer gewinnt
            var draftDecision = new Dictionary<string, (PlanLayer Layer, bool Draft)>();
            (int X, int Z)? retreatPoint = null;
            PlanLayer retreatPointLayer = PlanLayer.Default;
            var focusedFire = new Dictionary<string, (PlanLayer Layer, string Target)>();
            var assignedPos = new Dictionary<string, (PlanLayer Layer, (int X, int Z) Pos)>();

            foreach (var (plan, layer) in sorted)
            {
                if (plan == null) continue;
                MergeDraftSet(plan.Draft, layer, draft: true, draftDecision, decisionLog);
                MergeDraftSet(plan.Undraft, layer, draft: false, draftDecision, decisionLog);

                if (plan.RetreatPoint.HasValue && layer > retreatPointLayer)
                {
                    retreatPoint = plan.RetreatPoint;
                    retreatPointLayer = layer;
                }

                foreach (var kv in plan.FocusedFireTargets)
                {
                    if (!focusedFire.TryGetValue(kv.Key, out var existing) || layer > existing.Layer)
                    {
                        focusedFire[kv.Key] = (layer, kv.Value);
                    }
                }
                foreach (var kv in plan.AssignedPositions)
                {
                    if (!assignedPos.TryGetValue(kv.Key, out var existing) || layer > existing.Layer)
                    {
                        assignedPos[kv.Key] = (layer, kv.Value);
                    }
                }
            }

            var draftSet = ImmutableHashSet.CreateRange(
                draftDecision.Where(kv => kv.Value.Draft).Select(kv => kv.Key));
            var undraftSet = ImmutableHashSet.CreateRange(
                draftDecision.Where(kv => !kv.Value.Draft).Select(kv => kv.Key));

            return new DraftOrder(draftSet, undraftSet, retreatPoint)
            {
                FocusedFireTargets = focusedFire.ToImmutableDictionary(kv => kv.Key, kv => kv.Value.Target),
                AssignedPositions = assignedPos.ToImmutableDictionary(kv => kv.Key, kv => kv.Value.Pos)
            };
        }

        static void MergeDraftSet(
            ImmutableHashSet<string> set, PlanLayer layer, bool draft,
            Dictionary<string, (PlanLayer Layer, bool Draft)> decision,
            RecentDecisionsBuffer decisionLog)
        {
            foreach (var pawnId in set)
            {
                if (decision.TryGetValue(pawnId, out var existing))
                {
                    if (layer > existing.Layer)
                    {
                        // Cross-Layer Override: höhere Layer gewinnt automatisch.
                        // HIGH-1-Fix (Story 1.11 CR Round 1): bei gegensätzlicher Action loggen
                        // damit Story 8.7 Debug-Panel den Override-Pfad sichtbar machen kann.
                        if (existing.Draft != draft)
                        {
                            decisionLog?.Add(new DecisionLogEntry(
                                kind: "plan-arbitration-override",
                                reason: $"DraftOrder layer-override for pawn {pawnId}: {existing.Layer}.{(existing.Draft ? "Draft" : "Undraft")} → {layer}.{(draft ? "Draft" : "Undraft")}"));
                        }
                        decision[pawnId] = (layer, draft);
                    }
                    else if (layer == existing.Layer && existing.Draft != draft)
                    {
                        decisionLog?.Add(new DecisionLogEntry(
                            kind: "plan-arbitration-conflict",
                            reason: $"DraftOrder same-layer ({layer}) conflict for pawn {pawnId}: existing={existing.Draft}, new={draft} — keeping existing"));
                    }
                }
                else
                {
                    decision[pawnId] = (layer, draft);
                }
            }
        }

        // ----- WorkPriorityPlan -----

        public static WorkPriorityPlan ArbitrateWorkPriorityPlan(
            IEnumerable<(WorkPriorityPlan plan, PlanLayer layer)> producers,
            RecentDecisionsBuffer decisionLog = null)
        {
            var sorted = SortByLayerDescending(producers);
            if (sorted.Count == 0) return EmptyWorkPriorityPlan();

            // pawn → (workType → (Layer, Priority))
            var merged = new Dictionary<string, Dictionary<string, (PlanLayer Layer, int Priority)>>();

            foreach (var (plan, layer) in sorted)
            {
                if (plan?.Priorities == null) continue;
                foreach (var pawnEntry in plan.Priorities)
                {
                    if (!merged.TryGetValue(pawnEntry.Key, out var pawnPriorities))
                    {
                        pawnPriorities = new Dictionary<string, (PlanLayer, int)>();
                        merged[pawnEntry.Key] = pawnPriorities;
                    }
                    foreach (var workEntry in pawnEntry.Value)
                    {
                        if (!pawnPriorities.TryGetValue(workEntry.Key, out var existing) || layer > existing.Layer)
                        {
                            pawnPriorities[workEntry.Key] = (layer, workEntry.Value);
                        }
                        else if (layer == existing.Layer && existing.Priority != workEntry.Value)
                        {
                            decisionLog?.Add(new DecisionLogEntry(
                                kind: "plan-arbitration-conflict",
                                reason: $"WorkPriority same-layer ({layer}) conflict for {pawnEntry.Key}/{workEntry.Key}: existing={existing.Priority}, new={workEntry.Value} — keeping existing"));
                        }
                    }
                }
            }

            // Doppelt-nested ToImmutableDictionary: bei 50 Pawns × 30 WorkTypes = ~1500 Allocs/Tick.
            // PlanArbiter läuft selten (1× pro BotController-Tick = alle 60 Ticks ≈ 1×/s), deshalb
            // akzeptabel. CreateBuilder-Refactor optional wenn Profiling Hot-Spot zeigt.
            var result = merged.ToImmutableDictionary(
                kv => kv.Key,
                kv => kv.Value.ToImmutableDictionary(w => w.Key, w => w.Value.Priority));
            return new WorkPriorityPlan(result);
        }

        // ----- BuildPlan -----

        // BuildPlan-Konflikte: pro (X,Z)-Cell höchster Layer gewinnt. Niedrigere Layer-Intents
        // an gleicher Cell werden gedroppt mit DecisionLog.
        public static BuildPlan ArbitrateBuildPlan(
            IEnumerable<(BuildPlan plan, PlanLayer layer)> producers,
            RecentDecisionsBuffer decisionLog = null)
        {
            var sorted = SortByLayerDescending(producers);
            if (sorted.Count == 0) return new BuildPlan(ImmutableList<BlueprintIntent>.Empty);

            var byCell = new Dictionary<(int X, int Z), (PlanLayer Layer, BlueprintIntent Intent)>();

            foreach (var (plan, layer) in sorted)
            {
                if (plan?.Intents == null) continue;
                foreach (var intent in plan.Intents)
                {
                    var key = (intent.X, intent.Z);
                    if (!byCell.TryGetValue(key, out var existing) || layer > existing.Layer)
                    {
                        // existing.Intent != null gilt nur im Override-Fall (zweite Bedingung der || war true);
                        // bei First-Insert ist existing.Intent default(BlueprintIntent)=null, kein Log.
                        if (existing.Intent != null && existing.Intent.DefName != intent.DefName)
                        {
                            decisionLog?.Add(new DecisionLogEntry(
                                kind: "plan-arbitration-override",
                                reason: $"BuildPlan cell ({intent.X},{intent.Z}) higher-layer ({layer}) overrides {existing.Layer} {existing.Intent.DefName} → {intent.DefName}"));
                        }
                        byCell[key] = (layer, intent);
                    }
                    else if (layer == existing.Layer && existing.Intent.DefName != intent.DefName)
                    {
                        decisionLog?.Add(new DecisionLogEntry(
                            kind: "plan-arbitration-conflict",
                            reason: $"BuildPlan same-layer ({layer}) conflict at cell ({intent.X},{intent.Z}): existing={existing.Intent.DefName}, new={intent.DefName} — keeping existing"));
                    }
                }
            }

            return new BuildPlan(byCell.Values.Select(v => v.Intent).ToImmutableList());
        }

        // ----- BillPlan -----

        // BillPlan-Konflikte: pro (Workbench, Recipe)-Tupel höchster Layer gewinnt.
        // Add+Remove same-layer same-key → Add gewinnt (defensiv: falls Workbench im Phase-Goal-Layer
        // gleichzeitig Add+Remove kommt, ist das Bug → erste Producer gewinnt + Log).
        public static BillPlan ArbitrateBillPlan(
            IEnumerable<(BillPlan plan, PlanLayer layer)> producers,
            RecentDecisionsBuffer decisionLog = null)
        {
            var sorted = SortByLayerDescending(producers);
            if (sorted.Count == 0) return new BillPlan(ImmutableList<BillIntent>.Empty);

            var byKey = new Dictionary<(int WorkbenchId, string RecipeDefName), (PlanLayer Layer, BillIntent Intent)>();

            foreach (var (plan, layer) in sorted)
            {
                if (plan?.Intents == null) continue;
                foreach (var intent in plan.Intents)
                {
                    var key = (intent.WorkbenchThingIDNumber, intent.RecipeDefName);
                    if (!byKey.TryGetValue(key, out var existing) || layer > existing.Layer)
                    {
                        byKey[key] = (layer, intent);
                    }
                    else if (layer == existing.Layer && existing.Intent.Kind != intent.Kind)
                    {
                        decisionLog?.Add(new DecisionLogEntry(
                            kind: "plan-arbitration-conflict",
                            reason: $"BillPlan same-layer ({layer}) conflict at workbench {intent.WorkbenchThingIDNumber}/{intent.RecipeDefName}: existing={existing.Intent.Kind}, new={intent.Kind} — keeping existing"));
                    }
                }
            }

            return new BillPlan(byKey.Values.Select(v => v.Intent).ToImmutableList());
        }

        // ----- Helpers -----

        // Sortiert nach Layer absteigend (Emergency=4 zuerst, Default=0 zuletzt).
        // OrderByDescending ist stable → bei gleichem Layer bleibt Producer-Reihenfolge erhalten.
        static List<(T plan, PlanLayer layer)> SortByLayerDescending<T>(
            IEnumerable<(T plan, PlanLayer layer)> producers)
        {
            return producers?.OrderByDescending(p => (int)p.layer).ToList()
                ?? new List<(T, PlanLayer)>();
        }

        static DraftOrder EmptyDraftOrder() => new(
            ImmutableHashSet<string>.Empty,
            ImmutableHashSet<string>.Empty,
            null);

        static WorkPriorityPlan EmptyWorkPriorityPlan() => new(
            ImmutableDictionary<string, ImmutableDictionary<string, int>>.Empty);
    }
}
