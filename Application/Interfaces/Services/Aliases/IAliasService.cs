using Application.DTOs.Aliases;
using Domain.Enums;

namespace Application.Interfaces.Services.Aliases
{
    public interface IAliasService
    {
        Task<AliasResponse> CreateAsync(CreateAliasRequest request);
        Task<AliasResponse> UpdateAsync(Guid id, UpdateAliasRequest request);
        Task<bool> DeleteAsync(Guid id);
        Task<AliasResponse?> GetByIdAsync(Guid id);
        Task<AliasResolvedResponse?> ResolveAsync(string value, AliasType type);
    }
}