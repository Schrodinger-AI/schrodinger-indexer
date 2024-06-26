namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetHoldingRankInput
{
    public int RankNumber { get; set; }
}


public class RankItem
{
    public string Address { get; set; }
    public decimal Amount { get; set; }
    
    public DateTime UpdateTime { get; set; }
}


public class RarityRankItem
{
    public string Address { get; set; }
    public decimal Diamond { get; set; } = 0;
    public decimal Emerald { get; set; } = 0;
    public decimal Platinum { get; set; } = 0;
    public decimal Gold { get; set; } = 0;
    public decimal Silver { get; set; } = 0;
    public decimal Bronze { get; set; } = 0;
    
    public DateTime UpdateTime { get; set; }
}

