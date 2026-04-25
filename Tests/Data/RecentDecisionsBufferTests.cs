using RimWorldBot.Data;
using Xunit;

namespace RimWorldBot.Tests.Data
{
    // Story 1.14 — RecentDecisionsBuffer ist pure-logic (Verse-Dep nur in ExposeData,
    // die wir nicht testen koennen ohne Scribe-Init). Add/Pin/Trim ist test-bar nach D-38.
    public class RecentDecisionsBufferTests
    {
        [Fact]
        public void Add_NormalKind_OnlyTransient()
        {
            var buffer = new RecentDecisionsBuffer(transientCap: 100, pinnedCap: 25);
            buffer.Add(new DecisionLogEntry("state_change", "Off → Advisory"));
            Assert.Single(buffer.Transient);
            Assert.Empty(buffer.Pinned);
        }

        [Fact]
        public void Add_AutoPinnedKind_GoesToBoth()
        {
            var buffer = new RecentDecisionsBuffer();
            buffer.Add(new DecisionLogEntry("PhaseTransition", "Phase 1 → 2"));
            Assert.Single(buffer.Transient);
            Assert.Single(buffer.Pinned);
            Assert.True(buffer.Pinned[0].Pinned);
        }

        [Fact]
        public void Add_CrashRecoveryPrefix_AutoPinned()
        {
            var buffer = new RecentDecisionsBuffer();
            buffer.Add(new DecisionLogEntry("crash-recovery-phase-rollback", "pending != current"));
            Assert.Single(buffer.Pinned);
        }

        [Fact]
        public void Add_NullEntry_Ignored()
        {
            var buffer = new RecentDecisionsBuffer();
            buffer.Add(null);
            Assert.Empty(buffer.Transient);
        }

        [Fact]
        public void TransientCap_FifoDrop()
        {
            var buffer = new RecentDecisionsBuffer(transientCap: 3, pinnedCap: 25);
            buffer.Add(new DecisionLogEntry("k1", "first"));
            buffer.Add(new DecisionLogEntry("k2", "second"));
            buffer.Add(new DecisionLogEntry("k3", "third"));
            buffer.Add(new DecisionLogEntry("k4", "fourth"));   // overflow → drop "first"

            Assert.Equal(3, buffer.Transient.Count);
            Assert.Equal("k2", buffer.Transient[0].Kind);
            Assert.Equal("k4", buffer.Transient[2].Kind);
        }

        [Fact]
        public void PinnedCap_FifoDrop()
        {
            var buffer = new RecentDecisionsBuffer(transientCap: 100, pinnedCap: 2);
            buffer.Add(new DecisionLogEntry("PhaseTransition", "p1"));
            buffer.Add(new DecisionLogEntry("EndingSwitch", "e1"));
            buffer.Add(new DecisionLogEntry("EndingForcedOverride", "o1"));   // overflow

            Assert.Equal(2, buffer.Pinned.Count);
            Assert.Equal("EndingSwitch", buffer.Pinned[0].Kind);
        }

        [Fact]
        public void EmptyKindIgnoredForAutoPin()
        {
            var buffer = new RecentDecisionsBuffer();
            buffer.Add(new DecisionLogEntry("", "no kind"));
            Assert.Single(buffer.Transient);
            Assert.Empty(buffer.Pinned);
        }
    }
}
