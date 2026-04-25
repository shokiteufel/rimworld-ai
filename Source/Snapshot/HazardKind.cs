namespace RimWorldBot.Snapshot
{
    // Story 2.1 (Epic 2 Foundation) — Hazard-Klassifikation pro Cell.
    // Wird in CellSnapshot.HazardKind gefüllt; konsumiert von Story 2.3 (HazardScanner)
    // + Story 2.4 (DefensibilityScore) + Story 3.x (Emergency-Avoid-Filter).
    //
    // Identifier-only-Pattern (D-23 analog für Snapshots): enum statt RimWorld-Runtime-Type-Ref.
    public enum HazardKind
    {
        None = 0,
        Lava,         // TerrainDef "Lava" — F-AI-2 Avoid
        Pollution,    // Biotech-Pollution-Cell
        Toxic,        // Toxic-Fallout-Coverage
        Radiation     // Anomaly-Radiation
    }
}
