using AElf;
using Shouldly;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class SchrodingerAdoptProcessorTest : SchrodingerIndexerPluginTestBase
{
    
    [Fact]
    public async Task HandleEventAsync_Test()
    {
        await MockEventProcess(Adopted(), AdoptProcessor, SideChainId);
        
        var id = IdGenerateHelper.GetId(SideChainId, HashHelper.ComputeFrom(AdoptIdString).ToHex());

        var adoptIndex = await SchrodingerAdoptRepository.GetFromBlockStateSetAsync(id, SideChainId);
        
        adoptIndex.TokenName.ShouldBe(GEN1TokenName);
    }
}