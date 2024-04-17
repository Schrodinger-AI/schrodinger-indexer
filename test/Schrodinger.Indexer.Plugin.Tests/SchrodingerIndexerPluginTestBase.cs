using AElf;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Client.Providers;
using AElfIndexer.Grains;
using AElfIndexer.Grains.State.Client;
using Schrodinger.Indexer.Orleans.TestBase;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.Processors;
using Schrodinger.Indexer.Plugin.Tests.Helper;
using SchrodingerMain;

namespace Schrodinger.Indexer.Plugin.Tests;

public abstract class SchrodingerIndexerPluginTestBase : SchrodingerIndexerOrleansTestBase<SchrodingerIndexerPluginTestModule>
{
    private readonly IAElfIndexerClientInfoProvider _indexerClientInfoProvider;
    public IBlockStateSetProvider<LogEventInfo> _blockStateSetLogEventInfoProvider;
    private readonly IBlockStateSetProvider<TransactionInfo> _blockStateSetTransactionInfoProvider;
    private readonly IDAppDataProvider _dAppDataProvider;
    private readonly IDAppDataIndexManagerProvider _dAppDataIndexManagerProvider;

    protected readonly string MainChainId = "AELF";
    protected readonly string SideChainId = "tDVV";
    protected readonly string SideChainId_tDVW = "tDVW";
    protected const long BlockHeight = 120;
    protected readonly string Tick = "SGR";
    protected readonly string CollectionSymbol = "SGR-0";
    protected readonly string GEN0Symbol = "SGR-1";
    protected readonly string GEN1Symbol = "SGR-2";
    protected readonly string GEN9Symbol = "SGR-9";
    protected readonly string SwapSymbol = "ALP SGR-1-USDT";
    protected readonly string GEN0TokenName = "SGR";
    protected readonly string GEN1TokenName = "SGR-2GEN1";
    protected readonly string GEN9TokenName = "SGR-9GEN9";
    protected readonly string SwapTokenName = "Awaken SGR-1-USDT LP Token";
    protected readonly string TraitType1 = "traitType1";
    protected readonly string TraitType2 = "traitType2";
    protected readonly string TraitValue1 = "traitValue1";
    protected readonly string TraitValue2 = "traitValue2";
    protected readonly string Ancestor = "SGR-1";
    protected const int MaxGeneration = 9;
    protected const long TotalSupply = 2100;
    protected const int Decimals = 8;
    protected bool IsBurnable = true;
    protected int IssueChainId = 9992731;
    protected const string Issuer = "2N9DJYUUruS7bFqRyKMvafA75qTWgqpWcB78nNZzpmxHrMv4D";
    protected readonly string Owner = "2fbCtXNLVD2SC4AD6b8nqAkHtjqxRCfwvciX4MyH6257n8Gf63";
    protected readonly string Deployer = "2XXstcuHsCzwaYruJA1MdsXxkposUBr2gA8ubRjqqmUyRyBM2t";
    public static string FromAddress = "Ccc5pNs71BMbgDr2ZwpNqtegfkHkBsTJ57HBZ6gw3HNH6pb9S";
    public static string ToAddress = "aLyxCJvWMQH6UEykTyeWAcYss9baPyXkrMQ37BHnUicxD2LL3";
    protected const long LossRate = 500;
    protected const long IssuedAmount = 99;
    protected const long BurnedAmount = 1;
    protected const long CrossChainReceivedAmount = 3;
    protected const long TransferredAmount = 1;
    public const string InscriptionImageKey = "__inscription_image";
    public const string InscriptionImage = "InscriptionImage";
    protected const string NftAttributes = "__nft_attributes";
    protected const string AdoptIdString = "AdoptId";
    protected readonly string AdoptId = HashHelper.ComputeFrom(AdoptIdString).ToHex();

