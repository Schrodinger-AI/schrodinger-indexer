using Nest;

namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class SchrodingerHolderDailyChangeDto 
{
    public string  Address{ get; set; }
    public string Symbol { get; set; }
    public string Date { get; set; }
    public long ChangeAmount { get; set; }
    public long Balance { get; set; }
}