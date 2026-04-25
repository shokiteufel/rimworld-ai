using System.Collections.Immutable;
using RimWorldBot.Snapshot;
using Xunit;

namespace RimWorldBot.Tests.Snapshot
{
    // Story 2.1 — Empty-Defaults fuer Forward-Declaration-Snapshots (ColonySnapshot/PawnSnapshot).
    // Stubs werden in Folge-Stories gefuellt; hier nur Verifikation dass null-safe-Defaults greifen.
    public class SnapshotEmptyDefaultsTests
    {
        [Fact]
        public void ColonySnapshot_Empty_HasZeroFields()
        {
            var snap = ColonySnapshot.Empty;
            Assert.Equal(0.0, snap.FoodDays);
            Assert.Equal(0, snap.ColonistCount);
            Assert.Equal(0.0, snap.MoodAverage);
            Assert.Empty(snap.FactionGoodwill);
            Assert.Equal(0, snap.TechLevel);
        }

        [Fact]
        public void ColonySnapshot_FactionGoodwillIsImmutable()
        {
            var snap = ColonySnapshot.Empty;
            Assert.IsAssignableFrom<ImmutableDictionary<string, int>>(snap.FactionGoodwill);
        }

        [Fact]
        public void PawnSnapshot_Empty_PreservesLoadId()
        {
            var snap = PawnSnapshot.Empty("Pawn_42");
            Assert.Equal("Pawn_42", snap.PawnLoadId);
            Assert.Empty(snap.Skills);
            Assert.Empty(snap.Passions);
            Assert.Equal(HealthSummary.Healthy, snap.Health);
            Assert.Equal(0.5, snap.MoodCurrent);
        }

        [Fact]
        public void PawnSnapshot_RecordEqualityViaLoadId()
        {
            var a = PawnSnapshot.Empty("Pawn_1");
            var b = PawnSnapshot.Empty("Pawn_1");
            // Records sind value-equal: gleicher LoadId + gleiche Default-Werte → equal.
            Assert.Equal(a, b);

            var c = PawnSnapshot.Empty("Pawn_2");
            Assert.NotEqual(a, c);
        }

        [Fact]
        public void HazardKind_AllValuesDefined()
        {
            // Spec-Lock: AC-2 fordert genau diese 5 Werte. Wenn jemand Toxic entfernt o.ae.
            // bricht der Test → Decision-Log-Check pflicht.
            Assert.Equal(0, (int)HazardKind.None);
            Assert.True(System.Enum.IsDefined(typeof(HazardKind), HazardKind.Lava));
            Assert.True(System.Enum.IsDefined(typeof(HazardKind), HazardKind.Pollution));
            Assert.True(System.Enum.IsDefined(typeof(HazardKind), HazardKind.Toxic));
            Assert.True(System.Enum.IsDefined(typeof(HazardKind), HazardKind.Radiation));
            Assert.Equal(5, System.Enum.GetValues(typeof(HazardKind)).Length);
        }
    }
}
