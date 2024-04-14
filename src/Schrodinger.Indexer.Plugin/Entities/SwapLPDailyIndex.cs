using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SwapLPDailyIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{ 
    [Keyword] public string BizDate { get; set; }
    [Keyword] public string LPAddress { get; set; }
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string ContractAddress { get; set; }
    public int Decimals { get; set; }
    public long ChangeAmount { get; set; }
    public long Balance { get; set; }
    public DateTime UpdateTime { get; set; }
}