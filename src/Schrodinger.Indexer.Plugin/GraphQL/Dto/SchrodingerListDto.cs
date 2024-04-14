using System.Collections.Generic;

namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class SchrodingerListDto
{
    public long TotalCount { get; set; }
    public List<SchrodingerDto> Data { get; set; }
}