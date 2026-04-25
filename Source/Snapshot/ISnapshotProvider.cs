using Verse;

namespace RimWorldBot.Snapshot
{
    // Story 2.1 (Epic 2 Foundation, Architecture §2.2) — Single-Adapter zwischen RimWorld-Runtime
    // und Bot-Logik. ALLE Analyzer/Planner/Decision-Layers-Klassen konsumieren ISnapshotProvider,
    // NICHT Map/Pawn/Thing direkt.
    //
    // Testbarkeit (Architecture §9.1): FakeSnapshotProvider in Test-Assembly produziert in-memory
    // Snapshots ohne RimWorld-Dependency → Unit-Tests laufen ohne Verse-Runtime.
    //
    // Map-Parameter in GetCells: explizit übergeben statt aus Find.CurrentMap, weil Multi-Map-Unterstützung
    // (Caravans + Multiple-Colonies) Snapshot pro Map erlaubt. GetColony/GetPawns sind globally —
    // Colony ist single-state, Pawns aggregieren alle Maps.
    public interface ISnapshotProvider
    {
        CellSnapshot[] GetCells(Map map);
        ColonySnapshot GetColony();
        PawnSnapshot[] GetPawns();
    }
}
