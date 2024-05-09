namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class NFTActivityPageResultDto
{
    public long TotalRecordCount { get; set; }
    
    public List<NFTActivityDto> Data { get; set; }
}

public class NFTActivityDto
{
    public string NftInfoId { get; set; }
    public int Type { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public long Amount { get; set; }
    public decimal Price { get; set; }
    public string TransactionHash { get; set; }
    public DateTime Timestamp { get; set; }
}