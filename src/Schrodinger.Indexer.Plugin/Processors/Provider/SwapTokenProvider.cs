using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.Provider;

public interface ISwapTokenProvider
{
    Task HandleSwapLPInfoAsync(string contractAddress, string symbol, string address,
        long amount, LogEventContext context);
}

public class SwapTokenProvider : ISwapTokenProvider, ISingletonDependency
{
    private const int DefaultDecimals = 8;
    private readonly ILogger<SwapTokenProvider> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<SwapLPIndex, LogEventInfo> _swapLPIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<SwapLPDailyIndex, LogEventInfo> _swapLPDailyIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> _tokenIndexRepository;
    
    public SwapTokenProvider(ILogger<SwapTokenProvider> logger, IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<SwapLPIndex, LogEventInfo> swapLPIndexRepository, 
        IAElfIndexerClientEntityRepository<SwapLPDailyIndex, LogEventInfo> swapLPDailyIndexRepository, 
        IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenIndexRepository)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _swapLPIndexRepository = swapLPIndexRepository;
        _swapLPDailyIndexRepository = swapLPDailyIndexRepository;
        _tokenIndexRepository = tokenIndexRepository;
    }

    public async Task HandleSwapLPInfoAsync(string contractAddress, string symbol, string address, long amount,
        LogEventContext context)
    {
        await SaveSwapLPIndexAsync(contractAddress, symbol, address, amount, context);
        
        await SaveSwapLPDailyIndexAsync(contractAddress, symbol, address, amount, context);
    }

    private async Task SaveSwapLPIndexAsync(string contractAddress, string symbol, string address, long amount,
        LogEventContext context)
    { 
        var decimals = await GetTokenDecimalAsync(context.ChainId, symbol, contractAddress);
        var swapLpId = IdGenerateHelper.GetSwapLPId(context.ChainId, symbol, contractAddress, address);
        var swapLpIndex =
            await _swapLPIndexRepository.GetFromBlockStateSetAsync(swapLpId, context.ChainId) ?? new SwapLPIndex
            {
                Id = swapLpId,
                ContractAddress = contractAddress,
                Symbol = symbol,
                LPAddress = address,
                Decimals = decimals
            };
        swapLpIndex.Balance += amount;
        swapLpIndex.UpdateTime = context.BlockTime;
        _objectMapper.Map(context, swapLpIndex);
        await _swapLPIndexRepository.AddOrUpdateAsync(swapLpIndex);
    }
    
    private async Task SaveSwapLPDailyIndexAsync(string contractAddress, string symbol, string address, long amount, LogEventContext context)
    {
        var swapLpId = IdGenerateHelper.GetSwapLPId(context.ChainId, symbol, contractAddress, address);
        var swapLpIndex = await _swapLPIndexRepository.GetFromBlockStateSetAsync(swapLpId, context.ChainId);
        //handle SwapLPDaily
        var bizDate =  context.BlockTime.ToString("yyyyMMdd");
        var swapLpDailyId = IdGenerateHelper.GetSwapLPDailyId(context.ChainId, symbol, contractAddress, address, bizDate);
        var swapLpDailyIndex =
            await _swapLPDailyIndexRepository.GetFromBlockStateSetAsync(swapLpDailyId, context.ChainId) ??
            new SwapLPDailyIndex
            {
                Id = swapLpDailyId,
                BizDate = bizDate,
                ContractAddress = contractAddress,
                Symbol = symbol,
                LPAddress = address
            };
        swapLpDailyIndex.Decimals = swapLpIndex?.Decimals ?? DefaultDecimals;
        swapLpDailyIndex.Balance = swapLpIndex?.Balance ?? 0;
        swapLpDailyIndex.ChangeAmount += amount;
        swapLpDailyIndex.UpdateTime = context.BlockTime;
        _objectMapper.Map(context, swapLpDailyIndex);
        await _swapLPDailyIndexRepository.AddOrUpdateAsync(swapLpDailyIndex);
    }

    private async Task<int> GetTokenDecimalAsync(string chainId, string symbol, string contractAddress)
    {
        var tokenInfoId = IdGenerateHelper.GetSwapTokenInfoId(chainId, symbol, contractAddress);
        var tokenInfoIndex = await _tokenIndexRepository.GetFromBlockStateSetAsync(tokenInfoId, chainId);

        int decimals;
        if (tokenInfoIndex == null)
        {
            decimals = DefaultDecimals;
            _logger.LogError("TokenInfoIndex is null, tokenInfoId {tokenInfoId}", tokenInfoId);
        }
        else
        {
            decimals = tokenInfoIndex.Decimals;
        }

        return decimals;
    }
}