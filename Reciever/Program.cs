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
        #region NHibernate

        var hibernateConfig = new Configuration();
        hibernateConfig.DataBaseIntegration(x =>
        {
            x.ConnectionString = ConnectionStrings.NserviceBusConnection;
            x.Dialect<MsSql2012Dialect>();
        });

        var outBoxhibernateConfig = new NHibernate.Cfg.Configuration();
        outBoxhibernateConfig.DataBaseIntegration(x =>
        {
            x.ConnectionString = ConnectionStrings.BusuinessConnection;
            x.Dialect<MsSql2012Dialect>();
        });
        outBoxhibernateConfig.SetProperty("default_schema", "dbo");
        #endregion

        var endpointConfiguration = new EndpointConfiguration("Samples.SQLNHibernateOutbox.Receiver");
        endpointConfiguration.UseSerialization<JsonSerializer>();
        #region ReceiverConfiguration

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(ConnectionStrings.NserviceBusConnection);

        
        var routing = transport.Routing();
        routing.RouteToEndpoint(typeof(OrderSubmitted).Assembly, "Samples.SQLNHibernateOutboxEF.Sender");
        routing.RegisterPublisher(typeof(OrderSubmitted).Assembly, "Samples.SQLNHibernateOutboxEF.Sender");


        //var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
        //persistence.UseConfiguration(hibernateConfig);

        //var outboxpersistence = endpointConfiguration.UsePersistence<NHibernatePersistence, StorageType.Outbox>();
        //outboxpersistence.UseConfiguration(outBoxhibernateConfig);

        //var sagaPersistence = endpointConfiguration.UsePersistence<NHibernatePersistence, StorageType.Sagas>();
        //sagaPersistence.UseConfiguration(outBoxhibernateConfig);

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.SqlVariant(SqlVariant.MsSqlServer);
        persistence.ConnectionBuilder(
            connectionBuilder: () => new SqlConnection(ConnectionStrings.NserviceBusConnection));

        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.CacheFor(TimeSpan.FromMinutes(1));


        var outboxEnabled = true;
        EnableOutbox(endpointConfiguration, outboxEnabled);

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
        kernel.Bind<IOrderRepository>()
            .To<OrderRepository>();
        kernel.Bind<IOrderRepository2>()
            .To<OrderRepository2>();

        if (outboxEnabled)
            kernel.Bind<IDbContextProvider>().To<NsbDbContextProvider>();

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

    private static void EnableOutbox(EndpointConfiguration endpoint, bool enable)
    {
        if (enable)
        {
            endpoint.EnableOutbox();
            endpoint.RegisterComponents(x => x.ConfigureComponent<NsbDbContextProvider>(DependencyLifecycle.InstancePerUnitOfWork));
            endpoint.RegisterComponents(x => x.ConfigureComponent<DbContextHelper>(DependencyLifecycle.InstancePerUnitOfWork));
            endpoint.Pipeline.Register<BaseHandlingBehavior.Registration>();
            endpoint.PurgeOnStartup(true);
           // endpoint.ExecuteTheseHandlersFirst(typeof(OrderSubmittedHandler), typeof(OrderSaga));
        }
        else
        {
            endpoint.RegisterComponents(x => x.ConfigureComponent<IDbConnection>(
                factory => new SqlConnection(ConnectionStrings.BusuinessConnection),
                DependencyLifecycle.InstancePerUnitOfWork));
        }

        FeatureToggle.OutBoxEnabled = enable;
    }
}