using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class SchrodingerAdoptProcessor : SchrodingerProcessorBase<Adopted>
{
    public SchrodingerAdoptProcessor(ILogger<SchrodingerProcessorBase<Adopted>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> schrodingerAdoptRepository)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository,
            schrodingerTraitValueRepository, schrodingerSymbolRepository, schrodingerAdoptRepository)
    {
    }

    protected override async Task HandleEventAsync(Adopted adopted, LogEventContext context)
    {
        var chainId = context.ChainId;
        var symbol = adopted.Symbol;
        var adoptId = adopted.AdoptId?.ToHex();
        var parent = adopted.Parent;
        Logger.LogDebug("[Adopted] start chainId:{chainId} symbol:{symbol}, adoptId:{adoptId}, parent:{parent}", chainId,
            symbol, adoptId, parent);
        try
        {
            var adopt = ObjectMapper.Map<Adopted, SchrodingerAdoptIndex>(adopted);

            adopt.Id = IdGenerateHelper.GetId(chainId, symbol);
            adopt.AdoptTime = context.BlockTime;
         
            adopt.ParentInfo = await GetSchrodingerInfo(chainId, parent);
            // adopt.SchrodingerInfo = await GetSchrodingerInfo(chainId, symbol);
            ObjectMapper.Map(context, adopt);

            await SchrodingerAdoptRepository.AddOrUpdateAsync(adopt);
            Logger.LogDebug("[Adopted] end chainId:{chainId} symbol:{symbol}, adoptId:{adoptId}, parent:{parent}", chainId, symbol,
                adoptId, parent);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[Adopted] Exception chainId:{chainId} symbol:{symbol}, adoptId:{adoptId}, parent:{parent}", chainId,
                symbol,
                adoptId, parent);
            throw;
        }
    }
}