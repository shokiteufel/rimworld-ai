using System.Collections.Immutable;

namespace RimWorldBot.Snapshot
{
    // Story 2.1 — minimaler ColonySnapshot-Stub als Forward-Declaration für ISnapshotProvider.
    // Vollständige Implementation der Felder erfolgt in Folge-Stories (Architecture §2.2):
    //   - Food-Days: Story 3.5 (Emergency-Food)
    //   - Colonist-Count: Story 2.x via PawnSnapshot[]-Aggregation
    //   - Mood-Avg: Story 4.9a (Emergency-Mood)
    //   - Faction-Relations: Story 4.3 (Recruiting)
    //   - Tech-Level: Story 6.x (Industrialization)
    //
    // Identifier-only-Pattern (D-23 analog): Faction-Relations als ImmutableDictionary<string Faction-LoadID, int Goodwill>.
    //
    // Persistence (D-14): NICHT persistiert.
    public sealed record ColonySnapshot(
        double FoodDays,
        int ColonistCount,
        double MoodAverage,
        ImmutableDictionary<string, int> FactionGoodwill,
        int TechLevel)
    {
        public static ColonySnapshot Empty => new(
            FoodDays: 0.0,
            ColonistCount: 0,
            MoodAverage: 0.0,
            FactionGoodwill: ImmutableDictionary<string, int>.Empty,
            TechLevel: 0);
    }
}
