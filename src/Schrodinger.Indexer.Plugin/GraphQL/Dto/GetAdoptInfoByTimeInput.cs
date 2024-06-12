namespace Schrodinger.Indexer.Plugin.GraphQL.Dto;

public class GetAdoptInfoByTimeInput
{
    public long BeginTime { get; set; }
    public long EndTime { get; set; }
}