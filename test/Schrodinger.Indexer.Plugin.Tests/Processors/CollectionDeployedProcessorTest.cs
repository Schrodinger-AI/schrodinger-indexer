using Shouldly;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class CollectionDeployedProcessorTest : SchrodingerIndexerPluginTestBase
{
    [Fact]
    public async Task HandleEventAsync_Test()
    {
        await MockEventProcess(CollectionDeployed(), CollectionDeployedProcessor, MainChainId);

        var schrodingerId = IdGenerateHelper.GetId(MainChainId, Tick);
        var schrodingerIndex = await SchrodingerRepository.GetFromBlockStateSetAsync(schrodingerId, MainChainId);
        schrodingerIndex.ShouldNotBeNull();
        schrodingerIndex.Id.ShouldBe(schrodingerId);
        schrodingerIndex.Tick.ShouldBe(Tick);
        schrodingerIndex.Ancestor.ShouldBe(Ancestor);
        schrodingerIndex.TotalSupply.ShouldBe(TotalSupply);
        schrodingerIndex.Decimals.ShouldBe(Decimals);
        var externalInfo = schrodingerIndex.ExternalInfo;
        externalInfo.Count.ShouldBe(1);
        externalInfo[InscriptionImageKey].ShouldBe(InscriptionImage);
    }
}