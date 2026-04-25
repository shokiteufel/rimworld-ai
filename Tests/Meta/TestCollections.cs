using Xunit;

namespace RimWorldBot.Tests.Meta
{
    // Story 1.14 (D-38): Test-Collection-Definition fuer Tests die shared static state
    // in BotSafe (NowProvider, ErrorLogger, WarningLogger) oder QuestManagerPoller
    // (QuestSource, TickProvider) manipulieren.
    //
    // DisableParallelization = true → Tests in dieser Collection laufen sequentiell,
    // verhindert Race-Conditions wenn ein Test-Dispose Static-Felder reset waehrend
    // ein paralleler Test sie gerade setzt.
    [CollectionDefinition("StaticStateMutators", DisableParallelization = true)]
    public class StaticStateMutatorsCollection { }
}
