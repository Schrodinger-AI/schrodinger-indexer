namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;


public class GetTraitsInput
{
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string TraitType { get; set; }
}