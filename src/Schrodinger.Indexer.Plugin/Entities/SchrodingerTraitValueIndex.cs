using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SchrodingerTraitValueIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string Tick { get; set; }
    [Keyword] public string TraitType { get; set; } 
    [Keyword] public string Value { get; set; } 
    public long SchrodingerCount { get; set; }
}