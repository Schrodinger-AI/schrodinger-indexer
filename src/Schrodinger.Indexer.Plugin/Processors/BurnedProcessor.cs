using System;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Orleans.Runtime;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.Provider;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class BurnedProcessor : TokenProcessorBase<Burned>
{
    private readonly ISchrodingerHolderDailyChangeProvider _schrodingerHolderDailyChangeProvider;
    private readonly IAElfIndexerClientEntityRepository<TraitsCountIndex, LogEventInfo> _traitsCountIndexRepository;
    public BurnedProcessor(ILogger<TokenProcessorBase<Burned>> logger,
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
    
    protected override async Task HandleEventAsync(Burned eventValue, LogEventContext context)
    {
        var chainId = context.ChainId;
        var symbol = eventValue.Symbol;
        var owner = eventValue.Burner?.ToBase58();
        var amount = eventValue.Amount;
        try
        {
            var tick = TokenSymbolHelper.GetTickBySymbol(symbol);
            var schrodingerIndex = await SchrodingerRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(chainId, tick), chainId);
            if (schrodingerIndex == null)
            {
                return;
            }
            
            Logger.LogDebug("[Burned] start chainId:{chainId} symbol:{symbol}, owner:{owner}, amount:{amount}", chainId, symbol, owner, amount);
            var isGen0 = TokenSymbolHelper.GetIsGen0FromSymbol(symbol);
            var holderIndex = await UpdatedHolderRelatedAsync(chainId, symbol, owner, -amount,
                0, SchrodingerConstants.Burned, context);
            var holderCountAfterUpdate = await GetSymbolHolderCountAsync(chainId, symbol);
            
            Logger.LogDebug("[CrossChainReceived] UpdateSchrodingerCountAsync isGen0:{isGen0} holderCountAfterUpdate:{holderCountBeforeUpdate}", isGen0, holderCountAfterUpdate);

            if (!isGen0 && holderCountAfterUpdate <= 0)
            {
                await UpdateSchrodingerCountAsync(holderIndex, tick, -1, context);
                await UpdateTraitCountAsync(chainId, symbol, context);
            }
            
            await _schrodingerHolderDailyChangeProvider.SaveSchrodingerHolderDailyChangeAsync(symbol, owner, -amount, context);
            Logger.LogDebug("[Burned] end chainId:{chainId} symbol:{symbol}, owner:{owner}, amount:{amount}", chainId, symbol, owner, amount);

        }
        catch (Exception e)
        {
            Logger.LogError(e, "[Burned] Exception chainId:{chainId} symbol:{symbol}, owner:{owner}, amount:{amount}", chainId, symbol, owner, amount);
            throw;
        }
    }
    
    private async Task UpdateTraitCountAsync(string chainId, string symbol, LogEventContext context)
    {
        var symbolIndex = await GetSymbolAsync(chainId, symbol);
        foreach (var traitInfo in symbolIndex.Traits)
        {
            var traitType = traitInfo.TraitType;
            var traitValue = traitInfo.Value;

            await ReduceTraitCountAsync(traitType, traitValue, chainId, context);
        }
    }
    
    private async Task ReduceTraitCountAsync(string traitType, string traitValue, string chainId, LogEventContext context)
    {
        var traitCountIndexId = IdGenerateHelper.GetTraitCountId(chainId, traitType);
        var traitCountIndex = await _traitsCountIndexRepository.GetFromBlockStateSetAsync(traitCountIndexId, chainId);
        var now = DateTimeHelper.GetCurrentTimestamp();
        if (traitCountIndex == null)
        {
            Logger.LogError( "[Burned] TraitCountIndex Not Exist, chainId:{chainId} traitType:{type}, traitValue:{value}",  chainId, traitType, traitValue);
            return;
        }
           
        var valueInfos = traitCountIndex.Values;
        bool valueExist = false;
    
        for (int i = 0; i < valueInfos.Count; i++)
        {
            if (valueInfos[i].Value == traitValue)
            {
                valueInfos[i].Amount--;
                valueExist = true;
                break;
            }
        }
        
        if (!valueExist)
        {
            Logger.LogError( "[Burned] Trait Value Not Exist In Trait Type, chainId:{chainId} traitType:{type}, traitValue:{value}",  chainId, traitType, traitValue);
            return;
        }
        
        traitCountIndex.Values = valueInfos;
        traitCountIndex.Amount--;
        traitCountIndex.UpdateTime = now;
        ObjectMapper.Map(context, traitCountIndex);
        await _traitsCountIndexRepository.AddOrUpdateAsync(traitCountIndex);
        
        Logger.LogDebug("[Issued] UpdateTraitCountAsync index:{holderCountBeforeUpdate}", 
            JsonConvert.SerializeObject(traitCountIndex));
    }
}