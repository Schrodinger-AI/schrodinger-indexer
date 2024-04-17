using Shouldly;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class BurnedProcessorTest : SchrodingerIndexerPluginTestBase
{
    [Fact]
    public async Task HandleEventAsync_Gen0_MainChain_Test()
    {
        await MockEventProcess(CollectionDeployed(), CollectionDeployedProcessor, MainChainId);
        await MockEventProcess(TokenCreatedGen0(), TokenCreatedProcessor, MainChainId);
        await MockEventProcess(IssuedGen0(), IssuedProcessor, MainChainId);
        var logEvent = await MockEventProcess(BurnedGen0(), BurnedProcessor, MainChainId);
        
        var holderId = IdGenerateHelper.GetId(MainChainId, GEN0Symbol, Issuer);
        var holderIndex = await SchrodingerHolderRepository.GetFromBlockStateSetAsync(holderId, MainChainId);
        holderIndex.ShouldNotBeNull();
        holderIndex.Id.ShouldBe(holderId);
        holderIndex.Address.ShouldBe(Issuer);
        holderIndex.Traits.Count.ShouldBe(0);
        holderIndex.Amount.ShouldBe(IssuedAmount - BurnedAmount);
        var schrodingerInfo = holderIndex.SchrodingerInfo;
        schrodingerInfo.Tick.ShouldBe(Tick);
        schrodingerInfo.Symbol.ShouldBe(GEN0Symbol);
        schrodingerInfo.TokenName.ShouldBe(GEN0TokenName);
        var date =  logEvent.BlockTime.ToString("yyyyMMdd");
        var schrodingerHolderDailyChangeId = IdGenerateHelper.GetSchrodingerHolderDailyChangeId(MainChainId,date,GEN0Symbol,  Issuer);
        var schrodingerHolderDailyChangeIndex =
            await SchrodingerHolderDailyChangeIndex.GetFromBlockStateSetAsync(schrodingerHolderDailyChangeId, MainChainId);
        schrodingerHolderDailyChangeIndex.ShouldNotBeNull();
        schrodingerHolderDailyChangeIndex.Id.ShouldBe(schrodingerHolderDailyChangeId);
        schrodingerHolderDailyChangeIndex.ChainId.ShouldBe(MainChainId);
        schrodingerHolderDailyChangeIndex.Address.ShouldBe(Issuer);
        schrodingerHolderDailyChangeIndex.Symbol.ShouldBe(GEN0Symbol);
        schrodingerHolderDailyChangeIndex.Date.ShouldBe(date);
        schrodingerHolderDailyChangeIndex.ChangeAmount.ShouldBe(IssuedAmount-BurnedAmount);
        schrodingerHolderDailyChangeIndex.Balance.ShouldBe(IssuedAmount - BurnedAmount);
        
    }

    [Fact]
    public async Task HandleEventAsync_Gen1_MainChain_Test()
    {
        await MockEventProcess(CollectionDeployed(), CollectionDeployedProcessor, MainChainId);
        await MockEventProcess(TokenCreatedGen1(), TokenCreatedProcessor, MainChainId);
        await MockEventProcess(IssuedGen1(), IssuedProcessor, MainChainId);
        await MockEventProcess(BurnedGen1(), BurnedProcessor, MainChainId);
        
        var holderId = IdGenerateHelper.GetId(MainChainId, GEN1Symbol, Issuer);
        var holderIndex = await SchrodingerHolderRepository.GetFromBlockStateSetAsync(holderId, MainChainId);
        holderIndex.ShouldNotBeNull();
        holderIndex.Id.ShouldBe(holderId);
        holderIndex.Address.ShouldBe(Issuer);
        holderIndex.Traits.Count.ShouldBe(2);
        holderIndex.Amount.ShouldBe(IssuedAmount - BurnedAmount);
        var schrodingerInfo = holderIndex.SchrodingerInfo;
        schrodingerInfo.Tick.ShouldBe(Tick);
        schrodingerInfo.Symbol.ShouldBe(GEN1Symbol);
        schrodingerInfo.TokenName.ShouldBe(GEN1TokenName);
        
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
    public async Task HandleEventAsync_Gen1_SideChain_Test()
    {
        await MockEventProcess(CollectionDeployed(), CollectionDeployedProcessor, SideChainId);
        await MockEventProcess(TokenCreatedGen1(), TokenCreatedProcessor,SideChainId);
        await MockEventProcess(IssuedGen1(), IssuedProcessor, SideChainId);
        await MockEventProcess(BurnedGen1(), BurnedProcessor, SideChainId);
        
        var holderId = IdGenerateHelper.GetId(SideChainId, GEN1Symbol, Issuer);
        var holderIndex = await SchrodingerHolderRepository.GetFromBlockStateSetAsync(holderId, SideChainId);
        holderIndex.ShouldNotBeNull();
        holderIndex.Id.ShouldBe(holderId);
        holderIndex.Address.ShouldBe(Issuer);
        holderIndex.Traits.Count.ShouldBe(2);
        holderIndex.Amount.ShouldBe(IssuedAmount - BurnedAmount);
        var schrodingerInfo = holderIndex.SchrodingerInfo;
        schrodingerInfo.Tick.ShouldBe(Tick);
        schrodingerInfo.Symbol.ShouldBe(GEN1Symbol);
        schrodingerInfo.TokenName.ShouldBe(GEN1TokenName);
        
        var traitValueId1 = IdGenerateHelper.GetId(SideChainId, Tick, TraitType1, TraitValue1);
        var traitValueIndex1 = await SchrodingerTraitValueRepository.GetFromBlockStateSetAsync(traitValueId1, SideChainId);
        traitValueIndex1.ShouldNotBeNull();
        traitValueIndex1.Tick.ShouldBe(Tick);
        traitValueIndex1.TraitType.ShouldBe(TraitType1);
        traitValueIndex1.Value.ShouldBe(TraitValue1);
        
        var traitValueId2 = IdGenerateHelper.GetId(SideChainId, Tick, TraitType2, TraitValue2);
        var traitValueIndex2 = await SchrodingerTraitValueRepository.GetFromBlockStateSetAsync(traitValueId2, SideChainId);
        traitValueIndex2.ShouldNotBeNull();
        traitValueIndex2.Tick.ShouldBe(Tick);
        traitValueIndex2.TraitType.ShouldBe(TraitType2);
        traitValueIndex2.Value.ShouldBe(TraitValue2);
        
        var symbolId = IdGenerateHelper.GetId(SideChainId, GEN1Symbol);
        var symbolIndex = await SchrodingerSymbolRepository.GetFromBlockStateSetAsync(symbolId, SideChainId);
        symbolIndex.ShouldNotBeNull();
        symbolIndex.Id.ShouldBe(symbolId);
        symbolIndex.Amount.ShouldBe(IssuedAmount - BurnedAmount);
    }
}