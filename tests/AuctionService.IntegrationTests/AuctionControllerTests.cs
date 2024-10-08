
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

public class AuctionControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private string GT_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";
    
    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturn3Auctions()
    {
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");
        Assert.Equal(3, response.Count);
    }
    
    [Fact]
    public async Task GetAuctionById_WithValidIdShouldReturn404()
    {
        var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{GT_ID}");
        Assert.Equal("GT", response.Model);
    }
    
    [Fact]
    public async Task GetAuctionById_WithValidIdShouldReturn400()
    {
        var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAuctionById_WithInvalidShouldReturnAuction()
    {
         var response = await _httpClient.GetAsync($"api/auctions/notguid");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateAuction_ShouldReturn201()
    {
        var auction = GetAuctionForCreate();

        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAtAction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal("test", createdAtAction.Seller);
    }
    
    [Fact]
    public async Task CreateAuction_WithInvalidCreatedAuctionDto_ShouldReturn400()
    {
        var auction = GetAuctionForCreate();
        auction.Model = null;

        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdatedDto_ShouldReturn200()
    {
        var updatedAuction = new UpdateAuctionDto {Make = "updated"};
       
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updatedAuction);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

        [Fact]
    public async Task UpdateAuction_WithInvalidUpdatedDto_ShouldReturn200()
    {
        var updatedAuction = new UpdateAuctionDto {Make = null};
       
        var response = await _httpClient.PutAsJsonAsync($"api/auctions/{GT_ID}", updatedAuction);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDdForTests(db);
        return Task.CompletedTask;
    }

    private CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto 
        {
            Make = "test",
            Model = "testModel",
            ImageUrl = "test",
            Color = "test",
            Mileage  = 10,
            Year = 2010,
            ReservePrice = 10
        };
    }
}
