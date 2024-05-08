
namespace Schrodinger.Indexer.Plugin;

public class ContractInfoOptions
{
    public Dictionary<string, ContractInfo> ContractInfos { get; set; }
    
    public List<ContractInfo> SwapContractInfos { get; set; }

    public ContractInfo GetSwapContractInfo(string chainId, int level)
    {
        return SwapContractInfos.First(o => o.ChainId == chainId && o.Level == level);
    }
}
public class ContractInfo
{
    public string ChainId { get; set; }
    public string SchrodingerContractAddress { get; set; }
    public string TokenContractAddress { get; set; }
    
    public string SwapTokenContractAddress { get; set; }

    public string NFTMarketContractAddress { get; set; }
    
    public int Level { get; set; }
}