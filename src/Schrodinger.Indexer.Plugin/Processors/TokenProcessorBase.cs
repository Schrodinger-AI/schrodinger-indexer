using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public abstract class TokenProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent, LogEventInfo>
    where TEvent : IEvent<TEvent>, new()
{
    protected readonly ILogger<TokenProcessorBase<TEvent>> Logger;
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;

    protected readonly IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo>
        SchrodingerHolderRepository;

    protected readonly IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> SchrodingerRepository;

    protected readonly IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo>
        SchrodingerTraitValueRepository;

    protected readonly IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo>
        SchrodingerSymbolRepository;

    protected TokenProcessorBase(ILogger<TokenProcessorBase<TEvent>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository)
        : base(logger)
    {
        Logger = logger;
        ObjectMapper = objectMapper;
        ContractInfoOptions = contractInfoOptions.Value;
        SchrodingerHolderRepository = schrodingerHolderRepository;
        SchrodingerRepository = schrodingerRepository;
        SchrodingerTraitValueRepository = schrodingerTraitValueRepository;
        SchrodingerSymbolRepository = schrodingerSymbolRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos[chainId].TokenContractAddress;
    }

    protected async Task SaveIndexAsync(SchrodingerHolderIndex index, LogEventContext context)
    {
        ObjectMapper.Map(context, index);
        await SchrodingerHolderRepository.AddOrUpdateAsync(index);
    }

    protected async Task SaveIndexAsync(SchrodingerTraitValueIndex index, LogEventContext context)
    {
        ObjectMapper.Map(context, index);
        await SchrodingerTraitValueRepository.AddOrUpdateAsync(index);
    }

    protected async Task SaveIndexAsync(SchrodingerSymbolIndex index, LogEventContext context)
    {
        ObjectMapper.Map(context, index);
        await SchrodingerSymbolRepository.AddOrUpdateAsync(index);
    }

    protected string GetSymbolIndexId(string chainId, string symbol)
    {
        return IdGenerateHelper.GetId(chainId, symbol);
    }
    
    protected string GetTraitTypeValueIndexId(string chainId, string tick, string traitType, string traitValue)
    {
        return IdGenerateHelper.GetId(chainId, tick, traitType, traitValue);
    }

    protected string GetHolderIndexId(string chainId, string symbol, string owner)
    {
        return IdGenerateHelper.GetId(chainId, symbol, owner);
    }

    protected async Task<long> GetSymbolHolderCountAsync(string chainId, string symbol)
    {
        return (await GetSymbolAsync(chainId, symbol))?.HolderCount ?? 0;
    }

    protected async Task<SchrodingerSymbolIndex> GetSymbolAsync(string chainId, string symbol)
    {
        var symbolId = IdGenerateHelper.GetId(chainId, symbol);
        return await SchrodingerSymbolRepository.GetFromBlockStateSetAsync(symbolId, chainId);
    }

    protected async Task GenerateSchrodingerCountAsync(string chainId, string tick, string traitType, string traitValue,
        LogEventContext context)
    {
        var traitValueIndex = await SchrodingerTraitValueRepository.GetFromBlockStateSetAsync(GetTraitTypeValueIndexId(chainId, tick, traitType, traitValue), chainId);
        if (traitValueIndex == null)
        {
            await SaveIndexAsync(new SchrodingerTraitValueIndex
            {
                Id = GetTraitTypeValueIndexId(chainId, tick, traitType, traitValue),
                Tick = tick,
                TraitType = traitType,
                Value = traitValue,
                SchrodingerCount = 0
            }, context);   
        }
    }

    protected async Task UpdateSchrodingerCountAsync(SchrodingerHolderIndex holderIndex, string tick,
        long deltaCount, LogEventContext context)
    {
        foreach (var traitInfo in holderIndex.Traits)
        {
            var schrodingerTraitValueIndex =
                await SchrodingerTraitValueRepository.GetFromBlockStateSetAsync(
                    IdGenerateHelper.GetId(holderIndex.ChainId, tick, traitInfo.TraitType, traitInfo.Value)
                    , holderIndex.ChainId);
            Logger.LogDebug("UpdateSchrodingerCountAsync id: {id}", schrodingerTraitValueIndex.Id);
            Logger.LogDebug("UpdateSchrodingerCountAsync before count: {count}, type: {type}, value: {value}",
                schrodingerTraitValueIndex.SchrodingerCount, schrodingerTraitValueIndex.TraitType, schrodingerTraitValueIndex.Value);
            schrodingerTraitValueIndex.SchrodingerCount += deltaCount;
            if (schrodingerTraitValueIndex.SchrodingerCount < 0)
            {
                schrodingerTraitValueIndex.SchrodingerCount = 0;
            }
            await SaveIndexAsync(schrodingerTraitValueIndex, context);
            Logger.LogDebug("UpdateSchrodingerCountAsync after count: {count}, type: {type}, value: {value}",
                schrodingerTraitValueIndex.SchrodingerCount, traitInfo.TraitType, traitInfo.Value);
        }
    }

    protected async Task<SchrodingerHolderIndex> UpdatedHolderRelatedAsync(string chainId, string symbol, string owner,
        long deltaAmount, long initAmount, string tokenEventType, LogEventContext context)
    {
        var holderId = GetHolderIndexId(chainId, symbol, owner);
        var holderIndex = await SchrodingerHolderRepository.GetFromBlockStateSetAsync(holderId, chainId);
        var holderExist = holderIndex != null;
        var beforeAmount = holderExist ? holderIndex.Amount : 0;
        
        if (!holderExist)
        {
            var symbolIndex = await GetSymbolAsync(chainId, symbol);
            holderIndex = ObjectMapper.Map<SchrodingerSymbolIndex, SchrodingerHolderIndex>(symbolIndex) ?? new SchrodingerHolderIndex();
            holderIndex.Address = owner;
            holderIndex.Id = holderId;
            holderIndex.Amount = initAmount;
        }
        else
        {
            holderIndex.Amount += deltaAmount;
        }

        if (holderIndex.Amount < 0)
        {
            holderIndex.Amount = 0;
        }

        var afterAmount = holderIndex.Amount;
        await SaveIndexAsync(holderIndex, context);
        await UpdateSymbolHolderAsync(beforeAmount, afterAmount, tokenEventType, chainId, symbol, context);
        
        return holderIndex;
    }

    protected async Task UpdateSymbolHolderAsync(long beforeAmount, long afterAmount, string tokenEventType,
        string chainId, string symbol, LogEventContext context)
    {
        var symbolId = GetSymbolIndexId(chainId, symbol);
        var symbolIndex = await SchrodingerSymbolRepository.GetFromBlockStateSetAsync(symbolId, chainId);
        switch (tokenEventType)
        {
            case SchrodingerConstants.Issued:
                symbolIndex = ChangeHolderCount(beforeAmount, 1, symbolIndex);
                break;
            case SchrodingerConstants.Burned:
                symbolIndex = ChangeHolderCount(afterAmount, -1, symbolIndex);
                break;
            case SchrodingerConstants.CrossChainReceived:
                symbolIndex = ChangeHolderCount(beforeAmount, 1, symbolIndex);
                break;
            case SchrodingerConstants.TransferredFrom:
                symbolIndex = ChangeHolderCount(afterAmount, -1, symbolIndex);
                break;
            case SchrodingerConstants.TransferredTo:
                symbolIndex = ChangeHolderCount(beforeAmount, 1, symbolIndex);
                break;
        }

        if (symbolIndex.HolderCount < 0)
        {
            symbolIndex.HolderCount = 0;
        }
        await SaveIndexAsync(symbolIndex, context);
    }
    
    private static SchrodingerSymbolIndex ChangeHolderCount(long amount, long deltaCount, SchrodingerSymbolIndex symbolIndex)
    {
        if (amount <= 0)
        {
            symbolIndex.HolderCount += deltaCount;
        }

        return symbolIndex;
    }
}