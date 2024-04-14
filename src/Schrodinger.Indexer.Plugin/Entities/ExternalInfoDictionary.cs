using Nest;

namespace Ewell.Indexer.Plugin.Entities;

public class ExternalInfoDictionary
{
    [Keyword] public string Key { get; set; }
    [Keyword] public string Value { get; set; }
}