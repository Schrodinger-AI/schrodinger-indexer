using Shouldly;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class TransferredProcessorTest : SchrodingerIndexerPluginTestBase
{
    [Fact]
    public async Task HandleEventAsync_Gen0_MainChain_Test()
    {
        await MockEventProcess(CollectionDeployed(), CollectionDeployedProcessor, MainChainId);
        await MockEventProcess(TokenCreatedGen0(), TokenCreatedProcessor, MainChainId);
        await MockEventProcess(IssuedGen0(), IssuedProcessor, MainChainId);
        var logEvent =  await MockEventProcess(TransferredGen0(), TransferredProcessor, MainChainId);
        
        var oldHolderId = IdGenerateHelper.GetId(MainChainId, GEN0Symbol, Issuer);
        var oldHolderIndex = await SchrodingerHolderRepository.GetFromBlockStateSetAsync(oldHolderId, MainChainId);
        oldHolderIndex.ShouldNotBeNull();
        oldHolderIndex.Id.ShouldBe(oldHolderId);
        oldHolderIndex.Address.ShouldBe(Issuer);
        oldHolderIndex.Traits.Count.ShouldBe(0);
        oldHolderIndex.Amount.ShouldBe(IssuedAmount - TransferredAmount);
        var schrodingerInfo = oldHolderIndex.SchrodingerInfo;
        schrodingerInfo.Tick.ShouldBe(Tick);
        schrodingerInfo.Symbol.ShouldBe(GEN0Symbol);
        schrodingerInfo.TokenName.ShouldBe(GEN0TokenName);
        
        var newHolderId = IdGenerateHelper.GetId(MainChainId, GEN0Symbol, Owner);
        var newHolderIndex = await SchrodingerHolderRepository.GetFromBlockStateSetAsync(newHolderId, MainChainId);
        newHolderIndex.ShouldNotBeNull();
        newHolderIndex.Id.ShouldBe(newHolderId);
        newHolderIndex.Address.ShouldBe(Owner);
        newHolderIndex.Traits.Count.ShouldBe(0);
        newHolderIndex.Amount.ShouldBe(TransferredAmount);
        var schrodingerInfo1 = newHolderIndex.SchrodingerInfo;
        schrodingerInfo1.Tick.ShouldBe(Tick);
        schrodingerInfo1.Symbol.ShouldBe(GEN0Symbol);
        schrodingerInfo1.TokenName.ShouldBe(GEN0TokenName);
        var date =  logEvent.BlockTime.ToString("yyyyMMdd");
        var oldHolderDailyChangeId = IdGenerateHelper.GetSchrodingerHolderDailyChangeId(MainChainId,date,GEN0Symbol,  Issuer);
        var oldSchrodingerHolderDailyChangeIndex =
            await SchrodingerHolderDailyChangeIndex.GetFromBlockStateSetAsync(oldHolderDailyChangeId, MainChainId);
        oldSchrodingerHolderDailyChangeIndex.ShouldNotBeNull();
        oldSchrodingerHolderDailyChangeIndex.Id.ShouldBe(oldHolderDailyChangeId);
        oldSchrodingerHolderDailyChangeIndex.ChainId.ShouldBe(MainChainId);
        oldSchrodingerHolderDailyChangeIndex.Address.ShouldBe(Issuer);
        oldSchrodingerHolderDailyChangeIndex.Symbol.ShouldBe(GEN0Symbol);
        oldSchrodingerHolderDailyChangeIndex.Date.ShouldBe(date);
        oldSchrodingerHolderDailyChangeIndex.ChangeAmount.ShouldBe(IssuedAmount - TransferredAmount);
        oldSchrodingerHolderDailyChangeIndex.Balance.ShouldBe(IssuedAmount - TransferredAmount);
        
        var newHolderDailyChangeId = IdGenerateHelper.GetSchrodingerHolderDailyChangeId(MainChainId,date,GEN0Symbol,  Owner);
        var newSchrodingerHolderDailyChangeIndex =
            await SchrodingerHolderDailyChangeIndex.GetFromBlockStateSetAsync(newHolderDailyChangeId, MainChainId);
        newSchrodingerHolderDailyChangeIndex.ShouldNotBeNull();
        newSchrodingerHolderDailyChangeIndex.Id.ShouldBe(newHolderDailyChangeId);
        newSchrodingerHolderDailyChangeIndex.ChainId.ShouldBe(MainChainId);
        newSchrodingerHolderDailyChangeIndex.Address.ShouldBe(Owner);
        newSchrodingerHolderDailyChangeIndex.Symbol.ShouldBe(GEN0Symbol);
        newSchrodingerHolderDailyChangeIndex.Date.ShouldBe(date);
        newSchrodingerHolderDailyChangeIndex.ChangeAmount.ShouldBe(TransferredAmount);
        newSchrodingerHolderDailyChangeIndex.Balance.ShouldBe(TransferredAmount);
    }
}