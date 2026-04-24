using Verse;

namespace RimWorldBot.Data
{
    // Zwei-Tier-Retention: transient (100 FIFO) + pinned (25 für wichtige Kinds).
    // Auto-Pin-Regel in RecentDecisionsBuffer.Add() für Kinds:
    // PhaseTransition, EndingSwitch, CrashRecovery*, EndingForcedOverride, EndingAutoEscape, EndingCommitmentReleased.
    //
    // `class` statt `record` weil IExposable + public settable fields für Scribe nötig sind.
    // Value-Equality via expliziten Equals/GetHashCode überschrieben.
    public sealed class DecisionLogEntry : IExposable
    {
        public string Kind;
        public string Reason;
        public int Tick;
        public bool Pinned;

        public DecisionLogEntry() { }

        public DecisionLogEntry(string kind, string reason, int tick = 0, bool pinned = false)
        {
            Kind = kind;
            Reason = reason;
            Tick = tick;
            Pinned = pinned;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Kind, "kind");
            Scribe_Values.Look(ref Reason, "reason");
            Scribe_Values.Look(ref Tick, "tick", 0);
            Scribe_Values.Look(ref Pinned, "pinned", false);
        }

        public override bool Equals(object obj) =>
            obj is DecisionLogEntry o && Kind == o.Kind && Reason == o.Reason && Tick == o.Tick && Pinned == o.Pinned;

        public override int GetHashCode()
        {
            // net472 hat kein System.HashCode.Combine — manual unchecked combine.
            unchecked
            {
                int h = 17;
                h = h * 31 + (Kind?.GetHashCode() ?? 0);
                h = h * 31 + (Reason?.GetHashCode() ?? 0);
                h = h * 31 + Tick;
                h = h * 31 + (Pinned ? 1 : 0);
                return h;
            }
        }
    }
}
