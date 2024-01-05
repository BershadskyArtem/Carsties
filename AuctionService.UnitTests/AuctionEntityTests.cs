using System.Diagnostics.CodeAnalysis;
using AuctionService.Entities;
using AuctionService.Extensions;

namespace AuctionService.UnitTests;

[SuppressMessage("Naming", "CA1707:Идентификаторы не должны содержать символы подчеркивания", Justification = "This is a test class.")]
public class AuctionEntityTests
{
    /* 
     * Naming of method should be as follows
     * public void Method_Scenario_ExpectedResult
     *
     * We arrange
     * We act
     * We assert
     *
     * Also you can just add entityframeworkcore dependencies in this project so that
     * MSBuild does not complain.
     */ 

    [Fact]
    public void HasReservePrice_ReservePriceGtThanZero_True()
    {
        // Arrange
        var auction = new Auction()
        {
            Id = Guid.NewGuid(),
            ReservePrice = 10
        };
        
        // Act
        var hasReservePrice = auction.HasReservePrice();
        
        // Assert
        Assert.True(hasReservePrice);
    }
    
    [Fact]
    public void HasReservePrice_ReservePriceGtIsZero_False()
    {
        // Arrange
        var auction = new Auction()
        {
            Id = Guid.NewGuid(),
            ReservePrice = 0
        };
        
        // Act
        var hasReservePrice = auction.HasReservePrice();
        
        // Assert
        Assert.False(hasReservePrice);
    }
}