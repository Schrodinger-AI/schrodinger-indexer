using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Schrodinger.Indexer.TestBase;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Schrodinger.Indexer.Orleans.TestBase;

[DependsOn(typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(SchrodingerIndexerTestBaseModule)
)]
public class SchrodingerIndexerOrleansTestBaseModule : AbpModule
{
    private ClusterFixture _fixture;

    public override void ConfigureServices(ServiceConfigurationContext context)
    { 
        var _fixture = new ClusterFixture();
        context.Services.AddSingleton<ClusterFixture>(_fixture);
        context.Services.AddSingleton<IClusterClient>(sp => _fixture.Cluster.Client);
    }
}