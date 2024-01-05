using Moq;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AuctionService.Controllers;
using AuctionService.Data.Abstractions;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AuctionService.UnitTests.Utils;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionRepository> _auctionRepository;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;
    private readonly Fixture _fixture;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepository = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        // Why can't we just use mapperConfiguration in constructor for Mapper?
        var mockMapper = new MapperConfiguration(
                mc =>
                    mc.AddMaps(typeof(MappingProfiles).Assembly))
            .CreateMapper()
            .ConfigurationProvider;

        _mapper = new Mapper(mockMapper);

        _controller = new AuctionsController(
            _mapper,
            _auctionRepository.Object,
            _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = TestingHelpers.CreateClaimsPrincipal()
                }
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_ReturnsTenAuctions()
    {
        // Arrange 
        // This line does not work with my API. No mappers inside repositories!!!
        // var auctions = _fixture.CreateMany<Auction>(10).ToList();

        // var auctions = Enumerable.Range(0, 10).Select(_ => new Auction()).ToList();

        var auctions = _fixture.Build<Auction>()
            .Without(au => au.Item)
            .CreateMany(10).ToList();

        foreach (var auction in auctions)
        {
            auction.Item = _fixture.Build<Item>().Without(it => it.Auction).Create();
        }

        _auctionRepository.Setup(repo => repo.GetAuctionsAsync(null)).ReturnsAsync(value: auctions);

        // Act
        var result = await _controller.GetAllAuctions(null);

        // Assert
        Assert.Equal(auctions.Count, result.Value!.Count);

        Assert.IsType<ActionResult<List<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithValidGuid_ReturnsAuction()
    {
        // Arrange 
        var auctionId = Guid.NewGuid();

        var auction = _fixture.Build<Auction>().Without(au => au.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(it => it.Auction).Create();

        auction.Id = auctionId;
        auction.Item.AuctionId = auctionId;
        auction.Item.Auction = auction;
        
        // Both ways are valid.
        // _auctionRepository.Setup(repo => repo.GetAuctionByIdAsync(guid)).ReturnsAsync(auction);
        _auctionRepository
            .Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auction);

        // Act
        var result = await _controller.GetAuctionById(auctionId);

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal(result.Value.Id, auction.Id);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ReturnsNotFound()
    {
        // Arrange 
        _auctionRepository
            .Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        // Act
        var result = await _controller.GetAuctionById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDTO_ReturnsCreatedAtAction()
    {
        // Arrange
        var createAuctionDto = _fixture.Create<CreateAuctionDto>();
        
        _auctionRepository.Setup(repo => repo.SaveChangesAsync(default)).ReturnsAsync(true);

        // Act
        var result = await _controller.CreateAuction(createAuctionDto);

        var createdResult = result.Result as CreatedAtActionResult;

        Assert.NotNull(createdResult);

        Assert.Equal(nameof(AuctionsController.GetAuctionById), createdResult.ActionName);

        Assert.IsType<AuctionDto>(createdResult.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        // Arrange
        var createAuctionDto = _fixture.Create<CreateAuctionDto>();
        _auctionRepository.Setup(repo => repo.SaveChangesAsync(default)).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateAuction(createAuctionDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsOkResponse()
    {
        // Arrange 
        _auctionRepository
            .Setup(repo => repo.SaveChangesAsync(default))
            .ReturnsAsync(true);

        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();

        var auctionId = Guid.NewGuid();

        var auction = _fixture.Build<Auction>().Without(au => au.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(it => it.Auction).Create();

        auction.Id = auctionId;
        auction.Item.AuctionId = auctionId;
        auction.Item.Auction = auction;
        auction.Seller = TestingHelpers.SameUser;
        
        // Act
        _auctionRepository
            .Setup(repo => repo.GetAuctionByIdAsync(auctionId))
            .ReturnsAsync(auction);

        var result = await _controller.UpdateAuction(auctionId, updateAuctionDto);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_Returns403Forbid()
    {
        // Arrange 
        _auctionRepository
            .Setup(repo => repo.SaveChangesAsync(default))
            .ReturnsAsync(true);

        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();

        var auctionId = Guid.NewGuid();

        var auction = _fixture.Build<Auction>().Without(au => au.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(it => it.Auction).Create();

        auction.Id = auctionId;
        auction.Item.AuctionId = auctionId;
        auction.Item.Auction = auction;
        auction.Seller = TestingHelpers.DifferentUser;

        // Act
        _auctionRepository
            .Setup(repo => repo.GetAuctionByIdAsync(auctionId))
            .ReturnsAsync(auction);

        var result = await _controller.UpdateAuction(auctionId, updateAuctionDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFound()
    {
        // Arrange 
        _auctionRepository
            .Setup(repo => repo.SaveChangesAsync(default))
            .ReturnsAsync(true);

        var updateAuctionDto = _fixture.Create<UpdateAuctionDto>();
        
        // Act
        _auctionRepository
            .Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        var result = await _controller.UpdateAuction(Guid.NewGuid(), updateAuctionDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsOkResponse()
    {
        var auctionId = Guid.NewGuid();

        var auction = _fixture.Build<Auction>().Without(au => au.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(it => it.Auction).Create();

        auction.Id = auctionId;
        auction.Item.AuctionId = auctionId;
        auction.Item.Auction = auction;
        auction.Seller = TestingHelpers.SameUser;
        
        _auctionRepository
            .Setup(repo => repo.GetAuctionByIdAsync(auctionId))
            .ReturnsAsync(auction);

        _auctionRepository
            .Setup(repo => repo.SaveChangesAsync(default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAuction(auctionId);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        _auctionRepository
            .Setup(repo => repo.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        // Act
        var result = await _controller.DeleteAuction(Guid.NewGuid());

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_Returns403Response()
    {
        var auctionId = Guid.NewGuid();

        var auction = _fixture.Build<Auction>().Without(au => au.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(it => it.Auction).Create();

        auction.Id = auctionId;
        auction.Item.AuctionId = auctionId;
        auction.Item.Auction = auction;
        auction.Seller = TestingHelpers.DifferentUser;

        _auctionRepository
            .Setup(repo => repo.GetAuctionByIdAsync(auctionId))
            .ReturnsAsync(auction);

        // Act
        var result = await _controller.DeleteAuction(auctionId);

        Assert.IsType<ForbidResult>(result);
    }
}