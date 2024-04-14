using AElfIndexer.Client.Handlers;
using Awaken.Contracts.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.Provider;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.SwapToken;

public class TokenIssuedEventProcessor : SwapTokenProcessorBase<Issued>
{
    public TokenIssuedEventProcessor(ILogger<TokenIssuedEventProcessor> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        ISwapTokenProvider swapTokenProvider,
        ITokenInfoProvider tokenInfoProvider) : 
        base(logger, objectMapper, contractInfoOptions, swapTokenProvider, tokenInfoProvider)
    {
    }

    protected override async Task HandleEventAsync(Issued eventValue, LogEventContext context)
    {
        Logger.LogDebug("[SwapToken.Issued] start chainId {chainId} address {address} amount {amount}", context.ChainId,
            eventValue.To?.ToBase58(), eventValue.Amount);
        await SwapTokenProvider.HandleSwapLPInfoAsync(GetContractAddress(context.ChainId), eventValue.Symbol, eventValue.To?.ToBase58(),
            eventValue.Amount, context);
        Logger.LogDebug("[SwapToken.Issued] end chainId {chainId} address {address} amount {amount}", context.ChainId,
            eventValue.To?.ToBase58(), eventValue.Amount);
    }

    public override SwapTokenLevel GetLevel()
    {
        return SwapTokenLevel.Level1;
    }
}

public class TokenIssuedEventProcessor2 : TokenIssuedEventProcessor
{
    public TokenIssuedEventProcessor2(ILogger<TokenIssuedEventProcessor2> logger,
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

public class TokenIssuedEventProcessor3 : TokenIssuedEventProcessor
{
    public TokenIssuedEventProcessor3(ILogger<TokenIssuedEventProcessor3> logger,
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

public class TokenIssuedEventProcessor4 : TokenIssuedEventProcessor
{
    public TokenIssuedEventProcessor4(ILogger<TokenIssuedEventProcessor4> logger,
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

public class TokenIssuedEventProcessor5 : TokenIssuedEventProcessor
{
    public TokenIssuedEventProcessor5(ILogger<TokenIssuedEventProcessor5> logger,
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