using System.Collections.Generic;

namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class LatestSchrodingerListDto
{
    public long TotalCount { get; set; }
    public List<LatestSchrodingerDto> Data { get; set; }
}

public class LatestSchrodingerDto
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
    public string Tick { get; set; }
    public List<TraitInfos> Traits { get; set; }
}
public class TraitInfos
{
    public string TraitType { get; set; }
    public string Value { get; set; }
}