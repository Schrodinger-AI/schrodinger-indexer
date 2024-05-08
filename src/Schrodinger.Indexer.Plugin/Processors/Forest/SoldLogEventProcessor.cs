using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Forest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.Runtime;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors.Forest;

public class SoldLogEventProcessor : AElfLogEventProcessorBase<Sold, LogEventInfo>

{
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly ILogger<AElfLogEventProcessorBase<Sold, LogEventInfo>> _logger;
    private readonly INFTActivityProvider _nftActivityProvider;

    public SoldLogEventProcessor(
        ILogger<AElfLogEventProcessorBase<Sold, LogEventInfo>> logger,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        INFTActivityProvider nftActivityProvider) :
        base(logger)
    {
        _logger = logger;
        _contractInfoOptions = contractInfoOptions.Value;
        _nftActivityProvider = nftActivityProvider;
    }

    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos[chainId].NFTMarketContractAddress;
    }

    public static decimal ToPrice(long amount, int decimals)
    {
        return amount / (decimal)Math.Pow(10, decimals);
    }

    protected override async Task HandleEventAsync(Sold eventValue, LogEventContext context)
    {
        
        _logger.Debug("[HandleEventAsyncSold], event={Event}, context={context}", 
            JsonConvert.SerializeObject(eventValue), JsonConvert.SerializeObject(context));
            
        var nftInfoIndexId = "";

        if (SymbolHelper.CheckSymbolIsSeedSymbol(eventValue.NftSymbol))
        {
            nftInfoIndexId = IdGenerateHelper.GetSeedSymbolId(context.ChainId, eventValue.NftSymbol);
        }
        else if (SymbolHelper.CheckSymbolIsNoMainChainNFT(eventValue.NftSymbol, context.ChainId))
        {
            nftInfoIndexId = IdGenerateHelper.GetNFTInfoId(context.ChainId, eventValue.NftSymbol);
        }

        if (nftInfoIndexId.IsNullOrEmpty())
        {
            _logger.LogError("eventValue.NftSymbol is not nft, symbol={symbol}", eventValue.NftSymbol);
            return;
        }
        
        var totalPrice = ToPrice(eventValue.PurchaseAmount, TokenHelper.GetDecimal(eventValue.PurchaseSymbol));
        var totalCount = (int)TokenHelper.GetIntegerDivision(eventValue.NftQuantity, TokenHelper.GetDecimal(eventValue.NftSymbol));
        var singlePrice = CalSinglePrice(totalPrice,
            totalCount);

        // NFT activity
        var nftActivityIndexId =
            IdGenerateHelper.GetId(context.ChainId, eventValue.NftSymbol, "SOLD", context.TransactionId, Guid.NewGuid());
        var index = new NFTActivityIndex
        {
            Id = nftActivityIndexId,
            Type = NFTActivityType.Sale,
            From = FullAddressHelper.ToFullAddress(eventValue.NftFrom.ToBase58(), context.ChainId),
            To = FullAddressHelper.ToFullAddress(eventValue.NftTo.ToBase58(), context.ChainId),
            Amount = TokenHelper.GetIntegerDivision(eventValue.NftQuantity, TokenHelper.GetDecimal(eventValue.NftSymbol)),
            Price = singlePrice,
            TransactionHash = context.TransactionId,
            Timestamp = context.BlockTime,
            NftInfoId = nftInfoIndexId
        };
        var activitySaved = await _nftActivityProvider.AddNFTActivityAsync(context, index);
        if (!activitySaved)
        {
            _logger.Debug("save nft activity failed, index={index}", JsonConvert.SerializeObject(index));
            return;
        }
        
        _logger.Debug("HandleEventAsyncSold finished", 
            JsonConvert.SerializeObject(eventValue), JsonConvert.SerializeObject(context));
    }
    
    
    private decimal CalSinglePrice(decimal totalPrice, int count)
    {
        return Math.Round(totalPrice / Math.Max(1, count), 8);
    }
}