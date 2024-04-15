using System;
using AElfIndexer.Client;

namespace Schrodinger.Indexer.Plugin.Entities;

public class SchrodingerIndexerEntity<T> : AElfIndexerClientEntity<T>
{
    public DateTime BlockTime { get; set; }
}