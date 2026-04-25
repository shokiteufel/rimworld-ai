using System.Collections.Immutable;
using RimWorldBot.Decision;
using Xunit;

namespace RimWorldBot.Tests.Decision
{
    // Story 1.13 Carry-Over aus Story 1.11 — Tests fuer PlanArbiter Layer-Praezedenz (AI-7).
    // PlanArbiter ist pure (keine Verse-Calls), deshalb 1:1 testbar ohne Mock-Infrastruktur.
    public class PlanArbiterTests
    {
        // ----- DraftOrder -----

        [Fact]
        public void ArbitrateDraftOrder_HigherLayerWins()
        {
            var phaseGoal = new DraftOrder(
                Draft: ImmutableHashSet.Create("pawn-A"),
                Undraft: ImmutableHashSet<string>.Empty,
                RetreatPoint: null);
            var emergency = new DraftOrder(
                Draft: ImmutableHashSet<string>.Empty,
                Undraft: ImmutableHashSet.Create("pawn-A"),
                RetreatPoint: null);

            var result = PlanArbiter.ArbitrateDraftOrder(new[]
            {
                (phaseGoal, PlanLayer.PhaseGoal),
                (emergency, PlanLayer.Emergency)
            });

            Assert.DoesNotContain("pawn-A", result.Draft);
            Assert.Contains("pawn-A", result.Undraft);
        }

        [Fact]
        public void ArbitrateDraftOrder_NoProducers_ReturnsEmpty()
        {
            var result = PlanArbiter.ArbitrateDraftOrder(System.Array.Empty<(DraftOrder, PlanLayer)>());
            Assert.Empty(result.Draft);
            Assert.Empty(result.Undraft);
            Assert.Null(result.RetreatPoint);
        }

        [Fact]
        public void ArbitrateDraftOrder_RetreatPoint_HighestLayerWins()
        {
            var lowLayer = new DraftOrder(ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty,
                RetreatPoint: (10, 10));
            var highLayer = new DraftOrder(ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty,
                RetreatPoint: (50, 50));

            var result = PlanArbiter.ArbitrateDraftOrder(new[]
            {
                (lowLayer, PlanLayer.PhaseGoal),
                (highLayer, PlanLayer.Emergency)
            });

            Assert.Equal((50, 50), result.RetreatPoint);
        }

        // ----- WorkPriorityPlan -----

        [Fact]
        public void ArbitrateWorkPriorityPlan_HigherLayerOverridesPriority()
        {
            var phaseGoal = BuildSinglePawnPriority("pawn-A", "Hauling", 3);
            var emergency = BuildSinglePawnPriority("pawn-A", "Hauling", 1);

            var result = PlanArbiter.ArbitrateWorkPriorityPlan(new[]
            {
                (phaseGoal, PlanLayer.PhaseGoal),
                (emergency, PlanLayer.Emergency)
            });

            Assert.Equal(1, result.Priorities["pawn-A"]["Hauling"]);
        }

        static WorkPriorityPlan BuildSinglePawnPriority(string pawnId, string workType, int prio)
        {
            var inner = ImmutableDictionary<string, int>.Empty.Add(workType, prio);
            var outer = ImmutableDictionary<string, ImmutableDictionary<string, int>>.Empty.Add(pawnId, inner);
            return new WorkPriorityPlan(outer);
        }

        // ----- BuildPlan -----

        [Fact]
        public void ArbitrateBuildPlan_SameCellHigherLayerWins()
        {
            var skillGrind = new BuildPlan(ImmutableList.Create(
                new BlueprintIntent("Wall", X: 5, Z: 5, Rotation: 0)));
            var emergency = new BuildPlan(ImmutableList.Create(
                new BlueprintIntent("Sandbag", X: 5, Z: 5, Rotation: 0)));

            var result = PlanArbiter.ArbitrateBuildPlan(new[]
            {
                (skillGrind, PlanLayer.SkillGrind),
                (emergency, PlanLayer.Emergency)
            });

            Assert.Single(result.Intents);
            Assert.Equal("Sandbag", result.Intents[0].DefName);
        }

