using AElfIndexer.Client;
using AElfIndexer.Grains.State.Client;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.GraphQL;
using Schrodinger.Indexer.Plugin.GraphQL.Dto;
using Schrodinger.Indexer.Plugin.Processors.SwapToken;
using Schrodinger.Indexer.Plugin.Tests.Processors;
using Shouldly;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.GraphQL;

public class QueryTest : QueryTestBase
{
    [Fact]
    public async Task GetSchrodingerDetailAsync_Test()
    {
        await MockEventProcess(Deployed(), DeployedLogEventProcessor, SideChainId);
        await MockEventProcess(TokenCreatedGen1(), TokenCreatedProcessor, SideChainId);
        await MockEventProcess(IssuedGen1(), IssuedProcessor, SideChainId);

        await Query.GetSchrodingerDetailAsync(SchrodingerHolderRepository, SchrodingerTraitValueRepository, ObjectMapper, new GetSchrodingerDetailInput
        {
            ChainId = SideChainId,
            Address = Issuer,
            Symbol = GEN1Symbol
        });
    }
    
    [Fact]
    public async Task GetSwapLPDailyListAsync_Test()
    {
        // Arrange
        var swapTokenProcessorTests = new SwapTokenProcessorTests();
        //mock data
        await swapTokenProcessorTests.TokenIssuedLogEventProcessor_Test();
        
       // Act
        var swapLPDailyIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<SwapLPDailyIndex, LogEventInfo>>();

        var result = await Query.GetSwapLPDailyListAsync(swapLPDailyIndexRepository, ObjectMapper, new GetSwapLPDailyListInput
        {
            ChainId = SideChainId_tDVW,
            Symbol = SwapSymbol,
            SkipCount = 0,
            MaxResultCount = 1000
        });
        
        // Assert
        result.TotalCount.ShouldBe(5);
        result.Data.Count.ShouldBe(5);
    }
}