using AElf.Indexing.Elasticsearch;

namespace Schrodinger.Indexer.Plugin.Entities;

public class GenerationCountIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    public int Generation { get; set; }
    public long Count { get; set; }
    
    public long CreateTime { get; set; }
    public long UpdateTime { get; set; }
}