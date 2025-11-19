namespace Core.Services.Interfaces
{
    public interface IReportingService
    {
        public Task<string> GenerateExcelFile(string url);
    }
}
