using BidService.DTO;
using BidService.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BidService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly IBidService _bidService;

        public BidController(IBidService bidService)
        {
            _bidService = bidService;
        }

        //[HttpPost("process-auction")]
        //public async Task<ActionResult<BidResponse>> ProcessAuctionCreated([FromBody] AuctionCreatedDto auctionCreatedDto)
        //{
        //    var response = await _bidService.ProcessAuctionCreatedAsync(auctionCreatedDto);
        //    return Ok(response);
        //}

        [HttpPost("place-bid")]
        public async Task<ActionResult<BidResponse>> PlaceBid([FromBody] BidPlacedDto bidPlacedDto)
        {
            var response = await _bidService.PlaceBidAsync(bidPlacedDto);
            return Ok(response);
        }
    }
}
