using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MassTransit;
using Contracts;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly AuctionDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(
        IMapper mapper,
        AuctionDbContext context,
        IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string? date)
    {
        var query = _context.Auctions.OrderBy(a => a.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            var dateTime = DateTime.Parse(date).ToUniversalTime();

            query = query.Where(a => a.UpdatedAt > dateTime);
        }

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById([FromRoute] Guid id)
    {
        var auction = await _context
            .Auctions
            .Include(auction => auction.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

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

        await _context.Auctions.AddAsync(auction);

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

        var result = await _context.SaveChangesAsync() > 0;

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
        var auction = await _context
            .Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

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

        // var auctionUpdatedEvent = _mapper.Map<AuctionUpdated>(auction.Item);
        // auctionUpdatedEvent.Id = auction.Id.ToString();

        var auctionUpdatedEvent = _mapper.Map<AuctionUpdated>(auction);

        await _publishEndpoint.Publish<AuctionUpdated>(auctionUpdatedEvent);

        var result = await _context.SaveChangesAsync() > 0;

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
        var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id);
        if (auction is null)
        {
            return Ok();
        }

        if (auction.Seller != User.Identity?.Name)
        {
            return Forbid();
        }

        _context.Auctions.Remove(auction);

        var auctionDeleted = new AuctionDeleted()
        {
            Id = id.ToString(),
        };

        await _publishEndpoint.Publish<AuctionDeleted>(auctionDeleted);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest("Could not update DB");
        }

        return Ok();
    }
}