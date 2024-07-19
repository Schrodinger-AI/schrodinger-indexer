using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SchrodingerCancelIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string AdoptId { get; set; }
    [Keyword] public string From { get; set; }
    public long Amount { get; set; }
}