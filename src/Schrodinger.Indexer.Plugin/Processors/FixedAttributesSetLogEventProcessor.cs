using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class FixedAttributesSetLogEventProcessor : AElfLogEventProcessorBase<FixedAttributeSet, LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly ILogger<AElfLogEventProcessorBase<FixedAttributeSet, LogEventInfo>> _logger;
    private readonly IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo>
        _schrodingerIndexRepository;
    
    public FixedAttributesSetLogEventProcessor(ILogger<AElfLogEventProcessorBase<FixedAttributeSet, LogEventInfo>> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _contractInfoOptions = contractInfoOptions.Value;
        _schrodingerIndexRepository = schrodingerIndexRepository;
    }
    
    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos[chainId].SchrodingerContractAddress;
    }

    protected override async Task HandleEventAsync(FixedAttributeSet eventValue, LogEventContext context)
    {
        _logger.LogDebug("[FixedAttributeSet] {eventValue} context: {context}",JsonConvert.SerializeObject(eventValue), 
            JsonConvert.SerializeObject(context));
        if (eventValue == null || context == null) return;
        
        var schrodingerId = IdGenerateHelper.GetId(context.ChainId, eventValue.Tick);
        var schrodingerIndex = await _schrodingerIndexRepository.GetFromBlockStateSetAsync(schrodingerId, context.ChainId);
        if(schrodingerIndex == null) return;

        if (eventValue.AddedAttribute != null)
        {
            schrodingerIndex.AttributeSets.FixedAttributes.Add(
                    _objectMapper.Map<AttributeSet, Entities.AttributeSet>(eventValue.AddedAttribute));
        }

        if (eventValue.RemovedAttribute != null)
        {
            for (int i = schrodingerIndex.AttributeSets.FixedAttributes.Count - 1; i >= 0; i--)
            {
                var fixedAttribute =
                    _objectMapper.Map<Entities.AttributeSet, AttributeSet>(schrodingerIndex.AttributeSets
                        .FixedAttributes[i]);
                if (eventValue.RemovedAttribute.Equals(fixedAttribute))
                {
                    schrodingerIndex.AttributeSets.FixedAttributes.RemoveAt(i);
                }
            }
        }
        
        _objectMapper.Map(context, schrodingerIndex);
         await _schrodingerIndexRepository.AddOrUpdateAsync(schrodingerIndex);
    }
}