    protected readonly DeployedLogEventProcessor DeployedLogEventProcessor;
    protected readonly IssuedProcessor IssuedProcessor;
    protected readonly SchrodingerAdoptProcessor AdoptProcessor;
    protected readonly ConfirmedProcessor ConfirmedProcessor;
    protected readonly BurnedProcessor BurnedProcessor;
    protected readonly TransferredProcessor TransferredProcessor;
    protected readonly CollectionDeployedProcessor CollectionDeployedProcessor;
    protected readonly TokenCreatedProcessor TokenCreatedProcessor;
    protected readonly CrossChainReceivedProcessor CrossChainReceivedProcessor;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> SchrodingerRepository;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo> SchrodingerHolderRepository;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo> SchrodingerTraitValueRepository;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo> SchrodingerSymbolRepository;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo> SchrodingerAdoptRepository;
    protected readonly IAElfIndexerClientEntityRepository<SchrodingerHolderDailyChangeIndex, LogEventInfo> SchrodingerHolderDailyChangeIndex;
    public SchrodingerIndexerPluginTestBase()
    {
        _indexerClientInfoProvider = GetRequiredService<IAElfIndexerClientInfoProvider>();
        _blockStateSetLogEventInfoProvider = GetRequiredService<IBlockStateSetProvider<LogEventInfo>>();
        _blockStateSetTransactionInfoProvider = GetRequiredService<IBlockStateSetProvider<TransactionInfo>>();
        _dAppDataProvider = GetRequiredService<IDAppDataProvider>();
        _dAppDataIndexManagerProvider = GetRequiredService<IDAppDataIndexManagerProvider>();
        DeployedLogEventProcessor = GetRequiredService<DeployedLogEventProcessor>();
        IssuedProcessor = GetRequiredService<IssuedProcessor>();
        ConfirmedProcessor = GetRequiredService<ConfirmedProcessor>();
        BurnedProcessor = GetRequiredService<BurnedProcessor>();
        TransferredProcessor = GetRequiredService<TransferredProcessor>();
        CollectionDeployedProcessor = GetRequiredService<CollectionDeployedProcessor>();
        TokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        CrossChainReceivedProcessor = GetRequiredService<CrossChainReceivedProcessor>();
        SchrodingerRepository = GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo>>();
        SchrodingerHolderRepository = GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerHolderIndex, LogEventInfo>>();
        SchrodingerTraitValueRepository = GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerTraitValueIndex, LogEventInfo>>();
        SchrodingerSymbolRepository = GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerSymbolIndex, LogEventInfo>>();
        SchrodingerAdoptRepository = GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerAdoptIndex, LogEventInfo>>();
        AdoptProcessor = GetRequiredService<SchrodingerAdoptProcessor>();
        SchrodingerHolderDailyChangeIndex = GetRequiredService<IAElfIndexerClientEntityRepository<SchrodingerHolderDailyChangeIndex, LogEventInfo>>();
    }

    protected async Task<string> InitializeBlockStateSetAsync(BlockStateSet<LogEventInfo> blockStateSet, string chainId)
    {
        var key = GrainIdHelper.GenerateGrainId("BlockStateSets", _indexerClientInfoProvider.GetClientId(), chainId,
            _indexerClientInfoProvider.GetVersion());

        await _blockStateSetLogEventInfoProvider.SetBlockStateSetAsync(key, blockStateSet);
        await _blockStateSetLogEventInfoProvider.SetCurrentBlockStateSetAsync(key, blockStateSet);
        await _blockStateSetLogEventInfoProvider.SetLongestChainBlockStateSetAsync(key, blockStateSet.BlockHash);

        return key;
    }

    protected async Task<string> InitializeBlockStateSetAsync(BlockStateSet<TransactionInfo> blockStateSet,
        string chainId)
    {
        var key = GrainIdHelper.GenerateGrainId("BlockStateSets", _indexerClientInfoProvider.GetClientId(), chainId,
            _indexerClientInfoProvider.GetVersion());

        await _blockStateSetTransactionInfoProvider.SetBlockStateSetAsync(key, blockStateSet);
        await _blockStateSetTransactionInfoProvider.SetCurrentBlockStateSetAsync(key, blockStateSet);
        await _blockStateSetTransactionInfoProvider.SetLongestChainBlockStateSetAsync(key, blockStateSet.BlockHash);

        return key;
    }

    protected async Task BlockStateSetSaveDataAsync<TSubscribeType>(string key)
    {
        await _dAppDataProvider.SaveDataAsync();
        await _dAppDataIndexManagerProvider.SavaDataAsync();
        if (typeof(TSubscribeType) == typeof(TransactionInfo))
            await _blockStateSetTransactionInfoProvider.SaveDataAsync(key);
        else if (typeof(TSubscribeType) == typeof(LogEventInfo))
            await _blockStateSetLogEventInfoProvider.SaveDataAsync(key);
    }

