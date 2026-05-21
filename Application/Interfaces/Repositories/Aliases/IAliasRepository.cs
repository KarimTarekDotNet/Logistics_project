using Domain.Entities.Aliases;
using Domain.Enums;

namespace Application.Interfaces.Repositories.Aliases
{
    public interface IAliasRepository
    {
        Task Add(Alias alias);
        void Remove(Alias alias);
        void Update(Alias alias);
        Task<Alias?> GetById(Guid id);
        Task<Alias?> GetByName(string name);
        Task<Alias?> GetByNormalizedNameAsync(string normalizeName, AliasType type);
        Task<bool> EntityExistsAsync(Guid id, AliasType type);
    }
}