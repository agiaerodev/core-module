namespace Core.Middleware.TokenManager.Interfaces
{
    /// <summary>
    /// This interface defines the methods for managing tokens.
    /// </summary>
    public interface ITokenManager
    {
        /// <summary>
        /// Determines if the current token is active.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the current token is active or not.</returns>
        Task<bool> IsCurrentActiveToken();

        /// <summary>
        /// Deactivates the current token.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeactivateCurrentAsync();

        /// <summary>
        /// Determines if the specified token is active.
        /// </summary>
        /// <param name="token">The token to check for activity.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the specified token is active or not.</returns>
        Task<bool> IsActiveAsync(string token);

        /// <summary>
        /// Deactivates the specified token.
        /// </summary>
        /// <param name="token">The token to deactivate.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeactivateAsync(string token);


        /// <summary>
        /// Updates lastRequest field to UTC now, to see wich person is working
        /// </summary>
        /// <returns></returns>
        Task SetLastRequest();

    }
}
