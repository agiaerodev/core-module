namespace Core.Services.Interfaces
{
    public interface IGPTService
    {
        public Task<string> GenerateFormAsync(string prompt, string? systemPrompt = null);
    }
}