    protected LogEventContext MockLogEventContext(long inputBlockHeight = 100, string chainId = "tDVW")
    {
        const string blockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
        const string previousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
        const string transactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
        var blockHeight = inputBlockHeight;
        return new LogEventContext
        {
            ChainId = chainId,
            BlockHeight = blockHeight,
            BlockHash = blockHash,
            PreviousBlockHash = previousBlockHash,
            TransactionId = transactionId,
            BlockTime = DateTime.UtcNow.FromUnixTimeSeconds(1672502400),
            ExtraProperties = new Dictionary<string, string>
            {
                { "TransactionFee", "{\"ELF\": 10, \"DECIMAL\": 8}" },
                { "ResourceFee", "{\"ELF\": 10, \"DECIMAL\": 15}" }
            }
        };
    }

    protected LogEventInfo MockLogEventInfo(LogEvent logEvent)
    {
        var logEventInfo = LogEventHelper.ConvertAElfLogEventToLogEventInfo(logEvent);
        var logEventContext = MockLogEventContext(100);
        logEventInfo.BlockHeight = logEventContext.BlockHeight;
        logEventInfo.ChainId = logEventContext.ChainId;
        logEventInfo.BlockHash = logEventContext.BlockHash;
        logEventInfo.TransactionId = logEventContext.TransactionId;
        logEventInfo.BlockTime = DateTime.Now;
        return logEventInfo;
    }

    protected async Task<string> MockBlockState(LogEventContext logEventContext)
    {
        var blockStateSet = new BlockStateSet<LogEventInfo>
        {
            BlockHash = logEventContext.BlockHash,
            BlockHeight = logEventContext.BlockHeight,
            Confirmed = true,
            PreviousBlockHash = logEventContext.PreviousBlockHash
        };
        return await InitializeBlockStateSetAsync(blockStateSet, logEventContext.ChainId);
    }

    protected async Task<string> MockBlockStateForTransactionInfo(LogEventContext logEventContext)
    {
        var blockStateSet = new BlockStateSet<TransactionInfo>
        {
            BlockHash = logEventContext.BlockHash,
            BlockHeight = logEventContext.BlockHeight,
            Confirmed = true,
            PreviousBlockHash = logEventContext.PreviousBlockHash
        };
        return await InitializeBlockStateSetAsync(blockStateSet, logEventContext.ChainId);
    }
    
    protected async Task<LogEventContext> MockEventProcess(LogEvent logEvent, IAElfLogEventProcessor processor, string chainId)
    {
        var logEventContext = MockLogEventContext(BlockHeight, chainId);

        // step1: create blockStateSet
        var blockStateSetKey = await MockBlockState(logEventContext);

        // step2: create logEventInfo
        var logEventInfo = MockLogEventInfo(logEvent);

        // step3 call the logic
        await processor.HandleEventAsync(logEventInfo, logEventContext);

        // step4 save data after logic
        await BlockStateSetSaveDataAsync<LogEventInfo>(blockStateSetKey);
        return logEventContext;
    }

    protected LogEvent CollectionDeployed()
    {
        return new CollectionDeployed
        {
            Symbol = GEN0Symbol,
            Issuer = Address.FromBase58(Issuer),
            Owner = Address.FromBase58(Owner),
            Deployer = Address.FromBase58(Deployer),
            Decimals = Decimals,
            TotalSupply = TotalSupply,
            CollectionExternalInfos = new SchrodingerMain.ExternalInfos
            {
                Value =
                {
                    [InscriptionImageKey] = InscriptionImage
                }
            }
        }.ToLogEvent();
    }

    protected LogEvent Deployed()
    {
        return new Deployed
        {
            Tick = Tick,
            Ancestor = Ancestor,
            MaxGeneration = MaxGeneration,
            TotalSupply = TotalSupply,
            Decimals = Decimals,
            Issuer = Address.FromBase58(Issuer),
            Owner = Address.FromBase58(Owner),
            Deployer = Address.FromBase58(Deployer),
            LossRate = LossRate,
            ExternalInfos = new ExternalInfos
            {
                Value =
                {
                    [InscriptionImageKey] = InscriptionImage
                }
            },
        }.ToLogEvent();
    }
    
