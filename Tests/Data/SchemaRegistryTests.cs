using System.Linq;
using RimWorldBot.Data;
using Xunit;

namespace RimWorldBot.Tests.Data
{
    // Story 1.13 Carry-Over aus Story 1.9 — Tests fuer SchemaRegistry-Struktur.
    // Kein Test der eigentlichen BotGameComponent.Migrate() — die ruft Scribe-API
    // (Verse-Runtime-only). Migrate-Logic-Korrektheit wird via Game-Test verifiziert
    // (Save-Roundtrip-Manual nach jeder Schema-Bump-Story).
    public class SchemaRegistryTests
    {
        // CR Story 1.13 HIGH-2-Fix: Drift-Detection-Test — wenn jemand
        // CurrentSchemaVersion in einer Component bumpt aber den Eintrag in
        // SchemaRegistry.Bumps vergisst (oder umgekehrt), wird genau dieser Test rot.
        // Der vorige Test (LatestAppliedVersion_BotGame_MatchesAppliedBumps) hatte den
        // Wert hardcoded und konnte deshalb keine Drift erkennen — das ist gefixt durch
        // Direktvergleich mit dem internal const aus der Component (InternalsVisibleTo).
        [Fact]
        public void LatestAppliedVersion_BotGame_MatchesComponentConst()
        {
            Assert.Equal(BotGameComponent.CurrentSchemaVersion,
                         SchemaRegistry.LatestAppliedVersion(SchemaRegistry.ComponentBotGame));
        }

        [Fact]
        public void LatestAppliedVersion_BotMap_MatchesComponentConst()
        {
            Assert.Equal(BotMapComponent.CurrentSchemaVersion,
                         SchemaRegistry.LatestAppliedVersion(SchemaRegistry.ComponentBotMap));
        }

        [Fact]
        public void LatestAppliedVersion_UnknownComponent_ReturnsBaselineOne()
        {
            int latest = SchemaRegistry.LatestAppliedVersion("NonexistentComponent");
            Assert.Equal(1, latest);
        }

        // CR Story 1.13 HIGH-3-Fix: Edge-Cases fuer null/empty Component-Strings.
        // Source iteriert mit string-Equality; null/empty matcht nichts → returnt baseline 1.
        // Test schuetzt vor kuenftigen Refactors die auf .Equals() umstellen koennten (NRE-Risiko).
        [Fact]
        public void LatestAppliedVersion_NullComponent_ReturnsBaselineOne()
        {
            Assert.Equal(1, SchemaRegistry.LatestAppliedVersion(null));
        }

        [Fact]
        public void LatestAppliedVersion_EmptyString_ReturnsBaselineOne()
        {
            Assert.Equal(1, SchemaRegistry.LatestAppliedVersion(""));
        }

        [Fact]
        public void Bumps_ChainIsContiguous_PerComponent()
        {
            // Fuer jede Komponente: Versionssprung-Folge muss luecken-frei sein.
            // (1->2->3->4 ok; 1->2->4 waere Migrations-Loch).
            foreach (var component in new[] { SchemaRegistry.ComponentBotGame, SchemaRegistry.ComponentBotMap })
            {
                var bumps = SchemaRegistry.Bumps
                    .Where(b => b.Component == component)
                    .OrderBy(b => b.FromVersion)
                    .ToList();
                for (int i = 0; i < bumps.Count; i++)
                {
                    Assert.Equal(i + 1, bumps[i].FromVersion);
                    Assert.Equal(i + 2, bumps[i].ToVersion);
                }
            }
        }

        [Fact]
        public void Bumps_NoVersionGapBetweenAppliedAndPlanned()
        {
            // Applied-Bumps muessen die ersten N sein; danach kommen Planned. Niemals Planned->Applied.
            foreach (var component in new[] { SchemaRegistry.ComponentBotGame, SchemaRegistry.ComponentBotMap })
            {
                var bumps = SchemaRegistry.Bumps
                    .Where(b => b.Component == component)
                    .OrderBy(b => b.FromVersion)
                    .ToList();
                bool seenPlanned = false;
                foreach (var bump in bumps)
                {
                    if (bump.Status == SchemaRegistry.BumpStatus.Planned)
                    {
                        seenPlanned = true;
                    }
                    else if (seenPlanned)
                    {
                        Assert.Fail($"Component {component}: Applied-Bump v{bump.FromVersion}->v{bump.ToVersion} kommt nach Planned-Bump (Planned darf nur am Ende stehen).");
                    }
                }
            }
        }

        // Spec-Lock-Tests (CR Story 1.13 MED-4): wenn diese Tests rot werden, ist die
        // SchemaRegistry-Tabelle gegen ein dokumentiertes Decision (D-36) gedriftet.
        // Vor Fix-Versuch: Decision-Log pruefen ob die Verschiebung beabsichtigt war.
        [Fact]
        public void Story_1_12_Bump_Is_Applied()
        {
            // Spec-Lock fuer D-36: Story 1.12 belegt den v3->v4-Slot fuer lastSeenQuestIds.
            var bump = SchemaRegistry.Bumps
                .FirstOrDefault(b => b.Component == SchemaRegistry.ComponentBotGame && b.FromVersion == 3);
            Assert.NotNull(bump);
            Assert.Equal(4, bump.ToVersion);
            Assert.Equal("1.12", bump.StoryId);
            Assert.Equal(SchemaRegistry.BumpStatus.Applied, bump.Status);
            Assert.Contains("lastSeenQuestIds", bump.FieldChanges);
        }

        [Fact]
        public void Story_4_3_Bump_Shifted_To_v4_v5()
        {
            // Spec-Lock fuer D-36: botManagedGuests (Story 4.3) wurde von v3->v4 auf v4->v5
            // verschoben um Slot fuer Story 1.12 freizumachen.
            var bump = SchemaRegistry.Bumps
                .FirstOrDefault(b => b.StoryId == "4.3" && b.Component == SchemaRegistry.ComponentBotGame);
            Assert.NotNull(bump);
            Assert.Equal(4, bump.FromVersion);
            Assert.Equal(5, bump.ToVersion);
            Assert.Equal(SchemaRegistry.BumpStatus.Planned, bump.Status);
        }
    }
}
