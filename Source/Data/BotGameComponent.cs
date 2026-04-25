using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorldBot.Core;
using RimWorldBot.Events;
using RimWorldBot.Phases;
using UnityEngine;
using Verse;

namespace RimWorldBot.Data
{
    // Game-global Persistenz + Composition-Root-Placeholder (§5).
    // AI-4: einziger statischer Singleton ist RimWorldBotMod.Instance; BotGameComponent ist per-Game-Instanz.
    public class BotGameComponent : GameComponent
    {
        const int CurrentSchemaVersion = 4;   // Story 1.12: +lastSeenQuestIds (Schema-Bump v3→v4)
        int schemaVersion = CurrentSchemaVersion;

        // --- Private Runtime-Felder (F-ARCH-15) ---
        BotController controller;
        BoundedEventQueue<BotEvent> eventQueue;
        // configResolver wird in Story 2.x (Factory) in BuildController() assigned.
        // CS0649 bis dahin unterdrücken — null-Access ist durch ?.Invalidate() null-safe.
#pragma warning disable CS0649
        ConfigResolver configResolver;
#pragma warning restore CS0649
        // ------------------------------------------

        // Persistente State-Felder (§5 Snippet)
        public ToggleState masterState = ToggleState.Off;
        public int currentPhaseIndex;
        public int pendingPhaseIndex;                   // Zwei-Phasen-Commit (F-STAB-10)
        public EndingStrategy endingStrategy = EndingStrategy.Opportunistic;
        public Ending? primaryEnding;
        public EndingCommitment endingCommitment = EndingCommitment.None;
        public int autoEscapeStableCounter;             // F-AI-15
        public int ticksSincePhase7Entry;
        public int consecutiveSessionBudgetExhausts;    // F-STAB-23
        public List<PhaseGoal> completedGoals = new();
        public Dictionary<string, bool> perPawnPlayerUse = new();   // D-14: UniqueLoadID → playerUse
        public RecentDecisionsBuffer recentDecisions = new(transientCap: 100, pinnedCap: 25);
        // Story 1.12: Quest-IDs die der Poller zuletzt gesehen hat. Persistent damit nach Save-Load
        // nicht alle aktiven Quests fälschlich als "neu" detektiert werden.
        public HashSet<int> lastSeenQuestIds = new();

        // Public Accessor damit spätere Stories (z.B. Story 1.5 Keybinding) die Queue erreichen.
        public BoundedEventQueue<BotEvent> EventQueue => eventQueue;

        // Story 1.7 Hook: wird von RimWorldBotMod.WriteSettings nach jedem User-Settings-Change gerufen.
        // F-ARCH-08: invalidiert ConfigResolver-Cache damit neue Settings ohne Restart wirken.
        // Story-1.3-Stub: aktuell no-op weil ConfigResolver placeholder ist; echte Cache-Logik in Story 2.x.
        public void OnSettingsChanged()
        {
            configResolver?.Invalidate();
        }

        // Zentraler State-Change-Helper (Story 1.4 AC 4/6/7).
        // Alle Call-Sites (TabWindow, Keybinding in 1.5, Settings in 1.7) gehen über diese Methode
        // damit Log + DecisionLog + Persistenz konsistent laufen.
        public void SetMasterState(ToggleState newState)
        {
            if (newState == masterState) return;
            var oldState = masterState;
            masterState = newState;
            Log.Message($"[RimWorldBot] state changed: {oldState} → {newState}");
            recentDecisions.Add(new DecisionLogEntry(
                kind: "state_change",
                reason: $"{oldState} → {newState}",
                tick: GenTicks.TicksGame));
        }

        // Konstruktor: EventQueue VOR jeder Harmony-Patch-Möglichkeit initialisieren (D-24, F-ARCH-11).
        // RimWorld 1.6 GameComponent hat parameterless Konstruktor; Game-Ref ist via Current.Game erreichbar wenn nötig.
        public BotGameComponent(Game game)
        {
            eventQueue = new BoundedEventQueue<BotEvent>(criticalCap: 32, normalCap: 224);
        }

