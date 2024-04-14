using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SchrodingerResetIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string Ancestor { get; set; }
    [Keyword] public string Recipient { get; set; }
    public long Amount { get; set; }
}