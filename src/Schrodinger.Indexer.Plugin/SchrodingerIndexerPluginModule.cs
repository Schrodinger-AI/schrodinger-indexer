using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.DependencyInjection;
using Schrodinger.Indexer.Plugin.GraphQL;
using Schrodinger.Indexer.Plugin.Processors;
using Schrodinger.Indexer.Plugin.Processors.Forest;
using Schrodinger.Indexer.Plugin.Processors.SwapToken;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Schrodinger.Indexer.Plugin;

[DependsOn(typeof(AElfIndexerClientModule), typeof(AbpAutoMapperModule))]
public class SchrodingerIndexerPluginModule : AElfIndexerClientPluginBaseModule<SchrodingerIndexerPluginModule,
    SchrodingerIndexerPluginSchema, Query>
{
    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        var configuration = serviceCollection.GetConfiguration();
        Configure<ContractInfoOptions>(configuration.GetSection("ContractInfo"));
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, BurnedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, ConfirmedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, CrossChainReceivedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, DeployedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, CollectionDeployedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, IssuedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, MaxGenerationSetLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SchrodingerAdoptProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCreatedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TransferredProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, RerolledProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, AdoptionRerolledProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, FixedAttributesSetLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, RandomAttributesSetLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, AdoptionUpdatedProcessor>();
        //swap token
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCreatedLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCreatedLogEventProcessor2>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCreatedLogEventProcessor3>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCreatedLogEventProcessor4>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenCreatedLogEventProcessor5>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenIssuedEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenIssuedEventProcessor2>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenIssuedEventProcessor3>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenIssuedEventProcessor4>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenIssuedEventProcessor5>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenBurnedEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenBurnedEventProcessor2>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenBurnedEventProcessor3>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenBurnedEventProcessor4>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenBurnedEventProcessor5>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenTransferredLogEventProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenTransferredLogEventProcessor2>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenTransferredLogEventProcessor3>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenTransferredLogEventProcessor4>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, TokenTransferredLogEventProcessor5>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<LogEventInfo>, SoldLogEventProcessor>();
        
    }

    protected override string ClientId => "SchrodingerIndexer_DApp";
    protected override string Version => "b0fa1706beee4846b28b327585c9c9bf";
}