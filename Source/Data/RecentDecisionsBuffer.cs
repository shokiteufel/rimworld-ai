using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorldBot.Data
{
    // Zwei-Tier-Retention-Buffer für DecisionLog (TQ-S3-01 resolved: IExposable statt zwei separate Scribe-Calls).
    // Auto-Pin-Regel: bestimmte Kinds werden zusätzlich in pinned-Queue geschrieben,
    // damit long-retention-wichtige Ereignisse nicht aus dem transient-FIFO rausfallen.
    public sealed class RecentDecisionsBuffer : IExposable
    {
        // Kinds die auto-pinned werden (§5 + D-17 Ending-Commitment-Releases + F-STAB-23 Crash-Recovery).
        static readonly HashSet<string> AutoPinKinds = new()
        {
            "PhaseTransition",
            "EndingSwitch",
            "EndingForcedOverride",
            "EndingAutoEscape",
            "EndingCommitmentReleased",
            // Jeder Kind mit Prefix "crash-recovery-" wird ebenfalls auto-pinned (via StartsWith-Check in Add()).
        };

        int transientCap;
        int pinnedCap;
        List<DecisionLogEntry> transientEntries;
        List<DecisionLogEntry> pinnedEntries;

        public RecentDecisionsBuffer() : this(transientCap: 100, pinnedCap: 25) { }

        public RecentDecisionsBuffer(int transientCap, int pinnedCap)
        {
            this.transientCap = transientCap;
            this.pinnedCap = pinnedCap;
            transientEntries = new List<DecisionLogEntry>(transientCap);
            pinnedEntries = new List<DecisionLogEntry>(pinnedCap);
        }

        public IReadOnlyList<DecisionLogEntry> Transient => transientEntries;
        public IReadOnlyList<DecisionLogEntry> Pinned => pinnedEntries;

        public void Add(DecisionLogEntry entry)
        {
            if (entry == null) return;
            AddTransient(entry);
            if (ShouldAutoPin(entry.Kind)) AddPinned(entry);
        }

        public void AddTransient(DecisionLogEntry entry)
        {
            if (entry == null) return;
            transientEntries.Add(entry);
            // FIFO-Cap: bei Overflow ältesten droppen.
            while (transientEntries.Count > transientCap) transientEntries.RemoveAt(0);
        }

        public void AddPinned(DecisionLogEntry entry)
        {
            if (entry == null) return;
            // Kopie mit Pinned=true damit Render-Code weiß dass Entry pinned ist.
            var pinned = new DecisionLogEntry(entry.Kind, entry.Reason, entry.Tick, pinned: true);
            pinnedEntries.Add(pinned);
            while (pinnedEntries.Count > pinnedCap) pinnedEntries.RemoveAt(0);
        }

        static bool ShouldAutoPin(string kind)
        {
            if (string.IsNullOrEmpty(kind)) return false;
            if (AutoPinKinds.Contains(kind)) return true;
            if (kind.StartsWith("crash-recovery-")) return true;
            return false;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref transientCap, "transientCap", 100);
            Scribe_Values.Look(ref pinnedCap, "pinnedCap", 25);
            Scribe_Collections.Look(ref transientEntries, "transientEntries", LookMode.Deep);
            Scribe_Collections.Look(ref pinnedEntries, "pinnedEntries", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                transientEntries ??= new List<DecisionLogEntry>(transientCap);
                pinnedEntries ??= new List<DecisionLogEntry>(pinnedCap);
                // Defensiv: Caps respektieren auch nach Load (falls Save manipuliert).
                while (transientEntries.Count > transientCap) transientEntries.RemoveAt(0);
                while (pinnedEntries.Count > pinnedCap) pinnedEntries.RemoveAt(0);
            }
        }
    }
}
