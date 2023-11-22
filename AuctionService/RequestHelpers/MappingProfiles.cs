using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(auction => auction.Item);
        CreateMap<Item, AuctionDto>();
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(auction => auction.Item,
                mapper =>
                    mapper.MapFrom(auctionDto => auctionDto));
        ;
    }
}