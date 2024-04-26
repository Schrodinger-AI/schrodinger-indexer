namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetAllSchrodingerListInput
{
    public string ChainId { get; set; }
    public string Tick { get; set; }
    public List<TraitsInput> Traits { get; set; }
    public List<int> Generations { get; set; }
    public List<string> Raritys { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
    public string Keyword { get; set; }
    public bool FilterSgr { get; set; }
}

public class TraitsInput
{
    public string TraitType { get; set; }
    public List<string> Values { get; set; }
}