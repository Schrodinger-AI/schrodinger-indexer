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

public class IssuedProcessor : TokenProcessorBase<Issued>
{
    private readonly ISchrodingerHolderDailyChangeProvider _schrodingerHolderDailyChangeProvider;
    private readonly IAElfIndexerClientEntityRepository<TraitsCountIndex, LogEventInfo>
        _traitsCountIndexRepository;
    
    public IssuedProcessor(ILogger<TokenProcessorBase<Issued>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        ISchrodingerHolderDailyChangeProvider schrodingerHolderDailyChangeProvider,
        IAElfIndexerClientEntityRepository<TraitsCountIndex, LogEventInfo> traitsCountIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository, schrodingerTraitValueRepository, schrodingerSymbolRepository)
    {
        _schrodingerHolderDailyChangeProvider = schrodingerHolderDailyChangeProvider;
        _traitsCountIndexRepository = traitsCountIndexRepository;
    }
    
    protected override async Task HandleEventAsync(Issued eventValue, LogEventContext context)
    {
        var chainId = context.ChainId;
        var symbol = eventValue.Symbol;
        var owner = eventValue.To?.ToBase58();
        var amount = eventValue.Amount;
        try
        {
            var tick = TokenSymbolHelper.GetTickBySymbol(symbol);
            var schrodingerIndex = await SchrodingerRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(chainId, tick), chainId);
            if (schrodingerIndex == null)
            {
                return;
            }
            
            Logger.LogDebug("[Issued] start chainId:{chainId} symbol:{symbol}, owner:{owner}, amount:{amount}", chainId, symbol, owner, amount);
            var isGen0 = TokenSymbolHelper.GetIsGen0FromSymbol(symbol);
            var holderCountBeforeUpdate = await GetSymbolHolderCountAsync(chainId, symbol);
            var holderIndex = await UpdatedHolderRelatedAsync(chainId, symbol, owner, amount, 
                amount, SchrodingerConstants.Issued, context);
            
            Logger.LogDebug("[Issued] UpdateSchrodingerCountAsync isGen0:{isGen0} holderCountBeforeUpdate:{holderCountBeforeUpdate}", isGen0, holderCountBeforeUpdate);

            if (!isGen0 && holderCountBeforeUpdate <= 0)
            {
                Logger.LogDebug("[Issued] UpdateSchrodingerCountAsync chainId:{chainId} symbol:{symbol}, owner:{owner}, amount:{amount}", chainId, symbol, owner, amount);
                await UpdateSchrodingerCountAsync(holderIndex, tick, 1, context);
            }
            await _schrodingerHolderDailyChangeProvider.SaveSchrodingerHolderDailyChangeAsync(symbol, owner, amount, context);
            Logger.LogDebug("[Issued] end chainId:{chainId} symbol:{symbol}, owner:{owner}, amount:{amount}", chainId, symbol, owner, amount);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[Issued] Exception chainId:{chainId} symbol:{symbol}, owner:{owner}, amount:{amount}", chainId, symbol, owner, amount);
            throw;
        }
    }
}