using AElf.Types;
using AutoMapper;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace Schrodinger.Indexer.Plugin;

public class IndexerMapperBase : Profile
{
    protected static string MapHash(Hash hash)
    {
        return hash?.ToHex();
    }

    protected static string MapAddress(Address address)
    {
        return address?.ToBase58();
    }

    protected static DateTime? MapDateTime(Timestamp timestamp)
    {
        return timestamp?.ToDateTime();
    }

    protected static List<Entities.Attribute> MapAttributes(Confirmed eventValue)
    {
        return MapAttributes(eventValue.Attributes);
    }

    protected static Dictionary<string, string> MapExternalInfo(MapField<string, string> value)
    {
        return value?.ToDictionary(item => item.Key, item => item.Value)
               ?? new Dictionary<string, string>();
    }

    protected static List<Entities.Attribute> AdoptMapAttributes(Adopted eventValue)
    {
        return MapAttributes(eventValue.Attributes);
    }


    protected static List<Entities.Attribute> MapAttributes(Attributes eventValue)
    {
        return eventValue?.Data?.Select(attr => new Entities.Attribute
                { TraitType = attr.TraitType, Value = attr.Value })
            .ToList() ?? new List<Entities.Attribute>();
    }

    protected static string MapIpfsUri(string contractUri)
    {
        try
        {
            return SchrodingerConstants.IpfsHttpsString
                   + contractUri.Split(SchrodingerConstants.IpfsSeparator)[1];
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    protected static string MapInscriptionDeploy(Dictionary<string, string> map)
    {
        return map.TryGetValue(SchrodingerConstants.InscriptionDeploy, out var inscriptionDeploy)
            ? inscriptionDeploy
            : map.TryGetValue(SchrodingerConstants.InscriptionAdopt, out var inscriptionAdopt)
                ? inscriptionAdopt
                : string.Empty;
    }
}