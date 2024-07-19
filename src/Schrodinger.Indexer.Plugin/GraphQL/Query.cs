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
        
        var mustNotQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>
        {
            q => q.Prefix(i =>
                i.Field(f => f.SchrodingerInfo.TokenName).Value("SSGGRRCATTT"))
        };
        mustQuery.Add(q => q.Bool(b => b.MustNot(mustNotQuery)));
        
        if (input.FilterSgr)
        {
            mustQuery.Add(q => q.LongRange(i
                => i.Field(f => f.SchrodingerInfo.Gen).GreaterThan(0)));
           
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
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> symbolRepository,
        [FromServices] IObjectMapper objectMapper, GetSchrodingerDetailInput input)
    {
        var chainId = input.ChainId;
        var symbol = input.Symbol;
        var tick = TokenSymbolHelper.GetTickBySymbol(symbol);
        var holderId = IdGenerateHelper.GetId(chainId, symbol, input.Address);
        var holderIndex = await holderRepository.GetFromBlockStateSetAsync(holderId, chainId);
        
        List<TraitInfo> traitList;
        SchrodingerDetailDto schrodingerDetailDto;
        if (holderIndex == null || holderIndex.Amount == 0)
        {
            
            var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
            {
                q => q.Term(i
                    => i.Field(f => f.ChainId).Value(input.ChainId)),
                q => q.Term(i 
                    => i.Field(f => f.SchrodingerInfo.Symbol).Value(input.Symbol))
            };
            
            QueryContainer Filter(QueryContainerDescriptor<SchrodingerSymbolIndex> f) =>
                f.Bool(b => b.Must(mustQuery));

            var result = await symbolRepository.GetAsync(Filter);
            if (result == null)
            {
                return new SchrodingerDetailDto();
            }
            schrodingerDetailDto = objectMapper.Map<SchrodingerSymbolIndex, SchrodingerDetailDto>(result);
            traitList = result.Traits;
        }
        else
        {
            schrodingerDetailDto = objectMapper.Map<SchrodingerHolderIndex, SchrodingerDetailDto>(holderIndex);
            traitList = holderIndex.Traits;
        }
        
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
                Percent = percent,
                IsRare = IsRare(traitType, traitValue)
            });
        }
        
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
        
        if (input.Address.IsNullOrEmpty())
        {
            generationFilter = generationFilter.Where(x => x.GenerationName > 0).ToList();
        }
        
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
        
        for (int i = 0; i < 9; i++)
        {
            int generation = GenerationEnum.Generations[i];
            var traitsCountMustQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
            {
                q => q.Term(i
                    => i.Field(f => f.SchrodingerInfo.Gen).Value(generation)),
                q => q.LongRange(i
                    => i.Field(f => f.HolderCount).GreaterThan(0)),
                q => q.Term(i
                    => i.Field(f => f.ChainId).Value(input.ChainId))
            };
            QueryContainer traitsCountFilter(QueryContainerDescriptor<SchrodingerSymbolIndex> f) =>
                f.Bool(b => b.Must(traitsCountMustQuery));
        
            var countResp = await schrodingerSymbolRepository.CountAsync(traitsCountFilter);
        
            var generationDto = new GenerationDto
            {
                GenerationName = generation,
                GenerationAmount = (int)countResp.Count
            };
            
            generationFilter.Add(generationDto);
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
                Percent = percent * 100,
                IsRare = IsRare(attr.TraitType, attr.Value)
                
            });
        }

        return resp;
    }
    
    [Name("getStrayCats")]
    public static async Task<StrayCatListDto> GetStrayCatsAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> repository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> symbolRepository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerCancelIndex, LogEventInfo> cancelRepository,
        [FromServices] IObjectMapper objectMapper,
        StrayCatInput input)
    {
        if (input == null || string.IsNullOrEmpty(input.Adopter))
            throw new UserFriendlyException("Invalid input");
        
        var cancelMustQuery = new List<Func<QueryContainerDescriptor<SchrodingerCancelIndex>, QueryContainer>>
        {
            q => q.Term(f => f.Field(f => f.From).Value(input.Adopter)),
            q => q.Term(f => f.Field(f => f.ChainId).Value(input.ChainId))
        };
        QueryContainer CancelFilter(QueryContainerDescriptor<SchrodingerCancelIndex> f) => f.Bool(b => b.Must(cancelMustQuery));
        var cancelledAdoptionList = await GetAllIndex(CancelFilter, cancelRepository);
        var cancelledAdoptIdList = cancelledAdoptionList.Select(c => c.AdoptId).ToList();
        
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerAdoptIndex>, QueryContainer>>();
        mustQuery.Add(q => q.Term(f => f.Field(f => f.Adopter).Value(input.Adopter)));
        mustQuery.Add(q => q.Term(f => f.Field(f => f.IsConfirmed).Value(false)));

        if (!cancelledAdoptIdList.IsNullOrEmpty())
        {
            var mustNotQuery = new List<Func<QueryContainerDescriptor<SchrodingerAdoptIndex>, QueryContainer>>
            {
                q => q.Terms(f => f.Field(f => f.AdoptId).Terms(cancelledAdoptIdList))
            };
            mustQuery.Add(q => q.Bool(b => b.MustNot(mustNotQuery)));
        }
       
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
            
            strayCatDto.DirectAdoption = (schrodingerAdoptIndex.Gen - schrodingerAdoptIndex.ParentGen) > 1;
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
                => i.Field(f => f.ChainId).Value(input.ChainId))
        };
        
        if (!input.Date.IsNullOrEmpty())
        {
            mustQuery.Add( q => q.Term(i
                => i.Field(f => f.Date).Value(input.Date)));
        }
        
        if (!input.Address.IsNullOrEmpty())
        {
            mustQuery.Add( q => q.Term(i
                => i.Field(f => f.Address).Value(input.Address)));
        }
        
        if (!input.Symbol.IsNullOrEmpty())
        {
            mustQuery.Add( q => q.Term(i
                => i.Field(f => f.Symbol).Value(input.Symbol)));
        }

        if (!input.ExcludeDate.IsNullOrEmpty())
        {
            var mustNot = new List<Func<QueryContainerDescriptor<SchrodingerHolderDailyChangeIndex>, QueryContainer>>
            {
                q => q.Terms(i =>
                    i.Field(f => f.Date).Terms(input.ExcludeDate))
            };
            mustQuery.Add(q => q.Bool(b => b.MustNot(mustNot)));
        }
        
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
            list = (await repository.GetListAsync(filterFunc: filter, skip: skipCount, limit: 10000)).Item2;
            var count = list.Count;
            res.AddRange(list);
            if (list.IsNullOrEmpty() || count < 10000)
            {
                break;
            }
            skipCount += count;
        } while (!list.IsNullOrEmpty());

        return res;
    }
    
    [Name("getAllSchrodingerList")]
    public static async Task<AllSchrodingerListDto> GetAllSchrodingerListAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> symbolRepository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> adoptRepository,
        [FromServices] IObjectMapper objectMapper,
        GetAllSchrodingerListInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
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
            var mustNotQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
            {
                q => q.Prefix(i =>
                    i.Field(f => f.SchrodingerInfo.TokenName).Value("SSGGRRCATTT"))
            };
            mustQuery.Add(q => q.Bool(b => b.MustNot(mustNotQuery)));

            var mustNot = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
            {
                q => q.Term(i =>
                    i.Field(f => f.SchrodingerInfo.TokenName).Value("SGR"))
            };
            mustQuery.Add(q => q.Bool(b => b.MustNot(mustNot)));
        }
        
        if (!string.IsNullOrEmpty(input.Keyword))
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>();
            shouldQuery.Add(q => q.Term(i => i.Field(f => f.SchrodingerInfo.Symbol).Value(input.Keyword)));
            shouldQuery.Add(q => q.Term(i => i.Field(f => f.SchrodingerInfo.TokenName).Value(input.Keyword)));
            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        var shouldTraitsQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>();
        
        if (!input.Traits.IsNullOrEmpty())
        {
            foreach (var traitsInput in input.Traits)
            {
                var shouldMustQuery =
                    new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>();
        
                var shouldMushShouldQuery =
                    new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>();
        
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
        
        if (!input.Raritys.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Terms(i => i.Field(f => f.Rarity).Terms(input.Raritys)));
        }
        
        mustQuery.Add(q => q.Bool(b => b.Should(shouldTraitsQuery)));

        QueryContainer Filter(QueryContainerDescriptor<SchrodingerSymbolIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await symbolRepository.GetListAsync(Filter, skip: input.SkipCount,
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

        var response = new AllSchrodingerListDto
        {
            TotalCount = result.Item1,
            Data = objectMapper.Map<List<SchrodingerSymbolIndex>, List<AllSchrodingerDto>>(result.Item2)
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
    
    
     [Name("getSchrodingerSoldRecord")]
    public static async Task<NFTActivityPageResultDto> GetSchrodingerSoldRecordAsync(
        [FromServices] IAElfIndexerClientEntityRepository<NFTActivityIndex, LogEventInfo> _nftActivityIndexRepository,
        GetSchrodingerSoldRecordInput input, [FromServices] IObjectMapper objectMapper)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<NFTActivityIndex>, QueryContainer>>();
        
        if (input.Types?.Count > 0)
        {
            mustQuery.Add(q => q.Terms(i => i.Field(f => f.Type).Terms(input.Types)));
        }

        if (input.TimestampMin is > 0)
        {
            mustQuery.Add(q => q.DateRange(i =>
                i.Field(f => f.Timestamp)
                    .GreaterThanOrEquals(DateTime.UnixEpoch.AddMilliseconds((double)input.TimestampMin))));
        }
        
        if (!input.FilterSymbol.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Regexp(i => i.Field(f => f.NftInfoId).Value(".*"+input.FilterSymbol+".*")));
        }
        
        if (!input.Address.IsNullOrEmpty())
        {
            mustQuery.Add(q => q.Term(i => i.Field(f => f.From).Value(input.Address)));
        }

        QueryContainer Filter(QueryContainerDescriptor<NFTActivityIndex> f) => f.Bool(b => b.Must(mustQuery));

        var list = await _nftActivityIndexRepository.GetSortListAsync(Filter, limit: input.MaxResultCount,
            skip: input.SkipCount, sortFunc: GetSortForNFTActivityIndexs(input.SortType));
        var dataList = objectMapper.Map<List<NFTActivityIndex>, List<NFTActivityDto>>(list.Item2);
        return new NFTActivityPageResultDto
        {
            Data = dataList,
            TotalRecordCount = list.Item1
        };
    }
    
    private static Func<SortDescriptor<NFTActivityIndex>, IPromise<IList<ISort>>> GetSortForNFTActivityIndexs(string sortType)
    {
        SortDescriptor<NFTActivityIndex> sortDescriptor = new SortDescriptor<NFTActivityIndex>();
        if (sortType.IsNullOrEmpty() || sortType.Equals("DESC"))
        {
            sortDescriptor.Descending(a=>a.Timestamp);
        }else
        {
            sortDescriptor.Ascending(a=>a.Timestamp);
        }

        IPromise<IList<ISort>> promise = sortDescriptor;
        return s => promise;
    }

    [Name("getSchrodingerRank")]
    public static async Task<SchrodingerRankDto> GetSchrodingerRankAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> symbolRepository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> adoptRepository,
        [FromServices] IObjectMapper objectMapper,
        GetSchrodingerRankInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
        {
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId)),
            q => q.Term(i
                => i.Field(f => f.Symbol).Value(input.Symbol))
        };

        QueryContainer Filter(QueryContainerDescriptor<SchrodingerSymbolIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await symbolRepository.GetAsync(Filter);
        if (result == null)
        {
            return new SchrodingerRankDto();
        }

        var resp = objectMapper.Map<SchrodingerSymbolIndex, SchrodingerRankDto>(result);
        resp.TokenName = result.SchrodingerInfo.TokenName;
        resp.InscriptionImageUri = result.SchrodingerInfo.InscriptionImageUri;
        resp.Generation = result.SchrodingerInfo.Gen;
        return resp;
    }
    
    [Name("getAdoptInfoByTime")]
    public static async Task<List<AdoptInfoDto>> GetAdoptInfoByTimeAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> repository,
        [FromServices] IObjectMapper objectMapper,
        GetAdoptInfoByTimeInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerAdoptIndex>, QueryContainer>>();
        
        if (input.BeginTime > 0)
        {
            mustQuery.Add(q => q.DateRange(i =>
                i.Field(f => f.AdoptTime)
                    .GreaterThanOrEquals(DateTime.UnixEpoch.AddSeconds(input.BeginTime))));
        }
        if (input.EndTime > 0)
        {
            mustQuery.Add(q => q.DateRange(i =>
                i.Field(f => f.AdoptTime)
                    .LessThan(DateTime.UnixEpoch.AddSeconds(input.EndTime))));
        }
        
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerAdoptIndex> f) => f.Bool(b => b.Must(mustQuery));

        var result = await GetAllIndex(Filter, repository);
        
        return  objectMapper.Map<List<SchrodingerAdoptIndex>, List<AdoptInfoDto>>(result);
    }

    private static bool IsRare(string traitType, string traitValue)
    {
        var rareType = new List<string>
        {
            "Background",
            "Clothes",
            "Breed",
            "Hat",
            "Eyes",
            "Pet",
            "Mouth",
            "Face",
            "Necklace",
            "Paw",
            "Trousers",
            "Belt",
            "Shoes",
            "Mustache",
            "Wings",
            "Tail",
            "Ride",
            "Weapon",
            "Accessory"
        };

        string traitsProbabilityMapContent = File.ReadAllText("/app/rankData/TraitDataV8.json");
        var traitsProbabilityMap =
            JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, double>>>(
                traitsProbabilityMapContent);
        var traitsProbabilityList = traitsProbabilityMap.ToDictionary(x => x.Key,
            x => x.Value.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).Take(10).ToList());

        if (!rareType.Contains(traitType))
        {
            return false;
        }

        var rareValueInType = traitsProbabilityList[traitType];
        return rareValueInType.Contains(traitValue);
    }
    
    
    [Name("getHoldingRank")]
    public static async Task<List<RankItem>> GetHoldingRankAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> repository,
        GetHoldingRankInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>
        {
            q => q.LongRange(i
                => i.Field(f => f.Amount).GreaterThan(0)),
            q => q.LongRange(i
            => i.Field(f => f.SchrodingerInfo.Gen).GreaterThan(0)),
            q => q.Regexp(i => 
                i.Field(f => f.SchrodingerInfo.Symbol).Value("SGR-.*"))
        };
        
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerHolderIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var holderList = await GetAllIndex(Filter, repository);

        var holderRankList = holderList
            .GroupBy(x => x.Address)
            .Select(x => new RankItem
            {
                Address = x.Key,
                Amount = x.Sum(y => y.Amount) / (decimal)Math.Pow(10, 8),
                UpdateTime = x.Max(y => y.BlockTime)
            }).ToList();
        
        holderRankList.Sort((item1, item2) =>
        { 
            int scoreComparison = item2.Amount.CompareTo(item1.Amount);
            if (scoreComparison != 0)
            {
                return scoreComparison;
            }

            int timeComparison = item1.UpdateTime.CompareTo(item2.UpdateTime);
            return timeComparison;
        });

        return holderRankList.Take(input.RankNumber).ToList();
    }
    
    
    [Name("getRarityRank")]
    public static async Task<List<RarityRankItem>> GetRarityRankAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> holderIndexRepository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> symbolIndexRepository,
        GetHoldingRankInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>
        {
            q => q.LongRange(i
                => i.Field(f => f.Amount).GreaterThan(0)),
            q => q.Term(i =>
                i.Field(f => f.SchrodingerInfo.Gen).Value(9)),
            q => q.Regexp(i => 
                i.Field(f => f.SchrodingerInfo.Symbol).Value("SGR-.*"))
        };
        
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerHolderIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var holderList = await GetAllIndex(Filter, holderIndexRepository);
  
        var mustQueryOfSymbolIndex = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
        {
            q => q.Terms(i
                => i.Field(f => f.Rarity).Terms(LevelConstant.RarityList))
        };
        QueryContainer FilterOfSymbolIndex(QueryContainerDescriptor<SchrodingerSymbolIndex> f) =>
            f.Bool(b => b.Must(mustQueryOfSymbolIndex));
        var symbolList = await GetAllIndex(FilterOfSymbolIndex, symbolIndexRepository);
        var rarityDict = symbolList.GroupBy(x => x.Rarity).ToDictionary(g => g.Key, g => g.Select(x => x.Symbol).ToList());
        
        var rarityRankItemDict = new Dictionary<string, RarityRankItem>();
        foreach (var holderInfo in holderList)
        {
            var symbol = holderInfo.SchrodingerInfo.Symbol;
            var rarity = GetRarity(rarityDict, symbol);
            if (rarity.IsNullOrEmpty())
            {
                continue;
            }
            
            var address = holderInfo.Address;
            if (rarityRankItemDict.TryGetValue(address, out var rarityRankItem))
            {
                rarityRankItemDict[address] = SetRarityRankItem(rarity, holderInfo.Amount, holderInfo.BlockTime, rarityRankItem);
            }
            else
            {
                rarityRankItem = new RarityRankItem
                {
                    Address = address
                };
                rarityRankItemDict[address] = SetRarityRankItem(rarity, holderInfo.Amount, holderInfo.BlockTime, rarityRankItem);
            }
        }

        var rarityRankItemList = rarityRankItemDict.Select(x => x.Value).ToList();
        rarityRankItemList.Sort((item1, item2) =>
        {
            int diamondComparison = item2.Diamond.CompareTo(item1.Diamond);
            if (diamondComparison != 0)
            {
                return diamondComparison;
            }
            
            int emeraldComparison = item2.Emerald.CompareTo(item1.Emerald);
            if (emeraldComparison != 0)
            {
                return emeraldComparison;
            }
            
            int platinumComparison = item2.Platinum.CompareTo(item1.Platinum);
            if (platinumComparison != 0)
            {
                return platinumComparison;
            }
            
            int goldComparison = item2.Gold.CompareTo(item1.Gold);
            if (goldComparison != 0)
            {
                return goldComparison;
            }
            
            int silverComparison = item2.Silver.CompareTo(item1.Silver);
            if (silverComparison != 0)
            {
                return silverComparison;
            }
            
            int bronzeComparison = item2.Bronze.CompareTo(item1.Bronze);
            if (bronzeComparison != 0)
            {
                return bronzeComparison;
            }
            
            int timeComparison = item1.UpdateTime.CompareTo(item2.UpdateTime);
            return timeComparison;
        });
        
        return rarityRankItemList.Take(input.RankNumber).ToList();
    }

    private static string GetRarity(Dictionary<string, List<string>> rarityDict, string symbol)
    {
        foreach (var item in rarityDict)
        {
            if (item.Value.Contains(symbol))
            {
                return item.Key;
            }
        }

        return "";
    }

    private static RarityRankItem SetRarityRankItem(string rarity, long amount, DateTime updateTime, RarityRankItem rarityRankItem)
    {
        if (updateTime > rarityRankItem.UpdateTime)
        {
            rarityRankItem.UpdateTime = updateTime;
        }
        
        var decimals = (decimal)Math.Pow(10, 8);
        switch (rarity)
        {
            case "Diamond":
                rarityRankItem.Diamond += amount/decimals;
                break;
            case "Gold":
                rarityRankItem.Gold += amount/decimals;
                break;
            case "Silver":
                rarityRankItem.Silver += amount/decimals;
                break;
            case "Bronze":
                rarityRankItem.Bronze += amount/decimals;
                break;
            case "Emerald":
                rarityRankItem.Emerald += amount/decimals;
                break;
            case "Platinum":
                rarityRankItem.Platinum += amount/decimals;
                break;
        }
        return rarityRankItem;
    }

    [Name("getHomeData")]
    public static async Task<HomeDataDto> GetHomeDataAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> holderIndexRepository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> symbolIndexRepository,
        [FromServices] IAElfIndexerClientEntityRepository<NFTActivityIndex, LogEventInfo> nftActivityIndexRepository,
        GetHomeDataInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
        {
            q => q.LongRange(i
                => i.Field(f => f.Amount).GreaterThanOrEquals(100000000)),
            q => q.LongRange(i
                => i.Field(f => f.SchrodingerInfo.Gen).GreaterThan(0)),
            q => q.Regexp(i => 
                i.Field(f => f.SchrodingerInfo.Symbol).Value("SGR-.*")),
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId))
        };
        
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerSymbolIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var schrodingerSymbolCountRes = await symbolIndexRepository.CountAsync(Filter);


        var mustQuery2 = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>
        {
            q => q.LongRange(i
                => i.Field(f => f.Amount).GreaterThan(0)),
            q => q.LongRange(i
                => i.Field(f => f.SchrodingerInfo.Gen).GreaterThan(0)),
            q => q.Regexp(i => 
            i.Field(f => f.SchrodingerInfo.Symbol).Value("SGR-.*")),
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId))
        };

        QueryContainer Filter2(QueryContainerDescriptor<SchrodingerHolderIndex> f) =>
            f.Bool(b => b.Must(mustQuery2));
        
        var holderList = await GetAllIndex(Filter2, holderIndexRepository);
        var uniqueHolderCnt = holderList.GroupBy(i => i.Address).Select(i => i.Key).Count();
        
        
        var mustQuery3 = new List<Func<QueryContainerDescriptor<NFTActivityIndex>, QueryContainer>>()
        {
            q => q.Term(i => i.Field(f => f.Type).Value(NFTActivityType.Sale)),
            q => q.Regexp(i => i.Field(f => f.NftInfoId).Value(input.ChainId + "-SGR-.*"))
        };
        QueryContainer Filter3(QueryContainerDescriptor<NFTActivityIndex> f) => f.Bool(b => b.Must(mustQuery3));

        var totalTradeList = await GetAllIndex(Filter3, nftActivityIndexRepository);
        var totalTradeVolume = totalTradeList.Sum(i => i.Price * i.Amount);
        var tradeVolumeData = Math.Round(totalTradeVolume, 1);

        return new HomeDataDto
        {
            SymbolCount = schrodingerSymbolCountRes.Count,
            HoldingCount = uniqueHolderCnt,
            TradeVolume = tradeVolumeData
        };
    }

    [Name("getSchrodingerSoldList")]
    public static async Task<List<NFTActivityDto>> GetSchrodingerSoldListAsync(
        [FromServices] IAElfIndexerClientEntityRepository<NFTActivityIndex, LogEventInfo> _nftActivityIndexRepository,
        GetSchrodingerSoldListInput input, [FromServices] IObjectMapper objectMapper)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<NFTActivityIndex>, QueryContainer>>();
        
        mustQuery.Add(q => q.Term(i => i.Field(f => f.Type).Value(NFTActivityType.Sale)));

        if (input.TimestampMin  > 0)
        {
            mustQuery.Add(q => q.DateRange(i =>
                i.Field(f => f.Timestamp)
                    .GreaterThanOrEquals(DateTime.UnixEpoch.AddMilliseconds((double)input.TimestampMin))));
        }
        
        if (input.TimestampMax  > 0)
        {
            mustQuery.Add(q => q.DateRange(i =>
                i.Field(f => f.Timestamp)
                    .LessThan(DateTime.UnixEpoch.AddMilliseconds((double)input.TimestampMax))));
        }
        
        mustQuery.Add(q => q.Regexp(i => i.Field(f => f.NftInfoId).Value(input.ChainId + "-SGR-.*")));

        var baseTokenList = new List<string>
        {
            "tDVW-SGR-1",
            "tDVV-SGR-1"
        };
        var mustNotQuery = new List<Func<QueryContainerDescriptor<NFTActivityIndex>, QueryContainer>>
        {
            q => q.Terms(i =>
                i.Field(f => f.NftInfoId).Terms(baseTokenList))
        };
        
        mustQuery.Add(q => q.Bool(b => b.MustNot(mustNotQuery)));

        QueryContainer Filter(QueryContainerDescriptor<NFTActivityIndex> f) => f.Bool(b => b.Must(mustQuery));
        
        var nftSoldList = await GetAllIndex(Filter, _nftActivityIndexRepository);
        nftSoldList.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        return objectMapper.Map<List<NFTActivityIndex>, List<NFTActivityDto>>(nftSoldList);
    }
    
    
    [Name("getHoldingPointBySymbol")]
    public static async Task<HoldingPointBySymbolDto> GetHoldingPointBySymbol(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> symbolRepository,
        [FromServices] IObjectMapper objectMapper, GetSchrodingerDetailInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerSymbolIndex>, QueryContainer>>
        {
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId)),
            q => q.Term(i 
                => i.Field(f => f.SchrodingerInfo.Symbol).Value(input.Symbol))
        };
            
        QueryContainer Filter(QueryContainerDescriptor<SchrodingerSymbolIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await symbolRepository.GetAsync(Filter);
        if (result == null)
        {
            return new HoldingPointBySymbolDto();
        }

        var level = result.Level;
        var res = new HoldingPointBySymbolDto
        {
            Level = level,
            Point = 9
        };

        if (!level.IsNullOrEmpty() && LevelConstant.LevelPointDictionary.TryGetValue(level, out var pointOfLevel))
        {
            res.Point = pointOfLevel;
        }
        
        return res;
    }
    
    
    [Name("getSchrodingerHoldingList")]
    public static async Task<SchrodingerListDto> GetSchrodingerHoldingListAsync(
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> holderRepository,
        [FromServices] IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> adoptRepository,
        [FromServices] IObjectMapper objectMapper,
        GetSchrodingerHoldingListInput input)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<SchrodingerHolderIndex>, QueryContainer>>
        {
            q => q.Term(i
                => i.Field(f => f.ChainId).Value(input.ChainId)),
            q => q.LongRange(i
                => i.Field(f => f.Amount).GreaterThanOrEquals(100000000)),
            q => q.Regexp(i => 
                i.Field(f => f.SchrodingerInfo.Symbol).Value("SGR-.*")),
            q => q.LongRange(i
                => i.Field(f => f.SchrodingerInfo.Gen).GreaterThan(0))
        };
        
        if (!string.IsNullOrEmpty(input.Address))
        {
            mustQuery.Add(q => q.Term(i
                => i.Field(f => f.Address).Value(input.Address)));
        }

        QueryContainer Filter(QueryContainerDescriptor<SchrodingerHolderIndex> f) =>
            f.Bool(b => b.Must(mustQuery));

        var result = await holderRepository.GetListAsync(Filter, skip: input.SkipCount,
            limit: input.MaxResultCount, sortType: SortOrder.Descending, sortExp: o => o.BlockTime);
        
        
        var response = new SchrodingerListDto
        {
            TotalCount = result.Item1,
            Data = objectMapper.Map<List<SchrodingerHolderIndex>, List<SchrodingerDto>>(result.Item2)
        };

        return response;
    }
}