        [Fact]
        public void ArbitrateBuildPlan_DifferentCellsBothPersist()
        {
            var planA = new BuildPlan(ImmutableList.Create(
                new BlueprintIntent("Wall", 1, 1, 0)));
            var planB = new BuildPlan(ImmutableList.Create(
                new BlueprintIntent("Door", 2, 2, 0)));

            var result = PlanArbiter.ArbitrateBuildPlan(new[]
            {
                (planA, PlanLayer.PhaseGoal),
                (planB, PlanLayer.PhaseGoal)
            });

            Assert.Equal(2, result.Intents.Count);
        }

        // ----- BillPlan -----

        [Fact]
        public void ArbitrateBillPlan_SameKeyHigherLayerWins()
        {
            var phaseGoal = new BillPlan(ImmutableList.Create(
                new BillIntent(BillIntentKind.Add, "MakeMeal", WorkbenchThingIDNumber: 42, TargetCount: 5)));
            var emergency = new BillPlan(ImmutableList.Create(
                new BillIntent(BillIntentKind.Remove, "MakeMeal", WorkbenchThingIDNumber: 42, TargetCount: 0)));

            var result = PlanArbiter.ArbitrateBillPlan(new[]
            {
                (phaseGoal, PlanLayer.PhaseGoal),
                (emergency, PlanLayer.Emergency)
            });

            Assert.Single(result.Intents);
            Assert.Equal(BillIntentKind.Remove, result.Intents[0].Kind);
        }

        // ----- CR Story 1.13 HIGH-4 Coverage-Erweiterung -----

        [Fact]
        public void ArbitrateBillPlan_DifferentWorkbenches_BothPersist()
        {
            var planA = new BillPlan(ImmutableList.Create(
                new BillIntent(BillIntentKind.Add, "MakeMeal", WorkbenchThingIDNumber: 1, TargetCount: 5)));
            var planB = new BillPlan(ImmutableList.Create(
                new BillIntent(BillIntentKind.Add, "MakeMeal", WorkbenchThingIDNumber: 2, TargetCount: 3)));

            var result = PlanArbiter.ArbitrateBillPlan(new[]
            {
                (planA, PlanLayer.PhaseGoal),
                (planB, PlanLayer.PhaseGoal)
            });

            Assert.Equal(2, result.Intents.Count);
        }

        [Fact]
        public void ArbitrateBillPlan_SameWorkbenchDifferentRecipes_BothPersist()
        {
            var planA = new BillPlan(ImmutableList.Create(
                new BillIntent(BillIntentKind.Add, "MakeMeal", WorkbenchThingIDNumber: 1, TargetCount: 5)));
            var planB = new BillPlan(ImmutableList.Create(
                new BillIntent(BillIntentKind.Add, "BrewBeer", WorkbenchThingIDNumber: 1, TargetCount: 3)));

            var result = PlanArbiter.ArbitrateBillPlan(new[]
            {
                (planA, PlanLayer.PhaseGoal),
                (planB, PlanLayer.PhaseGoal)
            });

            Assert.Equal(2, result.Intents.Count);
        }

        [Fact]
        public void ArbitrateBuildPlan_SameLayerConflict_FirstProducerWins()
        {
            // Same-Layer-Konflikt ohne decisionLog → erste Producer (in Layer-stabiler Sort-Order
            // = Eingangsreihenfolge bei gleichem Layer) gewinnt deterministisch.
            var planA = new BuildPlan(ImmutableList.Create(new BlueprintIntent("Wall", 5, 5, 0)));
            var planB = new BuildPlan(ImmutableList.Create(new BlueprintIntent("Sandbag", 5, 5, 0)));

            var result = PlanArbiter.ArbitrateBuildPlan(new[]
            {
                (planA, PlanLayer.PhaseGoal),
                (planB, PlanLayer.PhaseGoal)
            });

            Assert.Single(result.Intents);
            Assert.Equal("Wall", result.Intents[0].DefName);
        }

        [Fact]
        public void ArbitrateWorkPriorityPlan_NullPriorities_Skipped()
        {
            // Source Z. 137 `if (plan?.Priorities == null) continue;` → Plan mit null-Dict
            // wird ignoriert, andere Producer bleiben gueltig.
            var nullPlan = new WorkPriorityPlan(null);
            var validPlan = BuildSinglePawnPriority("pawn-A", "Hauling", 2);

            var result = PlanArbiter.ArbitrateWorkPriorityPlan(new[]
            {
                (nullPlan, PlanLayer.Emergency),
                (validPlan, PlanLayer.PhaseGoal)
            });

            Assert.Equal(2, result.Priorities["pawn-A"]["Hauling"]);
        }

