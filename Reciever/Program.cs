using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Ninject;
using NServiceBus;
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
            x.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Database=nservicebus;Integrated Security=True";
            x.Dialect<MsSql2012Dialect>();
        });

        #endregion

    //    new SchemaExport(hibernateConfig).Execute(false, true, false);

        var endpointConfiguration = new EndpointConfiguration("Samples.SQLNHibernateOutbox.Receiver");
        endpointConfiguration.UseSerialization<JsonSerializer>();
        #region ReceiverConfiguration

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(@"Data Source=(localdb)\MSSQLLocalDB;Database=nservicebus;Integrated Security=True");

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

        kernel.Bind<IOrderRepository2>()
            .To<OrderRepository2>();

    //    string connectionString =
    //@"Data Source = (localdb)\MSSQLLocalDB;Integrated Security = True; Persist Security Info=False;Initial Catalog = nservicebus";

        //kernel.Bind<IDbConnection>().ToConstant(new SqlConnection(connectionString));

        endpointConfiguration.UseContainer<NinjectBuilder>(
            customizations: customizations =>
            {
                customizations.ExistingKernel(kernel);
            });

        endpointConfiguration.RegisterComponents(x => x.ConfigureComponent<ContextHelper>(DependencyLifecycle.InstancePerUnitOfWork));

        endpointConfiguration.Pipeline.Register<BaseHandlingBehavior.Registration>();


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