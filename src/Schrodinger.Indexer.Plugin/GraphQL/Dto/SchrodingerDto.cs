using Schrodinger.Indexer.Plugin.Entities;

namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class SchrodingerDto
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
    public string Address { get; set; }
    
    public List<TraitsInfo> Traits { get; set; }
    public class TraitsInfo
    {
        public string TraitType { get; set; }
        public string Value { get; set; }
    }
}