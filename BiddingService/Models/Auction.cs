using MongoDB.Entities;

namespace BiddingService.Models;

public class Auction : Entity
{
    public DateTime AuctionEnds { get; set; }
    public string Seller { get; set; } = default!;
    public int ReservePrice { get; set; }
    public bool Finished { get; set; }
}