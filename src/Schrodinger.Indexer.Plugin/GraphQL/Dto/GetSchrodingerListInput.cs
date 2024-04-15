namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetSchrodingerListInput
{
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string Tick { get; set; }
    public List<TraitInput> Traits { get; set; }
    public List<int> Generations { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
    public string Keyword { get; set; }
    public bool FilterSgr { get; set; }
}

public class TraitInput
{
    public string TraitType { get; set; }
    public List<string> Values { get; set; }
}