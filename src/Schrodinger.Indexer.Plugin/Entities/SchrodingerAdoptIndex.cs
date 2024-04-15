using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class  SchrodingerAdoptIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string AdoptId { get; set; }
    [Keyword] public string Tick { get; set; }
    public SchrodingerInfo ParentInfo { get; set; } = new();
    [Keyword] public string Parent { get; set; }
    [Keyword] public string Ancestor { get; set; }
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string Issuer { get; set; }
    [Keyword] public string Owner { get; set; }
    [Keyword] public string Deployer { get; set; }
    [Keyword] public string Adopter { get; set; }
    [Keyword] public string TokenName { get; set; }

    [Nested(Name = "Attributes", Enabled = true, IncludeInParent = true, IncludeInRoot = true)]
    public List<Attribute> Attributes { get; set; }

    public Dictionary<string, string> AdoptExternalInfo { get; set; } = new();
    public long InputAmount { get; set; }
    public long LossAmount { get; set; }
    public long CommissionAmount { get; set; }
    public long OutputAmount { get; set; }
    public long ImageCount { get; set; }
    public long TotalSupply { get; set; }
    public int IssueChainId { get; set; }
    public int Gen { get; set; }
    public int ParentGen { get; set; }
    public int Decimals { get; set; }
    public DateTime AdoptTime { get; set; }
    public bool IsConfirmed { get; set; }
    [Text(Index = false)] public string InscriptionImageUri { get; set; }
    [Keyword] public string TransactionId { get; set; }
}