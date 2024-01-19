using System.Globalization;
using AuctionService.Data;
using Grpc.Core;

namespace AuctionService.Services;

public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
{
    private readonly AuctionDbContext _context;

    public GrpcAuctionService(AuctionDbContext context)
    {
        _context = context;
    }

    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var auctionId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Cannot parse id."));
        }

        var auction = await _context.Auctions.FindAsync(auctionId);

        if (auction is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Not found"));
        }

        var response = new GrpcAuctionResponse()
        {
            Auction = new GrpcAuctionModel()
            {
                AuctionEnd = auction.AuctionEnd.ToString(CultureInfo.InvariantCulture),
                Id = auction.Id.ToString(),
                Seller = auction.Seller,
                ReservePrice = auction.ReservePrice
            }
        };
        
        return response;
    }
}