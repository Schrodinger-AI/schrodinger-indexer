using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.Forest;

public interface INFTActivityProvider
{
    public Task<bool> AddNFTActivityAsync(LogEventContext context, NFTActivityIndex nftActivityIndex);
}

public class NFTActivityProvider : INFTActivityProvider, ISingletonDependency
{
    private readonly IAElfIndexerClientEntityRepository<NFTActivityIndex, LogEventInfo> _nftActivityIndexRepository;
    private readonly ILogger<NFTActivityProvider> _logger;
    private readonly IObjectMapper _objectMapper;
    
    public NFTActivityProvider(
        IAElfIndexerClientEntityRepository<NFTActivityIndex, LogEventInfo> nftActivityIndexRepository,
        ILogger<NFTActivityProvider> logger,
        IObjectMapper objectMapper)
    {
        _nftActivityIndexRepository = nftActivityIndexRepository;
        _logger = logger;
        _objectMapper = objectMapper;
    }
    
    public async Task<bool> AddNFTActivityAsync(LogEventContext context, NFTActivityIndex nftActivityIndex)
    {
        var nftActivityIndexExists =
            await _nftActivityIndexRepository.GetFromBlockStateSetAsync(nftActivityIndex.Id, context.ChainId);
        if (nftActivityIndexExists != null)
        {
            _logger.Debug("[AddNFTActivityAsync] FAIL: activity EXISTS, nftActivityIndexId={Id}", nftActivityIndex.Id);
            return false;
        }

        var from = nftActivityIndex.From;
        var to = nftActivityIndex.To;
        _objectMapper.Map(context, nftActivityIndex);
        nftActivityIndex.From = FullAddressHelper.ToFullAddress(from, context.ChainId);
        nftActivityIndex.To = FullAddressHelper.ToFullAddress(to, context.ChainId);

        _logger.Debug("[AddNFTActivityAsync] SAVE: activity SAVE, nftActivityIndexId={Id}", nftActivityIndex.Id);
        await _nftActivityIndexRepository.AddOrUpdateAsync(nftActivityIndex);

        _logger.Debug("[AddNFTActivityAsync] SAVE: activity FINISH, nftActivityIndexId={Id}", nftActivityIndex.Id);
        return true;
    }
}