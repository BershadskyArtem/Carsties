using AuctionService.Entities;

namespace AuctionService.Data.Abstractions;

/// <summary>
/// Provides method to access <see cref="Auction"/>
/// </summary>
public interface IAuctionRepository
{
    /// <summary>
    /// Retrieves list of all auctions that comply with the <see cref="dateFilter"/>
    /// </summary>
    /// <param name="dateFilter">Filter below which auctions will be omitted.</param>
    /// <returns>List of <see cref="Auction"/> that comply with filter.</returns>
    Task<List<Auction>> GetAuctionsAsync(DateTime? dateFilter = null);

    /// <summary>
    /// Retrieves <see cref="Auction"/> with the same <see cref="id"/>.
    /// </summary>
    /// <param name="id"> <see cref="Auction"/> id to look for.</param>
    /// <returns>
    /// Null if <see cref="id"/> is not found.
    /// <see cref="Auction"/> if <see cref="id"/> is found.
    /// </returns>
    Task<Auction?> GetAuctionByIdAsync(Guid id);

    /// <summary>
    /// Adds <see cref="Auction"/> to the db.
    /// </summary>
    /// <param name="auction">Auction to add.</param>
    /// <returns>Task to wait for.</returns>
    Task AddAuctionAsync(Auction auction);

    /// <summary>
    /// Removes auction from the db.
    /// </summary>
    /// <param name="auction">Auction to remove.</param>
    void RemoveAuction(Auction auction);
    
    /// <summary>
    /// Saves changes made to the db.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel operation.</param>
    /// <returns>
    /// True if succeeded.
    /// False if not succeeded.
    /// </returns>
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}