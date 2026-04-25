using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorldBot.Core
{
    // Story 1.10 (CC-STORIES-02) — einheitliches Exception-Wrapper-Pattern für alle Tick-Host-
    // und Execution-Apply-Methoden. Analog D-13-Harmony-Skelett: kein unbehandelter Throw
    // darf in den Vanilla-Callstack propagieren (würde RimWorld-UI/Tick-Loop blockieren).
    //
    // Verwendung in Folge-Stories:
    //   public override void GameComponentUpdate() {
    //       BotSafe.SafeTick(() => {
    //           // Body
    //       }, context: "BotGameComponent.GameComponentUpdate");
    //   }
    //
    //   public bool Apply(MyPlan plan) {
    //       // Caller-Pattern: Return-Wert nutzen um Downstream-Logik zu skippen wenn
    //       // Apply gefailt ODER Context poisoned ist:
    //       //   if (!BotSafe.SafeApply(...)) return;  // skip downstream
    //       return BotSafe.SafeApply(p => DoApply(p), plan, context: "MyPlanner.Apply");
    //   }
    //
    // ErrorBudget-Logik: pro context-ID werden Exception-Timestamps in einem Sliding-Window
    // (60s) gehalten. Bei ≥2 Exceptions/Min wird der Context als "poisoned" markiert
    // und für 10 Minuten geskippt (Caller bekommt no-op statt Re-Throw).
    //
    // Time-Reference: NowProvider() default Time.realtimeSinceStartup (Unity, sekunden-genau,
    // läuft auch in Pause). Story 1.14 Test-Seam: NowProvider ist injectable (Mock-Clock).
    // GenTicks.TicksGame friert in Pause ein — wäre falsch für GameComponentUpdate-Hook
    // weil Update-Loop pro Frame läuft auch in Pause. Sliding-Window würde sonst kollabieren.
    //
    // Alle State ist transient — re-initialisiert bei LoadedGame/StartedNewGame via Clear().
    //
    // AI-4-Hinweis: BotSafe verwendet `private static` Dicts. Das ist kein "Mod-Singleton" im
    // AI-4-Sinne (RimWorldBotMod.Instance bleibt einziges Instance-Singleton). BotSafe ist
    // pure Helper-State ohne Lifecycle-Ownership; Clear() in LoadedGame/StartedNewGame
    // (BotGameComponent.cs) garantiert Per-Game-Reset.
    public static class BotSafe
    {
        const float ExceptionWindowSeconds = 60f;
        const float PoisonCooldownSeconds = 600f;   // 10 min
        const int ExceptionThreshold = 2;

        // Story 1.14 Test-Seams (D-38): injectable für Tests im xUnit-Runner ohne Unity-Runtime.
        // - NowProvider: default = UnityEngine.Time.realtimeSinceStartup (sekunden-genau, läuft auch
        //   in Pause). Mock-Clock pro Test damit Sliding-Window deterministisch testbar.
        // - ErrorLogger / WarningLogger: defaults wrappen Verse.Log.Error/Warning. Tests müssen sie
        //   überschreiben weil Verse.Log → UnityEngine.Debug.LogError im xUnit-Runner crasht
        //   (Unity nicht initialisiert).
        // ResetNowProviderForTesting() reset alle drei.
        // `internal` damit InternalsVisibleTo("RimWorldBot.Tests") darauf zugreift.
        internal static System.Func<float> NowProvider = () => Time.realtimeSinceStartup;
        internal static System.Action<string> ErrorLogger = msg => Verse.Log.Error(msg);
        internal static System.Action<string> WarningLogger = msg => Verse.Log.Warning(msg);

        // Per-Context Exception-Tracking: context → liste der Now-Werte aller jüngsten
        // Exceptions (≤ ExceptionWindowSeconds alt).
        // RemoveAt(0) im Prune ist O(n), bei Threshold=2 bleibt die Liste praktisch winzig
        // (max ~3 Einträge zwischen Reports), keine Performance-Sorge.
        static readonly Dictionary<string, List<float>> _exceptionTimestamps = new();

        // Per-Context Poison-Cooldown: context → unlock-Time (Now-Wert).
        static readonly Dictionary<string, float> _poisonedUntil = new();

        // ----- Public API -----

        // Wrapt Tick-Host-Code (GameComponentTick, GameComponentUpdate, MapComponentTick,
        // MapComponentOnGUI). Body wird nur aufgerufen wenn Context nicht poisoned.
        public static void SafeTick(Action body, string context)
        {
            if (body == null)
            {
                LogOnceWarning("BotSafe.SafeTick: null body — skipping");
                return;
            }
            if (string.IsNullOrEmpty(context))
            {
                LogOnceWarning("BotSafe.SafeTick: empty context — skipping");
                return;
            }
            if (IsPoisoned(context)) return;

            try
            {
                body();
            }
            catch (Exception ex)
            {
                Report(context, ex);
            }
        }

        // Wrapt Execution-Apply-Code (BlueprintPlacer.Apply, BillManager.Apply etc.).
        // body returnt true bei Success, false bei graceful-skip; Exception → ErrorBudget.
        // Returnt false wenn Context poisoned ist (no-op) oder body fehlgeschlagen.
        // Caller-Pattern: `if (!BotSafe.SafeApply(...)) { /* skip downstream */ }`
        public static bool SafeApply<T>(Func<T, bool> body, T plan, string context)
        {
            if (body == null)
            {
                LogOnceWarning("BotSafe.SafeApply: null body — skipping");
                return false;
            }
            if (string.IsNullOrEmpty(context))
            {
                LogOnceWarning("BotSafe.SafeApply: empty context — skipping");
                return false;
            }
            if (IsPoisoned(context)) return false;

            try
            {
                return body(plan);
            }
            catch (Exception ex)
            {
                Report(context, ex);
                return false;
            }
        }

        // Manueller Reset — BotGameComponent.LoadedGame() + StartedNewGame() rufen das auf.
        public static void Clear()
        {
            _exceptionTimestamps.Clear();
            _poisonedUntil.Clear();
            _warnedOnceMessages.Clear();
        }

        // Story 1.14 Test-Helper: setzt alle Test-Seams (NowProvider + ErrorLogger + WarningLogger)
        // auf Defaults zurueck und ruft Clear(). Tests rufen das in IDisposable.Dispose() auf damit
        // andere Tests nicht die injizierten Mocks erben.
        // `internal` damit nur Test-Assembly drauf zugreifen kann.
        internal static void ResetNowProviderForTesting()
        {
            NowProvider = () => Time.realtimeSinceStartup;
            ErrorLogger = msg => Verse.Log.Error(msg);
            WarningLogger = msg => Verse.Log.Warning(msg);
            Clear();
        }

        // Diagnostic-Helper für Tests + Debug-Panel (Story 8.7).
        public static bool IsPoisoned(string context)
        {
            if (!_poisonedUntil.TryGetValue(context, out var unlockTime)) return false;
            if (NowProvider() >= unlockTime)
            {
                _poisonedUntil.Remove(context);
                return false;
            }
            return true;
        }

        public static int GetExceptionCount(string context)
        {
            if (!_exceptionTimestamps.TryGetValue(context, out var list)) return 0;
            PruneOldExceptions(list);
            return list.Count;
        }

        // ----- Internals -----

        static readonly HashSet<string> _warnedOnceMessages = new();

        // CR Story 1.14 MED-2: Logger-Calls inner-try wrappen damit eine fehlerhafte
        // ErrorLogger/WarningLogger-Lambda (oder ein crashender Verse.Log) NICHT die
        // SafeTick/SafeApply-Garantie ("kein Throw aus Tick-Host") bricht. Fallback ist
        // schweigend — wir nehmen Logger-Verlust in Kauf um Tick-Loop-Robustheit zu schuetzen.
        static void SafeLogError(string msg)
        {
            try { ErrorLogger(msg); } catch { /* swallow — siehe MED-2 */ }
        }
        static void SafeLogWarning(string msg)
        {
            try { WarningLogger(msg); } catch { /* swallow — siehe MED-2 */ }
        }

        static void LogOnceWarning(string msg)
        {
            if (_warnedOnceMessages.Add(msg))
            {
                SafeLogWarning($"[RimWorldBot] {msg}");
            }
        }

        static void Report(string context, Exception ex)
        {
            SafeLogError($"[RimWorldBot] Exception in {context}: {ex}");

            if (!_exceptionTimestamps.TryGetValue(context, out var list))
            {
                list = new List<float>();
                _exceptionTimestamps[context] = list;
            }
            list.Add(NowProvider());
            PruneOldExceptions(list);

            if (list.Count >= ExceptionThreshold)
            {
                _poisonedUntil[context] = NowProvider() + PoisonCooldownSeconds;
                SafeLogWarning($"[RimWorldBot] Context '{context}' poisoned for {PoisonCooldownSeconds:F0}s (~10 min) after {list.Count} exceptions in {ExceptionWindowSeconds:F0}s window.");

                // TODO(Story 2.x): kritische Contexts (GameComponentUpdate, MapComponentOnGUI etc.)
                // sollen zusätzlich Master-Toggle auf Off setzen + User-Toast ("Bot disabled —
                // see log"). Unterscheidung kritisch vs. unkritisch über Context-Whitelist.
            }
        }

        static void PruneOldExceptions(List<float> list)
        {
            var cutoff = NowProvider() - ExceptionWindowSeconds;
            while (list.Count > 0 && list[0] < cutoff)
            {
                list.RemoveAt(0);
            }
        }
    }
}
