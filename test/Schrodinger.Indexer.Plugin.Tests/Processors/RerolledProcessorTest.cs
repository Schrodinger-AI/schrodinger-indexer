using AElf;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Grains.State.Client;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class RerolledProcessorTest : SchrodingerIndexerPluginTestBase
{
    [Fact]
    public async Task HandleAppliedProcessor()
    {
        var context = MockLogEventContext();
        var state = await MockBlockState(context);
        var rerolled = new Rerolled()
        {
            Symbol = "TEST-Symbol",
            Ancestor = "GEN2",
            Amount = 100,
            Recipient = Address.FromBase58("xsnQafDAhNTeYcooptETqWnYBksFGGXxfcQyJJ5tmu6Ak9ZZt"),
        };
        var logEvent = MockLogEventInfo(rerolled.ToLogEvent());

        var rerolledProcessor = GetRequiredService<RerolledProcessor>();
        var rerolledRepository = GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerResetIndex, LogEventInfo>>();
        var objectMapper = GetRequiredService<IObjectMapper>();

        await rerolledProcessor.HandleEventAsync(logEvent, context);
        await BlockStateSetSaveDataAsync<LogEventInfo>(state);
        
        
        var rerolledIndexId = IdGenerateHelper.GetId(context.ChainId, context.TransactionId);
        var rerolledIndex = await rerolledRepository.GetFromBlockStateSetAsync(rerolledIndexId, context.ChainId);
        
        
        rerolledIndex.ShouldNotBeNull();
        rerolledIndex.Symbol.ShouldBe("TEST-Symbol");
    }
}