using BidService.Data;
using BidService.DTO;
using BidService.Model;
using NATS.Client;
using System.Text;

namespace BidService.NatSubscriberService
{

    //public class NatsSubscriberService : BackgroundService
    //{
    //    private readonly IConnection _natsConnection;
    //    private readonly BidDbContext _context;

    //    public NatsSubscriberService(IConnection natsConnection, BidDbContext context)
    //    {
    //        _natsConnection = natsConnection;
    //        _context = context;
    //    }

    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        var subscription = _natsConnection.SubscribeAsync("auction.created", async (sender, args) =>
    //        {
    //            var messageData = Encoding.UTF8.GetString(args.Message.Data);
    //            var auctionCreatedDto = System.Text.Json.JsonSerializer.Deserialize<AuctionCreatedDto>(messageData);

    //            if (auctionCreatedDto != null)
    //            {
    //                var auction = new Auction
    //                {
    //                    Id = auctionCreatedDto.Id.ToString(),
    //                    CarId = auctionCreatedDto.CarId,
    //                    StartingPrice = auctionCreatedDto.StartingPrice,
    //                    //StartTime = auctionCreatedDto.StartTime,
    //                    //EndTime = auctionCreatedDto.EndTime
    //                };

    //                _context.Auctions.Add(auction);
    //                await _context.SaveChangesAsync(); // Assuming asynchronous save for better performance

    //                // Check if the starting price is equal to auctionCreatedDto's starting price
    //                if (auction.StartingPrice == auctionCreatedDto.StartingPrice)
    //                {
    //                    // Send a message "Let continue bidding for a new price" using NATS
    //                    var message = "Let continue bidding for a new price";
    //                    _natsConnection.Publish("bidding.continue", Encoding.UTF8.GetBytes(message));
    //                }
    //            }
    //        });

    //        Console.WriteLine("Subscribe to 'auction.created'. Listening for messages...");

    //        stoppingToken.Register(() =>
    //        {
    //            subscription.Unsubscribe();
    //            _natsConnection.Close();
    //        });

    //        await Task.Delay(Timeout.Infinite, stoppingToken);
    //    }
    //}

    public class NatsSubscriberService : IHostedService
    {
        private readonly IConnection _natsConnection;
        private readonly IServiceProvider _serviceProvider;

        public NatsSubscriberService(IConnection natsConnection, IServiceProvider serviceProvider)
        {
            _natsConnection = natsConnection;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var subscription = _natsConnection.SubscribeAsync("auction.created");
            subscription.MessageHandler += async (sender, args) =>
            {
                var messageData = Encoding.UTF8.GetString(args.Message.Data);
                Console.WriteLine($"Received message: {messageData}"); // 1. Show the received message in the console

                // Processing the message data...
                var auctionCreatedDto = System.Text.Json.JsonSerializer.Deserialize<AuctionCreatedDto>(messageData);
                if (auctionCreatedDto != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<BidDbContext>();
                        var auction = new Auction
                        {
                            Id = auctionCreatedDto.Id.ToString(),
                            CarId = auctionCreatedDto.CarId,
                            StartingPrice = auctionCreatedDto.StartingPrice,
                            // Additional properties...
                        };

                        _context.Auctions.Add(auction);
                        await _context.SaveChangesAsync(); // 2. Save the message data to the database in the background
                    }

                    Console.WriteLine("I have received the auction created. Let's start bidding Alerechi!"); // 3. Display a request message in the console to start bidding

                    // Send a response message back to ActionService with the Auction ID
                    var responseMessage = $"Auction {auctionCreatedDto.Id} is now open for bidding at starting price of {auctionCreatedDto.StartingPrice}";
                    _natsConnection.Publish("auction.response", Encoding.UTF8.GetBytes(responseMessage));
                }
            };
            subscription.Start();
            Console.WriteLine("Subscribed to 'auction.created'. Listening for messages...");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _natsConnection.Drain();
            _natsConnection.Close();
            return Task.CompletedTask;
        }




        //public class NatsSubscriberService : IHostedService
        //{
        //    private readonly IConnection _natsConnection;

        //    public NatsSubscriberService(IConnection natsConnection)
        //    {
        //        _natsConnection = natsConnection;
        //    }

        //    /// <summary>
        //    /// This is he NATs Service that Listen to the messsage pusblished by the Publisher which will display in the console
        //    /// NOTE: you dont have to launch before u receive the message... its uses jetstream to stream the data in the console
        //    /// </summary>
        //    /// <param name="cancellationToken"></param>
        //    /// <returns></returns>
        //    public Task StartAsync(CancellationToken cancellationToken)
        //    {
        //        var subscription = _natsConnection.SubscribeAsync("auction.created");
        //        subscription.MessageHandler += (sender, args) =>
        //        {
        //            var message = Encoding.UTF8.GetString(args.Message.Data);
        //            Console.WriteLine($"Received message: {message}");
        //        };
        //        subscription.Start();
        //        Console.WriteLine("Subscribe to 'auction.created'. Listening for messages...");

        //        return Task.CompletedTask;
        //    }

        //    public Task StopAsync(CancellationToken cancellationToken)
        //    {
        //        // Clean up resources, if necessary
        //        _natsConnection.Drain();
        //        _natsConnection.Close();
        //        return Task.CompletedTask;
        //    }
        //}
    }
}
