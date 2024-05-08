using Schrodinger.Indexer.Plugin.Processors.Forest;

namespace Schrodinger.Indexer.Plugin;

public class TokenHelper
{
    public static long GetIntegerDivision(long number, int decimals)
    {
        if (decimals == ForestIndexerConstants.IntZero || number == ForestIndexerConstants.IntZero)
        {
            return number;
        }

        var divisor = (long)Math.Pow(ForestIndexerConstants.IntTen, decimals);
        return number / divisor;
    }
    
    public static int GetDecimal(string symbol)
    {
        if (symbol == "ELF")
        {
            return 8;
        } 
        
        if (symbol.StartsWith("SGR"))
        {
            return 8;
        }

        return 8;
    }
}

public enum TokenType
{
    FT,
    NFT
}