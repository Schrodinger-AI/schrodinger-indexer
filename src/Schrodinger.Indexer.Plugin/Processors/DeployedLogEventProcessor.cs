using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schrodinger.Indexer.Plugin.Entities;
using Volo.Abp.Domain.Entities;
using Volo.Abp.ObjectMapping;

namespace Schrodinger.Indexer.Plugin.Processors;

public class DeployedLogEventProcessor: AElfLogEventProcessorBase<Deployed, LogEventInfo>
{
    private readonly IObjectMapper _objectMapper;
    private readonly ContractInfoOptions _contractInfoOptions;
    private readonly ILogger<AElfLogEventProcessorBase<Deployed, LogEventInfo>> _logger;
    private readonly IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo>
        _schrodingerIndexRepository;

    public DeployedLogEventProcessor(ILogger<AElfLogEventProcessorBase<Deployed, LogEventInfo>> logger,
        IObjectMapper objectMapper,
        IAElfIndexerClientEntityRepository<SchrodingerIndex, LogEventInfo> schrodingerIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions) : base(logger)
    {
        _logger = logger;
        _objectMapper = objectMapper;
        _contractInfoOptions = contractInfoOptions.Value;
        _schrodingerIndexRepository = schrodingerIndexRepository;
    }
    
    public override string GetContractAddress(string chainId)
    {
        return _contractInfoOptions.ContractInfos[chainId].SchrodingerContractAddress;
    }

    protected override async Task HandleEventAsync(Deployed eventValue, LogEventContext context)
    {
        if (eventValue == null || context == null) return;
        
        var schrodingerId = IdGenerateHelper.GetId(context.ChainId, eventValue.Tick);
        var fixedAttributeList = new List<Entities.AttributeSet>();
        var randomAttributeList = new List<Entities.AttributeSet>();
        if (eventValue.AttributeLists != null)
        {
            if (eventValue.AttributeLists.FixedAttributes != null)
            {
                fixedAttributeList =
                    _objectMapper.Map<List<AttributeSet>, List<Entities.AttributeSet>>(eventValue.AttributeLists
                        .FixedAttributes.ToList());
            }

            if (eventValue.AttributeLists.RandomAttributes != null)
            {
                randomAttributeList = _objectMapper.Map<List<AttributeSet>, List<Entities.AttributeSet>>(eventValue
                    .AttributeLists.RandomAttributes.ToList());
            }
        }

        var externalInfo = new Dictionary<string, string>();
        if (eventValue.ExternalInfos != null && eventValue.ExternalInfos.Value != null)
        {
            externalInfo = eventValue.ExternalInfos.Value.ToDictionary(item => item.Key, item => item.Value);
        }
        
        var schrodingerIndex = new SchrodingerIndex()
        {
            Id = schrodingerId,
            Tick = eventValue.Tick,
            Issuer = eventValue.Issuer.ToBase58(),
            Owner = eventValue.Owner.ToBase58(),
            Deployer = eventValue.Deployer.ToBase58(),
            TransactionId = context.TransactionId,
            Ancestor = eventValue.Ancestor,
            TokenName = eventValue.TokenName,
            Signatory = eventValue.Signatory?.ToBase58() ?? string.Empty,
            // CollectionExternalInfo = new Dictionary<string, string>(),
            ExternalInfo = externalInfo,
            // Rule = eventValue.coll,
            AttributeSets = new Entities.AttributeSets()
            {
                FixedAttributes = fixedAttributeList,
                RandomAttributes = randomAttributeList,
            },
            CrossGenerationConfig = _objectMapper.Map<CrossGenerationConfig,Entities.CrossGenerationConfig>(eventValue.CrossGenerationConfig),
            TotalSupply = eventValue.TotalSupply,
            IssueChainId = eventValue.IssueChainId,
            MaxGeneration = eventValue.MaxGeneration,
            Decimals = eventValue.Decimals,
            IsWeightEnabled = eventValue.IsWeightEnabled,
            LossRate = Convert.ToDouble(eventValue.LossRate) / 10000
        };
        _objectMapper.Map(context, schrodingerIndex);
        await _schrodingerIndexRepository.AddOrUpdateAsync(schrodingerIndex);
    }
    
}