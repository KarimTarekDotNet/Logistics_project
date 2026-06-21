namespace Application.Interfaces.Services.System
{
    public interface IIdempotencyService
    {
        Task<bool> TryStartOperation(string key, string value, TimeSpan ttl);
        Task<string?> GetExisting(string key);
    }
}
