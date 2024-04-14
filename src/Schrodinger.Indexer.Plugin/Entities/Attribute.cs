using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class Attribute
{
    [Keyword] public string TraitType { get; set; }
    [Keyword] public string Value { get; set; }
}