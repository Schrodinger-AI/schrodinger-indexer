using System.Collections.Generic;
using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SchrodingerSymbolIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string Symbol { get; set; }
    //no need [Nested]
    public List<TraitInfo> Traits { get; set; } = new();
    public SchrodingerInfo SchrodingerInfo { get; set; } = new();
    public long HolderCount { get; set; }
}