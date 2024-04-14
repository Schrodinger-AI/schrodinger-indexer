using System;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.Provider;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class TransferredProcessor : TokenProcessorBase<Transferred>
{
    private readonly ILogger<TokenProcessorBase<Transferred>> _logger;
    private readonly ISchrodingerHolderDailyChangeProvider _schrodingerHolderDailyChangeProvider;

    public TransferredProcessor(ILogger<TokenProcessorBase<Transferred>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        ISchrodingerHolderDailyChangeProvider schrodingerHolderDailyChangeProvider)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository, schrodingerTraitValueRepository, schrodingerSymbolRepository)
    {
        _logger = logger;
        _schrodingerHolderDailyChangeProvider = schrodingerHolderDailyChangeProvider;
    }
    
    protected override async Task HandleEventAsync(Transferred eventValue, LogEventContext context)
    {
        var chainId = context.ChainId;
        var symbol = eventValue.Symbol;
        var oldOwner = eventValue.From?.ToBase58();
        var newOwner = eventValue.To?.ToBase58();
        var amount = eventValue.Amount;
        try
        {
            var tick = TokenSymbolHelper.GetTickBySymbol(symbol);
            var schrodingerIndex = await SchrodingerRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(chainId, tick), chainId);
            if (schrodingerIndex == null)
            {
                return;
            }
            
            _logger.LogDebug("[Transferred] start chainId:{chainId} symbol:{symbol}, newOwner:{newOwner}, oldOwner:{oldOwner}, amount:{amount}", chainId, symbol, newOwner, oldOwner, amount);
            await UpdatedHolderRelatedAsync(chainId, symbol, oldOwner, -amount, 
                0, SchrodingerConstants.TransferredFrom, context);
            await UpdatedHolderRelatedAsync(chainId, symbol, newOwner, amount, 
                amount, SchrodingerConstants.TransferredTo, context);
            await _schrodingerHolderDailyChangeProvider.SaveSchrodingerHolderDailyChangeAsync(symbol, oldOwner, -amount, context);
            await _schrodingerHolderDailyChangeProvider.SaveSchrodingerHolderDailyChangeAsync(symbol, newOwner, amount, context);

            _logger.LogDebug("[Transferred] end chainId:{chainId} symbol:{symbol}, newOwner:{newOwner}, oldOwner:{oldOwner}, amount:{amount}", chainId, symbol, newOwner, oldOwner, amount);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[Transferred] Exception chainId:{chainId} symbol:{symbol}, newOwner:{owner}, oldOwner:{oldOwner}, amount:{amount}", chainId, symbol, newOwner, oldOwner, amount);
            throw;
        }
    }
}