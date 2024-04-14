using System;
using AElfIndexer.Client.GraphQL;

namespace Schrodinger.Indexer.Plugin.GraphQL;

public class SchrodingerIndexerPluginSchema : AElfIndexerClientSchema<Query>
{
    public SchrodingerIndexerPluginSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}