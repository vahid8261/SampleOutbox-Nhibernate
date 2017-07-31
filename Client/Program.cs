using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence.Sql;
using NServiceBus.Transport.SQLServer;
using Shared;

class Program
{
    static void Main()
    {
        AsyncMain().GetAwaiter().GetResult();
    }

    static async Task AsyncMain()
    {
        Console.Title = "Samples.SQLNHibernateOutboxEF.Sender";
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVXYZ";
        var random = new Random();

        //var hibernateConfig = new Configuration();
        //hibernateConfig.DataBaseIntegration(x =>
        //{
        //    x.ConnectionStringName = "NServiceBus/Persistence";
        //    x.Dialect<MsSql2012Dialect>();
        //});

        //hibernateConfig.SetProperty("default_schema", "dbo");

        var endpointConfiguration = new EndpointConfiguration("Samples.SQLNHibernateOutboxEF.Sender");
        endpointConfiguration.UseSerialization<JsonSerializer>();
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");


        #region SenderConfiguration

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.DefaultSchema("dbo");
        transport.UseSchemaForQueue("error", "dbo");
        transport.UseSchemaForQueue("audit", "dbo");
        transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);

        //endpointConfiguration.DisableFeature<MessageDrivenSubscriptions>();

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.SqlVariant(SqlVariant.MsSqlServer);
        persistence.ConnectionBuilder(
    connectionBuilder: () => new SqlConnection(ConnectionStrings.NserviceBusConnection));

        var routing = transport.Routing();
        routing.RouteToEndpoint(typeof(OrderSubmitted).Assembly, "Samples.SQLNHibernateOutboxEF.Reciever");


        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.CacheFor(TimeSpan.FromMinutes(1));

        // endpointConfiguration.EnableOutbox();

        endpointConfiguration.SendOnly();

        #endregion

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        try
        {
            Console.WriteLine("Press enter to send a message");
            Console.WriteLine("Press any key to exit");

            while (true)
            {
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.Key != ConsoleKey.Enter)
                {
                    return;
                }
                //var orderId = new string(Enumerable.Range(0, 4).Select(x => letters[random.Next(letters.Length)]).ToArray());
                var orderSubmitted = new 
                {
                    OrderId = Guid.NewGuid(),
                    Value = random.Next(100).ToString()
                };
                await endpointInstance.Send(orderSubmitted)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}