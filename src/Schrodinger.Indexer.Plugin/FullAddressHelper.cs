namespace Schrodinger.Indexer.Plugin;

public class FullAddressHelper
{
    private const string FullAddressPrefix = "ELF";
    private const char FullAddressSeparator = '_';
    
    public static string ToFullAddress(string address, string chainId)
    {
        if (address.IsNullOrEmpty() || chainId.IsNullOrEmpty())
            return address; 
        var parts = address.Split(FullAddressSeparator);
        if (parts.Length < 3)
            return string.Join(FullAddressSeparator, FullAddressPrefix, parts[parts.Length - 1], chainId);

        if (address.EndsWith(chainId))
            return address;
        
        return  string.Join(FullAddressSeparator, parts[0], parts[1], chainId);
    }
}