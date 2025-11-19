using System.Diagnostics;
using System.IO;
using Core.Exceptions;
using Core.Helpers;
using Core.Interfaces;
using Core.Repositories;
using Idata.Data.Entities.Isite;
using Ihelpers.Helpers;
using Ihelpers.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using TypeSupport.Assembly;

namespace Core.Filters.Attributes
{
    public class BasicCachingAttributes : Attribute, IAsyncActionFilter
    {
        private ICacheBase _cacheBase;
        private readonly IServiceProvider _serviceProvider;

        public BasicCachingAttributes(ICacheBase cacheBase, IServiceProvider serviceProvider) 
        { 
            _cacheBase = cacheBase; 
            _serviceProvider = serviceProvider;
        }
        
        /// <summary>
        /// Intercepts the execution of an action to apply caching logic based on the presence
        /// of the<see cref="CachingAttributes"/> attribute on the endpoint.
        /// </summary>
        /// <param name = "context" > The context for the action being executed.</param>
        /// <param name = "next" > The delegate to execute the next middleware or action.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if the action explicitly opts out of caching
            var isActionAuthorized = !context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(IgnoreCaching));
            if (!isActionAuthorized) await next();

            // Try to cast the controller
            var controller = context.Controller as ControllerBase;
            if (controller == null)
            {
                await next();
                return;
            }

            // Try to extract the 'requestBase' parameter from action arguments
            // Try to extract 'requestBase' or 'urlRequestBase' from action arguments
            // Instead of looking for specific keys, try to get the first UrlRequestBase parameter
            var requestBase = context.ActionArguments
                .FirstOrDefault(x => x.Value is UrlRequestBase)
                .Value as UrlRequestBase;

            if (requestBase == null)
            {
                await next();
                return;
            }

            // Retrieve the custom caching attribute from the endpoint
            var endpoint = context.HttpContext.GetEndpoint();
            var attribute = endpoint?.Metadata.GetMetadata<CachingAttributes>();
            if (attribute == null)
            {
                await next();
                return;
            }

            // Resolve the repository from the service provider
            dynamic repository = _serviceProvider.GetService(attribute.RepositoryServiceType)!;
            if (repository == null)
            {
                await next();
                return;
            }

            // Delegate to execute the original action if cache is not found
            Func<Task<IActionResult>> getFreshData = async () =>
            {
                var actionExecutedContext = await next();
                return actionExecutedContext.Result as IActionResult;
            };

            // Execute the caching handler
            var result = await ConfigCacheHandler(
                controller,
                requestBase,
                _cacheBase,
                repository,
                attribute.CacheKey,
                attribute.CacheTag,
                context.HttpContext,
                getFreshData
            );

            // Set the final result of the action
            context.Result = result;
        }

        /// <summary>
        /// Handles caching logic for controller actions. If the data exists in cache, returns it.
        /// Otherwise, fetches fresh data, caches it, and returns the result.
        /// </summary>
        /// <param name="controllerBase">The controller instance to access context and request path.</param>
        /// <param name="requestBase">The request model containing filter, pagination, and user context data.</param>
        /// <param name="cacheBase">Caching service used to store and retrieve data.</param>
        /// <param name="repository">Repository instance used to generate the query and log actions.</param>
        /// <param name="requestingUser">The user requesting the data; used for logging and cache context.</param>
        /// <param name="entityName">The entity name used as a cache key identifier.</param>
        /// <param name="getFreshData">A delegate that retrieves fresh data if cache is empty.</param>
        /// <returns>Returns a cached result or a freshly fetched result wrapped in an IActionResult.</returns>
        public static async Task<IActionResult> ConfigCacheHandler<T>(
            ControllerBase controllerBase,
            UrlRequestBase requestBase,
            ICacheBase cacheBase,
            IRepositoryBase<T> repository,
            string cacheKey,
            string? cacheTag,
            HttpContext httpContext,
            Func<Task<IActionResult>> getFreshData)
        {
            var requestingUser = requestBase.currentContextUser;
            
            try
            {
                // Parse the request context (filtering, pagination, etc.)
                await requestBase.Parse(controllerBase);

                // Exception: verify if the endpoint is /auth/me to set the user id to cacheKey value
                var isUserIdRequiered = await GetUserIdFromToken(httpContext);
                if (httpContext.Request.Path.ToString().EndsWith("/auth/me", StringComparison.OrdinalIgnoreCase)) cacheKey += isUserIdRequiered;
                
                cacheKey += requestBase.filter ?? "";

                // Check if the data is already cached
                var cachedResponse = await cacheBase.GetValue(cacheKey);
                if (cachedResponse != null)
                {
                    _ = Task.Factory.StartNew(() =>
                    repository.LogAction(
                            $"has listed: {cacheKey}",
                            logType: LogType.Information,
                            requestBase: requestBase
                        )
                    );
                    return new ObjectResult(cachedResponse);
                }

                // If not cached, get fresh data
                var freshResult = await getFreshData();

                // Cache the result if it's successful
                if (freshResult is ObjectResult objResult && objResult.StatusCode == 200)
                {
                    //_ = Task.Factory.StartNew(() =>
                    await cacheBase.Remember(
                        cacheKey,
                        objResult.Value!,
                        new List<string>
                        {
                        $"Idata.Data.Entities.Iprofile.User.{isUserIdRequiered ?? "null"}",
                            cacheTag ?? cacheKey
                        }
                    );
                  //  );
                }

                return freshResult;
            }
            catch (Exception ex)
            {
                string requestPath = controllerBase.HttpContext.Request.Path + controllerBase.HttpContext?.Request.QueryString.ToString();

                _ = Task.Factory.StartNew(() =>
                    ExceptionBase.HandleSilentException(
                        ex,
                        "Error inside caching logic",
                        $"ExceptionMessage = {ex.Message}, User = {requestingUser?.email ?? "unknown"}, userId = {requestingUser?.id ?? "null"}, Request = {requestPath}"
                    )
                );

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static async Task<string?> GetUserIdFromToken(HttpContext context)
        {
            var path = context.Request.Path.ToString();
           
            var accessToken = context.Request.Headers["Authorization"].ToString();
            var userId = await JWTHelper.getJWTTokenClaimAsync(accessToken, "UserId");
            return userId;
         
        }
    }

    /// <summary>
    /// Attribute used to enable caching logic on an action method. 
    /// Must specify the repository type to use and the target entity name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CachingAttributes : ServiceFilterAttribute
    {
        /// <summary>
        /// Gets the type of the repository service to be resolved at runtime.
        /// </summary>
        public Type RepositoryServiceType { get; }

        /// <summary>
        /// Gets the name of the entity associated with the cache logic.
        /// </summary>
        public string CacheKey { get; }

        /// <summary>
        /// Gets the name of the tag associated with the cache logic.
        /// </summary>
        public string? CacheTag { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingAttributes"/> class.
        /// </summary>
        /// <param name="repositoryServiceType">The type of the repository service to inject.</param>
        /// <param name="entityName">The name of the entity to associate with the cache.</param>
        public CachingAttributes(Type repositoryServiceType, string cacheKey, string? cacheTag = null)
            : base(typeof(BasicCachingAttributes))
        {
            RepositoryServiceType = repositoryServiceType!;
            CacheKey = cacheKey;
            CacheTag = cacheTag;
        }
    }
}
