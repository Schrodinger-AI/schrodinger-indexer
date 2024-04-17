using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains;
using AElfIndexer.Grains.Grain.Client;
using AElfIndexer.Grains.State.Client;
using Elasticsearch.Net;
using GraphQL;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using NUglify.Helpers;
using Orleans;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.GraphQL.Dto;
using Volo.Abp;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.GraphQL;

public partial class Query
{
    [Name("syncState")]
    public static async Task<SyncStateDto> SyncStateAsync(
        [FromServices] IClusterClient clusterClient,
        [FromServices] IAElfIndexerClientInfoProvider clientInfoProvider,
        GetSyncStateDto input)
    {
        var version = clientInfoProvider.GetVersion();
        var clientId = clientInfoProvider.GetClientId();
        var blockStateSetInfoGrain =
            clusterClient.GetGrain<IBlockStateSetInfoGrain>(
                GrainIdHelper.GenerateGrainId("BlockStateSetInfo", clientId, input.ChainId, version));
        var confirmedHeight = await blockStateSetInfoGrain.GetConfirmedBlockHeight(input.FilterType);
        return new SyncStateDto
        {
            ConfirmedBlockHeight = confirmedHeight
        };
    }
    
    [Name("getLatestSchrodingerListAsync")]
    public static async Task<LatestSchrodingerListDto> GetLatestSchrodingerListAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> adoptRepository,
        [FromServices] IObjectMapper objectMapper, GetLatestSchrodingerListInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerAdoptIndex>, QueryContainer>>
        {
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId)),
            q => q.Term(i
                => i.Field(f => f.IsConfirmed).Value(true))
        };

        var blackList = input.BlackList;
        var mustNotQuery = new List<Func<QueryContainerDescriptor<SchrodingerAdoptIndex>, QueryContainer>>();
        if (!blackList.IsNullOrEmpty())
        {
            mustNotQuery.Add(q => q.Terms(i
                => i.Field(f => f.Tick).Terms(blackList)));
        }
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerAdoptIndex> f) =>
            f.Bool(b => b.Must(mustQuery).MustNot(mustNotQuery));
        var result = await adoptRepository.GetListAsync(Filter, skip: input.SkipCount, limit: input.MaxResultCount,
            sortType: SortOrder.Descending, sortExp: o => o.AdoptTime);

        return new LatestSchrodingerListDto
        {
            TotalCount = result.Item1,
            Data = objectMapper.Map<List<SchrodingerAdoptIndex>, List<LatestSchrodingerDto>>(result.Item2)
        };
    }

    [Name("getSchrodingerList")]
    public static async Task<SchrodingerListDto> GetSchrodingerListAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> holderRepository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> adoptRepository,
        [FromServices] IObjectMapper objectMapper,
        GetSchrodingerListInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>
        {
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId)),
            q => q.LongRange(i
                => i.Field(f => f.Amount).GreaterThan(0))
        };
        
        if (input.FilterSgr)
        {
            mustQuery.Add(q => q.LongRange(i
                => i.Field(f => f.SchrodingerInfo.Gen).GreaterThan(0)));
            var mustNotQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>
            {
                q => q.Prefix(i =>
                    i.Field(f => f.SchrodingerInfo.TokenName).Value("SSGGRRCATTT"))
            };
            mustQuery.Add(q => q.Bool(b => b.MustNot(mustNotQuery)));

            var mustNot = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>
            {
                q => q.Term(i =>
                    i.Field(f => f.SchrodingerInfo.TokenName).Value("SGR"))
            };
            mustQuery.Add(q => q.Bool(b => b.MustNot(mustNot)));
        }
        
        if (!string.IsNullOrEmpty(input.Address))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Address).Value(input.Address)));
        }

        if (!string.IsNullOrEmpty(input.Keyword))
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>();
            shouldQuery.Add(q => q.Term(i => i.Field(f => f.SchrodingerInfo.Symbol).Value(input.Keyword)));
            shouldQuery.Add(q => q.Term(i => i.Field(f => f.SchrodingerInfo.TokenName).Value(input.Keyword)));
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        var shouldTraitsQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>();
        
        if (!input.Traits.IsNullOrEmpty())
        {
            foreach (var traitsInput in input.Traits)
            {
                var shouldMustQuery =
                    new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>();
        
                var shouldMushShouldQuery =
                    new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>();
        
                if (!string.IsNullOrEmpty(traitsInput.TraitType) && traitsInput.Values != null && !traitsInput.Values.IsNullOrEmpty())
                {
                    shouldMushShouldQuery.Add(n =>
                        n.Nested(n => n.Path("Traits").Query(q => 
                            q.Term(i => i.Field("Traits.traitType").Value(traitsInput.TraitType)))));
                    shouldMushShouldQuery.Add(n =>
                        n.Nested(n => n.Path("Traits").Query(q => 
                            q.Terms(i => i.Field("Traits.value").Terms(traitsInput.Values)))));
                }
        
                shouldMustQuery.Add(q => q.Bool(b => b.Must(shouldMushShouldQuery)));
        
                shouldTraitsQuery.Add(q => q.Bool(b => b.Should(shouldMustQuery)));
            }
        }
        
        if (!input.Generations.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Terms(i => i.Field(f => f.SchrodingerInfo.Gen).Terms(input.Generations)));
        }
        
        mustQuery.Add(q => q.Bool(b => b.Should(shouldTraitsQuery)));

        QueryContainer Filter(QueryContainerDescriptor<SchrodingerHolderIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await holderRepository.GetListAsync(Filter, skip: input.SkipCount,
            limit: input.MaxResultCount, sortType: SortOrder.Descending, sortExp: o => o.BlockTime);

        
        //query adopt
        var symbolList = result.Item2.Select(x => x.SchrodingerInfo.Symbol).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        var adoptMustQuery = new List<Func<QueryContainerDescriptor<SchrodingerAdoptIndex>, QueryContainer>>
        {
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId)),
            q => q.Terms(f 
                => f.Field(i => i.Symbol).Terms(symbolList))
        };
        QueryContainer AdoptFilter(QueryContainerDescriptor<SchrodingerAdoptIndex> f) =>
            f.Bool(b => b.Must(adoptMustQuery));
        
        var adoptResult = await adoptRepository.GetListAsync(AdoptFilter, skip: input.SkipCount, limit: input.MaxResultCount);
        var adopterDict = adoptResult.Item2
            .GroupBy(x => x.Symbol)
            .ToDictionary(g => g.Key, g => g.First().Adopter);
        var adoptTimeDict = adoptResult.Item2
            .GroupBy(x => x.Symbol)
            .ToDictionary(g => g.Key, g => g.First().AdoptTime);

        var response = new SchrodingerListDto
        {
            TotalCount = result.Item1,
            Data = objectMapper.Map<List<SchrodingerHolderIndex>, List<SchrodingerDto>>(result.Item2)
        };
        
        foreach (var schrodingerDto in response.Data)
        {
            schrodingerDto.AdoptTime = 0;
            schrodingerDto.Adopter = string.Empty;
            if (adoptTimeDict.TryGetValue(schrodingerDto.Symbol, out var adoptTime))
            {
                schrodingerDto.AdoptTime =  DateTimeHelper.ToUnixTimeMilliseconds(adoptTime);
            }
            if (adopterDict.TryGetValue(schrodingerDto.Symbol, out var adopter))
            {
                schrodingerDto.Adopter =  adopter ?? string.Empty;
            }
        }

        return response;
    }

    [Name("getSchrodingerDetail")]
    public static async Task<SchrodingerDetailDto> GetSchrodingerDetailAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> holderRepository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> traitValueRepository,
        [FromServices] IObjectMapper objectMapper, GetSchrodingerDetailInput input)
    {
        var chainId = input.ChainId;
        var symbol = input.Symbol;
        var tick = TokenSymbolHelper.GetTickBySymbol(symbol);
        var holderId = IdGenerateHelper.GetId(chainId, symbol, input.Address);
        var holderIndex = await holderRepository.GetFromBlockStateSetAsync(holderId, chainId);
        if (holderIndex == null || holderIndex.Amount == 0)
        {
            return new SchrodingerDetailDto();
        }
        var traitList = holderIndex.Traits;
        var traitTypeList = traitList.Select(x => x.TraitType).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        var traitTypeValueList = await GetAllIndex(GetTraitTypeValueFilter(tick, traitTypeList), traitValueRepository);

        var traitTypeValueDic = traitTypeValueList.GroupBy(x => x.TraitType)
            .ToDictionary(x => x.Key, x => 
                x.GroupBy(x => x.Value)
                    .ToDictionary(y => y.Key, y =>
                        y.Select(i => i.SchrodingerCount).Sum()));
        var traitListWithPercent = new List<TraitDto>();
        foreach (var trait in traitList)
        {
            var traitValue = trait.Value;
            var traitType = trait.TraitType;
            decimal percent = 1;

            if (traitTypeValueDic.TryGetValue(traitType, out var traitValueDic)
                && traitValueDic.TryGetValue(traitValue, out var numerator))
            {
                var denominator = traitValueDic.Values.Sum();
                if (denominator > 0 && numerator > 0)
                {
                    percent = (Convert.ToDecimal(numerator) / Convert.ToDecimal(denominator)) * 100;
                }
            }

            traitListWithPercent.Add(new TraitDto
            {
                TraitType = traitType,
                Value = traitValue,
                Percent = percent
            });
        }

        var schrodingerDetailDto = objectMapper.Map<SchrodingerHolderIndex, SchrodingerDetailDto>(holderIndex);
        schrodingerDetailDto.Traits = traitListWithPercent;
        return schrodingerDetailDto;
    }

    [Name("getAdoptRate")]
    public static async Task<AdoptRateDto> GetAdoptRateAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetAdoptRateInput input)
    {
        var symbol = input.Symbol;
        var tick = symbol.Split(SchrodingerConstants.Separator)[0];
        var chainId = input.ChainId;
        var id = IdGenerateHelper.GetId(input.ChainId, tick);
        var schrodingerIndex = await repository.GetFromBlockStateSetAsync(id, chainId);
        return new AdoptRateDto
        {
            Rate = schrodingerIndex == null ? 0 : 1 - schrodingerIndex.LossRate
        };
    }

    [Name("getTraits")]
    public static async Task<SchrodingerTraitsDto> GetTraitsAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> holderRepository,
        GetTraitsInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>
        {
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId)),
            q => q.Term(i
                => i.Field(f => f.Address).Value(input.Address)),
            q => q.LongRange(i
                => i.Field(f => f.Amount).GreaterThan(0))
        };

        QueryContainer Filter(QueryContainerDescriptor<SchrodingerHolderIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var list = await GetAllIndex(Filter, holderRepository);
        
        var traitsFilter = list
            .SelectMany(s => s.Traits)
            .GroupBy(t => t.TraitType)
            .Where(g => string.IsNullOrEmpty(input.TraitType) || input.TraitType == g.Key)
            .OrderBy(g => g.Key)
            .Select(g => new SchrodingerTraitsFilterDto
            {
                TraitType = g.Key,
                Amount = list.Count(s => s.Traits.Any(t => t.TraitType == g.Key)),
                Values = g.GroupBy(tv => tv.Value)
                    .OrderBy(tvGroup => tvGroup.Key)
                    .Select(gg => new TraitValueDto
                    {
                        Value = gg.Key,
                        Amount = gg.Count()
                    })
                    .ToList()
            })
            .ToList();

        var generationFilter = list
            .GroupBy(s => s.SchrodingerInfo.Gen)
            .Select(g => new GenerationDto
            {
                // GenerationName = GenerationOrdinalHelper.ConvertToOrdinal(g.Key),
                GenerationName = g.Key,
                GenerationAmount = g.Count()
            }).OrderBy(s => s.GenerationName).ToList();


        return new SchrodingerTraitsDto
        {
            TraitsFilter = traitsFilter,
            GenerationFilter = generationFilter
        };
    }


    [Name("getAllTraits")]
    public static async Task<SchrodingerTraitsDto> GetAllTraitsAsync(
        [FromServices] IAElfIndexerClientEntityRepository<TraitsCountIndex, LogEventInfo> traitValueRepository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        [FromServices] IObjectMapper objectMapper,
        GetAllTraitsInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<TraitsCountIndex>, QueryContainer>>
        {
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId)),
            q => q.LongRange(i
                => i.Field(f => f.Amount).GreaterThan(0))
        };

        if (!input.TraitType.IsNullOrEmpty())
        {
            mustQuery.Add( q => q.Term(i
                => i.Field(f => f.TraitType).Value(input.TraitType)));
        }
        
        QueryContainer Filter(QueryContainerDescriptor<TraitsCountIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await traitValueRepository.GetListAsync(Filter, skip: 0);
        
        List<GenerationDto> generationFilter = new List<GenerationDto>();
        
        var traitsFilter = result.Item2.Select(traits => objectMapper.Map<TraitsCountIndex, SchrodingerTraitsFilterDto>(traits)).ToList();

        for (int i = 1; i <= 9; i++)
        {
            var traitsCountMustQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
            {
                q => q.Term(i
                    => i.Field(f => f.SchrodingerInfo.Gen).Value(i)),
                q => q.LongRange(i
                    => i.Field(f => f.HolderCount).GreaterThan(0))
            };
            QueryContainer traitsCountFilter(QueryContainerDescriptor<SchrodingerSymbolIndex> f) =>
                f.Bool(b => b.Must(traitsCountMustQuery));

            var countResp = await schrodingerSymbolRepository.CountAsync(traitsCountFilter);
            var generation = new GenerationDto
            {
                GenerationName = i,
                GenerationAmount = (int)countResp.Count
            };
            generationFilter.Add(generation);
        }

        return new SchrodingerTraitsDto
        {
            TraitsFilter = traitsFilter,
            GenerationFilter = generationFilter
        };
    }


    [Name("getAdoptInfo")]
    public static async Task<AdoptInfoDto> GetAdoptInfoAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        [FromServices]
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> traitValueRepository,
        GetAdoptInfoInput input)
    {
        if (input == null || string.IsNullOrEmpty(input.AdoptId))
            throw new UserFriendlyException("Invalid input");
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerAdoptIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(f => f.Field(f => f.AdoptId).Value(input.AdoptId)));
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerAdoptIndex> f) => f.Bool(b => b.Must(mustQuery));
        var adopt = await repository.GetAsync(Filter);
        if (adopt == null)
            throw new UserFriendlyException("Adpot not found");

        var tick = TokenSymbolHelper.GetTickBySymbol(adopt.Symbol);
        var traitTypes = adopt.Attributes.Select(a => a.TraitType).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        var traitIndexList = await GetAllIndex(GetTraitTypeValueFilter(tick, traitTypes), traitValueRepository);
        var traitTypeDic = traitIndexList
            .GroupBy(t => t.TraitType)
            .ToDictionary(group => group.Key, group => group.ToList());

        var resp = objectMapper.Map<SchrodingerAdoptIndex, AdoptInfoDto>(adopt);
        resp.Attributes = new List<Trait>();
        foreach (var attr in adopt.Attributes)
        {
            decimal percent = 0;
            if (traitTypeDic.TryGetValue(attr.TraitType, out var traitValues))
            {
                var schrodingerCount =
                    traitValues.Where(t => t.Value == attr.Value)
                        .Select(x => x.SchrodingerCount).Sum();
                decimal totalCount = traitValues.Select(t => t.SchrodingerCount).Sum();
                percent = totalCount == 0 ? 0 : schrodingerCount / totalCount;
            }

            resp.Attributes.Add(new Trait()
            {
                TraitType = attr.TraitType,
                Value = attr.Value,
                Percent = percent * 100
            });
        }

        return resp;
    }
    
    [Name("getStrayCats")]
    public static async Task<StrayCatListDto> GetStrayCatsAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> repository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> symbolRepository,
        [FromServices] IObjectMapper objectMapper,
        StrayCatInput input)
    {
        if (input == null || string.IsNullOrEmpty(input.Adopter))
            throw new UserFriendlyException("Invalid input");
        
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerAdoptIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(f => f.Field(f => f.Adopter).Value(input.Adopter)));
        mustQuery.Add(q => q.Term(f => f.Field(f => f.IsConfirmed).Value(false)));
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerAdoptIndex> f) => f.Bool(b => b.Must(mustQuery));
        
        var result = await repository.GetListAsync(Filter, skip: input.SkipCount,
            limit: input.MaxResultCount, sortType: SortOrder.Descending, sortExp: o => o.BlockTime);
        
        var parentSymbolList = result.Item2.Select(i => i.ParentInfo?.Symbol ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
        var symbolMustQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>();
        symbolMustQuery.Add(q => q.Terms(f => f.Field(f => f.Symbol).Terms(parentSymbolList)));
        symbolMustQuery.Add(q => q.Term(f => f.Field(f => f.ChainId).Value(input.ChainId)));
        QueryContainer SymbolFilter(QueryContainerDescriptor<SchrodingerSymbolIndex> f) => f.Bool(b => b.Must(symbolMustQuery));
        var symbolResult = await symbolRepository.GetListAsync(SymbolFilter, skip: input.SkipCount, limit: input.MaxResultCount);
        
        var symbolDic = symbolResult.Item2
            .GroupBy(x => x.Symbol)
            .ToDictionary(s => s.Key, s => s.First().Traits);
        var list = new List<StrayCatDto>();
        foreach (var schrodingerAdoptIndex in result.Item2)
        {
            var strayCatDto = objectMapper.Map<SchrodingerAdoptIndex, StrayCatDto>(schrodingerAdoptIndex);
            strayCatDto.ParentTraits = new List<StrayCatTraitsDto>();
            if (symbolDic.TryGetValue(schrodingerAdoptIndex.ParentInfo?.Symbol ?? string.Empty, out var parentTraits))
            {
                strayCatDto.ParentTraits = objectMapper.Map<List<TraitInfo>, List<StrayCatTraitsDto>>(parentTraits);
            }

            strayCatDto.NextSymbol = schrodingerAdoptIndex.Symbol;
            strayCatDto.NextTokenName = schrodingerAdoptIndex.TokenName;
            strayCatDto.NextAmount = schrodingerAdoptIndex.OutputAmount;
            list.Add(strayCatDto);
        }
        return new StrayCatListDto
        {
            TotalCount = result.Item1,
            Data = list
        };
    }
    
    [Name("getSchrodingerHolderDailyChangeList")]
    public static async Task<SchrodingerHolderDailyChangeListDto> GetSchrodingerHolderDailyChangeAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerHolderDailyChangeIndex, LogEventInfo> holderDailyChangeRepository,
        [FromServices] IObjectMapper objectMapper,
        GetSchrodingerHolderDailyChangeListInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderDailyChangeIndex>, QueryContainer>>
        {
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId)),
            q => q.Term(i
                => i.Field(f => f.Date).Value(input.Date))
        };

        QueryContainer Filter(QueryContainerDescriptor<SchrodingerHolderDailyChangeIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await holderDailyChangeRepository.GetListAsync(Filter, skip: input.SkipCount,
            limit: input.MaxResultCount, sortType: SortOrder.Ascending, sortExp: o => o.BlockTime);

        return new SchrodingerHolderDailyChangeListDto
        {
            TotalCount = result.Item1,
            Data = objectMapper.Map<List<SchrodingerHolderDailyChangeIndex>, List<SchrodingerHolderDailyChangeDto>>(result.Item2)
        };
    }
    
    [Name("getSchrodingerSymbolList")]
    public static async Task<SchrodingerSymbolListDto> GetSchrodingerSymbolListAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        [FromServices] IObjectMapper objectMapper,
        GetSchrodingerHolderDailyChangeListInput input)
    {

        var result = await schrodingerSymbolRepository.GetListAsync( skip: input.SkipCount,
            limit: input.MaxResultCount, sortType: SortOrder.Ascending, sortExp: o => o.BlockTime);

        return new SchrodingerSymbolListDto
        {
            TotalCount = result.Item1,
            Data = objectMapper.Map<List<SchrodingerSymbolIndex>, List<SchrodingerSymbolDto>>(result.Item2)
        };
    }
    
     [Name("getAdoptInfoList")]
    public static async Task<AdoptInfoListDto> GetAdoptInfoListAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetAdoptInfoListInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerAdoptIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(f => f.Field(f => f.ChainId).Value(input.ChainId)));
        
        if (input.FromBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).GreaterThanOrEquals(input.FromBlockHeight)));
        }
        if (input.ToBlockHeight > 0)
        {
            mustQuery.Add(q => q.Range(i => i.Field(f => f.BlockHeight).LessThanOrEquals(input.ToBlockHeight)));
        }
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerAdoptIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await GetAllIndex(Filter, repository);
        
        return new AdoptInfoListDto
        {
            TransactionIds = result.Select(t => t.TransactionId).ToList()
        };
    }
    
    [Name("getSwapLPDailyList")]
    public static async Task<SwapLPDailyListDto> GetSwapLPDailyListAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SwapLPDailyIndex, LogEventInfo> swapLPDailyRepository,
        [FromServices] IObjectMapper objectMapper,
        GetSwapLPDailyListInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SwapLPDailyIndex>, QueryContainer>>();

        if (!input.ChainId.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(f 
                => f.Field(f => f.ChainId).Value(input.ChainId)));
        }
        
        if (!input.BizDate.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(f 
                => f.Field(f => f.BizDate).Value(input.BizDate)));
        }
        
        if (!input.Symbol.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(f 
                => f.Field(f => f.Symbol).Value(input.Symbol)));
        }

        
        if (!input.LPAddressList.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Terms(f 
                => f.Field(f => f.LPAddress).Terms(input.LPAddressList)));
        }
        
        if (!input.ContractAddress.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(f 
                => f.Field(f => f.ContractAddress).Value(input.ContractAddress)));
        }
        
        QueryContainer Filter(QueryContainerDescriptor<SwapLPDailyIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await swapLPDailyRepository.GetListAsync(Filter, skip: input.SkipCount,
            limit: input.MaxResultCount, sortType: SortOrder.Ascending, sortExp: o => o.BlockTime);

        return new SwapLPDailyListDto
        {
            TotalCount = result.Item1,
            Data = objectMapper.Map<List<SwapLPDailyIndex>, List<SwapLPDailyDto>>(result.Item2)
        };
    }

    private static Func<QueryContainerDescriptor<SchrodingerTraitValueIndex>, QueryContainer> GetTraitTypeValueFilter
        (string tick, List<string> traitTypes)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerTraitValueIndex>, QueryContainer>>
        {
            q => q.Term(f => f.Field(i => i.Tick).Value(tick)),
            q => q.Terms(f => f.Field(i => i.TraitType).Terms(traitTypes))
        };
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerTraitValueIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        return Filter;
    }

    private static async Task<List<T>> GetAllIndex<T>(Func<QueryContainerDescriptor<T>, QueryContainer> filter, 
        IAElfIndexerClientEntityRepository<T, LogEventInfo> repository) 
        where T : AElfIndexerClientEntity<string>, IIndexBuild, new()
    {
        var res = new List<T>();
        List<T> list;
        var skipCount = 0;
        
        do
        {
            list = (await repository.GetListAsync(filterFunc: filter, skip: skipCount, limit: 5000)).Item2;
            var count = list.Count;
            res.AddRange(list);
            if (list.IsNullOrEmpty() || count < 5000)
            {
                break;
            }
            skipCount += count;
        } while (!list.IsNullOrEmpty());

        return res;
    }
}