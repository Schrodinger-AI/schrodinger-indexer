using System.Collections.Generic;

namespace Schrodinger.Indexer.Plugin;

public class IdGenerateHelper
{
    public static string GetId(params object[] inputs)
    {
        return inputs.JoinAsString("-");
    }
    
    public static string GetTokenInfoId(string chainId, string symbol)
    {
        return GetId(chainId, symbol);
    }
    
    public static string GetSchrodingerHolderDailyChangeId(string chainId, string date, string symbol, string address)
    {
        return GetId(chainId, date,symbol,address);
    }
    
    public static string GetSwapTokenInfoId(string chainId, string symbol, string contractAddress)
    {
        return GetId(chainId, symbol, contractAddress);
    }
    
    public static string GetSwapLPId(string chainId, string symbol, string contractAddress, string address)
    {
        return GetId(chainId, symbol, contractAddress, address);
    }
    
    public static string GetSwapLPDailyId(string chainId, string symbol, string contractAddress, string address, string bizDate)
    {
        return GetId(chainId, symbol, contractAddress, address, bizDate);
    }
}