using System.Collections.Generic;
using RimWorld;
using RimWorldBot.Core;
using Verse;

namespace RimWorldBot.Events
{
    // Story 1.12 (CC-STORIES-09) — pollt Find.QuestManager.QuestsListForReading alle 1250 Ticks
    // (~21s bei 60 TPS) und detektiert neue/entfernte Quests. Neue → QuestOfferEvent (Critical),
    // entfernte → QuestRemovedEvent (Normal). Consumers in 7.7 (AIPersonaCore-Quest), 7.9
    // (JourneyQuest), 7.11, 7.14, 7.15.
    //
    // Modern-Quest-API-Pfad: ersetzt H6 (WindowStack.Add → Dialog_NodeTree)-Pattern als
    // primären Quest-Detektor. H6 bleibt für Finale-Dialog-Events erhalten (Ship-Start-Incident).
    //
    // BotSafe.SafeTick-Wrapper: Vanilla-Quest-API-Calls können theoretisch werfen
    // (z.B. wenn QuestManager mid-Tick disposed wird, sehr unwahrscheinlich aber defensiv).
    //
    // INVARIANT (CR Story 1.12 HIGH-1): PollIntervalTicks < BoundedEventQueue.StalenessThresholdTicks (=2500).
    // Sonst kann ein Critical-QuestOfferEvent zwischen Enqueue und Dequeue stale werden falls der
    // Consumer (Tick-Host in 7.7/7.9) nicht in jedem Tick zieht. 1250 = exakt halbe Staleness-Window —
    // garantiert dass jedes Event mindestens einen vollen Poll-Zyklus an Lebenszeit hat bevor es verfällt.
    // Quest-Akzeptanz-Fenster Vanilla = 60 Ingame-Tage = 3.6M Ticks; Detection-Latenz ist hier nicht limitierend.
    public static class QuestManagerPoller
    {
        public const int PollIntervalTicks = 1250;

        // Fallback wenn quest.root null ist (sehr selten, defensive). Konstante damit Consumer
        // explizit gegen den Sentinel matchen können statt Magic-String zu duplizieren (CR LOW-1).
        public const string UnknownQuestDef = "<unknown>";

        // Story 1.14 Test-Seams (D-38): injectable Quest-Source + Tick-Source fuer Tests
        // im xUnit-Runner ohne Verse-Runtime. Quest ist Verse-Type → wir extrahieren die
        // gebrauchten Felder (id + defName) als (int, string)-Tuple damit der Test-Code
        // keine echten Quest-Objekte instantiieren muss.
        // Defaults wrappen Find.QuestManager + GenTicks.TicksGame. ResetForTesting reset beide.
        // `internal` damit InternalsVisibleTo("RimWorldBot.Tests") darauf zugreift.
        internal static System.Func<IEnumerable<(int Id, string DefName)>> QuestSource = DefaultQuestSource;
        internal static System.Func<int> TickProvider = () => GenTicks.TicksGame;

        static IEnumerable<(int Id, string DefName)> DefaultQuestSource()
        {
            var qm = Find.QuestManager;
            if (qm == null) yield break;
            var quests = qm.QuestsListForReading;
            if (quests == null) yield break;
            foreach (var quest in quests)
            {
                if (quest == null) continue;
                yield return (quest.id, quest.root?.defName ?? UnknownQuestDef);
            }
        }

        internal static void ResetForTesting()
        {
            QuestSource = DefaultQuestSource;
            TickProvider = () => GenTicks.TicksGame;
        }

        // Aufruf aus BotGameComponent.GameComponentTick — übergibt das persistente Set + Queue.
        // lastSeenQuestIds wird hier mutiert; Persistierung passiert in BotGameComponent.ExposeData
        // (Story 1.12 Schema-Bump auf v4 in SchemaRegistry markiert).
        public static void Poll(HashSet<int> lastSeenQuestIds, BoundedEventQueue<BotEvent> eventQueue)
        {
            BotSafe.SafeTick(() =>
            {
                if (lastSeenQuestIds == null || eventQueue == null) return;

                // CR LOW-2: Per-Poll-Allokation akzeptiert — Static-Class kann keinen Reuse-Buffer halten,
                // und ~60 ints alle 21s ist GC-irrelevant. Falls künftig Static→Instance refactored wird,
                // hier einen Field-Buffer einführen.
                var current = new HashSet<int>();
                int tick = TickProvider();
                foreach (var (id, defName) in QuestSource())
                {
                    current.Add(id);

                    // Neue Quest → QuestOfferEvent
                    if (!lastSeenQuestIds.Contains(id))
                    {
                        eventQueue.Enqueue(new QuestOfferEvent(
                            EnqueueTick: tick,
                            QuestId: id,
                            QuestDefName: defName ?? UnknownQuestDef));
                    }
                }

                // Entfernte Quests → QuestRemovedEvent
                var removed = new List<int>();
                foreach (var oldId in lastSeenQuestIds)
                {
                    if (!current.Contains(oldId)) removed.Add(oldId);
                }
                foreach (var id in removed)
                {
                    eventQueue.Enqueue(new QuestRemovedEvent(
                        EnqueueTick: tick,
                        QuestId: id));
                }

                // State commit (CR LOW-3: idiomatisches UnionWith statt manueller foreach)
                lastSeenQuestIds.Clear();
                lastSeenQuestIds.UnionWith(current);
            }, context: "QuestManagerPoller.Poll");
        }
    }
}
