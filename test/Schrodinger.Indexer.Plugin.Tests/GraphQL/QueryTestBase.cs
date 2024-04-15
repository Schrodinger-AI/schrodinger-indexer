using Microsoft.Extensions.Options;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Tests.GraphQL;

public class QueryTestBase : SchrodingerIndexerPluginTestBase
{
    protected readonly IObjectMapper ObjectMapper;

    protected QueryTestBase()
    {
        ObjectMapper = GetRequiredService<IObjectMapper>();
    }
}