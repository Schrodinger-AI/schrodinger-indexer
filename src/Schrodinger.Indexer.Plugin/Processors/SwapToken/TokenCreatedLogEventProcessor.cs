using AElfIndexer.Client.Handlers;
using Awaken.Contracts.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.Provider;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.SwapToken;

public class TokenCreatedLogEventProcessor : SwapTokenProcessorBase<TokenCreated>
{
    public TokenCreatedLogEventProcessor(ILogger<TokenCreatedLogEventProcessor> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        ISwapTokenProvider swapTokenProvider,
        ITokenInfoProvider tokenInfoProvider) :
        base(logger, objectMapper, contractInfoOptions, swapTokenProvider, tokenInfoProvider)
    {
    }

    protected override async Task HandleEventAsync(TokenCreated eventValue, LogEventContext context)
    {
        Logger.LogInformation(
            "[SwapToken.TokenCreated] handle chainId {chainId} symbol {symbol} externalInfo {externalInfo} issuer {issuer}",
            context.ChainId, eventValue.Symbol, eventValue.Issuer, eventValue.ExternalInfo);
        await TokenInfoProvider.TokenInfoIndexCreateAsync(GetContractAddress(context.ChainId), eventValue, context);
    }

    public override SwapTokenLevel GetLevel()
    {
        return SwapTokenLevel.Level1;
    }
}

public class TokenCreatedLogEventProcessor2 : TokenCreatedLogEventProcessor
{
    public TokenCreatedLogEventProcessor2(ILogger<TokenCreatedLogEventProcessor2> logger,
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

public class TokenCreatedLogEventProcessor3 : TokenCreatedLogEventProcessor
{
    public TokenCreatedLogEventProcessor3(ILogger<TokenCreatedLogEventProcessor3> logger,
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

public class TokenCreatedLogEventProcessor4 : TokenCreatedLogEventProcessor
{
    public TokenCreatedLogEventProcessor4(ILogger<TokenCreatedLogEventProcessor4> logger,
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

public class TokenCreatedLogEventProcessor5 : TokenCreatedLogEventProcessor
{
    public TokenCreatedLogEventProcessor5(ILogger<TokenCreatedLogEventProcessor5> logger,
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