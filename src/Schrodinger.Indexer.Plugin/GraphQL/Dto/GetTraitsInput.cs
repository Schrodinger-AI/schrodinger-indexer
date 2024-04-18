namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;


public class GetTraitsInput
{
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string TraitType { get; set; }
}

public class GetAllTraitsInput
{
    public string ChainId { get; set; }
    public string TraitType { get; set; }
}

public class GenerationEnum
{
    public static List<int> Generations = new List<int>
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9
    };
}