using System;
using AElf.Contracts.MultiToken;
using Newtonsoft.Json;
using Schrodinger.Indexer.Plugin.Entities;

namespace Schrodinger.Indexer.Plugin;

public class TokenSymbolHelper
{
    public static int GetGenByTokenName(string tokenName)
    {
        try
        {
            return Convert.ToInt32(tokenName
                .Substring(tokenName.IndexOf(SchrodingerConstants.Gen, StringComparison.Ordinal))
                .Replace(SchrodingerConstants.Gen, string.Empty)
                );
        }
        catch (Exception)
        {
            return 0;
        }
    }
    
    public static string GetTickBySymbol(string symbol)
    {
        try
        {
            return symbol.Split(SchrodingerConstants.Separator)[0];
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
    
    public static bool GetIsCollectionFromSymbol(string symbol)
    {
        try
        {
            return Convert.ToInt32(symbol.Split(SchrodingerConstants.Separator)[1]) == 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool GetIsGen0FromSymbol(string symbol)
    {
        try
        {
            return Convert.ToInt32(symbol.Split(SchrodingerConstants.Separator)[1]) == 1;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public static bool GetIsGen9FromSchrodingerSymbolIndex(SchrodingerSymbolIndex symbolIndex)
    {
        if (symbolIndex == null || symbolIndex.SchrodingerInfo == null) return false;
        try
        {
            return symbolIndex.SchrodingerInfo.Gen == SchrodingerConstants.Gen9;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public static SchrodingerInfo OfSchrodingerInfo(SchrodingerIndex schrodingerIndex, TokenCreated eventValue, string symbol, string tokenName)
    {
        var externalInfoValue = eventValue.ExternalInfo.Value;
        return new SchrodingerInfo
        {
            InscriptionDeploy = externalInfoValue.TryGetValue(SchrodingerConstants.InscriptionDeploy, out var inscriptionDeploy)
                ? inscriptionDeploy
                : externalInfoValue.TryGetValue(SchrodingerConstants.InscriptionAdopt, out var inscriptionAdopt)
                    ? inscriptionAdopt
                    : string.Empty,
            InscriptionImageUri = eventValue.ExternalInfo.Value.TryGetValue(SchrodingerConstants.InscriptionImageUri, out var inscriptionImageUri) 
                ? inscriptionImageUri 
                : eventValue.ExternalInfo.Value.TryGetValue(SchrodingerConstants.InscriptionImageKey, out var inscriptionImage) 
                    ? inscriptionImage 
                    : string.Empty,
            Tick = GetTickBySymbol(symbol),
            Symbol = symbol,
            TokenName = tokenName,
            Decimals = schrodingerIndex.Decimals,
            Gen = GetGenByTokenName(tokenName)
        };
    }
}