    protected LogEvent IssuedGen0()
    {
        return new Issued
        {
            Symbol = GEN0Symbol,
            To = Address.FromBase58(Issuer),
            Amount = IssuedAmount
        }.ToLogEvent();
    }
    
    protected LogEvent IssuedGen1()
    {
        return new Issued
        {
            Symbol = GEN1Symbol,
            To = Address.FromBase58(Issuer),
            Amount = IssuedAmount
        }.ToLogEvent();
    }
    
    protected LogEvent IssuedGen9()
    {
        return new Issued
        {
            Symbol = GEN9Symbol,
            To = Address.FromBase58(Issuer),
            Amount = IssuedAmount
        }.ToLogEvent();
    }

    protected LogEvent Confirmed()
    {
        return new Confirmed
        {
            AdoptId = HashHelper.ComputeFrom(AdoptIdString),
            Symbol = GEN1Symbol,
            TokenName = GEN1TokenName,
            TotalSupply = BurnedAmount,
            Owner = Address.FromBase58(Owner),
            Attributes = new Attributes
            {
                Data =
                {
                    new Attribute
                    {
                        TraitType = TraitType1,
                        Value = TraitValue1
                    },
                    new Attribute
                    {
                        TraitType = TraitType2,
                        Value = TraitValue2
                    }
                }
            }
        }.ToLogEvent();
    }
    
    protected LogEvent ConfirmedDuplicateTrait()
    {
        return new Confirmed
        {
            AdoptId = HashHelper.ComputeFrom(AdoptIdString),
            Symbol = "SGR-3",
            TokenName = "SGR-3GEN1",
            TotalSupply = BurnedAmount,
            Owner = Address.FromBase58(Owner),
            Attributes = new Attributes
            {
                Data =
                {
                    new Attribute
                    {
                        TraitType = TraitType1,
                        Value = TraitValue1
                    },
                    new Attribute
                    {
                        TraitType = TraitType2,
                        Value = TraitValue2
                    }
                }
            }
        }.ToLogEvent();
    }
    
    protected LogEvent BurnedGen0()
    {
        return new Burned
        {
            Symbol = GEN0Symbol,
            Amount = BurnedAmount,
            Burner = Address.FromBase58(Issuer)
        }.ToLogEvent();
    }

    protected LogEvent BurnedGen1()
    {
        return new Burned
        {
            Symbol = GEN1Symbol,
            Amount = BurnedAmount,
            Burner = Address.FromBase58(Issuer)
        }.ToLogEvent();
    }
    
    protected LogEvent TransferredGen0()
    {
        return new Transferred
        {
            Symbol = GEN0Symbol,
            Amount = TransferredAmount,
            From = Address.FromBase58(Issuer),
            To = Address.FromBase58(Owner),
        }.ToLogEvent();
    }
    
    protected LogEvent TransferredGen1()
    {
        return new Transferred
        {
            Symbol = GEN1Symbol,
            Amount = TransferredAmount,
            From = Address.FromBase58(Issuer),
            To = Address.FromBase58(Owner),
        }.ToLogEvent();
    }

    protected LogEvent TokenCreatedGen0()
    {
        return new TokenCreated
        {
            Symbol = GEN0Symbol,
            TokenName = GEN0TokenName,
            ExternalInfo = new ExternalInfo
            {
                Value =
                {
                    [InscriptionImageKey] = InscriptionImage
                }
            }
        }.ToLogEvent();
    }
    
    protected LogEvent TokenCreatedGen1()
    {
        return new TokenCreated
        {
            Symbol = GEN1Symbol,
            TokenName = GEN1TokenName,
            ExternalInfo = new ExternalInfo
            {
                Value =
                {
                    [NftAttributes] = "[{traitType:\"traitType1\",value:\"traitValue1\"},{traitType:\"traitType2\",value:\"traitValue2\"}]",
                    [InscriptionImageKey] = InscriptionImage
                }
            }
        }.ToLogEvent();
    }
    
