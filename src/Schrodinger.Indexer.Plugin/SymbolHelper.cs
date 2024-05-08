using System.Text.RegularExpressions;
using AElf;
using Schrodinger.Indexer.Plugin.Processors.Forest;

namespace Schrodinger.Indexer.Plugin;

public static class SymbolHelper
{
    private static readonly string _mainChain = "AELF";

    public static bool CheckSymbolIsNoMainChainNFT(string symbol, string chainId)
    {
        return symbol.Length != 0 && !CheckChainIdIsMain(chainId) &&
               CheckSymbolIsNFT(symbol);
    }
    
    public static bool CheckSymbolIsELF(string symbol)
    {
        return symbol.Length != 0 && symbol.Equals(ForestIndexerConstants.TokenSimpleElf);
    }
    
    public static bool CheckSymbolIsNFT(string symbol)
    {
        return symbol.Length != 0 &&
               Regex.IsMatch(symbol, ForestIndexerConstants.NFTSymbolPattern);
    }
    
    public static bool CheckSymbolIsSeedSymbol(string symbol)
    {
        return symbol.Length != 0 &&
               Regex.IsMatch(symbol, ForestIndexerConstants.SeedSymbolPattern);
    }
    
    public static bool CheckChainIdIsMain(string chainId)
    {
        return chainId.Equals(_mainChain);
    }
}