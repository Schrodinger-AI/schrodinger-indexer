using AElf.Contracts.MultiToken;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Grains.State.Client;
using Forest;
using Nethereum.Hex.HexConvertors.Extensions;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.Forest;
using Shouldly;
using Volo.Abp.ObjectMapping;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class SoldLogEventProcessorTests : SchrodingerIndexerPluginTestBase 
{
    private readonly IObjectMapper _objectMapper;
    private readonly IAElfIndexerClientEntityRepository<NFTActivityIndex, LogEventInfo> _nftActivityIndexRepository;
    public SoldLogEventProcessorTests()
    {
        _objectMapper = GetRequiredService<IObjectMapper>();
        _nftActivityIndexRepository = GetRequiredService<IAElfIndexerClientEntityRepository<NFTActivityIndex, LogEventInfo>>();
    }

    [Fact]
    public async Task SoldProcessAsyncTest()
    {
        // await MockCreateNFT();
        
        const string symbol = "SGR-1";
        const string tokenSymbol = "ELF";
        const long amount = 10000000000;
        const long decimals = 8;
        const long quantity = 200000000;
        var address1 = Address.FromPublicKey("AAA".HexToByteArray());
        var address2 = Address.FromPublicKey("BBB".HexToByteArray());
        var address3 = Address.FromPublicKey("CCC".HexToByteArray());
        var sold = new Sold()
        {
            NftFrom = address1,
            NftTo = address2,
            NftQuantity = quantity,
            NftSymbol = symbol,
            PurchaseAmount = amount,
            PurchaseSymbol = tokenSymbol
        };
        
        
        var logEventContext = MockLogEventContext(100);
        var blockStateSetKey = await MockBlockState(logEventContext);
        
        var logEventProcessor = GetRequiredService<SoldLogEventProcessor>();
        logEventProcessor.GetContractAddress(logEventContext.ChainId);

        // sold
        var logEvent = MockLogEventInfo(sold.ToLogEvent());
        await logEventProcessor.HandleEventAsync(logEvent, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        var activityIndexList = await _nftActivityIndexRepository.GetListAsync();
        activityIndexList.Item1.ShouldBe(1);

        // another sold
        sold.PurchaseAmount = 20000000000;
        sold.NftFrom = address3;
        sold.NftTo = address1;
        logEventContext.TransactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da3";
        logEvent = MockLogEventInfo(sold.ToLogEvent());
        await logEventProcessor.HandleEventAsync(logEvent, logEventContext);
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        activityIndexList = await _nftActivityIndexRepository.GetListAsync();
        activityIndexList.Item1.ShouldBe(2);
    }
    
    protected async Task MockCreateNFT()
    {
        // Create NFT collection
        const string collectionSymbol = "SYB-0";
        const string nftSymbol = "SYB-1";
        const string tokenName = "SYB Token";
        const bool isBurnable = true;
        const long totalSupply = 1;
        const int decimals = 8;
        const int issueChainId = 9992731;
        var logEventContext = MockLogEventContext(100);
        var blockStateSetKey = await MockBlockState(logEventContext);

        var tokenCreated = new TokenCreated()
        {
            Symbol = collectionSymbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId,
            ExternalInfo = new ExternalInfo()
        };
        var logEventInfo = MockLogEventInfo(tokenCreated.ToLogEvent());
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);


        // Create NFT
        tokenCreated = new TokenCreated()
        {
            Symbol = nftSymbol,
            TokenName = tokenName,
            TotalSupply = totalSupply,
            Decimals = decimals,
            Issuer = Address.FromPublicKey("AAA".HexToByteArray()),
            IsBurnable = isBurnable,
            IssueChainId = issueChainId,
            ExternalInfo = new ExternalInfo(),
        };
        logEventInfo = MockLogEventInfo(tokenCreated.ToLogEvent());
        
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
    }
}