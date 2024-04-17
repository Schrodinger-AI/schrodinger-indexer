using AElf.Contracts.MultiToken;
using AElfIndexer.Client.Handlers;
using Newtonsoft.Json;
using Schrodinger.Indexer.Plugin.Entities;
using Schrodinger.Indexer.Plugin.GraphQL.Dto;
using Schrodinger.Indexer.Plugin.GraphQL;
using SchrodingerMain;

namespace Schrodinger.Indexer.Plugin;

public class SchrodingerIndexerClientAutoMapperProfile : IndexerMapperBase
{
    public SchrodingerIndexerClientAutoMapperProfile()
    {
        CreateMap<LogEventContext, SchrodingerHolderIndex>();
        CreateMap<LogEventContext, SchrodingerTraitValueIndex>();
        CreateMap<LogEventContext, SchrodingerSymbolIndex>();
        CreateMap<LogEventContext, SchrodingerAdoptIndex>();
        CreateMap<LogEventContext, SchrodingerResetIndex>();
        
        CreateMap<Issued, SchrodingerHolderIndex>()
            .ForMember(des => des.Address, opt
                => opt.MapFrom(source => MapAddress(source.To)))
            .ForMember(des => des.Amount, opt
                => opt.MapFrom(source => source.Amount))
            ;
        
        CreateMap<Confirmed, SchrodingerHolderIndex>()
            .ForMember(des => des.Address, opt
                => opt.MapFrom(source => MapAddress(source.Owner)))
            .ForMember(des => des.Amount, opt
                => opt.MapFrom(source => source.TotalSupply))
            ;

        CreateMap<SchrodingerHolderIndex, SchrodingerDto>()
            .ForMember(des => des.InscriptionDeploy, opt
                => opt.MapFrom(source => source.SchrodingerInfo.InscriptionDeploy))
            .ForMember(des => des.Decimals, opt
                => opt.MapFrom(source => source.SchrodingerInfo.Decimals))
            .ForMember(des => des.Symbol, opt
                => opt.MapFrom(source => source.SchrodingerInfo.Symbol))
            .ForMember(des => des.TokenName, opt
                => opt.MapFrom(source => source.SchrodingerInfo.TokenName))
            .ForMember(des => des.InscriptionImageUri, opt
                => opt.MapFrom(source => source.SchrodingerInfo.InscriptionImageUri))
            .ForMember(des => des.Generation, opt
                => opt.MapFrom(source => source.SchrodingerInfo.Gen))
            .ForMember(des => des.Traits, opt
                => opt.MapFrom(source => source.Traits.IsNullOrEmpty()?null:source.Traits.Select(item => new SchrodingerDto.TraitsInfo { TraitType = item.TraitType, Value = item.Value }).ToList()))
            ;
        
        CreateMap<SchrodingerAdoptIndex, LatestSchrodingerDto>()
            .ForMember(des => des.InscriptionDeploy, opt
                => opt.MapFrom(source => MapInscriptionDeploy(source.AdoptExternalInfo)))
            .ForMember(des => des.Decimals, opt
                => opt.MapFrom(source => source.Decimals))
            .ForMember(des => des.Symbol, opt
                => opt.MapFrom(source => source.Symbol))
            .ForMember(des => des.TokenName, opt
                => opt.MapFrom(source => source.TokenName))
            .ForMember(des => des.InscriptionImageUri, opt
                => opt.MapFrom(source => source.InscriptionImageUri))
            .ForMember(des => des.Generation, opt
                => opt.MapFrom(source => source.Gen))
            .ForMember(des => des.AdoptTime, opt
                => opt.MapFrom(source => DateTimeHelper.ToUnixTimeMilliseconds(source.AdoptTime)))
            .ForMember(des => des.Amount, opt
                => opt.MapFrom(source => source.OutputAmount))
            .ForMember(des => des.Traits, opt
                => opt.MapFrom(source => source.Attributes.IsNullOrEmpty()?null:source.Attributes.Select(item => new TraitInfos { TraitType = item.TraitType, Value = item.Value }).ToList()))
            ;

        CreateMap<SchrodingerHolderIndex, SchrodingerDetailDto>()
            .ForMember(des => des.Symbol, opt
                => opt.MapFrom(source => source.SchrodingerInfo.Symbol))
            .ForMember(des => des.TokenName, opt
                => opt.MapFrom(source => source.SchrodingerInfo.TokenName))
            .ForMember(des => des.InscriptionImageUri, opt
                => opt.MapFrom(source => source.SchrodingerInfo.InscriptionImageUri))
            .ForMember(des => des.Generation, opt
                => opt.MapFrom(source => source.SchrodingerInfo.Gen))
            .ForMember(des => des.Decimals, opt
                => opt.MapFrom(source => source.SchrodingerInfo.Decimals))
            ;

        CreateMap<Entities.Attribute, TraitInfo>()
            .ForMember(des => des.Value, opt
                => opt.MapFrom(source => source.Value))
            ;

        CreateMap<SchrodingerSymbolIndex, SchrodingerHolderIndex>()
            ;
        
        CreateMap<AttributeSet,Entities.AttributeSet>();
        CreateMap<AttributeInfo,Entities.AttributeInfo>();
        CreateMap<AttributeInfos,Entities.AttributeInfos>();
        CreateMap<LogEventContext, SchrodingerIndex>();
        CreateMap<CrossGenerationConfig,Entities.CrossGenerationConfig>();
        CreateMap<Entities.AttributeSet, AttributeSet>();
        CreateMap<Entities.AttributeInfo, AttributeInfo>();
        CreateMap<Entities.AttributeInfos, AttributeInfos>();
        
        CreateMap<Adopted, SchrodingerAdoptIndex>()
            .ForMember(des => des.Tick, opt
                => opt.MapFrom(source => TokenSymbolHelper.GetTickBySymbol(source.Symbol)))
            .ForMember(des => des.Attributes, opt
                => opt.MapFrom(source => AdoptMapAttributes(source)))
            .ForMember(des => des.AdoptId, opt
                => opt.MapFrom(source => MapHash(source.AdoptId)))
            .ForMember(des => des.Adopter, opt
                => opt.MapFrom(source => MapAddress(source.Adopter)))
            ;
        CreateMap<Attributes, Attribute>();
        CreateMap<Confirmed, SchrodingerAdoptIndex>()
            .ForMember(des => des.Tick, opt
                => opt.MapFrom(source => TokenSymbolHelper.GetTickBySymbol(source.Symbol)))
            .ForMember(des => des.AdoptId, opt
                => opt.MapFrom(source => MapHash(source.AdoptId)))
            .ForMember(des => des.Issuer, opt
                => opt.MapFrom(source => MapAddress(source.Issuer)))
            .ForMember(des => des.Owner, opt
                => opt.MapFrom(source => MapAddress(source.Owner)))
            .ForMember(des => des.Deployer, opt
                => opt.MapFrom(source => MapAddress(source.Deployer)))
            .ForMember(des => des.Attributes, opt
                => opt.MapFrom(source => MapAttributes(source)))
            .ForMember(des => des.AdoptExternalInfo, opt
                => opt.MapFrom(source => MapExternalInfo(source.ExternalInfos.Value)))
            .ForMember(des => des.InscriptionImageUri, opt
                => opt.MapFrom(source => source.ImageUri))
            ;
        CreateMap<SchrodingerAdoptIndex, AdoptInfoDto>()
            .ForMember(t => t.Attributes, m => m.Ignore())
            ;
        CreateMap<Schrodinger.Indexer.Plugin.Entities.Attribute, Trait>();
        CreateMap<CollectionDeployed, SchrodingerIndex>()
            .ForMember(des => des.Tick, opt
                => opt.MapFrom(source => TokenSymbolHelper.GetTickBySymbol(source.Symbol)))
            .ForMember(des => des.Ancestor, opt
                => opt.MapFrom(source => source.Symbol))
            .ForMember(des => des.Issuer, opt
                => opt.MapFrom(source => MapAddress(source.Issuer)))
            .ForMember(des => des.Owner, opt
                => opt.MapFrom(source => MapAddress(source.Owner)))
            .ForMember(des => des.Deployer, opt
                => opt.MapFrom(source => MapAddress(source.Deployer)))
            .ForMember(des => des.ExternalInfo, opt
                => opt.MapFrom(source => MapExternalInfo(source.CollectionExternalInfos.Value)))
            ;
        CreateMap<TraitInfo, TraitDto>();
        CreateMap<Rerolled, SchrodingerResetIndex>();
        
        
        CreateMap<SchrodingerAdoptIndex, StrayCatDto>()
            .ForMember(des => des.InscriptionImageUri, opt
                => opt.MapFrom(source => source.ParentInfo.InscriptionImageUri))
            .ForMember(des => des.ConsumeAmount, opt
                => opt.MapFrom(source => source.InputAmount))
            .ForMember(des => des.ReceivedAmount, opt
                => opt.MapFrom(source => source.OutputAmount))
            .ForMember(des => des.TokenName, opt
                => opt.MapFrom(source => source.ParentInfo.TokenName))
            .ForMember(des => des.Symbol, opt
                => opt.MapFrom(source => source.ParentInfo.Symbol))
            .ForMember(des => des.Gen, opt
                => opt.MapFrom(source => source.ParentInfo.Gen))
            .ForMember(des => des.Decimals, opt
                => opt.MapFrom(source => source.ParentInfo.Decimals))
            ;
        
        CreateMap<TraitInfo, StrayCatTraitsDto>();
        
        CreateMap<LogEventContext, SchrodingerHolderDailyChangeIndex>();
        CreateMap<SchrodingerHolderDailyChangeIndex, SchrodingerHolderDailyChangeDto>();
        CreateMap<SchrodingerSymbolIndex, SchrodingerSymbolDto>();
        //swap token
        CreateMap<Awaken.Contracts.Token.TokenCreated, TokenInfoIndex>();
        CreateMap<LogEventContext, TokenInfoIndex>();
        CreateMap<LogEventContext, SwapLPIndex>();
        CreateMap<LogEventContext, SwapLPDailyIndex>();
        CreateMap<SwapLPDailyIndex, SwapLPDailyDto>();
        
        CreateMap<SchrodingerSymbolIndex, AllSchrodingerDto>()
            .ForMember(des => des.InscriptionDeploy, opt
                => opt.MapFrom(source => source.SchrodingerInfo.InscriptionDeploy))
            .ForMember(des => des.Decimals, opt
                => opt.MapFrom(source => source.SchrodingerInfo.Decimals))
            .ForMember(des => des.Symbol, opt
                => opt.MapFrom(source => source.SchrodingerInfo.Symbol))
            .ForMember(des => des.TokenName, opt
                => opt.MapFrom(source => source.SchrodingerInfo.TokenName))
            .ForMember(des => des.InscriptionImageUri, opt
                => opt.MapFrom(source => source.SchrodingerInfo.InscriptionImageUri))
            .ForMember(des => des.Generation, opt
                => opt.MapFrom(source => source.SchrodingerInfo.Gen))
            .ForMember(des => des.Amount, opt
                => opt.MapFrom(source => source.Amount))
            .ForMember(des => des.Rank, opt
                => opt.MapFrom(source => source.Rank))
            .ForMember(des => des.Level, opt
                => opt.MapFrom(source => source.Level))
            .ForMember(des => des.Grade, opt
                => opt.MapFrom(source => source.Grade))
            .ForMember(des => des.Star, opt
                => opt.MapFrom(source => source.Star))
            .ForMember(des => des.Rarity, opt
                => opt.MapFrom(source => source.Rarity))
            .ForMember(des => des.Traits, opt
                => opt.MapFrom(source => source.Traits.IsNullOrEmpty()?null:source.Traits.Select(item => new AllSchrodingerDto.TraitInfo { TraitType = item.TraitType, Value = item.Value }).ToList()))
            ;
    }
}