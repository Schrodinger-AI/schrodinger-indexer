namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetAdoptInfoListInput
{
    public string ChainId { get; set; }
    public long FromBlockHeight { get; set; }
    public long ToBlockHeight { get; set; }
}