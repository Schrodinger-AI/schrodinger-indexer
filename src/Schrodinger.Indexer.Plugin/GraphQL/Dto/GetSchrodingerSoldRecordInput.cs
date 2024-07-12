using JetBrains.Annotations;
using Volo.Abp.Application.Dtos;

namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetSchrodingerSoldRecordInput : PagedResultRequestDto
{
    [CanBeNull] public List<int> Types { get; set; }
    public long? TimestampMin { get; set; }
    public string SortType { get; set; }
    
    public string Address { get; set; }
    public string FilterSymbol { get; set; }
    
    public string ChainId { get; set; }
}

public class GetSchrodingerSoldListInput 
{
    public long? TimestampMin { get; set; }
    public long? TimestampMax { get; set; }
    
    public string ChainId { get; set; }
}