        public override void ExposeData()
        {
            try
            {
                Scribe_Values.Look(ref schemaVersion, "schemaVersion", 1);
                if (Scribe.mode == LoadSaveMode.LoadingVars && schemaVersion < CurrentSchemaVersion)
                {
                    // Migration wird nach dem Scribe-Load in PostLoadInit durchgeführt,
                    // damit alle Felder erst geladen sind.
                }

                Scribe_Values.Look(ref masterState, "masterState", ToggleState.Off);
                Scribe_Values.Look(ref currentPhaseIndex, "phaseIndex", 0);
                Scribe_Values.Look(ref pendingPhaseIndex, "pendingPhaseIndex", 0);
                Scribe_Values.Look(ref endingStrategy, "endingStrategy", EndingStrategy.Opportunistic);
                Scribe_Values.Look(ref primaryEnding, "primaryEnding");
                Scribe_Values.Look(ref endingCommitment, "endingCommitment", EndingCommitment.None);
                Scribe_Values.Look(ref autoEscapeStableCounter, "autoEscapeStableCounter", 0);
                Scribe_Values.Look(ref ticksSincePhase7Entry, "ticksSincePhase7Entry", 0);
                Scribe_Values.Look(ref consecutiveSessionBudgetExhausts, "consecutiveSessionBudgetExhausts", 0);
                Scribe_Collections.Look(ref completedGoals, "completedGoals", LookMode.Deep);
                Scribe_Collections.Look(ref perPawnPlayerUse, "perPawnPlayerUse", LookMode.Value, LookMode.Value);
                Scribe_Deep.Look(ref recentDecisions, "recentDecisions");
                Scribe_Collections.Look(ref lastSeenQuestIds, "lastSeenQuestIds", LookMode.Value);

                if (Scribe.mode == LoadSaveMode.PostLoadInit)
                {
                    completedGoals ??= new List<PhaseGoal>();
                    perPawnPlayerUse ??= new Dictionary<string, bool>();
                    recentDecisions ??= new RecentDecisionsBuffer(transientCap: 100, pinnedCap: 25);
                    lastSeenQuestIds ??= new HashSet<int>();
                    if (schemaVersion < CurrentSchemaVersion) Migrate();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[RimWorldBot] BotGameComponent.ExposeData failure, falling back to defaults: {ex}");
                ResetToDefaults();
            }
        }

        void ResetToDefaults()
        {
            masterState = ToggleState.Off;
            currentPhaseIndex = 0;
            pendingPhaseIndex = 0;
            endingStrategy = EndingStrategy.Opportunistic;
            primaryEnding = null;
            endingCommitment = EndingCommitment.None;
            autoEscapeStableCounter = 0;
            ticksSincePhase7Entry = 0;
            consecutiveSessionBudgetExhausts = 0;
            completedGoals = new List<PhaseGoal>();
            perPawnPlayerUse = new Dictionary<string, bool>();
            recentDecisions = new RecentDecisionsBuffer(transientCap: 100, pinnedCap: 25);
            lastSeenQuestIds = new HashSet<int>();
            schemaVersion = CurrentSchemaVersion;
        }

        // Migrate läuft nur in PostLoadInit nachdem alle Felder geladen sind.
        // v1→v2: perPawnPlayerUse-Keying (thingIDNumber→UniqueLoadID).
        // v2→v3: no-op (BotMapComponent handled excludedCells separat).
        // v3→v4: +lastSeenQuestIds (Story 1.12 QuestManager-Polling).
        void Migrate()
        {
            int from = schemaVersion;
            try
            {
                if (schemaVersion < 2) MigrateV1ToV2();
                if (schemaVersion < 3) MigrateV2ToV3();
                if (schemaVersion < 4) MigrateV3ToV4();
                schemaVersion = CurrentSchemaVersion;
                recentDecisions.Add(new DecisionLogEntry(
                    kind: "schema-migration",
                    reason: $"migrated from v{from} to v{CurrentSchemaVersion}",
                    tick: GenTicks.TicksGame));
                Log.Message($"[RimWorldBot] Migrated BotGameComponent from schema v{from} to v{CurrentSchemaVersion}");
            }
            catch (Exception ex)
            {
                Log.Error($"[RimWorldBot] Migrate failure (from v{from}): {ex}. Falling back to defaults.");
                ResetToDefaults();
            }
        }

        // v1→v2: perPawnPlayerUse-Keying von int thingIDNumber → string UniqueLoadID via WorldPawns-Lookup.
        // Kombi-Threshold-Toast (NEW-STAB-05): dropped >= 1 AND (dropped/oldKeys.Count > 0.10 OR dropped >= 2).
        //
        // Pre-Release-Hinweis: v1-Saves existieren aktuell nicht in freier Wildbahn. Wenn dennoch ein Dict
        // bei schemaVersion<2 vorliegt, versucht Migrate den Lookup. Fehlgeschlagene Keys werden gedroppt,
        // erfolgreicher Lookup behält den Eintrag mit dem neuen UniqueLoadID-Key.
        //
        // Scribe-Detail: v1 hätte das Dict unter demselben Label `perPawnPlayerUse` mit int-Keys persistiert.
        // In v2+ ist das Dict-Typ-Signature bereits Dictionary<string,bool> — Scribe_Collections dekodiert
        // int-Keys aus v1 zu string (ToString()) via Fallback-Path, landet als string-"thingIDNumber" im Dict.
        // Lookup dann: WorldPawns → Pawn mit passendem thingIDNumber → GetUniqueLoadID() als neuer Key.
        void MigrateV1ToV2()
        {
            var oldDict = perPawnPlayerUse ?? new Dictionary<string, bool>();
            int oldCount = oldDict.Count;
            if (oldCount == 0) return;   // nichts zu migrieren

            var newDict = new Dictionary<string, bool>(oldCount);
            int droppedCount = 0;

            foreach (var kv in oldDict)
            {
                // v1-Key sollte als string-repräsentierte thingIDNumber parsebar sein.
                if (int.TryParse(kv.Key, out int thingIDNumber))
                {
                    var pawn = Find.WorldPawns?.AllPawnsAliveOrDead?
                        .FirstOrDefault(p => p != null && p.thingIDNumber == thingIDNumber);
                    var newKey = pawn?.GetUniqueLoadID();
                    if (!string.IsNullOrEmpty(newKey) && !newDict.ContainsKey(newKey))
                    {
                        newDict[newKey] = kv.Value;
                        continue;
                    }
                }
                // Nicht auflösbar → droppen.
                droppedCount++;
            }

            perPawnPlayerUse = newDict;

            if (droppedCount > 0)
            {
                Log.Warning($"[RimWorldBot] v1→v2 perPawnPlayerUse-Migrate: {droppedCount}/{oldCount} entries dropped (pawn-lookup failed).");
                recentDecisions.Add(new DecisionLogEntry(
                    kind: "user-toast-data-loss",
                    reason: $"v1→v2 migrate: {droppedCount}/{oldCount} entries dropped",
                    tick: GenTicks.TicksGame));

                // Kombi-Threshold (NEW-STAB-05): dropped >= 1 AND (droppedRatio > 0.10 OR dropped >= 2).
                // `oldCount` ist der Original-Divisor, NICHT droppedCount — frühere Implementierung war tautologisch.
                bool percentBranch = oldCount > 0 && (double)droppedCount / oldCount > 0.10;
                bool countBranch = droppedCount >= 2;
                if (droppedCount >= 1 && (percentBranch || countBranch))
                {
                    Messages.Message(
                        "RimWorldBot.Migration.DataLoss".Translate(droppedCount, oldCount),
                        MessageTypeDefOf.CautionInput, historical: false);
                }
            }
        }

        // v2→v3: excludedCells wird in BotMapComponent pro Map separat initialisiert (leeres HashSet).
        // BotGameComponent selbst hat in diesem Schritt keine Änderungen.
        void MigrateV2ToV3()
        {
            Log.Message("[RimWorldBot] v2→v3 migrate: no-op in BotGameComponent (excludedCells handled in BotMapComponent).");
        }

        // v3→v4 (Story 1.12): +lastSeenQuestIds initialisiert leer. Existierende Quests im
        // QuestManager werden beim ersten Poll als "neu" detektiert und Events ausgelöst — das ist
        // intendiert (Bot kennt sie noch nicht aus seiner Sicht), Consumer-Stories filtern selbst.
        void MigrateV3ToV4()
        {
            lastSeenQuestIds ??= new HashSet<int>();
            Log.Message("[RimWorldBot] v3→v4 migrate: +lastSeenQuestIds (empty, will populate on first QuestManagerPoller.Poll).");
        }

        public override void FinalizeInit()
        {
            BuildController();
        }

        void BuildController()
        {
            // Placeholder — echte Factory kommt in Story 2.x (BotControllerFactory + Kollaborateure).
            controller = null;
            Log.Message("[RimWorldBot] BotController build deferred to Epic 2");
        }

        public override void LoadedGame()
        {
            eventQueue?.Clear();                                 // F-STAB-20: alte Tick-Stamps verwerfen
            configResolver?.Invalidate();
            BotSafe.Clear();                                     // Story 1.10: ErrorBudget + Poison-Cooldown re-init
            if (controller == null) BuildController();
            ReconcilePendingPhase();
            ReconcilePhaseGoalOrphans();
            CheckBudgetExhaustHistory();
        }

        public override void StartedNewGame()
        {
            eventQueue?.Clear();
            configResolver?.Invalidate();
            BotSafe.Clear();                                     // Story 1.10
            if (controller == null) BuildController();
        }

        void ReconcilePendingPhase()
        {
            if (pendingPhaseIndex != currentPhaseIndex)
            {
                var entry = new DecisionLogEntry(
                    kind: "crash-recovery-phase-rollback",
                    reason: $"pending={pendingPhaseIndex} != current={currentPhaseIndex}",
                    tick: GenTicks.TicksGame);
                recentDecisions.Add(entry);   // Add() auto-pinned crash-recovery-* (siehe RecentDecisionsBuffer.AutoPinKinds)
                pendingPhaseIndex = currentPhaseIndex;
            }
        }

        void ReconcilePhaseGoalOrphans()
        {
            // F-STAB-10 + F-STAB-21 via D-25 Tag-Schema (§2.3b).
            foreach (var map in Find.Maps)
            {
                var botMap = map?.GetComponent<BotMapComponent>();
                botMap?.CancelOrphanedDesignations(currentPhaseIndex);
                botMap?.CancelOrphanedJobs(currentPhaseIndex);
            }
        }

        void CheckBudgetExhaustHistory()
        {
            if (consecutiveSessionBudgetExhausts >= 3)
            {
                Messages.Message(
                    "RimWorldBot.Stability.RepeatedSessionExhaust".Translate(),
                    MessageTypeDefOf.CautionInput, historical: false);
            }
        }

        // KeyDown-Handler für Ctrl+K-Toggle (Story 1.5).
        // Läuft pro Frame (~60 FPS) aus Unity Update()-Phase, damit KeyDownEvents auch im Pause-State erkannt werden.
        //
        // WICHTIG — Event.current ist hier NULL: `GameComponentUpdate` kommt aus Unity's Update(),
        // nicht aus OnGUI(). `Event.current` lebt nur im IMGUI-OnGUI-Dispatch. Modifier-Check deshalb
        // via `Input.GetKey(LeftControl/RightControl)`-Polling statt `Event.current.control`.
        //
        // Modifier-Check ist code-seitig — RimWorld 1.6 KeyBindingDef hat kein modifierA-XML-Feld.
        // Exception-Handling via Story 1.10 BotSafe.SafeTick (ErrorBudget + 10min-Poison bei ≥2 Exceptions/min).
        public override void GameComponentUpdate()
        {
            BotSafe.SafeTick(() =>
            {
                // GetNamedSilentFail: gibt null zurück bei fehlender Def statt zu werfen (Guard gegen Def-Load-Order).
                var def = DefDatabase<KeyBindingDef>.GetNamedSilentFail("RimWorldBot_ToggleMaster");
                if (def == null) return;
                if (!def.KeyDownEvent) return;

                // Control-Gate damit Plain-K (Vanilla Misc8 default) uns nicht ungewollt triggert.
                if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) return;

                // Cycle Off → Advisory → On → Off (Story 1.5 AC 3, identisch zum Button in Story 1.4).
                var next = masterState switch
                {
                    ToggleState.Off => ToggleState.Advisory,
                    ToggleState.Advisory => ToggleState.On,
                    ToggleState.On => ToggleState.Off,
                    _ => ToggleState.Off
                };
                SetMasterState(next);
            }, context: "BotGameComponent.GameComponentUpdate");
        }

