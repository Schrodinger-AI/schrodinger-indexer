
namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetSwapLPDailyListInput
{
    public string ChainId { get; set; }
    public string BizDate { get; set; }
    public string Symbol { get; set; }
    public List<string> LPAddressList { get; set; }
    public string ContractAddress { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
}