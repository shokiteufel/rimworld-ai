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
    // Time-Reference: Time.realtimeSinceStartup (Unity, sekunden-genau, läuft auch in Pause).
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

        // Per-Context Exception-Tracking: context → liste der Time.realtimeSinceStartup-Werte
        // aller jüngsten Exceptions (≤ ExceptionWindowSeconds alt).
        // RemoveAt(0) im Prune ist O(n), bei Threshold=2 bleibt die Liste praktisch winzig
        // (max ~3 Einträge zwischen Reports), keine Performance-Sorge.
        static readonly Dictionary<string, List<float>> _exceptionTimestamps = new();

        // Per-Context Poison-Cooldown: context → unlock-Time (Time.realtimeSinceStartup-Wert).
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

        // Diagnostic-Helper für Tests + Debug-Panel (Story 8.7).
        public static bool IsPoisoned(string context)
        {
            if (!_poisonedUntil.TryGetValue(context, out var unlockTime)) return false;
            if (Time.realtimeSinceStartup >= unlockTime)
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

        static void LogOnceWarning(string msg)
        {
            if (_warnedOnceMessages.Add(msg))
            {
                Log.Warning($"[RimWorldBot] {msg}");
            }
        }

        static void Report(string context, Exception ex)
        {
            Log.Error($"[RimWorldBot] Exception in {context}: {ex}");

            if (!_exceptionTimestamps.TryGetValue(context, out var list))
            {
                list = new List<float>();
                _exceptionTimestamps[context] = list;
            }
            list.Add(Time.realtimeSinceStartup);
            PruneOldExceptions(list);

            if (list.Count >= ExceptionThreshold)
            {
                _poisonedUntil[context] = Time.realtimeSinceStartup + PoisonCooldownSeconds;
                Log.Warning($"[RimWorldBot] Context '{context}' poisoned for {PoisonCooldownSeconds:F0}s (~10 min) after {list.Count} exceptions in {ExceptionWindowSeconds:F0}s window.");

                // TODO(Story 2.x): kritische Contexts (GameComponentUpdate, MapComponentOnGUI etc.)
                // sollen zusätzlich Master-Toggle auf Off setzen + User-Toast ("Bot disabled —
                // see log"). Unterscheidung kritisch vs. unkritisch über Context-Whitelist.
            }
        }

        static void PruneOldExceptions(List<float> list)
        {
            var cutoff = Time.realtimeSinceStartup - ExceptionWindowSeconds;
            while (list.Count > 0 && list[0] < cutoff)
            {
                list.RemoveAt(0);
            }
        }
    }
}
