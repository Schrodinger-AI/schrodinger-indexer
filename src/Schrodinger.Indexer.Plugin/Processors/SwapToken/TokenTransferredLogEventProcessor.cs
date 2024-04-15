using AElfIndexer.Client.Handlers;
using Awaken.Contracts.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.Provider;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.SwapToken;

public class TokenTransferredLogEventProcessor : SwapTokenProcessorBase<Transferred>
{
    public TokenTransferredLogEventProcessor(ILogger<TokenTransferredLogEventProcessor> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        ISwapTokenProvider swapTokenProvider,
        ITokenInfoProvider tokenInfoProvider) :
        base(logger, objectMapper, contractInfoOptions, swapTokenProvider, tokenInfoProvider)
    {
    }

    protected override async Task HandleEventAsync(Transferred eventValue, LogEventContext context)
    {
        Logger.LogInformation("[SwapToken.Transferred] handle chainId {chainId} from address {address} amount {amount}",
            context.ChainId,
            eventValue.From?.ToBase58(), eventValue.Amount);
        await SwapTokenProvider.HandleSwapLPInfoAsync(GetContractAddress(context.ChainId), eventValue.Symbol,
            eventValue.From?.ToBase58(),
            -eventValue.Amount, context);

        Logger.LogInformation("[SwapToken.Transferred] handle chainId {chainId} to address {address} amount {amount}",
            context.ChainId,
            eventValue.To?.ToBase58(), eventValue.Amount);
        await SwapTokenProvider.HandleSwapLPInfoAsync(GetContractAddress(context.ChainId), eventValue.Symbol,
            eventValue.To?.ToBase58(),
            eventValue.Amount, context);
    }

    public override SwapTokenLevel GetLevel()
    {
        return SwapTokenLevel.Level1;
    }
}

public class TokenTransferredLogEventProcessor2 : TokenTransferredLogEventProcessor
{
    public TokenTransferredLogEventProcessor2(ILogger<TokenTransferredLogEventProcessor2> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        ISwapTokenProvider swapTokenProvider,
        ITokenInfoProvider tokenInfoProvider) :
        base(logger, objectMapper, contractInfoOptions, swapTokenProvider, tokenInfoProvider)
    {
    }

    public override SwapTokenLevel GetLevel()
    {
        return SwapTokenLevel.Level2;
    }
}

public class TokenTransferredLogEventProcessor3 : TokenTransferredLogEventProcessor
{
    public TokenTransferredLogEventProcessor3(ILogger<TokenTransferredLogEventProcessor3> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        ISwapTokenProvider swapTokenProvider,
        ITokenInfoProvider tokenInfoProvider) :
        base(logger, objectMapper, contractInfoOptions, swapTokenProvider, tokenInfoProvider)
    {
    }

    public override SwapTokenLevel GetLevel()
    {
        return SwapTokenLevel.Level3;
    }
}

public class TokenTransferredLogEventProcessor4 : TokenTransferredLogEventProcessor
{
    public TokenTransferredLogEventProcessor4(ILogger<TokenTransferredLogEventProcessor4> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        ISwapTokenProvider swapTokenProvider,
        ITokenInfoProvider tokenInfoProvider) :
        base(logger, objectMapper, contractInfoOptions, swapTokenProvider, tokenInfoProvider)
    {
    }

    public override SwapTokenLevel GetLevel()
    {
        return SwapTokenLevel.Level4;
    }
}

public class TokenTransferredLogEventProcessor5 : TokenTransferredLogEventProcessor
{
    public TokenTransferredLogEventProcessor5(ILogger<TokenTransferredLogEventProcessor5> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        ISwapTokenProvider swapTokenProvider,
        ITokenInfoProvider tokenInfoProvider) :
        base(logger, objectMapper, contractInfoOptions, swapTokenProvider, tokenInfoProvider)
    {
    }

    public override SwapTokenLevel GetLevel()
    {
        return SwapTokenLevel.Level5;
    }
}