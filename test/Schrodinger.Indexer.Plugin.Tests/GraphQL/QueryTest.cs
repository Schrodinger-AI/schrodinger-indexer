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
    
    [Fact]
    public async Task GetAllCatsListAsync_Test()
    {
        await MockEventProcess(Deployed(), DeployedLogEventProcessor, SideChainId);
        await MockEventProcess(TokenCreatedGen1(), TokenCreatedProcessor, SideChainId);
        await MockEventProcess(IssuedGen1(), IssuedProcessor, SideChainId);
        
        await MockEventProcess(Deployed(), DeployedLogEventProcessor, SideChainId);
        await MockEventProcess(TokenCreatedGen9(), TokenCreatedProcessor, SideChainId);
        await MockEventProcess(IssuedGen9(), IssuedProcessor, SideChainId);
        
        await MockEventProcess(AdoptedGen1(), AdoptProcessor, SideChainId);
        await MockEventProcess(AdoptedGen9(), AdoptProcessor, SideChainId);

        // Act
        var symbolIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo>>();
        var adoptIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo>>();

        {
            var result = await Query.GetAllSchrodingerListAsync(symbolIndexRepository, adoptIndexRepository,
                ObjectMapper, new GetAllSchrodingerListInput
                {
                    ChainId = SideChainId,
                    //Symbol = GEN1Symbol,
                    SkipCount = 0,
                    MaxResultCount = 1000
                });

            // Assert
            result.TotalCount.ShouldBe(2);
            result.Data.Count.ShouldBe(2);
        }
        
        {
            var result = await Query.GetAllSchrodingerListAsync(symbolIndexRepository, adoptIndexRepository,
                ObjectMapper, new GetAllSchrodingerListInput
                {
                    ChainId = SideChainId,
                    Generations = new List<int>(){1,9},
                    SkipCount = 0,
                    MaxResultCount = 1000
                });

            // Assert
            result.TotalCount.ShouldBe(2);
            result.Data.Count.ShouldBe(2);
        }
        
        {
            var result = await Query.GetAllSchrodingerListAsync(symbolIndexRepository, adoptIndexRepository,
                ObjectMapper, new GetAllSchrodingerListInput
                {
                    ChainId = SideChainId,
                    Generations = new List<int>(){9},
                    SkipCount = 0,
                    MaxResultCount = 1000
                });

            // Assert
            result.TotalCount.ShouldBe(1);
            result.Data.Count.ShouldBe(1);
            result.Data[0].Symbol.ShouldBe(GEN9Symbol);
            result.Data[0].Rarity.ShouldBe("Bronze");
            result.Data[0].Level.ShouldBe("1");
            result.Data[0].Grade.ShouldBe("1");
            result.Data[0].Star.ShouldBe("1");
            result.Data[0].Rank.ShouldBe(52092);
        }
        
        {
            var result = await Query.GetAllSchrodingerListAsync(symbolIndexRepository, adoptIndexRepository,
                ObjectMapper, new GetAllSchrodingerListInput
                {
                    ChainId = SideChainId,
                    Keyword = GEN9Symbol,
                    SkipCount = 0,
                    MaxResultCount = 1000
                });

            // Assert
            result.TotalCount.ShouldBe(1);
            result.Data.Count.ShouldBe(1);
            result.Data[0].Symbol.ShouldBe(GEN9Symbol);
            result.Data[0].Rarity.ShouldBe("Bronze");
            result.Data[0].Level.ShouldBe("1");
            result.Data[0].Grade.ShouldBe("1");
            result.Data[0].Star.ShouldBe("1");
            result.Data[0].Rank.ShouldBe(52092);
        }
        
        {
            var result = await Query.GetAllSchrodingerListAsync(symbolIndexRepository, adoptIndexRepository,
                ObjectMapper, new GetAllSchrodingerListInput
                {
                    ChainId = SideChainId,
                    Keyword = GEN9Symbol,
                    Raritys = new List<string>(){"Bronze", "Diamond"},
                    SkipCount = 0,
                    MaxResultCount = 1000
                });

            // Assert
            result.TotalCount.ShouldBe(1);
            result.Data.Count.ShouldBe(1);
            result.Data[0].Symbol.ShouldBe(GEN9Symbol);
            result.Data[0].Rarity.ShouldBe("Bronze");
            result.Data[0].Level.ShouldBe("1");
            result.Data[0].Grade.ShouldBe("1");
            result.Data[0].Star.ShouldBe("1");
            result.Data[0].Rank.ShouldBe(52092);
        }
        
        {
            var result = await Query.GetAllSchrodingerListAsync(symbolIndexRepository, adoptIndexRepository,
                ObjectMapper, new GetAllSchrodingerListInput
                {
                    ChainId = SideChainId,
                    Keyword = GEN9Symbol,
                    Raritys = new List<string>(){"Bronze", "Diamond"},
                    Traits = new List<TraitsInput>()
                    {
                        new TraitsInput()
                        {
                            TraitType = "Background",
                            Values = new List<string>(){"Desert Sunrise","Indigo Vortex"}
                        }
                    },
                    SkipCount = 0,
                    MaxResultCount = 1000
                });

            // Assert
            result.TotalCount.ShouldBe(1);
            result.Data.Count.ShouldBe(1);
            result.Data[0].Symbol.ShouldBe(GEN9Symbol);
            result.Data[0].Rarity.ShouldBe("Bronze");
            result.Data[0].Level.ShouldBe("1");
            result.Data[0].Grade.ShouldBe("1");
            result.Data[0].Star.ShouldBe("1");
            result.Data[0].Rank.ShouldBe(52092);
        }
    }
    
    
    [Fact]
    public async Task GetAllTraitstAsync_Test()
    {
        // Arrange
        var issuedProcessorTest = new IssuedProcessorTest();
        //mock data
        await issuedProcessorTest.HandleEventAsync_Gen1_MainChain_Test();
        
        // Act
        var traitValueRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<TraitsCountIndex, LogEventInfo>>();
        
        var schrodingerSymbolRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo>>();
       
        var result = await Query.GetAllTraitsAsync(traitValueRepository, schrodingerSymbolRepository, ObjectMapper, new GetAllTraitsInput
        {
            ChainId = MainChainId,
        });
        
        // Assert
        result.TraitsFilter.Count.ShouldBe(2);
        result.GenerationFilter.Count.ShouldBe(9);
        result.GenerationFilter[0].GenerationAmount.ShouldBe(1);
    }
}