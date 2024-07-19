using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class AdoptionRerolledProcessor : SchrodingerProcessorBase<AdoptionRerolled>
{
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerCancelIndex, LogEventInfo> SchrodingerCancelRepository;
    
    public AdoptionRerolledProcessor(ILogger<SchrodingerProcessorBase<AdoptionRerolled>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> schrodingerAdoptRepository,
        IAElfIndexerClientEntityRepository<SchrodingerCancelIndex, LogEventInfo> schrodingerCancelRepository)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository, schrodingerTraitValueRepository, schrodingerSymbolRepository, schrodingerAdoptRepository)
    {
        SchrodingerCancelRepository = schrodingerCancelRepository;
    }

    protected override async Task HandleEventAsync(AdoptionRerolled eventValue, LogEventContext context)
    {
        Logger.LogDebug("[AdoptionRerolled] {eventValue} context: {context}",JsonConvert.SerializeObject(eventValue), 
            JsonConvert.SerializeObject(context));
        try
        {
            var AdoptionRerolledIndexId = IdGenerateHelper.GetId(context.ChainId, context.TransactionId);
            var AdoptionRerolledIndex = await SchrodingerCancelRepository.GetFromBlockStateSetAsync(AdoptionRerolledIndexId, context.ChainId);
            if (AdoptionRerolledIndex != null)
            {
                ObjectMapper.Map(eventValue, AdoptionRerolledIndex);
            }
            else
            {
                AdoptionRerolledIndex = ObjectMapper.Map<AdoptionRerolled, SchrodingerCancelIndex>(eventValue);
                AdoptionRerolledIndex.Id = AdoptionRerolledIndexId;
            }
            ObjectMapper.Map(context, AdoptionRerolledIndex);
            AdoptionRerolledIndex.From = eventValue.Account.ToBase58();
            await SchrodingerCancelRepository.AddOrUpdateAsync(AdoptionRerolledIndex);
            Logger.LogDebug("[AdoptionRerolled] end, index: {index}", AdoptionRerolledIndex);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[AdoptionRerolled] Exception");
            throw;
        }
    }
}