        public override void GameComponentTick()
        {
            int tick = GenTicks.TicksGame;

            // Story 1.12: QuestManager-Polling alle 1250 Ticks (~21s @ 60TPS).
            // Reihenfolge: Poll VOR dem 60000er-Cleanup damit der nachfolgende
            // `if (tick % 60000 != 0) return;` den Poll nicht überspringt.
            // CR HIGH-2: defensives ??= weil Reflection-Construction (Save-Load-Pfad ohne Ctor)
            // Field-Initializer überspringen kann; eventQueue-Null-Check + lastSeenQuestIds-Init
            // hier statt nur in PostLoadInit damit GameComponentTick robust gegen jeden Init-Pfad ist.
            if (tick % QuestManagerPoller.PollIntervalTicks == 0)
            {
                lastSeenQuestIds ??= new HashSet<int>();
                if (eventQueue != null)
                {
                    QuestManagerPoller.Poll(lastSeenQuestIds, eventQueue);
                }
            }

            // perPawnPlayerUse-Cleanup alle 60000 Ticks (§5, F-STAB-19) — defensiv gegen Map-Dispose-Races.
            // Läuft controller-unabhängig.
            if (tick % 60000 != 0) return;
            if (perPawnPlayerUse == null || perPawnPlayerUse.Count == 0) return;

            var validIds = Find.Maps
                .Where(m => m?.mapPawns != null)
                .SelectMany(m => m.mapPawns.AllPawns?.ToList() ?? Enumerable.Empty<Pawn>())
                .Where(p => p != null && !p.Destroyed)
                .Select(p => p.GetUniqueLoadID())
                .ToHashSet();
            var stale = perPawnPlayerUse.Keys.Where(id => !validIds.Contains(id)).ToList();
            foreach (var id in stale) perPawnPlayerUse.Remove(id);
        }
    }
}
