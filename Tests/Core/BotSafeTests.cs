using System;
using RimWorldBot.Core;
using Xunit;

namespace RimWorldBot.Tests.Core
{
    // Story 1.14 Carry-Over aus Story 1.10 — BotSafe Sliding-Window + Poison-Cooldown + ErrorBudget.
    // Nutzt BotSafe.NowProvider Test-Seam (D-38) fuer Mock-Clock statt Time.realtimeSinceStartup.
    // IDisposable.Dispose() reset NowProvider damit Tests nicht inter-dependent werden.
    // [Collection("StaticStateMutators")] verhindert Race-Conditions mit QuestManagerPollerTests
    // die ebenfalls BotSafe-Logger-Seams setzen.
    [Collection("StaticStateMutators")]
    public class BotSafeTests : IDisposable
    {
        // Mock-Clock — wir setzen den "aktuellen Zeitpunkt" pro Test explizit.
        float _now;

        public BotSafeTests()
        {
            _now = 0f;
            BotSafe.NowProvider = () => _now;
            // Verse.Log → UnityEngine.Debug crasht ohne Unity-Init. Mocks schlucken die Calls.
            BotSafe.ErrorLogger = _ => { };
            BotSafe.WarningLogger = _ => { };
            BotSafe.Clear();
        }

        public void Dispose()
        {
            BotSafe.ResetNowProviderForTesting();
        }

        // ----- SafeTick -----

        [Fact]
        public void SafeTick_NoException_RunsBody()
        {
            int counter = 0;
            BotSafe.SafeTick(() => counter++, context: "test-1");
            Assert.Equal(1, counter);
            Assert.Equal(0, BotSafe.GetExceptionCount("test-1"));
            Assert.False(BotSafe.IsPoisoned("test-1"));
        }

        [Fact]
        public void SafeTick_OneException_IncrementsBudget_NotYetPoisoned()
        {
            BotSafe.SafeTick(() => throw new InvalidOperationException("boom"), context: "test-2");
            Assert.Equal(1, BotSafe.GetExceptionCount("test-2"));
            Assert.False(BotSafe.IsPoisoned("test-2"));
        }

        [Fact]
        public void SafeTick_TwoExceptionsInWindow_PoisonsContext()
        {
            // Threshold = 2 Exceptions in 60s-Window → poisoned.
            BotSafe.SafeTick(() => throw new InvalidOperationException("boom1"), context: "test-3");
            _now = 30f;
            BotSafe.SafeTick(() => throw new InvalidOperationException("boom2"), context: "test-3");
            Assert.True(BotSafe.IsPoisoned("test-3"));
        }

        [Fact]
        public void SafeTick_PoisonedContext_SkipsBody()
        {
            // Setup: zwei Exceptions → poisoned.
            BotSafe.SafeTick(() => throw new Exception("e1"), context: "test-4");
            BotSafe.SafeTick(() => throw new Exception("e2"), context: "test-4");
            Assert.True(BotSafe.IsPoisoned("test-4"));

            // Act: Body sollte nicht laufen weil poisoned.
            int counter = 0;
            BotSafe.SafeTick(() => counter++, context: "test-4");
            Assert.Equal(0, counter);
        }

        [Fact]
        public void SafeTick_ExceptionsOutsideWindow_NotPoisoned()
        {
            // 1. Exception bei t=0
            BotSafe.SafeTick(() => throw new Exception("old"), context: "test-5");
            // 2. Exception bei t=70 (ausserhalb 60s-Window) → 1. wird gepruned
            _now = 70f;
            BotSafe.SafeTick(() => throw new Exception("new"), context: "test-5");
            Assert.False(BotSafe.IsPoisoned("test-5"));
            Assert.Equal(1, BotSafe.GetExceptionCount("test-5"));
        }

        [Fact]
        public void SafeTick_NullBody_LogOnceAndReturn()
        {
            BotSafe.SafeTick(null, context: "test-6");
            Assert.Equal(0, BotSafe.GetExceptionCount("test-6"));
        }

        [Fact]
        public void SafeTick_EmptyContext_LogOnceAndReturn()
        {
            int counter = 0;
            BotSafe.SafeTick(() => counter++, context: "");
            Assert.Equal(0, counter);
        }

        // ----- SafeApply -----

        [Fact]
        public void SafeApply_BodyReturnsTrue_PassesThrough()
        {
            bool result = BotSafe.SafeApply(plan => true, plan: "anyPlan", context: "apply-1");
            Assert.True(result);
        }

        [Fact]
        public void SafeApply_BodyReturnsFalse_PassesThrough()
        {
            bool result = BotSafe.SafeApply(plan => false, plan: 42, context: "apply-2");
            Assert.False(result);
        }

        [Fact]
        public void SafeApply_BodyThrows_ReturnsFalse_AndIncrementsBudget()
        {
            bool result = BotSafe.SafeApply<int>(plan => throw new Exception("boom"), plan: 0, context: "apply-3");
            Assert.False(result);
            Assert.Equal(1, BotSafe.GetExceptionCount("apply-3"));
        }

        [Fact]
        public void SafeApply_PoisonedContext_ReturnsFalse_WithoutCallingBody()
        {
            BotSafe.SafeApply<int>(_ => throw new Exception("e1"), 0, "apply-4");
            BotSafe.SafeApply<int>(_ => throw new Exception("e2"), 0, "apply-4");
            Assert.True(BotSafe.IsPoisoned("apply-4"));

            int callCount = 0;
            bool result = BotSafe.SafeApply<int>(p => { callCount++; return true; }, 0, "apply-4");
            Assert.False(result);
            Assert.Equal(0, callCount);
        }

        // ----- Poison-Cooldown-Ablauf -----

        [Fact]
        public void IsPoisoned_AfterCooldown_AutoExpires()
        {
            // Setup: poisoned bei t=0 → unlockTime = 600
            BotSafe.SafeTick(() => throw new Exception("e1"), context: "cool-1");
            BotSafe.SafeTick(() => throw new Exception("e2"), context: "cool-1");
            Assert.True(BotSafe.IsPoisoned("cool-1"));

            // Knapp vor Cooldown-Ende → noch poisoned
            _now = 599.9f;
            Assert.True(BotSafe.IsPoisoned("cool-1"));

            // Cooldown-Boundary inklusiv (Code: NowProvider() >= unlockTime)
            // → bei exakt unlockTime expired.
            _now = 600f;
            Assert.False(BotSafe.IsPoisoned("cool-1"));
        }

        // ----- Clear -----

        [Fact]
        public void Clear_ResetsAllState()
        {
            BotSafe.SafeTick(() => throw new Exception("e1"), context: "clear-1");
            BotSafe.SafeTick(() => throw new Exception("e2"), context: "clear-1");
            Assert.True(BotSafe.IsPoisoned("clear-1"));

            BotSafe.Clear();
            Assert.False(BotSafe.IsPoisoned("clear-1"));
            Assert.Equal(0, BotSafe.GetExceptionCount("clear-1"));
        }

        // ----- Multi-Context-Isolation -----

        [Fact]
        public void DifferentContexts_HavingIndependentBudgets()
        {
            BotSafe.SafeTick(() => throw new Exception("a1"), context: "ctx-A");
            BotSafe.SafeTick(() => throw new Exception("a2"), context: "ctx-A");
            BotSafe.SafeTick(() => throw new Exception("b1"), context: "ctx-B");

            Assert.True(BotSafe.IsPoisoned("ctx-A"));
            Assert.False(BotSafe.IsPoisoned("ctx-B"));
            Assert.Equal(1, BotSafe.GetExceptionCount("ctx-B"));
        }
    }
}
