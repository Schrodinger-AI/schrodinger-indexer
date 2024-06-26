using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Elasticsearch.Net.Specification.LicenseApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.Runtime;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.GraphQL.Dto;
using Schrodinger.Indexer.Plugin.Processors.Provider;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class TokenCreatedProcessor : TokenProcessorBase<TokenCreated>
{
    private readonly IRankProvider _rankProvider;

    public TokenCreatedProcessor(ILogger<TokenProcessorBase<TokenCreated>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        IRankProvider rankProvider)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository, schrodingerTraitValueRepository, schrodingerSymbolRepository)
    {
        _rankProvider = rankProvider;

    }
    
    protected override async Task HandleEventAsync(TokenCreated eventValue, LogEventContext context)
    {
        var chainId = context.ChainId;
        var symbol = eventValue.Symbol;
        try
        {
            var tick = TokenSymbolHelper.GetTickBySymbol(symbol);
            var schrodingerIndex = await SchrodingerRepository.GetFromBlockStateSetAsync(IdGenerateHelper.GetId(chainId, tick), chainId);
            if (schrodingerIndex == null)
            {
                return;
            }
            
            var isCollection = TokenSymbolHelper.GetIsCollectionFromSymbol(symbol);
            if (isCollection)
            {
                return;
            }
            
            Logger.LogDebug("[TokenCreated] start chainId:{chainId} symbol:{symbol}", chainId, symbol);
            var isGen0 = TokenSymbolHelper.GetIsGen0FromSymbol(symbol);
            var symbolIndex = new SchrodingerSymbolIndex
            {
                Id = GetSymbolIndexId(chainId, symbol), Symbol = symbol,
                SchrodingerInfo = TokenSymbolHelper.OfSchrodingerInfo(schrodingerIndex, eventValue, symbol, eventValue.TokenName)
            };
            if (!isGen0)
            {
                if (eventValue.ExternalInfo.Value.TryGetValue(SchrodingerConstants.NftAttributes, out var attributesJson))
                {
                    var attributeList = JsonConvert.DeserializeObject<List<Entities.Attribute>>(attributesJson?? string.Empty) ?? new List<Entities.Attribute>();
                    symbolIndex.Traits = ObjectMapper.Map<List<Entities.Attribute>, List<TraitInfo>>(attributeList);
                }

                foreach (var trait in symbolIndex.Traits)
                {
                    await GenerateSchrodingerCountAsync(chainId, tick, trait.TraitType, trait.Value, context);
                }
            }

            var isGen9 = TokenSymbolHelper.GetIsGen9FromSchrodingerSymbolIndex(symbolIndex);
            if (isGen9)
            {
                var traitsGenOne = new List<List<string>>();
                var traitsGenTwoToNine = new List<List<string>>();
                GetTraitsInput(symbolIndex.Traits, traitsGenOne, traitsGenTwoToNine);
                var rank = _rankProvider.GetRank(traitsGenOne, traitsGenTwoToNine);
                symbolIndex = SetRankRarity(symbolIndex, rank);
            }

            await SaveIndexAsync(symbolIndex, context);
            Logger.LogDebug("[TokenCreated] end chainId:{chainId} symbol:{symbol}", chainId, symbol);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[TokenCreated] Exception chainId:{chainId} symbol:{symbol}", chainId, symbol);
            throw;
        }
    }

    private void GetTraitsInput(List<TraitInfo> traitInfos, List<List<string>> traitsGenOne, List<List<string>> traitsGenTwoToNine)
    {
        var genOneTraitType = new List<string>();
        var genOneTraitValue = new List<string>();
        var genTwoTraitType = new List<string>();
        var genTwoTraitValue = new List<string>();

        for (int i=0; i<traitInfos.Count;i++)
        {
            if (i < 3)
            {
                genOneTraitType.Add(traitInfos[i].TraitType);
                genOneTraitValue.Add(traitInfos[i].Value);
                continue;
            }
            genTwoTraitType.Add(traitInfos[i].TraitType);
            genTwoTraitValue.Add(traitInfos[i].Value);
        }
        traitsGenOne.Add(genOneTraitType);
        traitsGenOne.Add(genOneTraitValue);
        traitsGenTwoToNine.Add(genTwoTraitType);
        traitsGenTwoToNine.Add(genTwoTraitValue);
    }

    private static SchrodingerSymbolIndex SetRankRarity(SchrodingerSymbolIndex symbolIndex, int rank)
    {
        symbolIndex.Rank = rank;
        symbolIndex.Level = "";
        symbolIndex.Grade = "";
        symbolIndex.Star = "";
        symbolIndex.Rarity = "";
        
        //get level
        var rankRes = LevelConstant.RankLevelGradeDictionary.TryGetValue(rank.ToString(), out var leaveGradeStar);
        if (!rankRes)
        {
            return symbolIndex;
        }
        symbolIndex.Level = leaveGradeStar.Split(SchrodingerConstants.RankLevelSegment)[0];
        symbolIndex.Grade = leaveGradeStar.Split(SchrodingerConstants.RankLevelSegment)[1];
        symbolIndex.Star = leaveGradeStar.Split(SchrodingerConstants.RankLevelSegment)[2];

        //get rarity
        symbolIndex.Rarity = LevelConstant.RarityDictionary.TryGetValue(symbolIndex.Level ?? "", out var rarity)
            ? rarity 
            : "";
        return symbolIndex;
    }
}