    protected LogEvent TokenCreatedGen9()
    {
        return new TokenCreated
        {
            Symbol = GEN9Symbol,
            TokenName = GEN9TokenName,
            ExternalInfo = new ExternalInfo
            {
                Value =
                {
                    [NftAttributes] = "[{traitType:\"Background\",value:\"Desert Sunrise\"},{traitType:\"Clothes\",value:\"Doraemon\"},{traitType:\"Breed\",value:\"Ragdoll\"},{traitType:\"Ride\",value:\"SUV\"},{traitType:\"Hat\",value:\"Pileus Cap\"},{traitType:\"Mouth\",value:\"Savoring\"},{traitType:\"Zodiac Signs\",value:\"Gemini\"},{traitType:\"Quantum State\",value:\"Wood\"},{traitType:\"Pet\",value:\"Baby Galactic Hedgehog\"},{traitType:\"Eyes\",value:\"Sunglasses\"},{traitType:\"Tail\",value:\"British Shorthair Tail\"}]",
                    [InscriptionImageKey] = InscriptionImage
                }
            }
        }.ToLogEvent();
    }

    protected LogEvent CrossChainReceivedGen0()
    {
        return new CrossChainReceived
        {
            From = Address.FromBase58(Owner),
            To = Address.FromBase58(Issuer),
            Symbol = GEN0Symbol,
            Amount = CrossChainReceivedAmount,
        }.ToLogEvent();
    }
    
    protected LogEvent CrossChainReceivedGen1()
    {
        return new CrossChainReceived
        {
            From = Address.FromBase58(Owner),
            To = Address.FromBase58(Issuer),
            Symbol = GEN1Symbol,
            Amount = CrossChainReceivedAmount,
        }.ToLogEvent();
    }
    
    protected LogEvent Adopted()
    {
        return new Adopted()
        {
            AdoptId = HashHelper.ComputeFrom(AdoptIdString),
            Parent = "test",
            ParentGen = 1,
            InputAmount = 1,
            LossAmount = 1,
            CommissionAmount = 1,
            OutputAmount = 1,
            ImageCount = 1,
            Adopter = Address.FromBase58(Issuer),
            BlockHeight = 1,
            Attributes = new Attributes
            {
                Data =
                {
                    new Attribute
                    {
                        TraitType = TraitType1,
                        Value = TraitValue1
                    },
                    new Attribute
                    {
                        TraitType = TraitType2,
                        Value = TraitValue2
                    }
                }
            },
            Gen = 1,
            Ancestor = Ancestor,
            Symbol = GEN0Symbol,
            TokenName = GEN1TokenName
        }.ToLogEvent();
    }
    protected LogEvent AdoptedGen1()
    {
        return new Adopted()
        {
            AdoptId = HashHelper.ComputeFrom(AdoptIdString),
            Parent = "test",
            ParentGen = 1,
            InputAmount = 1,
            LossAmount = 1,
            CommissionAmount = 1,
            OutputAmount = 1,
            ImageCount = 1,
            Adopter = Address.FromBase58(Issuer),
            BlockHeight = 1,
            Attributes = new Attributes
            {
                Data =
                {
                    new Attribute
                    {
                        TraitType = TraitType1,
                        Value = TraitValue1
                    },
                    new Attribute
                    {
                        TraitType = TraitType2,
                        Value = TraitValue2
                    }
                }
            },
            Gen = 1,
            Ancestor = Ancestor,
            Symbol = GEN1Symbol,
            TokenName = GEN1TokenName
        }.ToLogEvent();
    }
    protected LogEvent AdoptedGen9()
    {
        return new Adopted()
        {
            AdoptId = HashHelper.ComputeFrom(AdoptIdString),
            Parent = "test",
            ParentGen = 1,
            InputAmount = 1,
            LossAmount = 1,
            CommissionAmount = 1,
            OutputAmount = 1,
            ImageCount = 1,
            Adopter = Address.FromBase58(Issuer),
            BlockHeight = 1,
            Attributes = new Attributes
            {
                Data =
                {
                    new Attribute
                    {
                        TraitType = TraitType1,
                        Value = TraitValue1
                    },
                    new Attribute
                    {
                        TraitType = TraitType2,
                        Value = TraitValue2
                    }
                }
            },
            Gen = 9,
            Ancestor = Ancestor,
            Symbol = GEN9Symbol,
            TokenName = GEN9TokenName
        }.ToLogEvent();
    }
}