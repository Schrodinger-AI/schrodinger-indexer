using System.Collections.Generic;

namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class SchrodingerDetailDto
{
    public string Symbol { get; set; }
    public string TokenName { get; set; }
    public string InscriptionImageUri { get; set; }
    public long Amount { get; set; }
    public int Generation { get; set; }
    public int Decimals { get; set; }
    public string Address { get; set; }
    public List<TraitDto> Traits { get; set; }
}