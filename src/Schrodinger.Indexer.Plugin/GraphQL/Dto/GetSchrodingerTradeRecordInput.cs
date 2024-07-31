namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetSchrodingerTradeRecordInput
{
    public string ChainId { get; set; }
    public string Symbol { get; set; }
    public string Buyer { get; set; }
    public DateTime TradeTime { get; set; }
}