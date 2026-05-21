using Application.DTOs.Aliases;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Aliases;
using AutoMapper;
using Domain.Entities.Aliases;
using Domain.Enums;
using Domain.Exceptions;

namespace Infrastructure.Services.Aliases
{
    public class AliasService : IAliasService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AliasService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AliasResponse> CreateAsync(CreateAliasRequest request)
        {
            var normalizedAlias = NormalizeAlias(request.AliasName);
            if (string.IsNullOrWhiteSpace(normalizedAlias))
                throw new BusinessRuleException("Alias is invalid after normalization.");

            var exists = await _unitOfWork.Alias.EntityExistsAsync(request.EntityId, request.Type);
            if (!exists)
                throw new BusinessRuleException("Target entity does not exist.");

            var duplicate = await _unitOfWork.Alias.GetByNormalizedNameAsync(normalizedAlias, request.Type);
            if(duplicate != null)
                return _mapper.Map<AliasResponse>(duplicate);

            var newAlias = _mapper.Map<Alias>(request);
            newAlias.AliasName = request.AliasName.Trim();
            newAlias.NormalizedAlias = normalizedAlias;

            await _unitOfWork.Alias.Add(newAlias);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AliasResponse>(newAlias);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var alias = await _unitOfWork.Alias.GetById(id);
            if(alias == null)
                return false;

            _unitOfWork.Alias.Remove(alias);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<AliasResponse?> GetByIdAsync(Guid id)
        {
            return _mapper.Map<AliasResponse>(await _unitOfWork.Alias.GetById(id));
        }

        public async Task<AliasResolvedResponse?> ResolveAsync(string value, AliasType type)
        {
            var normalized = NormalizeAlias(value);
            if (string.IsNullOrWhiteSpace(normalized))
                throw new BusinessRuleException("Alias is invalid after normalization.");

            var alias = await _unitOfWork.Alias
                .GetByNormalizedNameAsync(normalized, type);

            if (alias == null)
            {
                return new AliasResolvedResponse
                {
                    Resolved = false,
                    EntityId = null,
                    MatchedAlias = null,
                    Type = type.ToString()
                };
            }

            return new AliasResolvedResponse
            {
                Resolved = true,
                EntityId = alias.EntityId,
                MatchedAlias = alias.AliasName,
                Type = type.ToString()
            };
        }

        public async Task<AliasResponse> UpdateAsync(Guid id, UpdateAliasRequest request)
        {
            var alias = await _unitOfWork.Alias.GetById(id);
            if (alias == null)
                throw new KeyNotFoundException("Alias not found.");

            var targetType = request.Type ?? alias.Type;
            var targetEntityId = request.EntityId ?? alias.EntityId;

            var exists = await _unitOfWork.Alias.EntityExistsAsync(targetEntityId, targetType);
            if (!exists)
                throw new BusinessRuleException("Target entity does not exist.");

            if (!string.IsNullOrWhiteSpace(request.AliasName))
            {
                var normalized = NormalizeAlias(request.AliasName);
                if (string.IsNullOrWhiteSpace(normalized))
                    throw new BusinessRuleException("Alias is invalid after normalization.");

                var duplicate = await _unitOfWork.Alias.GetByNormalizedNameAsync(normalized, targetType);

                if (duplicate != null && duplicate.Id != alias.Id)
                    throw new BusinessRuleException("Alias already exists.");

                alias.AliasName = request.AliasName.Trim();
                alias.NormalizedAlias = normalized;
            }

            alias.Type = targetType;
            alias.EntityId = targetEntityId;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AliasResponse>(alias);
        }

        private static string NormalizeAlias(string value)
        {
            return value
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace("-", "");
        }
    }
}
