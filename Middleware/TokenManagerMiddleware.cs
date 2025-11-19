using Core.Exceptions;
using Core.Middleware.TokenManager.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Ihelpers.Middleware.TokenManager.Middleware
{
    /// <summary>
    /// This class implements middleware that manages tokens.
    /// </summary>
    public class TokenManagerMiddleware : IMiddleware
    {
        private readonly ITokenManager _tokenManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// Initializes a new instance of the `TokenManagerMiddleware` class.
        /// </summary>
        /// <param name="tokenManager">The token manager to use for managing tokens.</param>
        public TokenManagerMiddleware(ITokenManager tokenManager, IHttpContextAccessor httpContextAccessor)
        {
            _tokenManager = tokenManager;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorizationHeader.ToString()) && authorizationHeader.ToString() != "null") {

                // Check if the current token is active
                bool isActiveToken = await _tokenManager.IsCurrentActiveToken();

                // If the current token is active, pass the request to the next middleware in the pipeline
                if (isActiveToken == true)
                {
                    await _tokenManager.SetLastRequest();
                    await next(context);
                    return;
                }

                // If the current token is not active, return a unauthorized status code
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
     

                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { data = "YOU ARE NOT AUTHORIZED" })); 


            }
            else
            {
                await next(context);
            }

        }
    }
}
