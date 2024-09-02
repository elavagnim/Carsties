﻿using AuctionService.DTOs;
using AutoMapper;

namespace AuctionService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDto>();
        //d = destination, o = options
        CreateMap<CreateAuctionDto, Auction>().ForMember(d => d.Item, o => o.MapFrom(s => s));
        CreateMap<CreateAuctionDto, Auction>()
                .ForMember(d => d.Item, o => o.MapFrom(s => s));
        CreateMap<CreateAuctionDto, Item>();
    }
}
