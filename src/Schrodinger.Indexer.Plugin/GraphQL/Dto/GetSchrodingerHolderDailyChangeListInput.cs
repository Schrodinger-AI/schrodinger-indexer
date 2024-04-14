
namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetSchrodingerHolderDailyChangeListInput
{
    public string ChainId { get; set; }
    public string Date { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
}