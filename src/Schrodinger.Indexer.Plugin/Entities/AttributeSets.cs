using System.Collections.Generic;
using Nest;

namespace Schrodinger.Indexer.Plugin.Entities;

public class AttributeSets {
    public List<AttributeSet> FixedAttributes { get; set; }
    public List<AttributeSet> RandomAttributes { get; set; }
}

public class AttributeSet {
    public AttributeInfo TraitType { get; set; }
    public AttributeInfos Values { get; set; }
}

public class AttributeInfos {
    public List<AttributeInfo> Data { get; set; }
}

public class AttributeInfo {
    [Keyword] public string Name { get; set; }
    public long Weight { get; set; }
}