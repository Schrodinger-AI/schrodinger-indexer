using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;

namespace Schrodinger.Indexer.Plugin.Processors.Provider;


public interface IRankProvider
{
    public int GetRank(List<List<string>> traitsGenOne, List<List<string>> traitsGenTwoToNinet);
}

public class RankProvider : IRankProvider, ISingletonDependency
{
    private readonly List<char> _order  = new List<char> { 'A', 'B', 'C', 'E', 'F', 'D', 'G' };
    
    private readonly Dictionary<string, Dictionary<string, double>> _traitsDataV8;
    private readonly Dictionary<string, Dictionary<string, int>> _traitsProbabilityMap;
    private readonly  Dictionary<string, string> _probabilityGenOneToNineMap;
    private readonly List<string> _probabilityRankMap;

    private ILogger<RankProvider> _logger;
    
    
    public RankProvider(ILogger<RankProvider> logger)
    {
        _logger = logger;
        
        string traitsDataV8Content = File.ReadAllText("/app/rankData/TraitDataV8.json");
        _traitsDataV8 = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, double>>>(traitsDataV8Content);
        
        string traitsProbabilityMapContent = File.ReadAllText("/app/rankData/TraitsProbabilityMap.json");
        _traitsProbabilityMap = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(traitsProbabilityMapContent);
        
        string probabilityGenOneToNineMapContent = File.ReadAllText("/app/rankData/ProbabilityMap.json");
        _probabilityGenOneToNineMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(probabilityGenOneToNineMapContent);
        
        string probabilityRankMapContent = File.ReadAllText("/app/rankData/ProbabilityRankMap.json");
        _probabilityRankMap = JsonConvert.DeserializeObject<List<string>>(probabilityRankMapContent);
    }

    public int GetRank(List<List<string>> traitsGenOne, List<List<string>> traitsGenTwoToNine)
    {
        try
        {
            var rankOfGenOneProbabilityTypes = getRankOfGenOne(traitsGenOne);
            var rankTwoToNineProbabilityTypes = getRankTwoToNine(traitsGenTwoToNine);
            var rank = getRankOneToNine(rankOfGenOneProbabilityTypes, rankTwoToNineProbabilityTypes);
            return rank;
        }
        catch (Exception e)
        {
            _logger.LogError("GetRank Error: {err}", e.Message);
            return 0;
        }
    }
    
    
    private List<string> getRankOfGenOne(List<List<string>> traits)
    {
        var probabilityTypes = traits[0].Zip(
            traits[1],
            (type, trait) =>
            {
                var word = GetAGWord(type);
                var level = GetTraitLevel(type, trait);
                return word + level;
            }).ToList();
        
        return probabilityTypes;
    }
    
    private List<string> getRankTwoToNine(List<List<string>> traits)
    {
        var probabilityTypes = traits[0].Zip(
            traits[1],
            (type, trait) =>
            {
                var typeMapped = type;
                if (Constants.Mappings.TwoToNineTypeMap.TryGetValue(type, out var value))
                {
                    typeMapped = value;
                }
                var typeMappedTrimmed = typeMapped.Trim();
                var word = GetAGWord(type);
                var level = GetTraitLevel(typeMappedTrimmed, trait);
                return word + level;
            }).ToList();
        
        probabilityTypes.Sort(SortABCEFDG);
        return probabilityTypes;
    }
    
    public int getRankOneToNine(List<string> rankOfGenOneProbabilityTypes, List<string> rankTwoToNineProbabilityTypes) {
        
        rankOfGenOneProbabilityTypes.AddRange(rankTwoToNineProbabilityTypes);
        var wordType = string.Join("", rankOfGenOneProbabilityTypes);
        var probability = _probabilityGenOneToNineMap[wordType];
        var rankIndex = _probabilityRankMap.IndexOf(probability) + 1;
        return rankIndex;
    }
    
    private  string GetAGWord(string type)
    {
        return Constants.Mappings.TypeABCDEFGMap[type];
    }
    
    private int GetTraitLevel(string type, string trait)
    {
        var traitProbability = _traitsDataV8[type][trait];
        return _traitsProbabilityMap[type][traitProbability.ToString()];
    }
    
    
    private int SortABCEFDG(string a, string b) {
        var indexA = _order.IndexOf(a[0]);
        var indexB = _order.IndexOf(b[0]);
    
         if (indexA < indexB) {
            return -1;
        }
        if (indexA > indexB) {
            return 1;
        }
    
        if (a[1] > b[1]) {
            return 1;
        }
        if (a[1] < b[1]) {
            return -1;
        }
        
        return 0;
    }
    
}