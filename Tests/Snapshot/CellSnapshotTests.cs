using RimWorldBot.Snapshot;
using Xunit;

namespace RimWorldBot.Tests.Snapshot
{
    // Story 2.1 — record-semantics + Value-Equality fuer CellSnapshot.
    // Records garantieren strukturelle Gleichheit; Tests verifizieren dass das nicht versehentlich
    // verloren geht (z.B. wenn jemand auf `class` umstellt oder Equals/GetHashCode manuell ueberschreibt).
    public class CellSnapshotTests
    {
        static CellSnapshot MakeDefault((int X, int Z) pos = default, string terrain = "Soil")
            => new(
                Position: pos == default ? (5, 7) : pos,
                TerrainDefName: terrain,
                Fertility: 1.0f,
                HasWater: false,
                HazardKind: HazardKind.None,
                HasRoof: false,
                IsMountain: false,
                HasResources: false,
                ChokepointScore: 0f);
                // WildPlant default null via optionalem Konstruktor-Parameter (Story 2.2).

        [Fact]
        public void EqualValues_ProduceEqualRecords()
        {
            var a = MakeDefault();
            var b = MakeDefault();
            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void DifferentPosition_NotEqual()
        {
            var a = MakeDefault((1, 2));
            var b = MakeDefault((3, 4));
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void DifferentTerrain_NotEqual()
        {
            var a = MakeDefault(terrain: "Soil");
            var b = MakeDefault(terrain: "Sand");
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void HazardKindAffectsEquality()
        {
            var none = MakeDefault();
            var lava = none with { HazardKind = HazardKind.Lava };
            Assert.NotEqual(none, lava);
            Assert.Equal(HazardKind.Lava, lava.HazardKind);
            // with-expression behaelt andere Felder identisch.
            Assert.Equal(none.Position, lava.Position);
        }

        [Fact]
        public void RecordIsImmutable_WithExpressionProducesNewInstance()
        {
            var original = MakeDefault();
            var modified = original with { ChokepointScore = 0.7f };
            Assert.Equal(0f, original.ChokepointScore);   // Original unveraendert
            Assert.Equal(0.7f, modified.ChokepointScore);
            Assert.NotSame(original, modified);
        }

        [Fact]
        public void TupleCompareInPosition_StructurallyEqual()
        {
            var a = MakeDefault((10, 20));
            var b = MakeDefault((10, 20));
            Assert.Equal(a.Position, b.Position);
            Assert.Equal(10, a.Position.X);
            Assert.Equal(20, a.Position.Z);
        }

        // Story 2.2: WildPlant ist optional (default null) — Konstruktor-Compat preserved.
        [Fact]
        public void WildPlant_DefaultNull_ViaOptionalParam()
        {
            var snap = MakeDefault();
            Assert.Null(snap.WildPlant);
        }

        [Fact]
        public void WildPlant_WithExpression_PreservesOtherFields()
        {
            var none = MakeDefault();
            var withBerries = none with { WildPlant = WildPlantKind.Berries };
            Assert.Equal(WildPlantKind.Berries, withBerries.WildPlant);
            Assert.Null(none.WildPlant);
            Assert.Equal(none.Position, withBerries.Position);
            Assert.Equal(none.TerrainDefName, withBerries.TerrainDefName);
        }
    }
}
