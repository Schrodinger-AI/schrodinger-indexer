using Shouldly;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class DeployedLogEventProcessorTest : SchrodingerIndexerPluginTestBase
{
    [Fact]
    public async Task HandleEventAsync_Test()
    {
        await MockEventProcess(Deployed(), DeployedLogEventProcessor, SideChainId);

        var schrodingerId = IdGenerateHelper.GetId(SideChainId, Tick);
        var schrodingerIndex = await SchrodingerRepository.GetFromBlockStateSetAsync(schrodingerId, SideChainId);
        schrodingerIndex.ShouldNotBeNull();
        schrodingerIndex.Id.ShouldBe(schrodingerId);
        schrodingerIndex.Tick.ShouldBe(Tick);
        schrodingerIndex.Ancestor.ShouldBe(Ancestor);
        schrodingerIndex.MaxGeneration.ShouldBe(MaxGeneration);
        schrodingerIndex.TotalSupply.ShouldBe(TotalSupply);
        schrodingerIndex.Decimals.ShouldBe(Decimals);
        var externalInfo = schrodingerIndex.ExternalInfo;
        externalInfo.Count.ShouldBe(1);
        externalInfo[InscriptionImageKey].ShouldBe(InscriptionImage);
    }
}