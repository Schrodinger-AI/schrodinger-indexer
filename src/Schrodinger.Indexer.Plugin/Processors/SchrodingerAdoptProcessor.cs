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

public class SchrodingerAdoptProcessor : SchrodingerProcessorBase<Adopted>
{
    private readonly IRankProvider _rankProvider;
    
    public SchrodingerAdoptProcessor(ILogger<SchrodingerProcessorBase<Adopted>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> schrodingerAdoptRepository,
        IRankProvider rankProvider)
        : base(logger, objectMapper, contractInfoOptions, schrodingerHolderRepository, schrodingerRepository,
            schrodingerTraitValueRepository, schrodingerSymbolRepository, schrodingerAdoptRepository)
    {
        _rankProvider = rankProvider;
    }

    protected override async Task HandleEventAsync(Adopted adopted, LogEventContext context)
    {
        var chainId = context.ChainId;
        var symbol = adopted.Symbol;
        var adoptId = adopted.AdoptId?.ToHex();
        var parent = adopted.Parent;
        Logger.LogInformation("[Adopted] start chainId:{chainId} symbol:{symbol}, adoptId:{adoptId}, parent:{parent}", chainId,
            symbol, adoptId, parent);
        try
        {
            var adopt = ObjectMapper.Map<Adopted, SchrodingerAdoptIndex>(adopted);

            adopt.Id = IdGenerateHelper.GetId(chainId, symbol);
            adopt.AdoptTime = context.BlockTime;
         
            adopt.ParentInfo = await GetSchrodingerInfo(chainId, parent);
            // adopt.SchrodingerInfo = await GetSchrodingerInfo(chainId, symbol);
            ObjectMapper.Map(context, adopt);

            if (adopt.Gen == 9)
            {
                var traitsGenOne = new List<List<string>>();
                var traitsGenTwoToNine = new List<List<string>>();
                GetTraitsInput(adopt.Attributes, traitsGenOne, traitsGenTwoToNine);
                var rank = _rankProvider.GetRank(traitsGenOne, traitsGenTwoToNine);
                adopt.AdoptExternalInfo["rank"] = rank.ToString();

                if (LevelConstant.RankLevelGradeDictionary.TryGetValue(rank.ToString(), out var leaveGradeStar))
                {
                    var level = leaveGradeStar.Split(SchrodingerConstants.RankLevelSegment)[0];
                    var rarityValue = LevelConstant.RarityDictionary.TryGetValue(level ?? "", out var rarity)
                        ? rarity : "";
                    adopt.AdoptExternalInfo["rarity"] = rarityValue;
                }
            }

            await SchrodingerAdoptRepository.AddOrUpdateAsync(adopt);
            Logger.LogInformation("[Adopted] end chainId:{chainId} symbol:{symbol}, adoptId:{adoptId}, parent:{parent}", chainId, symbol,
                adoptId, parent);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "[Adopted] Exception chainId:{chainId} symbol:{symbol}, adoptId:{adoptId}, parent:{parent}", chainId,
                symbol,
                adoptId, parent);
            throw;
        }
    }
    
    private void GetTraitsInput(List<Entities.Attribute> traitInfos, List<List<string>> traitsGenOne, List<List<string>> traitsGenTwoToNine)
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
}