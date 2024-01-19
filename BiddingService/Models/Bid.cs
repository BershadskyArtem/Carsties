using MongoDB.Entities;

namespace BiddingService.Models;

public class Bid : Entity
{
    public string AuctionId { get; set; } = default!;
    public string Bidder { get; set; } = default!;
    public DateTime BidTime { get; set; } = DateTime.UtcNow.ToUniversalTime();
    public int Amount { get; set; }
    public BidStatus BidStatus { get; set; }
}