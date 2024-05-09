using AElf.Indexing.Elasticsearch;
using AElfIndexer.Client;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class NFTActivityIndex : NFTActivityBase, IIndexBuild
{
    public TokenInfoBase PriceTokenInfo { get; set; }
    public decimal Prices { get; set; }
}


public class NFTActivityBase: AElfIndexerClientEntity<string>
{
    [Keyword] public override string Id { get; set; }
    
    [Keyword] public string NftInfoId { get; set; }
    
    public NFTActivityType Type { get; set; }
    
    [Keyword] public string From { get; set; }
    
    [Keyword] public string To { get; set; }
    
    public long Amount { get; set; }
    
    public decimal Price { get; set; }
    
    [Keyword] public string TransactionHash { get; set; }
    
    public DateTime Timestamp { get; set; }
}

public enum NFTActivityType
{
    Issue,
    Burn,
    Transfer,
    Sale,
    ListWithFixedPrice,
    DeList,
    MakeOffer,
    CancelOffer,
    PlaceBid
}