namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class HomeDataDto
{
    public long SymbolCount { get; set; }
    public long HoldingCount { get; set; }
    public decimal TradeVolume { get; set; }
}