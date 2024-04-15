using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SchrodingerHolderIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string Address { get; set; }

    [Nested(Name = "Traits", Enabled = true, IncludeInParent = true, IncludeInRoot = true)]
    public List<TraitInfo> Traits { get; set; } = new();

    public SchrodingerInfo SchrodingerInfo { get; set; } = new();
    public long Amount { get; set; }
}

public class TraitInfo
{
    [Keyword] public string TraitType { get; set; }
    [Keyword] public string Value { get; set; }
}

public class SchrodingerInfo
{
    [Text(Index = false)] public string InscriptionImageUri { get; set; }
    public string InscriptionDeploy { get; set; } = "";
    [Keyword] public string Tick { get; set; }
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string TokenName { get; set; }
    public int Decimals { get; set; }
    public int Gen { get; set; }
}