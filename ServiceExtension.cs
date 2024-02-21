using BidService.Data;
using BidService.NatSubscriberService;
using BidService.Repository;
using Microsoft.EntityFrameworkCore;
using NATS.Client;

namespace BidService
{
    public static class ServiceExtension
    {
        public static void ConfigureConnectionString(this IServiceCollection services, IConfiguration Configuration)
        {

            services.AddDbContext<BidDbContext>(options =>
            {
                options.UseMySql(connectionString: Configuration.GetConnectionString("ApplicationConnectionString"),
                    serverVersion: ServerVersion.AutoDetect(Configuration.GetConnectionString("ApplicationConnectionString")),
                    mySqlOptionsAction: sqlOptions =>
                    {
                        sqlOptions.UseNetTopologySuite();
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    });
            });

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = "nats://localhost:4222";
            options.MaxReconnect = Options.ReconnectForever;
            options.ReconnectWait = 2000;

            var natsConnection = new ConnectionFactory().CreateConnection(options);
            //var natsConnection = new ConnectionFactory().CreateConnection("nats://localhost:4222");
            services.AddSingleton<IConnection>(natsConnection);
            services.AddScoped<IBidService, BidServices>();
            var natsUrl = Configuration.GetValue<string>("Nats:Url");
            services.AddSingleton(new NatsConnector(natsUrl));


            services.AddHostedService<NatsSubscriberService>();

        }
    }
}
