using Core.Events;
using Core.Events.Interfaces;
using Core.Factory;
using Core.Filters.Attributes;
using Core.Interfaces;
using Core.Middleware.TokenManager.Interfaces;
using Core.Repositories;
using Core.Services;
using Core.Services;
using Core.Services.Interfaces;
using Core.Services.Interfaces;
using Core.Storage;
using Core.Storage.Interfaces;
using Core.SyncHandlers.Messages;
using Hangfire;
using Idata.Data;
using Ihelpers.Caching;
using Ihelpers.Helpers;
using Ihelpers.Helpers.Interfaces;
using Ihelpers.Interfaces;
using Ihelpers.Messages;
using Ihelpers.Messages.Interfaces;
using Ihelpers.Middleware.TokenManager;
using Ihelpers.Middleware.TokenManager.Middleware;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;


namespace Core
{
    public static class CoreServiceProvider
    {

        /// <summary>
        /// Boot method for initializing the required services for the application Core module
        /// </summary>
        /// <param name="builder">The <see cref="WebApplicationBuilder"/> instance</param>
        /// <returns>The updated <see cref="WebApplicationBuilder"/> instance</returns>
        public static WebApplicationBuilder? Boot(WebApplicationBuilder? builder)
        {
            // Add transient and singleton services for HTTP context accessor   
            builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<IReportingService, ReportingService>();

            // Services for base CRUD repository and controller operations
            builder.Services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));

            // Services for embeddings
            builder.Services.AddScoped(typeof(IEmbeddingService), typeof(EmbeddingService));
            builder.Services.AddScoped(typeof(IGPTService), typeof(GPTService));

            builder.Services.AddTransient(typeof(IClassHelper<>), typeof(ClassHelper<>));
            builder.Services.AddTransient(typeof(IEventBase<>), typeof(EventBase<>));
            builder.Services.AddTransient(typeof(IEventHandlerBase<>), typeof(EventHandlerBase<>));
            builder.Services.AddTransient<IEventBase, EventBase>();
            builder.Services.AddTransient<IEventHandlerBase, EventHandlerBase>();

            // Add HTTP context accessor service
            builder.Services.AddHttpContextAccessor();

            //Parametrized caching
            bool isCacheEnabled = ConfigurationHelper.GetConfig<bool>("DefaultConfigs:Caching:Enabled");

            if (isCacheEnabled)
            {
                string activeProvider = $"Ihelpers.Caching.{ConfigurationHelper.GetConfig<string>("DefaultConfigs:Caching:ActiveProvider")}Cache";
                // Get the type of the caching provider specified in the appsettings.json trough reflection instead of adding it here every new provider
                Type? cachingProviderType = Ihelpers.Helpers.TypeHelper.GetTypeOf(activeProvider);

                if (cachingProviderType != null)
                {
                    // Register the caching provider as a singleton
                    builder.Services.AddSingleton(typeof(ICacheBase), cachingProviderType);
                }
                else
                {
                    throw new InvalidOperationException($"Unable to find the caching provider with the specified type: {activeProvider}");
                }

            }

            //Messaging between modules DEFAULT
            builder.Services.AddSingleton<IMessageProvider, AzureServiceBus>();


            //File report storage DEFAULT
            builder.Services.AddScoped<IStorageBase, AzureStorageBase>();

            // Add token black list handling services
            builder.Services.AddTransient<TokenManagerMiddleware>();
            builder.Services.AddTransient<ITokenManager, DatabaseJsonWebTokenManager>();
            builder.Services.AddScoped<UniversalDispatcher>();
            // Get the connection string from the configuration
            var connectionString = ConfigurationHelper.GetConfig("ConnectionStrings:DefaultConnection");

            // Add Hangfire service with specified options
            builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_110)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSerializerSettings(new Newtonsoft.Json.JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore })
            .UseSqlServerStorage(connectionString, new Hangfire.SqlServer.SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(15),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(15),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

            builder.Services.AddScoped<UseCaching>();
            builder.Services.AddScoped<BasicCachingAttributes>();

            //Repository dependencies factory
            builder.Services.AddTransient(typeof(RepositoryFactory<>));

           
            //Base identification service
            builder.Services.AddTransient(typeof(Core.Helpers.AuthHelper));

            // Return the builder
            return builder;
        }


        /// <summary>
        /// Method to boot the Azure Functions Host Builder core module, for Azure Functions only
        /// </summary>
        /// <param name="builder">The Functions Host Builder instance</param>
        /// <returns>The Functions Host Builder instance with the necessary services added</returns>
        public static IFunctionsHostBuilder? Boot(IFunctionsHostBuilder? builder)
        {
            // Add a transient HTTP context accessor service to the services collection
            builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<IReportingService, ReportingService>();
            // Add services for base CRUD repository operations
            builder.Services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
            builder.Services.AddTransient(typeof(IClassHelper<>), typeof(ClassHelper<>));
            builder.Services.AddTransient(typeof(IEventBase<>), typeof(EventBase<>));
            builder.Services.AddTransient(typeof(IEventHandlerBase<>), typeof(EventHandlerBase<>));

            // Add HTTP context accessor and other necessary services to the collection
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<ICacheBase, MemoryCache>();
            
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IStorageBase, AzureStorageBase>();


            ConfigurationHelper.isAzureFunction = true; 
            // Return the builder instance
            return builder;
        }


        /// <summary>
        /// For configuring inside a azure function or standalone service with dynamic purposes
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="connString"></param>
        public static void Boot(IFunctionsHostBuilder builder, string connString, bool setFunction = false)
        {
            ConfigurationHelper.isAzureFunction = setFunction;
            //Necesary calls for configuring LoggerContext inside an azure function
            builder.Services.AddDbContext<IdataContext>(options =>
            {
                options.UseSqlServer(connString);
            });
            //register conString inside Core
            IdataContext.ConfigureContext(connString);

            bool isCacheEnabled = ConfigurationHelper.GetConfig<bool>("DefaultConfigs:Caching:Enabled");

            if (isCacheEnabled)
            {
                string activeProvider = $"Ihelpers.Caching.{ConfigurationHelper.GetConfig<string>("DefaultConfigs:Caching:ActiveProvider")}Cache";
                // Get the type of the caching provider specified in the appsettings.json trough reflection instead of adding it here every new provider
                Type? cachingProviderType = Ihelpers.Helpers.TypeHelper.GetTypeOf(activeProvider);

                if (cachingProviderType != null)
                {
                    // Register the caching provider as a singleton
                    builder.Services.AddSingleton(typeof(ICacheBase), cachingProviderType);
                }
                else
                {
                    throw new InvalidOperationException($"Unable to find the caching provider with the specified type: {activeProvider}");
                }

            }
        }

    }
}
