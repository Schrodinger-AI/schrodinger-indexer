using System;
using System.Threading.Tasks;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using SchrodingerMain;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class CollectionDeployedProcessor: AElfLogEventProcessorBase<CollectionDeployed, LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly ILogger<AElfLogEventProcessorBase<CollectionDeployed, LogEventInfo>> _logger;
    private readonly IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo>
        _schrodingerIndexRepository;

    public CollectionDeployedProcessor(ILogger<AElfLogEventProcessorBase<CollectionDeployed, LogEventInfo>> logger,
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

    protected override async Task HandleEventAsync(CollectionDeployed eventValue, LogEventContext context)
    {
        var chainId = context.ChainId;
        var symbol = eventValue.Symbol;
        _logger.LogDebug("[CollectionDeployed] start chainId:{chainId} symbol:{symbol}", chainId, symbol);
        try
        {
            var tick = TokenSymbolHelper.GetTickBySymbol(symbol);
            var schrodingerIndex = await _schrodingerIndexRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(chainId, tick), chainId);
            if (schrodingerIndex != null)
            {
                _logger.LogDebug("[CollectionDeployed] schrodingerIndex alreadyExisted chainId:{chainId} symbol:{symbol}", chainId, symbol);
                return;
            }

            schrodingerIndex = _objectMapper.Map<CollectionDeployed, SchrodingerIndex>(eventValue);
            schrodingerIndex.Id = IdGenerateHelper.GetId(chainId, TokenSymbolHelper.GetTickBySymbol(symbol));
            _objectMapper.Map(context, schrodingerIndex);
            await _schrodingerIndexRepository.AddOrUpdateAsync(schrodingerIndex);
            _logger.LogDebug("[CollectionDeployed] end chainId:{chainId} symbol:{symbol}", chainId, symbol);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[CollectionDeployed] Exception chainId:{chainId} symbol:{symbol}", chainId, symbol);
            throw;
        }
    }
    
}