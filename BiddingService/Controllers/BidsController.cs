using AutoMapper;
using MongoDB.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BiddingService.Dtos;
using BiddingService.Models;
using BiddingService.Services;
using Contracts;
using MassTransit;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/bids")]
public class BidsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly GrpcAuctionClient _auctionClient;

    public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, GrpcAuctionClient auctionClient)
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _auctionClient = auctionClient;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        var auction = await DB.Find<Auction>().OneAsync(auctionId);

        if (auction is null)
        {
            auction = _auctionClient.GetAuction(auctionId);

            if (auction is null)
            {
                return NotFound();
            }
        }

        if (auction.Seller == User.Identity?.Name)
        {
            return BadRequest("Cannot bid on your own auction.");
        }

        var bid = new Bid()
        {
            Amount = amount,
            AuctionId = auctionId,
            Bidder = User.Identity!.Name!,
        };

        if (auction.AuctionEnds < DateTime.UtcNow.ToUniversalTime())
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highestBid = await DB.Find<Bid>()
                .Match(b => b.AuctionId == auctionId)
                .Sort(s => s.Descending(df => df.Amount))
                .ExecuteFirstAsync();

            if ((highestBid is not null && amount > highestBid.Amount) || (highestBid is null) )
            {
                bid.BidStatus = amount > auction.ReservePrice ? BidStatus.Accepted : BidStatus.AcceptedBelowReserve;
            }

            if (highestBid is not null && bid.Amount <= highestBid.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }
        
        await DB.SaveAsync(bid);

        var bidPlacedEvent = _mapper.Map<BidPlaced>(bid);
        
        await _publishEndpoint.Publish(bidPlacedEvent);
        
        var bidDto = _mapper.Map<BidDto>(bid);
        
        return Ok(bidDto);
    }

    [HttpGet("{auctionId}")]
    public async Task<List<BidDto>> GetBidsForAuction(string auctionId)
    {
        var bids = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .Sort(sd => sd.Descending(b => b.BidTime))
            .ExecuteAsync();

        var listOfBids = _mapper.Map<List<BidDto>>(bids);
        
        return listOfBids;
    }
}