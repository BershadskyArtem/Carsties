using Grpc.Net.Client;
using AuctionService;
using BiddingService.Models;

namespace BiddingService.Services;

public class GrpcAuctionClient
{
    private readonly ILogger<GrpcAuctionClient> _logger;
    private readonly IConfiguration _configuration;

    public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Auction? GetAuction(string id)
    {
        _logger.LogInformation("Calling gRPC to find auction with id {Id}", id);

        var gRpcAddress = _configuration["GrpcAuction"];

        if (gRpcAddress is null)
        {
            throw new ArgumentNullException(nameof(gRpcAddress));
        }
        
        var channel = GrpcChannel.ForAddress(gRpcAddress);
        var client = new GrpcAuction.GrpcAuctionClient(channel);
        var request = new GetAuctionRequest()
        {
            Id = id
        };

        try
        {
            var reply = client.GetAuction(request);
            var foundAuction = reply.Auction;
            var auctionEnd = DateTime.Parse(foundAuction.AuctionEnd);
            var auction = new Auction()
            {
                ID = foundAuction.Id,
                AuctionEnds = auctionEnd,
                Seller = foundAuction.Seller,
                ReservePrice = foundAuction.ReservePrice,
                Finished = auctionEnd < DateTime.UtcNow.ToUniversalTime()
            };
            return auction;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not call gRPC server.");
            return null;
        }
    }
    
}