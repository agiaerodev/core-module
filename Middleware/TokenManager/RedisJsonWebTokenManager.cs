using Ihelpers.Helpers;
using Ihelpers.Interfaces;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Core.Middleware.TokenManager.Interfaces;
namespace Ihelpers.Middleware.TokenManager
{


    /// <summary>
    /// This class implements the `ITokenManager` interface to manage JSON Web Tokens.
    /// </summary>
    public class RedisJsonWebTokenManager : ITokenManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the `JsonWebTokenManager` class.
        /// </summary>
        /// <param name="cache">The cache to use for storing token information.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor to use for accessing the current HTTP context.</param>
        public RedisJsonWebTokenManager(ICacheBase cache, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Determines whether the current token is active.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a boolean value indicating whether the current token is active.</returns>
        public async Task<bool> IsCurrentActiveToken() => await IsActiveAsync(GetCurrentAsync());

        /// <summary>
        /// Deactivates the current token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeactivateCurrentAsync() => await DeactivateAsync(GetCurrentAsync());

        /// <summary>
        /// Determines whether a specific token is active.
        /// </summary>
        /// <param name="token">The token to check for activity.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean value indicating whether the specified token is active.</returns>
        public async Task<bool> IsActiveAsync(string token) => await Ihelpers.Extensions.ConfigContainer.cache.GetValue(token) is null;

        /// <summary>
        /// Deactivates a specific token.
        /// </summary>
        /// <param name="token">The token to deactivate.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeactivateAsync(string token)
        {
            // Get the expiration time from the token
            string expiringDateString = await JWTHelper.getJWTTokenClaimAsync(token, "expiresIn");
            DateTime expiringDate = DateTime.Parse(expiringDateString);

            DateTime dateTime = DateTime.UtcNow;
            var totalMinutesLeft = Math.Abs((dateTime - expiringDate).TotalMinutes);

            // Add the token to the cache
           await Ihelpers.Extensions.ConfigContainer.cache.CreateValue(token, false, totalMinutesLeft);
        }

        /// <summary>
        /// Gets the current token.
        /// </summary>
        /// <returns>The current token.</returns>
        private string GetCurrentAsync()
        {
            // Add the current version of the app to the response headers
            _httpContextAccessor.HttpContext.Response.Headers["x-app-version"] = ConfigurationHelper.GetConfig<string>("App:Version");
            _httpContextAccessor.HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "x-app-version";

            // Get the authorization header from the current HTTP request
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
    
            //return token without "Bearer" string
            return authorizationHeader == StringValues.Empty
                ? string.Empty
                : authorizationHeader.Single().Split(" ").Last().Trim();

        }

        public async Task SetLastRequest()
        {
            return;
        }
    }
}
