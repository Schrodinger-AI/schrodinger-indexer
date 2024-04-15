using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SchrodingerHolderDailyChangeIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string  Address{ get; set; }
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string Date { get; set; }
    public long ChangeAmount { get; set; }
    public long Balance { get; set; }
}