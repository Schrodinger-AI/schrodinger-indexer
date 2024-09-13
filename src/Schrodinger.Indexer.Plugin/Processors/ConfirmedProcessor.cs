using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class ConfirmedProcessor : SchrodingerProcessorBase<Confirmed>
{
    public ConfirmedProcessor(ILogger<SchrodingerProcessorBase<Confirmed>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> schrodingerAdoptRepository)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository, schrodingerTraitValueRepository, schrodingerSymbolRepository, schrodingerAdoptRepository)
    {
    }

    protected override async Task HandleEventAsync(Confirmed eventValue, LogEventContext context)
    {
        var chainId = context.ChainId;
        var symbol = eventValue.Symbol;
        var owner = eventValue.Owner?.ToBase58();
        var adoptId = eventValue.AdoptId?.ToHex();
        Logger.LogDebug("[Confirmed] start chainId:{chainId} symbol:{symbol}, owner:{owner}", chainId, symbol, owner);
        try
        {
            var adoptIndexId = IdGenerateHelper.GetId(chainId, symbol);
            var adoptIndex = await SchrodingerAdoptRepository.GetFromBlockStateSetAsync(adoptIndexId, chainId);
            if (adoptIndex != null)
            {
                ObjectMapper.Map(eventValue, adoptIndex);
            }
            else
            {
                adoptIndex = ObjectMapper.Map<Confirmed, SchrodingerAdoptIndex>(eventValue);
                adoptIndex.Id = adoptIndexId;
            }
            
            adoptIndex.IsConfirmed = true;

            if (!eventValue.ExternalInfos.Value.IsNullOrEmpty())
            {
                foreach (var item in eventValue.ExternalInfos.Value)
                {
                    adoptIndex.AdoptExternalInfo[item.Key] = item.Value;
                } 
            }
            
            
            await SaveIndexAsync(adoptIndex, context);
            Logger.LogDebug("[Confirmed] end chainId:{chainId} symbol:{symbol}, owner:{owner}", chainId, symbol, owner);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[Confirmed] Exception chainId:{chainId} symbol:{symbol}, owner:{owner}", chainId, symbol, owner);
            throw;
        }
    }
}