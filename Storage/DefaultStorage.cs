using Core.Storage.Interfaces;

namespace Core.Storage
{
    public class DefaultStorage : IStorageBase
    {
        public async Task<string> CreateFile(string fileName, Stream fileStream, UrlRequestBase request)
        {
            Core.Logger.CoreLogger.LogMessage($"Default storage used for store file '{fileName}'", logType: Ihelpers.Helpers.LogType.Warning);

            return "";
        }

        public async Task<Stream?> DownloadFile(string fileName)
        {
            Core.Logger.CoreLogger.LogMessage($"Default storage used for download file '{fileName}'", logType: Ihelpers.Helpers.LogType.Warning);

            return null;
        }

        public async Task<Dictionary<string, object>?> ReadFile(string fileName)
        {
            Core.Logger.CoreLogger.LogMessage($"Default storage used for read file '{fileName}'", logType: Ihelpers.Helpers.LogType.Warning);

            return null;
        }

        public async Task<bool> RemoveFile(string fileName)
        {
            Core.Logger.CoreLogger.LogMessage($"Default storage used for store file '{fileName}'", logType: Ihelpers.Helpers.LogType.Warning);

            return true;
        }
    }
}
