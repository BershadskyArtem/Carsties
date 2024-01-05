using AuctionService.Entities;

namespace AuctionService.Extensions;

/// <summary>
/// Provides extensions for <see cref="Auction"/> type. <see cref="Auction"/> must not contain any logic.
/// </summary>
public static class AuctionExtensions
{
    /// <summary>
    /// Return if the <see cref="Auction"/>s <see cref="Auction.ReservePrice"/> is greater than zero.
    /// </summary>
    /// <param name="auction">The auction.</param>
    /// <returns>True if <see cref="Auction.ReservePrice"/> is greater than zero.</returns>
    public static bool HasReservePrice(this Auction auction)
    {
        return auction.ReservePrice > 0;
    }
}