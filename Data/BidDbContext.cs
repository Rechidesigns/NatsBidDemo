using BidService.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BidService.Data
{
    public class BidDbContext : DbContext
    {
        public BidDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Auction> Auctions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bid>().HasKey(b => b.Id);
        }
    }

}
