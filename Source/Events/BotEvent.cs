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

    // Quest-Offer detektiert via QuestManager-Polling (Story 1.12, CC-STORIES-09).
    // Critical weil Quest-Offer-Akzeptanz-Zeitfenster limited (Vanilla-Quest läuft i.d.R. 60 Tage ab).
    // questDefName = Quest.root.defName für Identifier-only-Pattern (D-23) — kein Quest-Runtime-Ref.
    public sealed record QuestOfferEvent(int EnqueueTick, int QuestId, string QuestDefName)
        : BotEvent(EnqueueTick, IsCritical: true);

    // Quest verschwunden aus QuestManager (expired/failed/completed). Consumers cleanen ihre State auf.
    public sealed record QuestRemovedEvent(int EnqueueTick, int QuestId)
        : BotEvent(EnqueueTick, IsCritical: false);

    // Legacy-Event für Dialog_NodeTree-basierte Quest-Windows (z.B. Ship-Start-Finale).
    // Bleibt erhalten für H6-Hook (Story 7.x), nicht mehr Primary-Pfad für moderne Quests.
    public sealed record QuestWindowEvent(int EnqueueTick, int QuestId)
        : BotEvent(EnqueueTick, IsCritical: false);

    // Pawn verlässt Home-Map (Caravan, Downed-Transport). Storyisch wichtig für Ending-Detection.
    public sealed record PawnExitMapEvent(int EnqueueTick, string PawnUniqueLoadId, int FromMapUniqueId)
        : BotEvent(EnqueueTick, IsCritical: false);
}
