using System.Collections.Generic;

namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class AllSchrodingerListDto
{
    public long TotalCount { get; set; }
    public List<AllSchrodingerDto> Data { get; set; }
}