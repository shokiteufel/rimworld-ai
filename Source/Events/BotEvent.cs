namespace RimWorldBot.Events
{
    // Transient Event-Basistyp (D-18 + D-19).
    // EnqueueTick = GenTicks.TicksGame bei Enqueue; BoundedEventQueue verwirft abgestandene Events
    // (älter als StalenessThreshold) beim Dequeue statt bei Enqueue.
    // Identifier-only (D-23): KEINE RimWorld-Runtime-Typen in Records, nur primitive IDs.
    public abstract record BotEvent(int EnqueueTick, bool IsCritical);

    // Map-Analyzer-Trigger. mapId via Map.uniqueID (int).
    public sealed record MapFinalizedEvent(int EnqueueTick, int MapUniqueId)
        : BotEvent(EnqueueTick, IsCritical: true);

    // Raid-Beginn. raidFactionLoadId = Faction.loadID-String, pointsBucket = grobe Bedrohungsklasse.
    public sealed record RaidEvent(int EnqueueTick, string RaidFactionLoadId, int PointsBucket)
        : BotEvent(EnqueueTick, IsCritical: true);

    // User hat einen Pawn manuell gedraftet — Advisory-Mode soll nicht re-drafting auslösen.
    public sealed record DraftedEvent(int EnqueueTick, string PawnUniqueLoadId)
        : BotEvent(EnqueueTick, IsCritical: false);

    // Quest-Dialog geöffnet (für Journey/Royal/Archonexus-Endings relevant).
    public sealed record QuestWindowEvent(int EnqueueTick, int QuestId)
        : BotEvent(EnqueueTick, IsCritical: false);

    // Pawn verlässt Home-Map (Caravan, Downed-Transport). Storyisch wichtig für Ending-Detection.
    public sealed record PawnExitMapEvent(int EnqueueTick, string PawnUniqueLoadId, int FromMapUniqueId)
        : BotEvent(EnqueueTick, IsCritical: false);
}
