
namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetSchrodingerHolderDailyChangeListInput
{
    public string ChainId { get; set; }
    public string Date { get; set; }
    public int SkipCount { get; set; }
    public int MaxResultCount { get; set; }
    
    public  string Address { get; set; }
    
    public string Symbol { get; set; }
    
    public List<string> ExcludeDate { get; set; }
}