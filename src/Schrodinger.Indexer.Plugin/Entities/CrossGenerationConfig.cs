using System.Collections.Generic;

namespace Schrodinger.Indexer.Plugin.Entities;

public class CrossGenerationConfig 
{
    public int Gen { get; set; }
    public bool CrossGenerationFixed { get; set; }
    public long CrossGenerationProbability { get; set; }
    public List<long> Weights { get; set; }
    public bool IsWeightEnabled { get; set; }
}