using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class RerolledProcessor : SchrodingerProcessorBase<Rerolled>
{
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerResetIndex, LogEventInfo> SchrodingerResetRepository;
    
    public RerolledProcessor(ILogger<SchrodingerProcessorBase<Rerolled>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> schrodingerAdoptRepository,
        IAElfIndexerClientEntityRepository<SchrodingerResetIndex, LogEventInfo> schrodingerResetRepository)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository, schrodingerTraitValueRepository, schrodingerSymbolRepository, schrodingerAdoptRepository)
    {
        SchrodingerResetRepository = schrodingerResetRepository;
    }

    protected override async Task HandleEventAsync(Rerolled eventValue, LogEventContext context)
    {
        Logger.LogDebug("[Rerolled] {eventValue} context: {context}",JsonConvert.SerializeObject(eventValue), 
            JsonConvert.SerializeObject(context));
        try
        {
            var rerolledIndexId = IdGenerateHelper.GetId(context.ChainId, context.TransactionId);
            var rerolledIndex = await SchrodingerResetRepository.GetFromBlockStateSetAsync(rerolledIndexId, context.ChainId);
            if (rerolledIndex != null)
            {
                ObjectMapper.Map(eventValue, rerolledIndex);
            }
            else
            {
                rerolledIndex = ObjectMapper.Map<Rerolled, SchrodingerResetIndex>(eventValue);
                rerolledIndex.Id = rerolledIndexId;
            }
            ObjectMapper.Map(context, rerolledIndex);
            await SchrodingerResetRepository.AddOrUpdateAsync(rerolledIndex);
            Logger.LogDebug("[Rerolled] end, index: {index}", rerolledIndex);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[Rerolled] Exception");
            throw;
        }
    }
}