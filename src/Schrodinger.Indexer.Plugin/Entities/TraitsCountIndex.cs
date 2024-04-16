using AElf.Indexing.Elasticsearch;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class TraitsCountIndex : SchrodingerIndexerEntity<string>, IIndexBuild
{
    [Keyword] public string TraitType { get; set; }
    public List<ValueInfo> Values { get; set; }
    
    public long CreateTime { get; set; }
    public long UpdateTime { get; set; }

    public class ValueInfo
    {
        [Keyword] public string Value { get; set; }
        public long Count { get; set; }
    }
}