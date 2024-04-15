using AElf.CSharp.Core;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.Provider;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.SwapToken;

public abstract class SwapTokenProcessorBase<TEvent> : AElfLogEventProcessorBase<TEvent, LogEventInfo>
    where TEvent : IEvent<TEvent>, new()
{
    protected readonly ILogger<SwapTokenProcessorBase<TEvent>> Logger;
    protected readonly IObjectMapper ObjectMapper;
    protected readonly ContractInfoOptions ContractInfoOptions;
    protected readonly ISwapTokenProvider SwapTokenProvider;
    protected readonly ITokenInfoProvider TokenInfoProvider;

    protected SwapTokenProcessorBase(ILogger<SwapTokenProcessorBase<TEvent>> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        ISwapTokenProvider swapTokenProvider,
        ITokenInfoProvider tokenInfoProvider) : base(logger)
    {
        Logger = logger;
        ObjectMapper = objectMapper;
        ContractInfoOptions = contractInfoOptions.Value;
        SwapTokenProvider = swapTokenProvider;
        TokenInfoProvider = tokenInfoProvider;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.GetSwapContractInfo(chainId, (int)GetLevel()).SwapTokenContractAddress;
    }

    public abstract SwapTokenLevel GetLevel();
}