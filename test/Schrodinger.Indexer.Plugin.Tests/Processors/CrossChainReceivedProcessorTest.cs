using Shouldly;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class CrossChainReceivedProcessorTest : SchrodingerIndexerPluginTestBase
{
    [Fact]
    public async Task HandleEventAsync_Gen0_MainChain_Test()
    {
        await MockEventProcess(CollectionDeployed(), CollectionDeployedProcessor, MainChainId);
        await MockEventProcess(TokenCreatedGen0(), TokenCreatedProcessor, MainChainId);
        var logEvent = await MockEventProcess(CrossChainReceivedGen0(), CrossChainReceivedProcessor, MainChainId);
        
        var holderId = IdGenerateHelper.GetId(MainChainId, GEN0Symbol, Issuer);
        var holderIndex = await SchrodingerHolderRepository.GetFromBlockStateSetAsync(holderId, MainChainId);
        holderIndex.ShouldNotBeNull();
        holderIndex.Id.ShouldBe(holderId);
        holderIndex.Address.ShouldBe(Issuer);
        holderIndex.Traits.Count.ShouldBe(0);
        holderIndex.Amount.ShouldBe(CrossChainReceivedAmount);
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
        schrodingerHolderDailyChangeIndex.ChangeAmount.ShouldBe(CrossChainReceivedAmount);
        schrodingerHolderDailyChangeIndex.Balance.ShouldBe(CrossChainReceivedAmount);
    }

    [Fact]
    public async Task HandleEventAsync_Gen1_MainChain_Test()
    {
        await MockEventProcess(CollectionDeployed(), CollectionDeployedProcessor, MainChainId);
        await MockEventProcess(TokenCreatedGen1(), TokenCreatedProcessor, MainChainId);
        await MockEventProcess(CrossChainReceivedGen1(), CrossChainReceivedProcessor, MainChainId);
        
        var holderId = IdGenerateHelper.GetId(MainChainId, GEN1Symbol, Issuer);
        var holderIndex = await SchrodingerHolderRepository.GetFromBlockStateSetAsync(holderId, MainChainId);
        holderIndex.ShouldNotBeNull();
        holderIndex.Id.ShouldBe(holderId);
        holderIndex.Address.ShouldBe(Issuer);
        holderIndex.Traits.Count.ShouldBe(2);
        var traits = holderIndex.Traits;
        traits.Count.ShouldBe(2);
        var trait1 = traits[0];
        trait1.TraitType.ShouldBe(TraitType1);
        trait1.Value.ShouldBe(TraitValue1);
        var trait2 = traits[1];
        trait2.TraitType.ShouldBe(TraitType2);
        trait2.Value.ShouldBe(TraitValue2);
        holderIndex.Amount.ShouldBe(CrossChainReceivedAmount);
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
        traitValueIndex1.SchrodingerCount.ShouldBe(1);
        
        var traitValueId2 = IdGenerateHelper.GetId(MainChainId, Tick, TraitType2, TraitValue2);
        var traitValueIndex2 = await SchrodingerTraitValueRepository.GetFromBlockStateSetAsync(traitValueId2, MainChainId);
        traitValueIndex2.ShouldNotBeNull();
        traitValueIndex2.Tick.ShouldBe(Tick);
        traitValueIndex2.TraitType.ShouldBe(TraitType2);
        traitValueIndex2.Value.ShouldBe(TraitValue2);
        traitValueIndex2.SchrodingerCount.ShouldBe(1);
    }
}