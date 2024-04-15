using System.Threading.Tasks;
using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.DependencyInjection;
using IObjectMapper = Volo.Abp.ObjectMapping.IObjectMapper;

namespace Schrodinger.Indexer.Plugin.Processors;

public abstract class SchrodingerProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent, LogEventInfo>
    where TEvent : IEvent<TEvent>,new()
{
    protected readonly ILogger<SchrodingerProcessorBase<TEvent>> Logger;
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> SchrodingerHolderRepository;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> SchrodingerRepository;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> SchrodingerTraitValueRepository;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> SchrodingerSymbolRepository;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> SchrodingerAdoptRepository;
    protected SchrodingerProcessorBase(ILogger<SchrodingerProcessorBase<TEvent>> logger,
        IObjectMapper objectMapper, IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> schrodingerHolderRepository,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerRepository,
        IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> schrodingerTraitValueRepository,
        IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> schrodingerSymbolRepository,
        IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> schrodingerAdoptRepository)
        : base(logger)
    {
        Logger = logger;
        ObjectMapper = objectMapper;
        ContractInfoOptions = contractInfoOptions.Value;
        SchrodingerHolderRepository = schrodingerHolderRepository;
        SchrodingerRepository = schrodingerRepository;
        SchrodingerTraitValueRepository = schrodingerTraitValueRepository;
        SchrodingerSymbolRepository = schrodingerSymbolRepository;
        SchrodingerAdoptRepository = schrodingerAdoptRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos[chainId].SchrodingerContractAddress;
    }
    
    protected async Task SaveIndexAsync(SchrodingerAdoptIndex index, LogEventContext context)
    {
        ObjectMapper.Map(context, index);
        await SchrodingerAdoptRepository.AddOrUpdateAsync(index);
    }

    protected async Task<SchrodingerInfo> GetSchrodingerInfo(string chainId, string symbol)
    {
        var symbolId = IdGenerateHelper.GetId(chainId, symbol);
        var symbolIndex = await SchrodingerSymbolRepository.GetFromBlockStateSetAsync(symbolId, chainId);
        return symbolIndex?.SchrodingerInfo ?? new SchrodingerInfo();
    }
}