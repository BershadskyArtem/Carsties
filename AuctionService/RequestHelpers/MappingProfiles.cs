using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(auction => auction.Item);
        CreateMap<Item, AuctionDto>();
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(
                auction => auction.Item,
                mapper =>
                    mapper.MapFrom(auctionDto => auctionDto));

        CreateMap<AuctionDto, AuctionCreated>();
     
        // CreateMap<Item, AuctionUpdated>().ForMember(e => e.Id, (mapper) =>
        // {
        //     mapper.MapFrom(a => a.Id.ToString());
        // });

        CreateMap<Auction, AuctionUpdated>().IncludeMembers(a => a.Item);
        CreateMap<Item, AuctionUpdated>().ForMember(a => a.Id, mapper => mapper.Ignore());
    }
}