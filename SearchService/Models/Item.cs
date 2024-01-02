﻿using MongoDB.Entities;

namespace SearchService.Models;

public class Item : Entity
{
    public int ReservePrice { get; set; }
    
    public string Seller { get; set; } = default!;
    
    public string? Winner { get; set; }
    
    public int SoldAmount { get; set; } 
    
    public int CurrentHighBid { get; set; } 
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime AuctionEnd { get; set; }
    
    // TODO: Resolve nullable later.
    public string Status { get; set; }
    
    public string Make { get; set; } = default!;
    
    public string Model { get; set; } = default!;
    
    public int Year { get; set; }
    
    public string Color { get; set; } = default!;
    
    public int Mileage { get; set; }
    
    public string ImageUrl { get; set; } = default!;
}