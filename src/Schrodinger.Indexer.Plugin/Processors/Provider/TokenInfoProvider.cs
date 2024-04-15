using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Token;
using Ewell.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.Provider;


public interface ITokenInfoProvider
{
    public Task TokenInfoIndexCreateAsync(string contractAddress, TokenCreated eventValue, LogEventContext context);
}

public class TokenInfoProvider  : ITokenInfoProvider, ISingletonDependency
{
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> _tokenIndexRepository;
    private readonly IObjectMapper _objectMapper;
    
    public TokenInfoProvider(IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo> tokenIndexRepository,
        IObjectMapper objectMapper)
    {
        _tokenIndexRepository = tokenIndexRepository;
        _objectMapper = objectMapper;
    }
    
    public async Task TokenInfoIndexCreateAsync(string contractAddress, TokenCreated eventValue, LogEventContext context)
    {
        if (eventValue == null || context == null)
        {
            return;
        }
        var tokenInfoIndex = _objectMapper.Map<TokenCreated, TokenInfoIndex>(eventValue);
        tokenInfoIndex.ExternalInfoDictionary = eventValue.ExternalInfo?.Value
            .Select(entity => new ExternalInfoDictionary
            {
                Key = entity.Key,
                Value = entity.Value
            }).ToList();
        //tokenInfoIndex.Owner = (eventValue.Owner ?? eventValue.Issuer).ToBase58();
        tokenInfoIndex.Issuer = eventValue.Issuer?.ToBase58();
    
        tokenInfoIndex.Id = IdGenerateHelper.GetSwapTokenInfoId(context.ChainId, eventValue.Symbol, contractAddress);
        tokenInfoIndex.CreateTime = context.BlockTime;

        _objectMapper.Map(context, tokenInfoIndex);
        await _tokenIndexRepository.AddOrUpdateAsync(tokenInfoIndex);
    }
}