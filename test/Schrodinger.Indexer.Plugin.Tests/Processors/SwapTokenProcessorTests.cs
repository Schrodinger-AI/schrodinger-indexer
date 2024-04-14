using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Grains.State.Client;
using Awaken.Contracts.Token;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors.SwapToken;
using Shouldly;
using Xunit;

namespace Schrodinger.Indexer.Plugin.Tests.Processors;

public class SwapTokenProcessorTests : SchrodingerIndexerPluginTestBase
{
    private readonly IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo>
        _tokenIndexRepo;

    private readonly IAElfIndexerClientEntityRepository<SwapLPIndex, LogEventInfo> _swapLPIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<SwapLPDailyIndex, LogEventInfo> _swapLPDailyIndexRepository;
    protected readonly ContractInfoOptions _contractInfoOptions;

    public SwapTokenProcessorTests()
    {
        _tokenIndexRepo = GetRequiredService<IAElfIndexerClientEntityRepository<TokenInfoIndex, LogEventInfo>>();

        _swapLPIndexRepository = GetRequiredService<IAElfIndexerClientEntityRepository<SwapLPIndex, LogEventInfo>>();

        _swapLPDailyIndexRepository =
            GetRequiredService<IAElfIndexerClientEntityRepository<SwapLPDailyIndex, LogEventInfo>>();

        _contractInfoOptions = GetRequiredService<IOptionsSnapshot<ContractInfoOptions>>().Value;
    }


    [Fact]
    public async Task TokenCreatedLogEventProcessor_Test()
    {
        await TokenCreated_Test( GetRequiredService<TokenCreatedLogEventProcessor>());
        await TokenCreated_Test(GetRequiredService<TokenCreatedLogEventProcessor2>());
        await TokenCreated_Test(GetRequiredService<TokenCreatedLogEventProcessor3>());
        await TokenCreated_Test(GetRequiredService<TokenCreatedLogEventProcessor4>());
        await TokenCreated_Test(GetRequiredService<TokenCreatedLogEventProcessor5>());
    }

    [Fact]
    public async Task TokenIssuedLogEventProcessor_Test()
    {
        await TokenIssued_Test(GetRequiredService<TokenCreatedLogEventProcessor>(), GetRequiredService<TokenIssuedEventProcessor>());
        
        await TokenIssued_Test(GetRequiredService<TokenCreatedLogEventProcessor2>(), GetRequiredService<TokenIssuedEventProcessor2>());

        await TokenIssued_Test(GetRequiredService<TokenCreatedLogEventProcessor3>(), GetRequiredService<TokenIssuedEventProcessor3>());

        await TokenIssued_Test(GetRequiredService<TokenCreatedLogEventProcessor4>(), GetRequiredService<TokenIssuedEventProcessor4>());

        await TokenIssued_Test(GetRequiredService<TokenCreatedLogEventProcessor5>(), GetRequiredService<TokenIssuedEventProcessor5>());

    }
    
    [Fact]
    public async Task TokenTransferredLogEventProcessor_Test()
    {
        await TokenTransferred_Test(GetRequiredService<TokenCreatedLogEventProcessor>(), GetRequiredService<TokenTransferredLogEventProcessor>());
        
        await TokenTransferred_Test(GetRequiredService<TokenCreatedLogEventProcessor2>(), GetRequiredService<TokenTransferredLogEventProcessor2>());

        await TokenTransferred_Test(GetRequiredService<TokenCreatedLogEventProcessor3>(), GetRequiredService<TokenTransferredLogEventProcessor3>());

        await TokenTransferred_Test(GetRequiredService<TokenCreatedLogEventProcessor4>(), GetRequiredService<TokenTransferredLogEventProcessor4>());

        await TokenTransferred_Test(GetRequiredService<TokenCreatedLogEventProcessor5>(), GetRequiredService<TokenTransferredLogEventProcessor5>());

    }
    
    [Fact]
    public async Task TokenBurnedEventProcessor_Test()
    {
        await TokenBurned_Test(GetRequiredService<TokenCreatedLogEventProcessor>(), GetRequiredService<TokenBurnedEventProcessor>());
        
        await TokenBurned_Test(GetRequiredService<TokenCreatedLogEventProcessor2>(), GetRequiredService<TokenBurnedEventProcessor2>());

        await TokenBurned_Test(GetRequiredService<TokenCreatedLogEventProcessor3>(), GetRequiredService<TokenBurnedEventProcessor3>());

        await TokenBurned_Test(GetRequiredService<TokenCreatedLogEventProcessor4>(), GetRequiredService<TokenBurnedEventProcessor4>());

        await TokenBurned_Test(GetRequiredService<TokenCreatedLogEventProcessor5>(), GetRequiredService<TokenBurnedEventProcessor5>());

    }

