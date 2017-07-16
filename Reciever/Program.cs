using System;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Autofac;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Ninject;
using NServiceBus;
using NServiceBus.ObjectBuilder.Ninject;
using NServiceBus.Persistence;
using NServiceBus.Persistence.Sql;
using Reciever;
using Shared;
using Configuration = NHibernate.Cfg.Configuration;

class Program
{
    static void Main()
    {
        AsyncMain().GetAwaiter().GetResult();
    }

    static async Task AsyncMain()
    {
        Console.Title = "Samples.SQLNHibernateOutbox.Receiver";

        var endpointConfiguration = new EndpointConfiguration("Samples.SQLNHibernateOutbox.Receiver");
        endpointConfiguration.UseSerialization<JsonSerializer>();
        #region ReceiverConfiguration

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(ConnectionStrings.NserviceBusConnection);

        
        var routing = transport.Routing();
        routing.RouteToEndpoint(typeof(OrderSubmitted).Assembly, "Samples.SQLNHibernateOutboxEF.Sender");
        routing.RegisterPublisher(typeof(OrderSubmitted).Assembly, "Samples.SQLNHibernateOutboxEF.Sender");

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.SqlVariant(SqlVariant.MsSqlServer);
        persistence.ConnectionBuilder(
            connectionBuilder: () => new SqlConnection(ConnectionStrings.NserviceBusConnection));

        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.CacheFor(TimeSpan.FromMinutes(1));


        #endregion

        #region RetriesConfiguration

        endpointConfiguration.Recoverability()
            .Immediate(immediate => immediate.NumberOfRetries(2))
            .Delayed(delayed => delayed.NumberOfRetries(1));

        #endregion

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");
        endpointConfiguration.EnableInstallers();

        var kernel = new StandardKernel();
        endpointConfiguration.UseContainer<NinjectBuilder>(
        customizations: customizations =>
        {
            customizations.ExistingKernel(kernel);
        });

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        try
        {
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
        finally
        {
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }

}