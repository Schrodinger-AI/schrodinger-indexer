using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SchrodingerSymbolIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string Symbol { get; set; }
    //no need [Nested]
    [Nested(Name = "Traits", Enabled = true, IncludeInParent = true, IncludeInRoot = true)]
    public List<TraitInfo> Traits { get; set; } = new();
    public SchrodingerInfo SchrodingerInfo { get; set; } = new();
    public long HolderCount { get; set; }
    public long Amount { get; set; }
    public int Rank { get; set; }
    [Keyword] public string Level { get; set; }
    [Keyword] public string Grade { get; set; }
    [Keyword] public string Star{ get; set; }
    [Keyword] public string Rarity { get; set; }
}
