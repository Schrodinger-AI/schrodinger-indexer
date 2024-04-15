using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.Provider;

public class SchrodingerHolderDailyChangeProvider : ISchrodingerHolderDailyChangeProvider, ISingletonDependency
{
    private readonly IAElfIndexerClientEntityRepository<SchrodingerHolderDailyChangeIndex, LogEventInfo> _schrodingerHolderDailyChangeIndex;
    private readonly IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> _schrodingerHolderRepository;
    private readonly IObjectMapper _objectMapper;
    private readonly ILogger<ISchrodingerHolderDailyChangeProvider> _logger;
    
    public SchrodingerHolderDailyChangeProvider(IObjectMapper objectMapper, 
        IAElfIndexerClientEntityRepository<SchrodingerHolderDailyChangeIndex, LogEventInfo> schrodingerHolderDailyChangeIndex, 
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        ILogger<ISchrodingerHolderDailyChangeProvider> logger)
    {
        _objectMapper = objectMapper;
        _schrodingerHolderDailyChangeIndex = schrodingerHolderDailyChangeIndex;
        _schrodingerHolderRepository = schrodingerHolderRepository;
        _logger = logger;
    }

    public async Task SaveSchrodingerHolderDailyChangeAsync(string symbol, string address, long amount, LogEventContext context)
    {
        var holderId = IdGenerateHelper.GetId(context.ChainId, symbol, address);
        var holderIndex = await _schrodingerHolderRepository.GetFromBlockStateSetAsync(holderId, context.ChainId);
        if (holderIndex == null)
        {
            _logger.LogWarning("holderIndex is null, chainId:{chainId} symbol:{symbol}, address:{address}", context.ChainId, symbol, address);
            return;
        }

        var date =  context.BlockTime.ToString("yyyyMMdd");
        var schrodingerHolderDailyChangeId = IdGenerateHelper.GetSchrodingerHolderDailyChangeId(context.ChainId,date,symbol,address );
        var schrodingerHolderDailyChangeIndex =
            await _schrodingerHolderDailyChangeIndex.GetFromBlockStateSetAsync(schrodingerHolderDailyChangeId, context.ChainId);
        if (schrodingerHolderDailyChangeIndex == null)
        {
            schrodingerHolderDailyChangeIndex = new SchrodingerHolderDailyChangeIndex
            {
                Id = schrodingerHolderDailyChangeId,
                ChainId = context.ChainId,
                Address = address,
                Symbol = symbol,
                Date = date,
                ChangeAmount = amount,
                Balance = holderIndex.Amount
            };
        }
        else
        {
            schrodingerHolderDailyChangeIndex.ChangeAmount += amount;
            schrodingerHolderDailyChangeIndex.Balance = holderIndex.Amount;
        }
        _objectMapper.Map(context, schrodingerHolderDailyChangeIndex); 
        await _schrodingerHolderDailyChangeIndex.AddOrUpdateAsync(schrodingerHolderDailyChangeIndex);
    }
   
}

    