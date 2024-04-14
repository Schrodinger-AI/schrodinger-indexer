namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetLatestSchrodingerListInput
{
    public string ChainId { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
    public List<string> BlackList { get; set; }
}