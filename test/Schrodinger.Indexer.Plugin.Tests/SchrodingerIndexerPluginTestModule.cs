using System.Reflection;
using AElf.Indexing.Elasticsearch;
using AElf.Indexing.Elasticsearch.Options;
using AElf.Indexing.Elasticsearch.Services;
using AElfIndexer.BlockScan;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Client.Providers;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Schrodinger.Indexer.Orleans.TestBase;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace Schrodinger.Indexer.Plugin.Tests;

[DependsOn(
    typeof(SchrodingerIndexerOrleansTestBaseModule),
    typeof(SchrodingerIndexerPluginModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpAutofacModule),
    typeof(AElfIndexingElasticsearchModule))]
public class SchrodingerIndexerPluginTestModule : AbpModule
{
    private string ClientId { get; } = "TestSchrodingerClient";
    private string Version { get; } = "TestSchrodingerVersion";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var mockEventbus = new Mock<IDistributedEventBus>();
        mockEventbus.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        context.Services.AddSingleton(mockEventbus.Object);
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<SchrodingerIndexerPluginTestModule>(); });
        context.Services.AddSingleton<IAElfIndexerClientInfoProvider, AElfIndexerClientInfoProvider>();
        context.Services.AddSingleton<ISubscribedBlockHandler, SubscribedBlockHandler>();
        context.Services.AddTransient<IBlockChainDataHandler, LogEventDataHandler>();
        context.Services.AddTransient(typeof(IAElfIndexerClientEntityRepository<,>),
            typeof(AElfIndexerClientEntityRepository<,>));
        context.Services.AddSingleton(typeof(IBlockStateSetProvider<>), typeof(BlockStateSetProvider<>));
        context.Services.AddSingleton<IAElfClientProvider, AElfClientProvider>();
        context.Services.AddSingleton(mockEventbus.Object);

        context.Services.Configure<NodeOptions>(o =>
        {
            o.NodeConfigList = new List<NodeConfig>
            {
                new NodeConfig { ChainId = "AELF", Endpoint = "http://mainchain.io" },
                new NodeConfig { ChainId = "tDVW", Endpoint = "http://mainchain.io" }
            };
        });

        context.Services.Configure<EsEndpointOption>(options =>
        {
            options.Uris = new List<string> { "http://127.0.0.1:9200" };
        });

        context.Services.Configure<IndexSettingOptions>(options =>
        {
            options.NumberOfReplicas = 1;
            options.NumberOfShards = 1;
            options.Refresh = Refresh.True;
            options.IndexPrefix = "SchrodingerIndexer";
        });

        var chainId = "tDVW";
        context.Services.Configure<ContractInfoOptions>(options =>
        {
            options.ContractInfos = new Dictionary<string, ContractInfo>()
            {
                {
                    chainId, new ContractInfo()
                    {
                        //TODO
                    }
                }
            };
            options.SwapContractInfos = new List<ContractInfo>()
            {
                new ()
                {
                    ChainId = chainId, 
                    SwapTokenContractAddress = "2L8uLZRJDUNdmeoA7RT6QbB7TZvu2xHra2gTz2bGrv9Wxs7KPS",
                    Level = 1
                },
                new ()
                {
                    ChainId = chainId,
                    SwapTokenContractAddress = "pVHzzPLV8U3XEAb3utFPnuFL7p6AZtxemgX1yX4tCvKQDQNud",
                    Level = 2
                },
                new ()
                {
                    ChainId = chainId,
                    SwapTokenContractAddress = "5KN5uqSC1vz521Lpfh9H1ZLWpU96x6ypEdHrTZF8WdjMmQFQ5",
                    Level = 3
                },
                new ()
                {
                    ChainId = chainId,
                    SwapTokenContractAddress = "2iFrdeaSKHwpNGWviSMVacjHjdgtZbfrkNeoV1opRzsfBrPVsm",
                    Level = 4
                },
                new ()
                {
                    ChainId = chainId,
                    SwapTokenContractAddress = "T25QvHLdWsyHaAeLKu9hvk33MTZrkWD1M7D4cZyU58JfPwhTh",
                    Level = 5
                }
            };
        });

        var applicationBuilder = new ApplicationBuilder(context.Services.BuildServiceProvider());
        context.Services.AddObjectAccessor<IApplicationBuilder>(applicationBuilder);
        var mockBlockScanAppService = new Mock<IBlockScanAppService>();
        mockBlockScanAppService.Setup(p => p.GetMessageStreamIdsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(new List<Guid>()));
        context.Services.AddSingleton<IBlockScanAppService>(mockBlockScanAppService.Object);
    }

    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        var provider = context.ServiceProvider.GetRequiredService<IAElfIndexerClientInfoProvider>();
        provider.SetClientId(ClientId);
        provider.SetVersion(Version);
        AsyncHelper.RunSync(async () =>
            await CreateIndexAsync(context.ServiceProvider)
        );
    }

    private async Task CreateIndexAsync(IServiceProvider serviceProvider)
    {
        var types = GetTypesAssignableFrom<IIndexBuild>(typeof(SchrodingerIndexerPluginModule).Assembly);
        var elasticIndexService = serviceProvider.GetRequiredService<IElasticIndexService>();
        foreach (var t in types)
        {
            var indexName = $"{ClientId}-{Version}.{t.Name}".ToLower();
            await elasticIndexService.CreateIndexAsync(indexName, t);
        }
    }

    private List<Type> GetTypesAssignableFrom<T>(Assembly assembly)
    {
        var compareType = typeof(T);
        return assembly.DefinedTypes
            .Where(type => compareType.IsAssignableFrom(type) && !compareType.IsAssignableFrom(type.BaseType) &&
                           !type.IsAbstract && type.IsClass && compareType != type)
            .Cast<Type>().ToList();
    }

    private async Task DeleteIndexAsync(IServiceProvider serviceProvider)
    {
        var elasticIndexService = serviceProvider.GetRequiredService<IElasticIndexService>();
        var types = GetTypesAssignableFrom<IIndexBuild>(typeof(SchrodingerIndexerPluginModule).Assembly);

        foreach (var t in types)
        {
            var indexName = $"{ClientId}-{Version}.{t.Name}".ToLower();
            await elasticIndexService.DeleteIndexAsync(indexName);
        }
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        AsyncHelper.RunSync(async () =>
            await DeleteIndexAsync(context.ServiceProvider)
        );
    }
}