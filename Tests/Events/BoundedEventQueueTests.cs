using RimWorldBot.Events;
using Xunit;

namespace RimWorldBot.Tests.Events
{
    // Story 1.14 — wiederhergestellt aus Story 1.13 Wegwurf nach Production-csproj-Refactor (D-38).
    // Ehemals scheiterten alle 8 Tests mit TypeLoadException Queue<T> wegen Krafs-Mono-mscorlib
    // vs Microsoft net472 mscorlib. Nach Option-A-Refactor: Production-DLL nutzt Microsoft mscorlib
    // → Type-Identity matcht xUnit-Runtime → Tests laufen.
    public class BoundedEventQueueTests
    {
        // Test-Event-Subtyp: erlaubt Critical/Normal-Toggle pro Test ohne Cast-Gymnastik
        // ueber die abstract record-Hierarchie.
        sealed record TestEvent(int EnqueueTick, bool IsCritical) : BotEvent(EnqueueTick, IsCritical);

        [Fact]
        public void Enqueue_NormalEvent_FillsNormalQueue()
        {
            var queue = new BoundedEventQueue<TestEvent>(criticalCap: 4, normalCap: 4);
            queue.Enqueue(new TestEvent(EnqueueTick: 100, IsCritical: false));
            Assert.Equal(0, queue.CriticalCount);
            Assert.Equal(1, queue.NormalCount);
        }

        [Fact]
        public void Enqueue_CriticalEvent_FillsCriticalQueue()
        {
            var queue = new BoundedEventQueue<TestEvent>(criticalCap: 4, normalCap: 4);
            queue.Enqueue(new TestEvent(EnqueueTick: 100, IsCritical: true));
            Assert.Equal(1, queue.CriticalCount);
            Assert.Equal(0, queue.NormalCount);
        }

        [Fact]
        public void Enqueue_NullEvent_IsIgnored()
        {
            var queue = new BoundedEventQueue<TestEvent>(criticalCap: 4, normalCap: 4);
            queue.Enqueue(null);
            Assert.Equal(0, queue.CriticalCount);
            Assert.Equal(0, queue.NormalCount);
        }

        [Fact]
        public void TryDequeue_CriticalBeforeNormal()
        {
            var queue = new BoundedEventQueue<TestEvent>(criticalCap: 4, normalCap: 4);
            queue.Enqueue(new TestEvent(100, IsCritical: false));
            queue.Enqueue(new TestEvent(101, IsCritical: true));

            bool ok = queue.TryDequeue(currentTick: 200, out var ev);
            Assert.True(ok);
            Assert.True(ev.IsCritical);
        }

        [Fact]
        public void TryDequeue_StaleEvent_DroppedAndNextReturned()
        {
            var queue = new BoundedEventQueue<TestEvent>(criticalCap: 4, normalCap: 4);
            queue.Enqueue(new TestEvent(EnqueueTick: 0, IsCritical: false));    // wird stale bei tick 5000
            queue.Enqueue(new TestEvent(EnqueueTick: 4000, IsCritical: false)); // bleibt fresh

            bool ok = queue.TryDequeue(currentTick: 5000, out var ev);
            Assert.True(ok);
            Assert.Equal(4000, ev.EnqueueTick);
        }

        [Fact]
        public void TryDequeue_AllStale_ReturnsFalse()
        {
            var queue = new BoundedEventQueue<TestEvent>(criticalCap: 4, normalCap: 4);
            queue.Enqueue(new TestEvent(0, IsCritical: true));
            queue.Enqueue(new TestEvent(0, IsCritical: false));

            bool ok = queue.TryDequeue(currentTick: 10_000, out var ev);
            Assert.False(ok);
            Assert.Null(ev);
        }

        [Fact]
        public void Enqueue_NormalOverflow_SilentDropOldest()
        {
            var queue = new BoundedEventQueue<TestEvent>(criticalCap: 4, normalCap: 2);
            queue.Enqueue(new TestEvent(100, false));
            queue.Enqueue(new TestEvent(101, false));
            queue.Enqueue(new TestEvent(102, false));   // Overflow: dropped 100

            Assert.Equal(2, queue.NormalCount);
            queue.TryDequeue(currentTick: 200, out var first);
            Assert.Equal(101, first.EnqueueTick);
        }

        [Fact]
        public void Clear_EmptiesBothQueues()
        {
            var queue = new BoundedEventQueue<TestEvent>(criticalCap: 4, normalCap: 4);
            queue.Enqueue(new TestEvent(100, true));
            queue.Enqueue(new TestEvent(101, false));
            queue.Clear();
            Assert.Equal(0, queue.CriticalCount);
            Assert.Equal(0, queue.NormalCount);
        }
    }
}
