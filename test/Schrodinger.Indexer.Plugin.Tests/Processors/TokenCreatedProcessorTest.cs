using Shouldly;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class TokenCreatedProcessorTest : SchrodingerIndexerPluginTestBase
{
    [Fact]
    public async Task HandleEventAsync_Gen0_MainChain_Test()
    {
        await MockEventProcess(CollectionDeployed(), CollectionDeployedProcessor, MainChainId);
        await MockEventProcess(TokenCreatedGen0(), TokenCreatedProcessor, MainChainId);
        
        var symbolId = IdGenerateHelper.GetId(MainChainId, GEN0Symbol);
        var symbolIndex = await SchrodingerSymbolRepository.GetFromBlockStateSetAsync(symbolId, MainChainId);
        symbolIndex.ShouldNotBeNull();
        symbolIndex.Symbol.ShouldBe(GEN0Symbol);
        symbolIndex.Traits.Count.ShouldBe(0);
        var schrodingerInfo = symbolIndex.SchrodingerInfo;
        schrodingerInfo.Tick.ShouldBe(Tick);
        schrodingerInfo.Symbol.ShouldBe(GEN0Symbol);
        schrodingerInfo.TokenName.ShouldBe(GEN0TokenName);
        schrodingerInfo.Decimals.ShouldBe(8);
        schrodingerInfo.Gen.ShouldBe(0);
    }
    
    [Fact]
    public async Task HandleEventAsync_Gen1_MainChain_Test()
    {
        await MockEventProcess(CollectionDeployed(), CollectionDeployedProcessor, MainChainId);
        await MockEventProcess(TokenCreatedGen1(), TokenCreatedProcessor, MainChainId);
        
        var symbolId = IdGenerateHelper.GetId(MainChainId, GEN1Symbol);
        var symbolIndex = await SchrodingerSymbolRepository.GetFromBlockStateSetAsync(symbolId, MainChainId);
        symbolIndex.ShouldNotBeNull();
        symbolIndex.Symbol.ShouldBe(GEN1Symbol);
        
        var traits = symbolIndex.Traits;
        traits.Count.ShouldBe(2);
        var trait1 = traits[0];
        trait1.TraitType.ShouldBe(TraitType1);
        trait1.Value.ShouldBe(TraitValue1);
        var trait2 = traits[0];
        trait2.TraitType.ShouldBe(TraitType2);
        trait2.Value.ShouldBe(TraitValue2);

        var schrodingerInfo = symbolIndex.SchrodingerInfo;
        schrodingerInfo.Tick.ShouldBe(Tick);
        schrodingerInfo.Symbol.ShouldBe(GEN1Symbol);
        schrodingerInfo.TokenName.ShouldBe(GEN1TokenName);
        schrodingerInfo.Decimals.ShouldBe(8);
        schrodingerInfo.Gen.ShouldBe(1);
        
        var traitValueId1 = IdGenerateHelper.GetId(MainChainId, Tick, TraitType1, TraitValue1);
        var traitValueIndex1 = await SchrodingerTraitValueRepository.GetFromBlockStateSetAsync(traitValueId1, MainChainId);
        traitValueIndex1.ShouldNotBeNull();
        traitValueIndex1.Tick.ShouldBe(Tick);
        traitValueIndex1.TraitType.ShouldBe(TraitType1);
        traitValueIndex1.Value.ShouldBe(TraitValue1);
        traitValueIndex1.SchrodingerCount.ShouldBe(0);
        
        var traitValueId2 = IdGenerateHelper.GetId(MainChainId, Tick, TraitType2, TraitValue2);
        var traitValueIndex2 = await SchrodingerTraitValueRepository.GetFromBlockStateSetAsync(traitValueId2, MainChainId);
        traitValueIndex2.ShouldNotBeNull();
        traitValueIndex2.Tick.ShouldBe(Tick);
        traitValueIndex2.TraitType.ShouldBe(TraitType2);
        traitValueIndex2.Value.ShouldBe(TraitValue2);
        traitValueIndex2.SchrodingerCount.ShouldBe(0);
    }

    [Fact]
    public async Task HandleEventAsync_Gen0_SideChain_Test()
    {
        await MockEventProcess(Deployed(), DeployedLogEventProcessor, SideChainId);
        await MockEventProcess(TokenCreatedGen0(), TokenCreatedProcessor, SideChainId);
        
        var symbolId = IdGenerateHelper.GetId(SideChainId, GEN0Symbol);
        var symbolIndex = await SchrodingerSymbolRepository.GetFromBlockStateSetAsync(symbolId, SideChainId);
        symbolIndex.ShouldNotBeNull();
        symbolIndex.Symbol.ShouldBe(GEN0Symbol);
        symbolIndex.Traits.Count.ShouldBe(0);
        var schrodingerInfo = symbolIndex.SchrodingerInfo;
        schrodingerInfo.Tick.ShouldBe(Tick);
        schrodingerInfo.Symbol.ShouldBe(GEN0Symbol);
        schrodingerInfo.TokenName.ShouldBe(GEN0TokenName);
        schrodingerInfo.Decimals.ShouldBe(8);
        schrodingerInfo.Gen.ShouldBe(0);
    }

    [Fact]
    public async Task HandleEventAsync_Gen1_SideChain_Test()
    {
        await MockEventProcess(Deployed(), DeployedLogEventProcessor, SideChainId);
        await MockEventProcess(TokenCreatedGen1(), TokenCreatedProcessor, SideChainId);
        
        var symbolId = IdGenerateHelper.GetId(SideChainId, GEN1Symbol);
        var symbolIndex = await SchrodingerSymbolRepository.GetFromBlockStateSetAsync(symbolId, SideChainId);
        symbolIndex.ShouldNotBeNull();
        symbolIndex.Symbol.ShouldBe(GEN1Symbol);
        
        var traits = symbolIndex.Traits;
        traits.Count.ShouldBe(2);
        var trait1 = traits[0];
        trait1.TraitType.ShouldBe(TraitType1);
        trait1.Value.ShouldBe(TraitValue1);
        var trait2 = traits[1];
        trait2.TraitType.ShouldBe(TraitType2);
        trait2.Value.ShouldBe(TraitValue2);

        var schrodingerInfo = symbolIndex.SchrodingerInfo;
        schrodingerInfo.Tick.ShouldBe(Tick);
        schrodingerInfo.Symbol.ShouldBe(GEN1Symbol);
        schrodingerInfo.TokenName.ShouldBe(GEN1TokenName);
        schrodingerInfo.Decimals.ShouldBe(8);
        schrodingerInfo.Gen.ShouldBe(1);
        
        var traitValueId1 = IdGenerateHelper.GetId(SideChainId, Tick, TraitType1, TraitValue1);
        var traitValueIndex1 = await SchrodingerTraitValueRepository.GetFromBlockStateSetAsync(traitValueId1, SideChainId);
        traitValueIndex1.ShouldNotBeNull();
        traitValueIndex1.Tick.ShouldBe(Tick);
        traitValueIndex1.TraitType.ShouldBe(TraitType1);
        traitValueIndex1.Value.ShouldBe(TraitValue1);
        traitValueIndex1.SchrodingerCount.ShouldBe(0);
        
        var traitValueId2 = IdGenerateHelper.GetId(SideChainId, Tick, TraitType2, TraitValue2);
        var traitValueIndex2 = await SchrodingerTraitValueRepository.GetFromBlockStateSetAsync(traitValueId2, SideChainId);
        traitValueIndex2.ShouldNotBeNull();
        traitValueIndex2.Tick.ShouldBe(Tick);
        traitValueIndex2.TraitType.ShouldBe(TraitType2);
        traitValueIndex2.Value.ShouldBe(TraitValue2);
        traitValueIndex2.SchrodingerCount.ShouldBe(0);
    }
}