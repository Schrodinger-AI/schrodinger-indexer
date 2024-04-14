namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class SwapLPDailyListDto
{
    public long TotalCount { get; set; }
    public List<SwapLPDailyDto> Data { get; set; }
}

public class SwapLPDailyDto
{
    public string BizDate { get; set; }
    public string LPAddress { get; set; }
    public string Symbol { get; set; }
    public string ContractAddress { get; set; }
    public int Decimals { get; set; }
    public long ChangeAmount { get; set; }
    public long Balance { get; set; }
    public DateTime UpdateTime { get; set; }
}