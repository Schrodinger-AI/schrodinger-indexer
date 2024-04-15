using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SwapLPIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{ 
    [Keyword] public string LPAddress { get; set; }
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string ContractAddress { get; set; }
    
    public int Decimals { get; set; }
    public long Balance { get; set; }
    public DateTime UpdateTime { get; set; }
}