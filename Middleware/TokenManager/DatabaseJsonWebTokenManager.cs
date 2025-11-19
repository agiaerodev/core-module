using Ihelpers.Helpers;
using Ihelpers.Interfaces;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Core.Middleware.TokenManager.Interfaces;
using Idata.Data;
using Core.Helpers;
using Microsoft.EntityFrameworkCore;
using Core.Exceptions;
using System.Net;
using Core.Logger;
using Idata.Entities.Iprofile;
using Idata.Data.Entities.Iprofile;
namespace Ihelpers.Middleware.TokenManager
{


    /// <summary>
    /// Manages JSON Web Tokens (JWT) within the context of a database.
    /// Provides methods to check the active status of tokens and to deactivate tokens.
    /// </summary>
    public class DatabaseJsonWebTokenManager : ITokenManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IdataContext _dbContext;
        private readonly string _currentToken;
        private readonly string _currentHash;
        private readonly AuthClient? _authClient;
        private readonly User? _authUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseJsonWebTokenManager"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="dbContext">The database context.</param>
        public DatabaseJsonWebTokenManager(IHttpContextAccessor httpContextAccessor, IdataContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _currentToken = GetRequestTokenAsync();
            _currentHash = !string.IsNullOrEmpty(_currentToken) ? CreateClientHash(_currentToken).GetAwaiter().GetResult() : null;
            _authClient = !string.IsNullOrEmpty(_currentHash) ?  _dbContext.AuthClients.Include("user").SingleOrDefault(ac => ac.hash == _currentHash && ac.revoked == false) : null;
            _authUser = _authClient?.user;
        }

        /// <summary>
        /// Checks if the current token is active.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating if the token is active.</returns>
        public async Task<bool> IsCurrentActiveToken() => await IsActiveAsync(_currentToken);

        /// <summary>
        /// Deactivates the current token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeactivateCurrentAsync() => await DeactivateAsync(_currentToken);

        /// <summary>
        /// Checks if the specified token is active.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating if the token is active.</returns>
        public async Task<bool> IsActiveAsync(string token)
        {

            if (string.IsNullOrEmpty(_currentHash))
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                CoreLogger.LogMessage("Invalid token received", logType: LogType.Error, userId: AuthHelper.AuthUser(token)?.id);

                return false;
            }

            if (_authClient == null || _authClient.revoked || _authClient.expires_at < DateTime.UtcNow)
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                CoreLogger.LogMessage("Inactive or non-existent token received", logType: LogType.Error, userId: AuthHelper.AuthUser(token)?.id);

                return false;
            }


            return true;
        }

        /// <summary>
        /// Deactivates the specified token.
        /// </summary>
        /// <param name="token">The token to deactivate.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeactivateAsync(string token)
        {
            if (string.IsNullOrEmpty(_currentHash))
            {
                CoreLogger.LogMessage("Invalid token received", logType: LogType.Error, userId: AuthHelper.AuthUser(token)?.id);
                return;
            }

            if (_authClient != null)
            {
                _authClient.revoked = true;
                _authClient.expires_at = DateTime.UtcNow;
                _dbContext.AuthClients.Update(_authClient);
                await _dbContext.SaveChangesAsync(CancellationToken.None);
            }
        }

        /// <summary>
        /// Retrieves the token from the current request.
        /// </summary>
        /// <returns>The token string.</returns>
        private string GetRequestTokenAsync()
        {
            _httpContextAccessor.HttpContext.Response.Headers["x-app-version"] = ConfigurationHelper.GetConfig<string>("App:Version");
            _httpContextAccessor.HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "x-app-version";

            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            return (authorizationHeader == StringValues.Empty || authorizationHeader.ToString() == "null")
                ? string.Empty
                : authorizationHeader.Single().Split(" ").Last().Trim();
        }

        /// <summary>
        /// Creates a client hash based on the provided token.
        /// </summary>
        /// <param name="token">The token to create a hash for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the client hash string.</returns>
        private async Task<string?> CreateClientHash(string token)
        {
            var user = await AuthHelper.AuthUserAsync(token);
            if (user != null)
            {
                var clientHash = await EncryptionHelper.Encrypt($"{token.Substring(token.Length - 10)}:{user.email}||{user.id}", ConfigurationHelper.GetConfig("EncryptKey:ClientHashesKey"));
                return clientHash;
            }
            else
            {
                return null;
            }
        }

        public async Task SetLastRequest()
        {
           

            if (string.IsNullOrEmpty(_currentHash))
            {
                CoreLogger.LogMessage("Invalid token received", logType: LogType.Error, userId: AuthHelper.AuthUser(_currentToken)?.id);
                return;
            }

            if (_authClient != null)
            {
                _authUser.last_request = _authClient.last_request = DateTime.UtcNow;
                _dbContext.AuthClients.Update(_authClient);
                _dbContext.Users.Update(_authUser);
                await _dbContext.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}
