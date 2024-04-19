using System;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.Runtime;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.Provider;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class CrossChainReceivedProcessor : TokenProcessorBase<CrossChainReceived>
{
    private readonly ISchrodingerHolderDailyChangeProvider _schrodingerHolderDailyChangeProvider;
    private readonly IAElfIndexerClientEntityRepository<TraitsCountIndex, LogEventInfo> _traitsCountIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<GenerationCountIndex, LogEventInfo> _generationCountIndexRepository;
    
    public CrossChainReceivedProcessor(ILogger<TokenProcessorBase<CrossChainReceived>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        ISchrodingerHolderDailyChangeProvider schrodingerHolderDailyChangeProvider,
        IAElfIndexerClientEntityRepository<TraitsCountIndex, LogEventInfo> traitsCountIndexRepository,
        IAElfIndexerClientEntityRepository<GenerationCountIndex, LogEventInfo> generationCountIndexRepository)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository, schrodingerTraitValueRepository, schrodingerSymbolRepository)
    {
        _schrodingerHolderDailyChangeProvider = schrodingerHolderDailyChangeProvider;
        _traitsCountIndexRepository = traitsCountIndexRepository;
        _generationCountIndexRepository = generationCountIndexRepository;
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
                await UpdateTraitCountAsync(chainId, symbol, context);
            }
            await _schrodingerHolderDailyChangeProvider.SaveSchrodingerHolderDailyChangeAsync(symbol, newOwner, amount, context);
            Logger.LogDebug("[CrossChainReceived] end chainId:{chainId} symbol:{symbol}, newOwner:{newOwner}, oldOwner:{oldOwner}, amount:{amount}", chainId, symbol, newOwner, oldOwner, amount);        }
        catch (Exception e)
        {
            Logger.LogError(e, "[CrossChainReceived] Exception chainId:{chainId} symbol:{symbol}, newOwner:{owner}, oldOwner:{oldOwner}, amount:{amount}", chainId, symbol, newOwner, oldOwner, amount);
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

            await AddTraitCountAsync(traitType, traitValue, chainId, context);
        }
        await UpdateGenerationCountAsync(symbolIndex.SchrodingerInfo.Gen, chainId, context);
    }
    
    private async Task AddTraitCountAsync(string traitType, string traitValue, string chainId, LogEventContext context)
    {
        var traitCountIndexId = IdGenerateHelper.GetTraitCountId(chainId, traitType);
        var traitCountIndex = await _traitsCountIndexRepository.GetFromBlockStateSetAsync(traitCountIndexId, chainId);
        var now = DateTimeHelper.GetCurrentTimestamp();
        if (traitCountIndex == null)
        {
            traitCountIndex = new TraitsCountIndex
            {
                Id = traitCountIndexId,
                TraitType = traitType,
                CreateTime = now,
                UpdateTime = now,
                Amount = 1,
                Values = new List<TraitsCountIndex.ValueInfo>
                {
                    new()
                    {
                        Value = traitValue,
                        Amount = 1
                    }
                }
            };
            
            ObjectMapper.Map(context, traitCountIndex);
            await _traitsCountIndexRepository.AddOrUpdateAsync(traitCountIndex);
        }
        else
        {
            var valueInfos = traitCountIndex.Values;
            bool valueExist = false;
    
            for (int i = 0; i < valueInfos.Count; i++)
            {
                if (valueInfos[i].Value == traitValue)
                {
                    valueInfos[i].Amount++;
                    valueExist = true;
                    break;
                }
            }
    
            if (!valueExist)
            {
                valueInfos.Add(new TraitsCountIndex.ValueInfo
                {
                    Value = traitValue,
                    Amount = 1
                });
            }
            
            traitCountIndex.Values = valueInfos;
            traitCountIndex.Amount++;
            traitCountIndex.UpdateTime = now;
            
            ObjectMapper.Map(context, traitCountIndex);
            await _traitsCountIndexRepository.AddOrUpdateAsync(traitCountIndex);
        }
        Logger.LogDebug("[Issued] UpdateTraitCountAsync index:{holderCountBeforeUpdate}", 
            JsonConvert.SerializeObject(traitCountIndex));
    }
    
    private async Task UpdateGenerationCountAsync(int generation, string chainId, LogEventContext context)
    {
        var generationCountIndexId = IdGenerateHelper.GetId(chainId, generation);
        var generationCountIndex = await _generationCountIndexRepository.GetFromBlockStateSetAsync(generationCountIndexId, chainId);
        var now = DateTimeHelper.GetCurrentTimestamp();
        if (generationCountIndex == null)
        {
            generationCountIndex = new GenerationCountIndex() {
                Id = generationCountIndexId,
                Generation = generation,
                CreateTime = now,
                UpdateTime = now,
                Count = 1
            };
            
            ObjectMapper.Map(context, generationCountIndex);
            await _generationCountIndexRepository.AddOrUpdateAsync(generationCountIndex);
        }
        else
        {
            generationCountIndex.Count ++;
            generationCountIndex.UpdateTime = now;
            ObjectMapper.Map(context, generationCountIndex);
            await _generationCountIndexRepository.AddOrUpdateAsync(generationCountIndex);
        }
    }
}