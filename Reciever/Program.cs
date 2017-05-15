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

        #endregion

        var endpointConfiguration = new EndpointConfiguration("Samples.SQLNHibernateOutbox.Receiver");
        endpointConfiguration.UseSerialization<JsonSerializer>();
        #region ReceiverConfiguration

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(ConnectionStrings.NserviceBusConnection);

        var routing = transport.Routing();
        routing.RouteToEndpoint(typeof(OrderSubmitted).Assembly, "Samples.SQLNHibernateOutboxEF.Sender");
        routing.RegisterPublisher(typeof(OrderSubmitted).Assembly, "Samples.SQLNHibernateOutboxEF.Sender");


        var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
        persistence.UseConfiguration(hibernateConfig);

        endpointConfiguration.EnableOutbox();

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

        endpointConfiguration.RegisterComponents(registration: x => x.ConfigureComponent<IContextProvider>(componentFactory: c =>
        {
            var ctxhelper = c.Build<ContextHelper>();

            var nsbProvider = new NSBContextProvider()
            {
                DbConnection = ctxhelper.getDbConnection(),
                DbTransaction = ctxhelper.GetDbTransaction(),
                Ref = ctxhelper.Ref
            };
            ctxhelper.PropertyChanged += nsbProvider.PropertyChangedEventHandler;
            return nsbProvider;
        }, dependencyLifecycle: DependencyLifecycle.InstancePerUnitOfWork));

        endpointConfiguration.Pipeline.Register<BaseHandlingBehavior.Registration>();
        endpointConfiguration.PurgeOnStartup(true);
        endpointConfiguration.RegisterComponents(x=> x.ConfigureComponent<ContextHelper>(dependencyLifecycle:DependencyLifecycle.InstancePerUnitOfWork));
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