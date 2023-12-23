namespace Contracts;

public class AuctionFinished
{
    public bool ItemSold { get; set; }
    public string AuctionId { get; set; } = default!;
    public string Seller { get; set; } = default!;
    public string? Winner { get; set; }
    public int? Amount { get; set; }
}