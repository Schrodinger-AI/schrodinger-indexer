using AElf.CSharp.Core;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.Forest;

public abstract class OfferLogEventProcessorBase<TEvent>: AElfLogEventProcessorBase<TEvent, LogEventInfo> where TEvent : IEvent<TEvent>, new()
{
    protected readonly IObjectMapper _objectMapper;
    protected readonly ContractInfoOptions _contractInfoOptions;
    protected readonly IAElfIndexerClientEntityRepository<NFTActivityIndex, LogEventInfo> _nftActivityIndexRepository;
    protected readonly INFTActivityProvider _nftActivityProvider;

    
    public OfferLogEventProcessorBase(
        ILogger<OfferLogEventProcessorBase<TEvent>> logger, 
        IObjectMapper objectMapper,
        INFTActivityProvider nftActivityProvider,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        _objectMapper = objectMapper;
        _contractInfoOptions = contractInfoOptions.Value;
        _nftActivityProvider = nftActivityProvider;
    }
    
    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos[chainId].NFTMarketContractAddress;
    }

    protected async Task AddNFTActivityRecordAsync(string symbol, string offerFrom, string offerTo,
        long quantity, decimal price, NFTActivityType activityType, LogEventContext context,
        TokenInfoIndex tokenInfoIndex)
    {
        var nftActivityIndexId = IdGenerateHelper.GetId(context.ChainId, symbol, offerFrom,
            offerTo, context.TransactionId);
        var nftActivityIndex =
            await _nftActivityIndexRepository.GetFromBlockStateSetAsync(nftActivityIndexId, context.ChainId);
        if (nftActivityIndex != null) return;

        var nftInfoIndexId = IdGenerateHelper.GetId(context.ChainId, symbol);

        var decimals = TokenHelper.GetDecimal(symbol);
        
        nftActivityIndex = new NFTActivityIndex
        {
            Id = nftActivityIndexId,
            Type = activityType,
            TransactionHash = context.TransactionId,
            Timestamp = context.BlockTime,
            NftInfoId = nftInfoIndexId
        };
        _objectMapper.Map(context, nftActivityIndex);
        nftActivityIndex.From = FullAddressHelper.ToFullAddress(offerFrom, context.ChainId);
        nftActivityIndex.To = FullAddressHelper.ToFullAddress(offerTo, context.ChainId);

        nftActivityIndex.Amount = TokenHelper.GetIntegerDivision(quantity, decimals);
        nftActivityIndex.Price = price;
        nftActivityIndex.PriceTokenInfo = tokenInfoIndex;

        await _nftActivityIndexRepository.AddOrUpdateAsync(nftActivityIndex);
    }
}