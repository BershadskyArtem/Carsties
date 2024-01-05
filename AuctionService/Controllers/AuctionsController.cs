using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using MassTransit;
using Contracts;
using AuctionService.Data.Abstractions;
using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(
        IMapper mapper,
        IAuctionRepository auctionRepository,
        IPublishEndpoint publishEndpoint
    )
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _auctionRepository = auctionRepository;
    }

    [HttpGet]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string? date)
    {
        DateTime? dateFilter = null;

        if (date is not null && DateTime.TryParse(date, out var parsedDateFilter))
        {
            dateFilter = parsedDateFilter.ToUniversalTime();
        }

        var auctions = await _auctionRepository.GetAuctionsAsync(dateFilter);

        var auctionDtos = _mapper.Map<List<AuctionDto>>(auctions);

        return auctionDtos;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById([FromRoute] Guid id)
    {
        var auction = await _auctionRepository.GetAuctionByIdAsync(id);

        if (auction is null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(auction);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);

        if (User.Identity is not null && User.Identity.Name is not null)
        {
            auction.Seller = User.Identity.Name;
        }
        else
        {
            return Forbid();
        }

        await _auctionRepository.AddAuctionAsync(auction);

        var outputAuction = _mapper.Map<AuctionDto>(auction);

        // You can move this up because outbox pattern is now embedded inside AuctionDbContext
        var auctionCreatedEvent = _mapper.Map<AuctionCreated>(outputAuction);

        // Publish saves event in EF Core. Using outbox config in Program.cs
        await _publishEndpoint.Publish(auctionCreatedEvent);

        // I add following code to test my theories about Mass Transit.
        await _publishEndpoint.Publish(new TestingContract()
        {
            Message = Guid.NewGuid().ToString(),
        });

        var result = await _auctionRepository.SaveChangesAsync();

        if (!result)
        {
            return BadRequest("Could not save changes to the DB");
        }

        // Name of endpoint, then goes arguments of the endpoint method and then created value.
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, outputAuction);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction([FromRoute] Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _auctionRepository.GetAuctionByIdAsync(id);

        if (auction is null)
        {
            return NotFound();
        }

        if (auction.Seller != User.Identity?.Name)
        {
            return Forbid();
        }

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var auctionUpdatedEvent = _mapper.Map<AuctionUpdated>(auction);

        await _publishEndpoint.Publish<AuctionUpdated>(auctionUpdatedEvent);

        var result = await _auctionRepository.SaveChangesAsync();

        if (result)
        {
            return Ok();
        }

        return BadRequest("Could not save changes.");
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction([FromRoute] Guid id)
    {
        var auction = await _auctionRepository.GetAuctionByIdAsync(id);
        
        if (auction is null)
        {
            return Ok();
        }

        if (auction.Seller != User.Identity?.Name)
        {
            return Forbid();
        }

        _auctionRepository.RemoveAuction(auction);

        var auctionDeleted = new AuctionDeleted()
        {
            Id = id.ToString(),
        };

        await _publishEndpoint.Publish<AuctionDeleted>(auctionDeleted);

        var result = await _auctionRepository.SaveChangesAsync();

        if (!result)
        {
            return BadRequest("Could not update DB");
        }

        return Ok();
    }
}