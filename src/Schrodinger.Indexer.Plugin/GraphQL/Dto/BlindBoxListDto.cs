namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class BlindBoxListDto
{
    public long TotalCount { get; set; }
    public List<BlindBoxDto> Data { get; set; }
}

public class BlindBoxDto
{
    public string AdoptId { get; set; }
    public string TokenName { get; set; }
    public int Gen { get; set; }
    public string Symbol { get; set; }
    public long Amount { get; set; }
    public string Rarity { get; set; }
    public string Adopter { get; set; }
    public long AdoptTime { get; set; }
    public int Decimals { get; set; }
    public int Rank { get; set; }
    public long ConsumeAmount { get; set; }
    public bool DirectAdoption { get; set; }
    public List<TraitDto> Traits { get; set; }
}