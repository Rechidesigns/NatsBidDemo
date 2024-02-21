namespace BidService.DTO
{
    public class BidPlacedDto
    {
        public string AuctionId { get; set; }
        public double BidAmount { get; set; }
    }

    public class AuctionCreatedDto
    {
        public string Id { get; set; }
        public string CarId { get; set; }
        public double StartingPrice { get; set; }
    }
    public class BidResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string BidId { get; set; }
    }
}