        [Fact]
        public void ArbitrateDraftOrder_FocusedFireTargets_HigherLayerOverrides()
        {
            // Architecture §2.3a CC-STORIES-14: FocusedFireTargets als per-Pawn-Layer-Map.
            // Source Z. 59-65 — pro Pawn-Key gewinnt die höhere Layer.
            var phaseGoal = new DraftOrder(
                ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty, null)
            {
                FocusedFireTargets = ImmutableDictionary<string, string>.Empty.Add("pawn-A", "raider-1")
            };
            var emergency = new DraftOrder(
                ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty, null)
            {
                FocusedFireTargets = ImmutableDictionary<string, string>.Empty.Add("pawn-A", "raider-7")
            };

            var result = PlanArbiter.ArbitrateDraftOrder(new[]
            {
                (phaseGoal, PlanLayer.PhaseGoal),
                (emergency, PlanLayer.Emergency)
            });

            Assert.Equal("raider-7", result.FocusedFireTargets["pawn-A"]);
        }

        [Fact]
        public void ArbitrateDraftOrder_AssignedPositions_HigherLayerOverrides()
        {
            // Source Z. 66-72 analog zu FocusedFireTargets.
            var phaseGoal = new DraftOrder(
                ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty, null)
            {
                AssignedPositions = ImmutableDictionary<string, (int X, int Z)>.Empty.Add("pawn-A", (10, 10))
            };
            var emergency = new DraftOrder(
                ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty, null)
            {
                AssignedPositions = ImmutableDictionary<string, (int X, int Z)>.Empty.Add("pawn-A", (50, 50))
            };

            var result = PlanArbiter.ArbitrateDraftOrder(new[]
            {
                (phaseGoal, PlanLayer.PhaseGoal),
                (emergency, PlanLayer.Emergency)
            });

            Assert.Equal((50, 50), result.AssignedPositions["pawn-A"]);
        }

        // Story 1.14 (D-38): Skip-deferred Tests reaktiviert weil mscorlib-Mismatch gefixt
        // (Production-DLL nutzt jetzt Microsoft net472 mscorlib statt Krafs-Mono-mscorlib).

        [Fact]
        public void ArbitrateBuildPlan_SameLayerConflict_LogsToDecisionLog()
        {
            var log = new RimWorldBot.Data.RecentDecisionsBuffer(transientCap: 100, pinnedCap: 25);
            var planA = new BuildPlan(ImmutableList.Create(new BlueprintIntent("Wall", 5, 5, 0)));
            var planB = new BuildPlan(ImmutableList.Create(new BlueprintIntent("Sandbag", 5, 5, 0)));

            PlanArbiter.ArbitrateBuildPlan(new[]
            {
                (planA, PlanLayer.PhaseGoal),
                (planB, PlanLayer.PhaseGoal)
            }, log);

            Assert.Contains(log.Transient, e => e.Kind == "plan-arbitration-conflict");
        }

        [Fact]
        public void ArbitrateDraftOrder_SameLayerConflict_LogsConflict()
        {
            // Same-Layer mit gegensaetzlicher Draft/Undraft-Action → "plan-arbitration-conflict".
            // (Cross-Layer-Override-Logging in MergeDraftSet ist defensive-code fuer Race-Conditions
            // beim manuellen Producer-Reordering; mit aktuellem SortByLayerDescending nicht
            // erreichbar weil hoehere Layer immer zuerst processed werden.)
            var log = new RimWorldBot.Data.RecentDecisionsBuffer();
            var planA = new DraftOrder(
                Draft: ImmutableHashSet.Create("pawn-A"),
                Undraft: ImmutableHashSet<string>.Empty,
                RetreatPoint: null);
            var planB = new DraftOrder(
                Draft: ImmutableHashSet<string>.Empty,
                Undraft: ImmutableHashSet.Create("pawn-A"),
                RetreatPoint: null);

            PlanArbiter.ArbitrateDraftOrder(new[]
            {
                (planA, PlanLayer.PhaseGoal),
                (planB, PlanLayer.PhaseGoal)
            }, log);

            Assert.Contains(log.Transient, e => e.Kind == "plan-arbitration-conflict");
        }
    }
}
