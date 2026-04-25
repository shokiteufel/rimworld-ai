using Xunit;

namespace RimWorldBot.Tests.Meta
{
    // Story 1.13 AC-6 — Meta-Test: beweist dass die xUnit-Infrastruktur tatsaechlich laeuft
    // (Test-Discovery + Assertions). Dient als Smoke-Test fuer kuenftige Test-Module:
    // wenn dieser Test bricht, ist die Test-Pipeline kaputt, nicht der Production-Code.
    public class TestFrameworkMetaTest
    {
        [Fact]
        public void Xunit_Discovery_Works()
        {
            Assert.True(true);
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(2, 3, 5)]
        [InlineData(0, 0, 0)]
        public void Theory_Discovery_Works(int a, int b, int sum)
        {
            Assert.Equal(sum, a + b);
        }

        [Fact]
        public void ImmutableCollections_Available_OnNet472()
        {
            // Story 1.11 bundlet System.Collections.Immutable.dll — Tests sollen sie nutzen koennen.
            var list = System.Collections.Immutable.ImmutableList.Create(1, 2, 3);
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void ImmutableCollections_RoundtripThroughProductionRecord_Works()
        {
            // CR Story 1.13 LOW-1-Fix: nicht nur ImmutableList allein, sondern via Production-Record
            // (BlueprintIntent in BuildPlan). Verifiziert dass die *gebundelte* Immutable-DLL aus
            // Story 1.11 (~545KB in Assemblies/) zur Test-Assembly-Resolve passt — wenn das mit
            // Version-Mismatch crasht, ist der bundle-flow im Production-csproj kaputt.
            var intent = new RimWorldBot.Decision.BlueprintIntent("Wall", X: 1, Z: 2, Rotation: 0);
            var plan = new RimWorldBot.Decision.BuildPlan(
                System.Collections.Immutable.ImmutableList.Create(intent));
            Assert.Single(plan.Intents);
            Assert.Equal("Wall", plan.Intents[0].DefName);
        }
    }
}
