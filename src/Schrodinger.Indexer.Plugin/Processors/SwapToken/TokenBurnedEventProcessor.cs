using AElfIndexer.Client.Handlers;
using Awaken.Contracts.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.Provider;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.SwapToken;
 

public class TokenBurnedEventProcessor : SwapTokenProcessorBase<Burned>
{
    public TokenBurnedEventProcessor(ILogger<TokenBurnedEventProcessor> logger,
        IObjectMapper objectMapper,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        ISwapTokenProvider swapTokenProvider,
        ITokenInfoProvider tokenInfoProvider) : 
        base(logger, objectMapper, contractInfoOptions, swapTokenProvider, tokenInfoProvider)
    {
    }

    protected override async Task HandleEventAsync(Burned eventValue, LogEventContext context)
    {
        var chainId = context.ChainId;
        var symbol = eventValue.Symbol;
        var owner = eventValue.Burner?.ToBase58();
        Logger.LogDebug("[SwapToken.Burned] start chainId:{chainId} symbol:{symbol}, owner:{owner}", chainId, symbol,
            owner);
        await SwapTokenProvider.HandleSwapLPInfoAsync(GetContractAddress(chainId), eventValue.Symbol,
            eventValue.Burner?.ToBase58(), -eventValue.Amount, context);
        Logger.LogDebug("[SwapToken.Burned] end chainId:{chainId} symbol:{symbol}, owner:{owner}", chainId, symbol,
            owner);
    }

    public override SwapTokenLevel GetLevel()
    {
        return SwapTokenLevel.Level1;
    }
}

public class TokenBurnedEventProcessor2 : TokenBurnedEventProcessor
{
    public TokenBurnedEventProcessor2(ILogger<TokenBurnedEventProcessor2> logger,
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

public class TokenBurnedEventProcessor3 : TokenBurnedEventProcessor
{
    public TokenBurnedEventProcessor3(ILogger<TokenBurnedEventProcessor3> logger,
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

public class TokenBurnedEventProcessor4 : TokenBurnedEventProcessor
{
    public TokenBurnedEventProcessor4(ILogger<TokenBurnedEventProcessor4> logger,
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

public class TokenBurnedEventProcessor5 : TokenBurnedEventProcessor
{
    public TokenBurnedEventProcessor5(ILogger<TokenBurnedEventProcessor5> logger,
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