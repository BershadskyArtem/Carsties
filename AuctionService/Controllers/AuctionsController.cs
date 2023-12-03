using System.Collections;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly AuctionDbContext _context;

    public AuctionsController(IMapper mapper, AuctionDbContext context)
    {
        _mapper = mapper;
        _context = context;
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
            return NotFound();

        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = "test";
        await _context.Auctions.AddAsync(auction);
        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not save changes to the DB");

        var outputAuction = _mapper.Map<AuctionDto>(auction);
        //Name of endpoint, then goes arguments of the endpoint method and then created value.
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, outputAuction);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction([FromRoute] Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context
            .Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction is null)
            return NotFound();

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;

        if (result)
            return Ok();

        return BadRequest("Could not save changes.");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction([FromRoute] Guid id)
    {
        var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id);
        if (auction is null)
            return Ok();

        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
            return BadRequest("Could not update DB");

        return Ok();
    }
}