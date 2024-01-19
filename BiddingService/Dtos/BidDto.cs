namespace BiddingService.Dtos;

public class BidDto
{
    public string AuctionId { get; set; } = default!;
    public string Bidder { get; set; } = default!;
    public DateTime BidTime { get; set; } = DateTime.UtcNow.ToUniversalTime();
    public int Amount { get; set; }
    public string BidStatus { get; set; } = default!;
}