﻿namespace Contracts;

public class AuctionCreated
{
    public Guid Id { get; set; }
    public int ReservePrice { get; set; } = 0;
    public string Seller { get; set; } = default!;
    public string? Winner { get; set; }
    public int SoldAmount { get; set; } = 0;
    public int CurrentHighBid { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime AuctionEnd { get; set; }
    public string Status { get; set; } = default!;
    
    public string Make { get; set; } = default!;
    public string Model { get; set; } = default!;
    public int Year { get; set; }
    public string Color { get; set; } = default!;
    public int Mileage { get; set; }
    public string ImageUrl { get; set; } = default!;
}