    private async Task TokenCreated_Test(TokenCreatedLogEventProcessor processor)
    {
        await MockEventProcess(SwapTokenCreated(), processor, SideChainId_tDVW);
        var contractAddress = _contractInfoOptions.GetSwapContractInfo(SideChainId_tDVW, (int)processor.GetLevel())
            .SwapTokenContractAddress;
        var tokenId = IdGenerateHelper.GetSwapTokenInfoId(SideChainId_tDVW, SwapSymbol, contractAddress);
        var tokenInfoIndex = await _tokenIndexRepo.GetFromBlockStateSetAsync(tokenId, SideChainId_tDVW);
        tokenInfoIndex.ShouldNotBeNull();
        tokenInfoIndex.Id.ShouldBe(tokenId);
        tokenInfoIndex.Decimals.ShouldBe(Decimals);
    }

    private async Task TokenIssued_Test(TokenCreatedLogEventProcessor createdLogEventProcessor,
        TokenIssuedEventProcessor processor)
    {
        await TokenCreated_Test(createdLogEventProcessor);
        
        var contractAddress = _contractInfoOptions.GetSwapContractInfo(SideChainId_tDVW, (int)processor.GetLevel())
            .SwapTokenContractAddress;
        var swapLpId = IdGenerateHelper.GetSwapLPId(SideChainId_tDVW, SwapSymbol, contractAddress, ToAddress);
        var balanceBefore =
            await _swapLPIndexRepository.GetFromBlockStateSetAsync(swapLpId, SideChainId_tDVW);
        
        var issued = new Issued
        {
            Symbol = SwapSymbol,
            Amount = IssuedAmount,
            To = Address.FromBase58(ToAddress)
        };
        var logEventContext = await MockEventProcess(issued.ToLogEvent(), processor, SideChainId_tDVW);
        
        var swapLpIndex = await _swapLPIndexRepository.GetFromBlockStateSetAsync(swapLpId, SideChainId_tDVW);
        swapLpIndex.ShouldNotBeNull();
        var resultAmount = (balanceBefore?.Balance ?? 0) + IssuedAmount;
        swapLpIndex.Balance.ShouldBe(resultAmount);
        
        //check swapLPDaily 
        var bizDate = logEventContext.BlockTime.ToString("yyyyMMdd");
        var swapLpDailyId = IdGenerateHelper.GetSwapLPDailyId(SideChainId_tDVW, SwapSymbol, contractAddress, ToAddress, bizDate);
        var swapLpDailyIndex = await _swapLPDailyIndexRepository.GetFromBlockStateSetAsync(swapLpDailyId, SideChainId_tDVW);
        swapLpDailyIndex.ShouldNotBeNull();
        swapLpDailyIndex.BizDate.ShouldBe(bizDate);
        swapLpDailyIndex.Balance = swapLpIndex.Balance;
        //before as 0
        var changeAmount = 0 + IssuedAmount;
        swapLpDailyIndex.ChangeAmount.ShouldBe(changeAmount);
    }
    
    
    private async Task TokenTransferred_Test(TokenCreatedLogEventProcessor createdLogEventProcessor,
        TokenTransferredLogEventProcessor processor)
    {
        await TokenCreated_Test(createdLogEventProcessor);
        
        var contractAddress = _contractInfoOptions.GetSwapContractInfo(SideChainId_tDVW, (int)processor.GetLevel())
            .SwapTokenContractAddress;
        
        var fromSwapLpId = IdGenerateHelper.GetSwapLPId(SideChainId_tDVW, SwapSymbol, contractAddress, FromAddress);
        var toSwapLpId = IdGenerateHelper.GetSwapLPId(SideChainId_tDVW, SwapSymbol, contractAddress, ToAddress);

        var fromBalanceBefore =
            await _swapLPIndexRepository.GetFromBlockStateSetAsync(fromSwapLpId, SideChainId_tDVW);
        var toBalanceBefore =
            await _swapLPIndexRepository.GetFromBlockStateSetAsync(toSwapLpId, SideChainId_tDVW);
        
        var transferAmount = 1;
        var logEvent = new Transferred()
        {
            Symbol = SwapSymbol,
            Amount = transferAmount,
            From = Address.FromBase58(FromAddress), 
            To = Address.FromBase58(ToAddress)
        };
        var logEventContext = await MockEventProcess(logEvent.ToLogEvent(), processor, SideChainId_tDVW);
        var bizDate = logEventContext.BlockTime.ToString("yyyyMMdd");

        var fromSwapLpIndex = await _swapLPIndexRepository.GetFromBlockStateSetAsync(fromSwapLpId, SideChainId_tDVW);
        fromSwapLpIndex.ShouldNotBeNull();
        var fromResultAmount = (fromBalanceBefore?.Balance ?? 0) - transferAmount;
        fromSwapLpIndex.Balance.ShouldBe(fromResultAmount);
        //check swapLPDaily 
        var fromSwapLpDailyId = IdGenerateHelper.GetSwapLPDailyId(SideChainId_tDVW, SwapSymbol, contractAddress, FromAddress, bizDate);
        var swapLpDailyIndex = await _swapLPDailyIndexRepository.GetFromBlockStateSetAsync(fromSwapLpDailyId, SideChainId_tDVW);
        swapLpDailyIndex.ShouldNotBeNull();
        swapLpDailyIndex.BizDate.ShouldBe(bizDate);
        swapLpDailyIndex.Balance = fromSwapLpIndex.Balance;
        //before as 0
        var changeAmount = 0 - transferAmount;
        swapLpDailyIndex.ChangeAmount.ShouldBe(changeAmount);
        
        var toSwapLpIndex = await _swapLPIndexRepository.GetFromBlockStateSetAsync(toSwapLpId, SideChainId_tDVW);
        fromSwapLpIndex.ShouldNotBeNull();
        var toResultAmount = (fromBalanceBefore?.Balance ?? 0) + transferAmount;
        toSwapLpIndex.Balance.ShouldBe(toResultAmount);
        
        //check swapLPDaily 
        var toSwapLpDailyId = IdGenerateHelper.GetSwapLPDailyId(SideChainId_tDVW, SwapSymbol, contractAddress, ToAddress, bizDate);
        var toSwapLpDailyIndex = await _swapLPDailyIndexRepository.GetFromBlockStateSetAsync(toSwapLpDailyId, SideChainId_tDVW);
        toSwapLpDailyIndex.ShouldNotBeNull();
        toSwapLpDailyIndex.BizDate.ShouldBe(bizDate);
        toSwapLpDailyIndex.Balance = fromSwapLpIndex.Balance;
        //before as 0
        var toChangeAmount = 0 + transferAmount;
        toSwapLpDailyIndex.ChangeAmount.ShouldBe(toChangeAmount);
    }
    
    
    private async Task TokenBurned_Test(TokenCreatedLogEventProcessor createdLogEventProcessor,
        TokenBurnedEventProcessor processor)
    {
        await TokenCreated_Test(createdLogEventProcessor);
        
        var contractAddress = _contractInfoOptions.GetSwapContractInfo(SideChainId_tDVW, (int)processor.GetLevel())
            .SwapTokenContractAddress;
        var swapLpId = IdGenerateHelper.GetSwapLPId(SideChainId_tDVW, SwapSymbol, contractAddress, FromAddress);
        var balanceBefore =
            await _swapLPIndexRepository.GetFromBlockStateSetAsync(swapLpId, SideChainId_tDVW);
        
        var burnedAmount = 2;
        var issued = new Burned()
        {
            Symbol = SwapSymbol,
            Amount = burnedAmount,
            Burner = Address.FromBase58(FromAddress)
        };
        var logEventContext = await MockEventProcess(issued.ToLogEvent(), processor, SideChainId_tDVW);

        var swapLpIndex = await _swapLPIndexRepository.GetFromBlockStateSetAsync(swapLpId, SideChainId_tDVW);
        swapLpIndex.ShouldNotBeNull();
        var resultAmount = (balanceBefore?.Balance ?? 0) - burnedAmount;
        swapLpIndex.Balance.ShouldBe(resultAmount);
        
        //check swapLPDaily 
        var bizDate = logEventContext.BlockTime.ToString("yyyyMMdd");
        var swapLpDailyId = IdGenerateHelper.GetSwapLPDailyId(SideChainId_tDVW, SwapSymbol, contractAddress, FromAddress, bizDate);
        var swapLpDailyIndex = await _swapLPDailyIndexRepository.GetFromBlockStateSetAsync(swapLpDailyId, SideChainId_tDVW);
        swapLpDailyIndex.ShouldNotBeNull();
        swapLpDailyIndex.BizDate.ShouldBe(bizDate);
        swapLpDailyIndex.Balance = swapLpIndex.Balance;
        //before as 0
        var changeAmount = 0 - burnedAmount;
        swapLpDailyIndex.ChangeAmount.ShouldBe(changeAmount);
    }
    
    private LogEvent SwapTokenCreated()
    {
        return new TokenCreated
        {
            Symbol = SwapSymbol,
            TokenName = SwapTokenName,
            TotalSupply = TotalSupply,
            Decimals = Decimals,
            Issuer = Address.FromBase58(Issuer),
            IsBurnable = IsBurnable,
            ExternalInfo = new ExternalInfo
            {
                Value =
                {
                    {
                        "__seed_owned_symbol",
                        "WILLTESTHH"
                    },
                    {
                        "__nft_image_url",
                        ""
                    }
                }
            }
        }.ToLogEvent();
    }
}