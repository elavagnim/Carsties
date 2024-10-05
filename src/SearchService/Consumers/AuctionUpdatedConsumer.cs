using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService;

public class AuctionUpdatedConsumer(IMapper mapper) : IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine($"--> Consuming auction updated: {context.Message.Id}");

        var item = mapper.Map<Item>(context.Message);

        if (item.Model == "BrandX") throw new ArgumentException("BrandX is not allowed");

        var result = await DB.Update<Item>()
            .MatchID(item.ID)
            .ModifyOnly(x => new { x.Model, x.Color, x.Make, x.Mileage, x.Year }, item)
            .ExecuteAsync();

        if (!result.IsAcknowledged)
        {
            throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");
        }
    }
}
