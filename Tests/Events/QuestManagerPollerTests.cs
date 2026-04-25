using System;
using System.Collections.Generic;
using System.Linq;
using RimWorldBot.Events;
using Xunit;

namespace RimWorldBot.Tests.Events
{
    // Story 1.14 Carry-Over aus Story 1.12 — QuestManagerPoller Diff-Detection.
    // Nutzt Test-Seams QuestSource + TickProvider (D-38) damit kein echter Find.QuestManager
    // im xUnit-Runner gebraucht wird.
    // [Collection("StaticStateMutators")] sequentiell mit BotSafeTests — beide manipulieren
    // BotSafe.ErrorLogger/WarningLogger via shared static state.
    [Collection("StaticStateMutators")]
    public class QuestManagerPollerTests : IDisposable
    {
        // Auch BotSafe-Logger mocken weil Poll() ueber BotSafe.SafeTick laeuft (Source ruft
        // bei jedem Throw Verse.Log → UnityEngine.Debug → crash).
        public QuestManagerPollerTests()
        {
            RimWorldBot.Core.BotSafe.ErrorLogger = _ => { };
            RimWorldBot.Core.BotSafe.WarningLogger = _ => { };
            RimWorldBot.Core.BotSafe.Clear();
        }

        public void Dispose()
        {
            // CR Story 1.14 HIGH-1-Fix: BotSafe-State auch reset. Race-Condition-Argument
            // war zirkulaer — Collection("StaticStateMutators") DisableParallelization=true
            // verhindert genau das. Stale-Mock-Logger wuerden sonst andere Test-Klassen verseuchen.
            QuestManagerPoller.ResetForTesting();
            RimWorldBot.Core.BotSafe.ResetNowProviderForTesting();
        }

        BoundedEventQueue<BotEvent> NewQueue() => new(criticalCap: 32, normalCap: 224);

        void SetQuests(params (int id, string defName)[] quests)
        {
            QuestManagerPoller.QuestSource = () => quests;
        }

        void SetTick(int tick)
        {
            QuestManagerPoller.TickProvider = () => tick;
        }

        // ----- New-Quest-Detection -----

        [Fact]
        public void Poll_NewQuest_EnqueuesQuestOfferEvent()
        {
            var seen = new HashSet<int>();
            var queue = NewQueue();
            SetTick(1000);
            SetQuests((42, "OpportunitySite_ItemStash"));

            QuestManagerPoller.Poll(seen, queue);

            Assert.Equal(1, queue.CriticalCount);
            queue.TryDequeue(currentTick: 1000, out var ev);
            var offer = Assert.IsType<QuestOfferEvent>(ev);
            Assert.Equal(42, offer.QuestId);
            Assert.Equal("OpportunitySite_ItemStash", offer.QuestDefName);
            Assert.Equal(1000, offer.EnqueueTick);
            Assert.Contains(42, seen);
        }

        [Fact]
        public void Poll_MultipleNewQuests_AllEnqueued()
        {
            var seen = new HashSet<int>();
            var queue = NewQueue();
            SetQuests((1, "A"), (2, "B"), (3, "C"));

            QuestManagerPoller.Poll(seen, queue);

            Assert.Equal(3, queue.CriticalCount);
            Assert.Equal(new[] { 1, 2, 3 }, seen.OrderBy(i => i).ToArray());
        }

        // ----- Re-Poll without diff -----

        [Fact]
        public void Poll_RePollNoChanges_NoNewEvents()
        {
            var seen = new HashSet<int> { 42 };
            var queue = NewQueue();
            SetQuests((42, "A"));

            QuestManagerPoller.Poll(seen, queue);

            Assert.Equal(0, queue.CriticalCount);
            Assert.Equal(0, queue.NormalCount);
        }

        // ----- Removed-Quest-Detection -----

        [Fact]
        public void Poll_RemovedQuest_EnqueuesQuestRemovedEvent()
        {
            var seen = new HashSet<int> { 42 };
            var queue = NewQueue();
            SetTick(2000);
            SetQuests();   // leer — Quest 42 ist weg

            QuestManagerPoller.Poll(seen, queue);

            Assert.Equal(0, queue.CriticalCount);
            Assert.Equal(1, queue.NormalCount);
            queue.TryDequeue(currentTick: 2000, out var ev);
            var removed = Assert.IsType<QuestRemovedEvent>(ev);
            Assert.Equal(42, removed.QuestId);
            Assert.Equal(2000, removed.EnqueueTick);
            Assert.DoesNotContain(42, seen);
        }

        [Fact]
        public void Poll_PartialChange_NewAndRemoved()
        {
            // 42 weg, 99 neu
            var seen = new HashSet<int> { 42 };
            var queue = NewQueue();
            SetQuests((99, "Brand New"));

            QuestManagerPoller.Poll(seen, queue);

            Assert.Equal(1, queue.CriticalCount);   // 99 als Offer
            Assert.Equal(1, queue.NormalCount);     // 42 als Removed
            Assert.Equal(new[] { 99 }, seen.ToArray());
        }

        // ----- Defensive Edge-Cases -----

        [Fact]
        public void Poll_NullLastSeen_NoOp()
        {
            var queue = NewQueue();
            SetQuests((1, "A"));

            QuestManagerPoller.Poll(lastSeenQuestIds: null, eventQueue: queue);

            Assert.Equal(0, queue.CriticalCount);
        }

        [Fact]
        public void Poll_NullEventQueue_NoOp()
        {
            var seen = new HashSet<int>();
            SetQuests((1, "A"));

            QuestManagerPoller.Poll(seen, eventQueue: null);

            Assert.Empty(seen);   // State auch nicht aktualisiert weil Poll early-returned
        }

        [Fact]
        public void Poll_EmptyQuestSource_NoEvents()
        {
            var seen = new HashSet<int>();
            var queue = NewQueue();
            SetQuests();

            QuestManagerPoller.Poll(seen, queue);

            Assert.Equal(0, queue.CriticalCount);
            Assert.Equal(0, queue.NormalCount);
        }

        [Fact]
        public void Poll_NullDefName_UsesUnknownSentinel()
        {
            var seen = new HashSet<int>();
            var queue = NewQueue();
            SetQuests((42, null));

            QuestManagerPoller.Poll(seen, queue);

            queue.TryDequeue(currentTick: 0, out var ev);
            var offer = Assert.IsType<QuestOfferEvent>(ev);
            Assert.Equal(QuestManagerPoller.UnknownQuestDef, offer.QuestDefName);
        }
    }
}
