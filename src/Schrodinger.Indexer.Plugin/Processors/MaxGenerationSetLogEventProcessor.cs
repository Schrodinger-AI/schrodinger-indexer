using System.Threading.Tasks;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class MaxGenerationSetLogEventProcessor: AElfLogEventProcessorBase<MaxGenerationSet, LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly ILogger<AElfLogEventProcessorBase<MaxGenerationSet, LogEventInfo>> _logger;
    private readonly IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo>
        _schrodingerIndexRepository;
    
    public MaxGenerationSetLogEventProcessor(ILogger<AElfLogEventProcessorBase<MaxGenerationSet, LogEventInfo>> logger,
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

    protected override async Task HandleEventAsync(MaxGenerationSet eventValue, LogEventContext context)
    {
        if (eventValue == null || context == null) return;
        
        var schrodingerId = IdGenerateHelper.GetId(context.ChainId, eventValue.Tick);
        var schrodingerIndex = await _schrodingerIndexRepository.GetFromBlockStateSetAsync(schrodingerId, context.ChainId);
        if(schrodingerIndex == null) return;

        schrodingerIndex.MaxGeneration = eventValue.Gen;
        _objectMapper.Map(context, schrodingerIndex);
        await _schrodingerIndexRepository.AddOrUpdateAsync(schrodingerIndex);
    }
}