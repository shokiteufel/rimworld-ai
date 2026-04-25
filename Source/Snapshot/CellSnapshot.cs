namespace RimWorldBot.Snapshot
{
    // Story 2.1 (Epic 2 Foundation) — POCO-Snapshot pro Map-Cell.
    // Architecture §2.2 + Identifier-only-Pattern (D-23 analog für Snapshots): keine RimWorld-Runtime-
    // Refs (TerrainDef/IntVec3/Map), nur primitive + enum + record-tuple.
    //
    // Felder:
    //   - Position: (X, Z) statt IntVec3 — keine Verse-Dep, hashable für Dictionary-Keys.
    //   - TerrainDefName: string-Lookup statt TerrainDef-Ref. Resolved bei Apply via DefDatabase.
    //   - Fertility: 0.0..1.0, von TerrainDef.fertility.
    //   - HasWater: terrain.IsWater (Story 2.x Wild-Plant-Avoid + 4.x Power-Generator-Placement).
    //   - HazardKind: enum-Klassifikation (Story 2.3 erweitert ClassifyHazard um weitere Quellen).
    //   - HasRoof: für Build-Planner + Mood-Buff (jeder Roof-Typ).
    //   - IsMountain: TRUE wenn Cell unter `RoofRockThick`-overhead steht (Vanilla `RoofDef.isThickRoof`).
    //     ACHTUNG: NICHT identisch mit "mineable rock wall" (das ist ThingDef.mineable=true, separat
    //     in HasResources erfasst). IsMountain == "Mountain-Base-Overhang" für Insectoid-Spawn-Risk
    //     + Drop-Pod-Block + Mountain-Base-Strategy. CR Story 2.1 HIGH-2 — Naming-Disambiguation.
    //   - HasResources: schnelle "Cell hat mineable Thing (Stein/Mineral)"-Flag (Story 2.x Mining-Planner).
    //   - ChokepointScore: 0.0..1.0, von Story 2.4 DefensibilityScore gefüllt; default 0 in 2.1.
    //   - WildPlant: nullable enum (Story 2.2). null = keine wilde Ess-/Medizinal-Pflanze auf der Cell.
    //     Story 2.5 Scoring W_FOOD-Beitrag konsumiert das.
    //
    // Persistence (D-14): NICHT persistiert. Per-Tick recomputable. MapAnalysisSummary persistiert
    // separat nur Top-3-Sites pro Map.
    public sealed record CellSnapshot(
        (int X, int Z) Position,
        string TerrainDefName,
        float Fertility,
        bool HasWater,
        HazardKind HazardKind,
        bool HasRoof,
        bool IsMountain,
        bool HasResources,
        float ChokepointScore,
        WildPlantKind? WildPlant = null);
}
