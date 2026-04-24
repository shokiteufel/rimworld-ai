using System.Collections.Generic;
using Verse;

namespace RimWorldBot.Events
{
    // D-18 + D-19: Zwei-Klassen-Queue (Critical never-drop bis criticalCap, Normal drop-oldest bei normalCap-Overflow).
    // Stale-Check beim Dequeue, nicht Enqueue (D-19): Events älter als StalenessThresholdTicks werden verworfen.
    // TRANSIENT (D-24): Konstruktor-initialisiert, Clear() bei LoadedGame/StartedNewGame. Nicht in ExposeData.
    public sealed class BoundedEventQueue<TEvent> where TEvent : BotEvent
    {
        public const int StalenessThresholdTicks = 2500; // ≈ 40s bei 60 TPS; Events älter als das sind stale.

        readonly int criticalCap;
        readonly int normalCap;
        readonly Queue<TEvent> critical;
        readonly Queue<TEvent> normal;

        public BoundedEventQueue(int criticalCap, int normalCap)
        {
            this.criticalCap = criticalCap;
            this.normalCap = normalCap;
            critical = new Queue<TEvent>(criticalCap);
            normal = new Queue<TEvent>(normalCap);
        }

        public int CriticalCount => critical.Count;
        public int NormalCount => normal.Count;

        public void Enqueue(TEvent ev)
        {
            if (ev == null) return;
            if (ev.IsCritical)
            {
                if (critical.Count >= criticalCap)
                {
                    // Critical-Queue-Overflow ist ein Bug-Signal — loggen, aber droppen wir trotzdem den ältesten
                    // damit der Stream nicht permanent geblockt wird (F-STAB-18).
                    var dropped = critical.Dequeue();
                    Log.Warning($"[RimWorldBot] Critical-EventQueue overflow (cap={criticalCap}), dropped: {dropped}");
                }
                critical.Enqueue(ev);
            }
            else
            {
                if (normal.Count >= normalCap) normal.Dequeue(); // silent drop-oldest für Normal
                normal.Enqueue(ev);
            }
        }

        // Dequeue prüft Staleness; stale Events werden verworfen und der nächste genommen.
        // Critical-Events werden zuerst dequeued.
        public bool TryDequeue(int currentTick, out TEvent ev)
        {
            while (critical.Count > 0)
            {
                var candidate = critical.Dequeue();
                if (currentTick - candidate.EnqueueTick <= StalenessThresholdTicks)
                {
                    ev = candidate;
                    return true;
                }
                // stale — drop + try next
            }
            while (normal.Count > 0)
            {
                var candidate = normal.Dequeue();
                if (currentTick - candidate.EnqueueTick <= StalenessThresholdTicks)
                {
                    ev = candidate;
                    return true;
                }
            }
            ev = null;
            return false;
        }

        public void Clear()
        {
            critical.Clear();
            normal.Clear();
        }
    }
}
