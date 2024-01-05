using AuctionService.Data.Abstractions;
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data.Implementations;

public class AuctionRepository : IAuctionRepository
{
    private readonly AuctionDbContext _context;

    public AuctionRepository(AuctionDbContext context)
    {
        _context = context;
    }

    public Task<List<Auction>> GetAuctionsAsync(DateTime? dateFilter = null)
    {
        var query = _context.Auctions
            .Include(a => a.Item)
            .OrderBy(a => a.Item.Make)
            .AsQueryable();

        if (dateFilter is not null)
        {
            query = query.Where(a => a.UpdatedAt > dateFilter);
        }

        return query.ToListAsync();
    }

    public Task<Auction?> GetAuctionByIdAsync(Guid id)
    {
        return _context
            .Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAuctionAsync(Auction auction)
    {
        await _context.Auctions.AddAsync(auction);
    }

    public void RemoveAuction(Auction auction)
    {
        _context.Auctions.Remove(auction);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }
}