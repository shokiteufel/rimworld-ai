using System.Collections.Immutable;

namespace RimWorldBot.Decision
{
    // Story 1.11 — Plan-Records aus Architecture §2.3a vorgezogen damit PlanArbiter
    // typsicher arbeiten kann. Werden von den Stories 3.8/3.9/3.10/5.4 (BuildPlanner,
    // BillPlanner, WorkPlanner, DraftController) konsumiert.
    //
    // D-23 Identifier-only-Pattern: keine RimWorld-Runtime-Typen (ThingDef, IntVec3, Pawn etc.),
    // nur strings, ints, enums, ImmutableCollections. Apply-Layer (Story 5.4 etc.) resolved
    // Identifier zu Runtime-Typen via DefDatabase.Named()/Find.Maps.SelectMany etc.
    //
    // F-ARCH-17 Immutability: ImmutableList/Dictionary/HashSet aus System.Collections.Immutable
    // statt IReadOnly* — echte Immutabilität, kein View-Leak via Cast-back.

    public enum BillIntentKind
    {
        Add,
        Remove,
        UpdateCount
    }

    public sealed record BillIntent(
        BillIntentKind Kind,
        string RecipeDefName,
        int WorkbenchThingIDNumber,
        int TargetCount);

    public sealed record BillPlan(
        ImmutableList<BillIntent> Intents);

    public sealed record WorkPriorityPlan(
        // pawn UniqueLoadID → (WorkTypeDef.defName → priority 0-4)
        ImmutableDictionary<string, ImmutableDictionary<string, int>> Priorities);

    public sealed record BlueprintIntent(
        string DefName,
        int X,
        int Z,
        byte Rotation);   // Rot4.AsByte

    public sealed record BuildPlan(
        ImmutableList<BlueprintIntent> Intents);

    // DraftOrder mit Combat-Subtypen-Erweiterung aus CC-STORIES-14 (Round-1-Review).
    // FocusedFireTargets + AssignedPositions als init-only Properties mit Empty-Defaults
    // damit existierende 3-arg-Konstruktor-Aufrufe (5.3, 5.5, 5.7, 5.8, 7.8, 7.13) nicht brechen.
    public sealed record DraftOrder(
        ImmutableHashSet<string> Draft,
        ImmutableHashSet<string> Undraft,
        (int X, int Z)? RetreatPoint)
    {
        public ImmutableDictionary<string, string> FocusedFireTargets { get; init; }
            = ImmutableDictionary<string, string>.Empty;
        public ImmutableDictionary<string, (int X, int Z)> AssignedPositions { get; init; }
            = ImmutableDictionary<string, (int X, int Z)>.Empty;
    }
}
