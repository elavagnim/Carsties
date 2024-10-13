using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly IAuctionRepository _auctionRepository = Substitute.For<IAuctionRepository>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly IMapper _mapper; private readonly Fixture _fixture;
    private readonly AuctionsController _controller;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _controller = new AuctionsController(_auctionRepository, _mapper, _publishEndpoint);

        var mockMapper = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapper);
        _controller = new AuctionsController(_auctionRepository, _mapper, _publishEndpoint);
    }

    [Fact]
    public async Task GetAllAuctions_WithNoParams_ShouldReturns10Auctions()
    {
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepository.GetAuctionsAsync(Arg.Any<string>()).Returns(auctions);

        var result = await _controller.GetAllAuctions(null);

        Assert.Equal(10, result?.Value?.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithValidGuid_ShouldReturnsAuction()
    {
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepository.GetAuctionByIdAsync(Arg.Any<Guid>()).Returns(auction);

        var result = await _controller.GetAuctionById(auction.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAuction = Assert.IsType<AuctionDto>(okResult.Value);
        Assert.Equal(auction.Make, returnedAuction.Make);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ReturnsNotFound()
    {
        AuctionDto? auction = null;
        _auctionRepository.GetAuctionByIdAsync(Arg.Any<Guid>()).Returns(auction);

        var result = await _controller.GetAuctionById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtAction()
    {
        var auctiondto = _fixture.Create<CreateAuctionDto>();
        _auctionRepository.AddAuction(Arg.Any<Auction>());
        _auctionRepository.SaveChangesAsync().Returns(true);

        var result = await _controller.CreateAuction(auctiondto);
        var createdResult = result.Result as CreatedAtActionResult;

        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDto>(createdResult.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        var auctiondto = _fixture.Create<CreateAuctionDto>();
        _auctionRepository.AddAuction(Arg.Any<Auction>());
        _auctionRepository.SaveChangesAsync().Returns(false);

        var result = await _controller.CreateAuction(auctiondto);
        Assert.IsType<BadRequestObjectResult>(result.Result);

    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        var updatedDto = _fixture.Create<UpdateAuctionDto>();

        _auctionRepository.GetAuctionEntityByIdAsync(Arg.Any<Guid>()).Returns(auction);
        _auctionRepository.SaveChangesAsync().Returns(true);

        var result = await _controller.UpdateAuction(auction.Id, updatedDto);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        var updatedDto = _fixture.Create<UpdateAuctionDto>();

        _auctionRepository.AddAuction(Arg.Any<Auction>());
        _auctionRepository.SaveChangesAsync().Returns(false);

        var result = await _controller.UpdateAuction(auction.Id, updatedDto);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _auctionRepository.GetAuctionEntityByIdAsync(Arg.Any<Guid>()).Returns(auction);
        _auctionRepository.RemoveAuction(Arg.Any<Auction>());
        _auctionRepository.SaveChangesAsync().Returns(true);

        var result = await _controller.DeleteAuction(auction.Id);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        var id = Guid.NewGuid();

        _auctionRepository.RemoveAuction(Arg.Any<Auction>());
        _auctionRepository.SaveChangesAsync().Returns(false);

        var result = await _controller.DeleteAuction(id);
        Assert.IsType<NotFoundResult>(result);
    }
}
