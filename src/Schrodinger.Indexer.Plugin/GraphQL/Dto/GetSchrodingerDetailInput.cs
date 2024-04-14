namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetSchrodingerDetailInput
{
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string Symbol { get; set; }
}