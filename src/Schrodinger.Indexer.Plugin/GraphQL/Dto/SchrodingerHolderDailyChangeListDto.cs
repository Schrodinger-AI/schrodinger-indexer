
namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class SchrodingerHolderDailyChangeListDto
{
    public long TotalCount { get; set; }
    public List<SchrodingerHolderDailyChangeDto> Data { get; set; }
}