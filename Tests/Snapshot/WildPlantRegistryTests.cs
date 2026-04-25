using RimWorldBot.Snapshot;
using Xunit;

namespace RimWorldBot.Tests.Snapshot
{
    // Story 2.2 — WildPlantRegistry-Lookup-Verifikation.
    // Tests gegen die D-42-verifizierten Vanilla-defNames. Spec-Lock-Charakter:
    // wenn jemand einen defName aus der Map entfernt oder das Mapping aendert,
    // bricht der entsprechende Test → Decision-Log-Pruefung pflicht.
    public class WildPlantRegistryTests
    {
        // ----- Berries (2 Varianten — Leafless + non-Leafless) -----

        [Theory]
        [InlineData("Plant_Berry")]
        [InlineData("Plant_Berry_Leafless")]
        public void Berries_Variants_ClassifyAsBerries(string defName)
        {
            Assert.Equal(WildPlantKind.Berries, WildPlantRegistry.TryClassify(defName));
        }

        // ----- Healroot (kultiviert + wild) -----

        [Theory]
        [InlineData("Plant_Healroot")]
        [InlineData("Plant_HealrootWild")]
        public void Healroot_Variants_ClassifyAsHealroot(string defName)
        {
            Assert.Equal(WildPlantKind.Healroot, WildPlantRegistry.TryClassify(defName));
        }

        // ----- Single-defName-Plants -----

        [Fact]
        public void Agave_ClassifyAsAgave()
        {
            Assert.Equal(WildPlantKind.Agave, WildPlantRegistry.TryClassify("Plant_Agave"));
        }

        [Fact]
        public void Ambrosia_ClassifyAsAmbrosiaBush_NoBiotechGuard()
        {
            // D-42: Plant_Ambrosia ist Core, kein DLC-Guard noetig.
            Assert.Equal(WildPlantKind.AmbrosiaBush, WildPlantRegistry.TryClassify("Plant_Ambrosia"));
        }

        // ----- Drug-Plants (kultiviert + wild) -----

        [Theory]
        [InlineData("Plant_Psychoid")]
        [InlineData("Plant_Psychoid_Wild")]
        public void Psychoid_Variants_ClassifyAsPsychoidPlant(string defName)
        {
            Assert.Equal(WildPlantKind.PsychoidPlant, WildPlantRegistry.TryClassify(defName));
        }

        [Theory]
        [InlineData("Plant_Smokeleaf")]
        [InlineData("Plant_Smokeleaf_Wild")]
        public void Smokeleaf_Variants_ClassifyAsSmokeleaf(string defName)
        {
            Assert.Equal(WildPlantKind.Smokeleaf, WildPlantRegistry.TryClassify(defName));
        }

        // ----- Unknown / Edge-Cases -----

        [Fact]
        public void Unknown_DefName_ReturnsNull()
        {
            Assert.Null(WildPlantRegistry.TryClassify("Plant_Tree"));   // Vanilla-Tree, nicht in Whitelist
            Assert.Null(WildPlantRegistry.TryClassify("SomeMod_FakeDef"));
        }

        [Fact]
        public void NullDefName_ReturnsNull()
        {
            Assert.Null(WildPlantRegistry.TryClassify(null));
        }

        [Fact]
        public void EmptyDefName_ReturnsNull()
        {
            Assert.Null(WildPlantRegistry.TryClassify(""));
        }

        [Fact]
        public void GlowyMushroom_NotInRegistry_D42()
        {
            // D-42 Spec-Lock: GlowyMushroom existiert nicht in Vanilla, deshalb nicht in der Map.
            Assert.Null(WildPlantRegistry.TryClassify("Plant_GlowyMushroom"));
        }

        // ----- Map-Sanity -----

        [Fact]
        public void Map_Has_10_VerifiedDefNames()
        {
            // Spec-Lock: 10 verifizierte Core+Odyssey-defNames.
            // Wenn jemand etwas hinzufuegt ohne D-42-Update, bricht der Test → Decision-Log-
            // Aktualisierung pflicht.
            Assert.Equal(10, WildPlantRegistry.VerifiedDefNameCount);
        }

        [Fact]
        public void Map_ContainsExactlyExpectedDefNames()
        {
            // CR Story 2.2 HIGH-2 Spec-Lock-Robustheit: HashSet-Vergleich faengt Tippfehler ab
            // (z.B. HealrootWild → Healroot_Wild). Count-only-Test war zu schwach.
            var expected = new System.Collections.Generic.HashSet<string>
            {
                "Plant_Berry",
                "Plant_Berry_Leafless",
                "Plant_Healroot",
                "Plant_HealrootWild",
                "Plant_Agave",
                "Plant_Ambrosia",
                "Plant_Psychoid",
                "Plant_Psychoid_Wild",
                "Plant_Smokeleaf",
                "Plant_Smokeleaf_Wild",
            };
            var actual = new System.Collections.Generic.HashSet<string>(WildPlantRegistry.Map.Keys);
            Assert.Equal(expected, actual);
        }
    }
}
