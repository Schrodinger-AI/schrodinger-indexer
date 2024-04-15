using AElfIndexer;

namespace Schrodinger.Indexer.Plugin.Entities;

public class GetSyncStateDto
{
    public string ChainId { get; set; }
    public BlockFilterType FilterType { get; set; }
}