namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetHoldingPointBySymbolInput
{
    public string ChainId { get; set; }
    public string Symbol { get; set; }
}