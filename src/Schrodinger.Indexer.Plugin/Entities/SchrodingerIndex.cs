using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SchrodingerIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string Tick { get; set; }
    [Keyword] public string Issuer { get; set; }
    [Keyword] public string Owner { get; set; }
    [Keyword] public string Deployer { get; set; }
    [Keyword] public string TransactionId { get; set; }
    [Keyword] public string Ancestor { get; set; }
    [Keyword] public string TokenName { get; set; }
    [Keyword] public string Signatory { get; set; }
    public Dictionary<string, string> CollectionExternalInfo { get; set; } = new();
    public Dictionary<string, string> ExternalInfo { get; set; } = new();
    public Dictionary<long, long> Rule { get; set; } = new();
    public AttributeSets AttributeSets { get; set; } = new();
    public CrossGenerationConfig CrossGenerationConfig { get; set; } = new();
    public long TotalSupply { get; set; }
    public int IssueChainId { get; set; }
    public int MaxGeneration { get; set; }
    public int Decimals { get; set; }
    public bool IsWeightEnabled { get; set; }
    public double LossRate { get; set; }
}