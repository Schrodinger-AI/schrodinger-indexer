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

public class CrossChainReceivedProcessor : TokenProcessorBase<CrossChainReceived>
{
    private readonly ISchrodingerHolderDailyChangeProvider _schrodingerHolderDailyChangeProvider;

    public CrossChainReceivedProcessor(ILogger<TokenProcessorBase<CrossChainReceived>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        ISchrodingerHolderDailyChangeProvider schrodingerHolderDailyChangeProvider)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository, schrodingerTraitValueRepository, schrodingerSymbolRepository)
    {
        _schrodingerHolderDailyChangeProvider = schrodingerHolderDailyChangeProvider;
    }
    
    protected override async Task HandleEventAsync(CrossChainReceived eventValue, LogEventContext context)
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

            Logger.LogDebug("[CrossChainReceived] start chainId:{chainId} symbol:{symbol}, newOwner:{newOwner}, oldOwner:{oldOwner}, amount:{amount}", chainId, symbol, newOwner, oldOwner, amount);
            var isGen0 = TokenSymbolHelper.GetIsGen0FromSymbol(symbol);
            var holderCountBeforeUpdate = await GetSymbolHolderCountAsync(chainId, symbol);
            var holderIndex = await UpdatedHolderRelatedAsync(chainId, symbol, newOwner, amount, 
                amount, SchrodingerConstants.CrossChainReceived, context);

            Logger.LogDebug("[CrossChainReceived] UpdateSchrodingerCountAsync isGen0:{isGen0} holderCountBeforeUpdate:{holderCountBeforeUpdate}", isGen0, holderCountBeforeUpdate);

            if (!isGen0 && holderCountBeforeUpdate <= 0)
            {
                await UpdateSchrodingerCountAsync(holderIndex, tick, 1, context);
            }
            await _schrodingerHolderDailyChangeProvider.SaveSchrodingerHolderDailyChangeAsync(symbol, newOwner, amount, context);
            Logger.LogDebug("[CrossChainReceived] end chainId:{chainId} symbol:{symbol}, newOwner:{newOwner}, oldOwner:{oldOwner}, amount:{amount}", chainId, symbol, newOwner, oldOwner, amount);        }
        catch (Exception e)
        {
            Logger.LogError(e, "[CrossChainReceived] Exception chainId:{chainId} symbol:{symbol}, newOwner:{owner}, oldOwner:{oldOwner}, amount:{amount}", chainId, symbol, newOwner, oldOwner, amount);
            throw;
        }
    }
}