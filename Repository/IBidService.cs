using BidService.DTO;

namespace BidService.Repository
{
    public interface IBidService
    {
       // Task<BidResponse> ProcessAuctionCreatedAsync(AuctionCreatedDto auctionCreatedDto);
        Task<BidResponse> PlaceBidAsync(BidPlacedDto bidPlacedDto);
    }

}
