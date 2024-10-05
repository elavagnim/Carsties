using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService;

public class AuctionCreatedConsumer(IMapper mapper) : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"--> Consuming auction created: {context.Message.Id}");

        var item = mapper.Map<Item>(context.Message);

        if (item.Model == "BrandX") throw new ArgumentException("BrandX is not allowed");

        await item.SaveAsync();
    }
}
