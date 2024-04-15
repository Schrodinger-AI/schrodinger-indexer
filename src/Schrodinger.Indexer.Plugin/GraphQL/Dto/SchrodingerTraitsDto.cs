using System.Collections.Generic;

namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class SchrodingerTraitsDto
{
    public List<SchrodingerTraitsFilterDto> TraitsFilter { get; set; }
    public List<GenerationDto> GenerationFilter { get; set; }
}
public class SchrodingerTraitsFilterDto
{
    public string TraitType { get; set; }
    public long Amount { get; set; }
    public List<TraitValueDto> Values { get; set; }
}

public class TraitValueDto
{
    public string Value { get; set; }
    public long Amount { get; set; }
}

public class GenerationDto
{
    public int GenerationName { get; set; }
    public int GenerationAmount { get; set; }
}