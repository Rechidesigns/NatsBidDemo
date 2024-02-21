using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BidService.Model
{
    public class Bid
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string AuctionId { get; set; }
        public double BidAmount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    //Reflects the data structure of an auction
    public class Auction
    {
        public string Id { get; set; }
        public string CarId { get; set; }
        public double StartingPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
