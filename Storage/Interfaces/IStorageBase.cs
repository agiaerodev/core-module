namespace Core.Storage.Interfaces
{
    /// <summary>
    /// Provides a base interface for storage functionality.
    /// </summary>
    public interface IStorageBase
    {
        /// <summary>
        /// Creates a file with the given name and contents.
        /// </summary>
        /// <param name="fileName">The name of the file to be created.</param>
        /// <param name="fileStream">The stream of data to be written to the file.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<string> CreateFile(string fileName, Stream fileStream, UrlRequestBase request);

        /// <summary>
        /// Reads the contents of a file with the given name.
        /// </summary>
        /// <param name="fileName">The name of the file to be read.</param>
        /// <returns>A task that represents the asynchronous operation,
        /// and returns the contents of the file as a dictionary of key-value pairs.</returns>
        Task<Dictionary<string, object>?> ReadFile(string fileName);

        /// <summary>
        /// Downloads a file with the given name.
        /// </summary>
        /// <param name="fileName">The name of the file to be downloaded.</param>
        /// <returns>A task that represents the asynchronous operation,
        /// and returns a stream of data representing the contents of the file.</returns>
        Task<Stream?> DownloadFile(string fileName);

        Task<bool> RemoveFile(string fileName);
    }
}