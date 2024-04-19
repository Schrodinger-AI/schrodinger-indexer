namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class AllSchrodingerDto
{
    public string Symbol { get; set; }
    public string TokenName { get; set; }
    public string InscriptionImageUri { get; set; }
    public long Amount { get; set; }
    public int Generation { get; set; }
    public int Decimals { get; set; }
    public string InscriptionDeploy { get; set; }
    public string Adopter { get; set; }
    public long AdoptTime { get; set; }
    public int Rank { get; set; }
    public string Level { get; set; }
    public string Grade { get; set; }
    public string Star { get; set; }
    public List<TraitInfo> Traits { get; set; }
    public string Rarity { get; set; }

    public class TraitInfo
    {
        public string TraitType { get; set; }
        public string Value { get; set; }
    }
}