using Orleans.TestingHost;
using Schrodinger.Indexer.TestBase;
using Volo.Abp.Modularity;

namespace Schrodinger.Indexer.Orleans.TestBase;

public abstract class SchrodingerIndexerOrleansTestBase<TStartupModule> : SchrodingerIndexerTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected readonly TestCluster Cluster;

    public SchrodingerIndexerOrleansTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}