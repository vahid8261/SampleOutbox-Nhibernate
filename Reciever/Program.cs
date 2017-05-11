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

        //kernel.Bind<ContextHelper>().To<ContextHelper>().WhenInUnitOfWork().InUnitOfWorkScope();
        //kernel.Bind<ContextHelper>().ToSelf()
        //            .WhenInUnitOfWork()
        //            .InUnitOfWorkScope();

        //kernel.Bind<IOrderRepository2>()
        //    .To<OrderRepository2>();

        //    string connectionString =
        //@"Data Source = (localdb)\MSSQLLocalDB;Integrated Security = True; Persist Security Info=False;Initial Catalog = nservicebus";

        //kernel.Bind<IDbConnection>().ToConstant(new SqlConnection(connectionString));


        //var builder = new ContainerBuilder();
        //builder.RegisterType<OrderRepository>().As<IOrderRepository>();
        //builder.RegisterType<OrderRepository2>().As<IOrderRepository2>();
        //builder.RegisterType<ContextHelper>().As<ContextHelper>().in
        //var container = builder.Build();
        //endpointConfiguration.UseContainer<AutofacBuilder>(
        //    customizations: customizations =>
        //    {
        //        customizations.ExistingLifetimeScope(container);
        //    });


        //endpointConfiguration.RegisterComponents(x => x.ConfigureComponent<ContextHelper>(DependencyLifecycle.InstancePerUnitOfWork));
      
        // endpointConfiguration.RegisterComponents(registration: x => x.ConfigureComponent<IDbTransaction>(componentFactory: c =>
        //{

        //    var ctxhelper = c.Build<ContextHelper>();
        //    //var nsbProvider = new NSBContextProvider()
        //    //{
        //    //    dbConnection = ctxhelper.DbConnection,
        //    //    dbTransaction = ctxhelper.DbTransaction,
        //    //    Ref = ctxhelper.Ref
        //    //};
        //    //ctxhelper.PropertyChanged += nsbProvider.PropertyChangedEventHandler;
        //    //return nsbProvider;
        //    return ctxhelper.GetDbTransaction();
        //}, dependencyLifecycle: DependencyLifecycle.InstancePerUnitOfWork));

        // endpointConfiguration.RegisterComponents(registration: x => x.ConfigureComponent<IDbConnection>(componentFactory: c =>
        // {

        //     var ctxhelper = c.Build<ContextHelper>();
        //     //var nsbProvider = new NSBContextProvider()
        //     //{
        //     //    dbConnection = ctxhelper.DbConnection,
        //     //    dbTransaction = ctxhelper.DbTransaction,
        //     //    Ref = ctxhelper.Ref
        //     //};
        //     //ctxhelper.PropertyChanged += nsbProvider.PropertyChangedEventHandler; 
        //     //return nsbProvider;
        //     return ctxhelper.getDbConnection();
        // }, dependencyLifecycle: DependencyLifecycle.InstancePerUnitOfWork));

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
        endpointConfiguration.LimitMessageProcessingConcurrencyTo(5);

        endpointConfiguration.UseContainer<NinjectBuilder>(
        customizations: customizations =>
        {
            customizations.ExistingKernel(kernel);
        });

        kernel.Bind<ContextHelper>().ToSelf().InThreadScope();

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