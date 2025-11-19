namespace Core.Services.Interfaces
{
    public interface IEmbeddingService
    {
        public Task<float[]> GetEmbeddingAsync(string text);
    }
}
