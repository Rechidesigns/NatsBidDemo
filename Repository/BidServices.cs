using BidService.Data;
using BidService.DTO;
using BidService.Model;
using Microsoft.EntityFrameworkCore;
using NATS.Client;
using Newtonsoft.Json;
using System.Text;

namespace BidService.Repository
{
    public class BidServices : IBidService
    {
        private readonly IConnection _connection;
        private readonly BidDbContext _context;
        private readonly NatsConnector _natsConnector;
        public BidServices(IConnection natsConnection, BidDbContext bidDbContext, NatsConnector natsConnector)
        {
            _connection = natsConnection;
            _context = bidDbContext;
            _natsConnector = natsConnector;

            // Assuming you want to use a durable name without a queue group
            _natsConnector.Subscribe("auction.created", "bidServiceDurable", HandleAuctionCreatedMessage);

        }
        //private void HandleAuctionCreatedMessage(object sender, MsgHandlerEventArgs e)
        //{
        //    var messageData = Encoding.UTF8.GetString(e.Message.Data);
        //    var auctionCreatedDto = System.Text.Json.JsonSerializer.Deserialize<AuctionCreatedDto>(messageData);

        //    if (auctionCreatedDto != null)
        //    {
        //        var auctionId = auctionCreatedDto.Id.ToString();
        //        var existingAuction = _context.Auctions.Local.FirstOrDefault(a => a.Id == auctionId);

        //        if (existingAuction != null)
        //        {
        //            existingAuction.Id = auctionId;
        //            existingAuction.CarId = auctionCreatedDto.CarId;
        //            existingAuction.StartingPrice = auctionCreatedDto.StartingPrice;

        //            _context.Auctions.Update(existingAuction);
        //        }
        //        else
        //        {
        //            var auction = new Auction
        //            {
        //                Id = auctionCreatedDto.Id.ToString(),
        //                CarId = auctionCreatedDto.CarId,
        //                StartingPrice = auctionCreatedDto.StartingPrice,
        //            };

        //            _context.Auctions.Add(auction);
        //        }

        //        //_context.SaveChanges();
        //        //ProcessAuctionCreatedAsync(auctionCreatedDto).Wait();
        //    }
        //}
        private void HandleAuctionCreatedMessage(object sender, MsgHandlerEventArgs e)
        {
            var messageData = Encoding.UTF8.GetString(e.Message.Data);
            var auctionCreatedDto = System.Text.Json.JsonSerializer.Deserialize<AuctionCreatedDto>(messageData);

            if (auctionCreatedDto != null)
            {
                var auction = new Auction
                {
                    Id = auctionCreatedDto.Id.ToString(),
                    CarId = auctionCreatedDto.CarId,
                    StartingPrice = auctionCreatedDto.StartingPrice,

                };

                _context.Auctions.Add(auction);
                //_context.SaveChanges();
                //ProcessAuctionCreatedAsync(auctionCreatedDto).Wait();
            }
        }
        public async Task<BidResponse> PlaceBidAsync(BidPlacedDto bidPlacedDto)
        {
            var auctionExists = await _context.Auctions.Where(a => a.Id == bidPlacedDto.AuctionId).FirstOrDefaultAsync();
            if (bidPlacedDto.BidAmount <= 0)
            {
                return new BidResponse
                {
                    Success = false,
                    Message = "Invalid bid amount.",
                    BidId = null
                };
            }

            if (auctionExists == null)
            {
                return new BidResponse
                {
                    Success = false,
                    Message = "Auction does not exist.",
                    BidId = null
                };
            }
            var bid = new Bid
            {
                AuctionId = bidPlacedDto.AuctionId,
                BidAmount = bidPlacedDto.BidAmount,
                Timestamp = DateTime.UtcNow
            };

            _context.Bids.Add(bid);
            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return new BidResponse
                {
                    Success = false,
                    Message = "Error placing bid. Please try again later.",
                    BidId = null
                };
            }

            var message = JsonConvert.SerializeObject(bidPlacedDto);
            _connection.Publish("bid.placed", Encoding.UTF8.GetBytes(message));

            return new BidResponse
            {
                Success = true,
                Message = "Bid placed successfully.",
                BidId = bid.Id,
            };
        }
        
    }
}
