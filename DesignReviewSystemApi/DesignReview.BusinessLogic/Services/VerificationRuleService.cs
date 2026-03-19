using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.BusinessLogic.DbContext;
using DesignReview.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DesignReview.BusinessLogic.Services
{
    public class VerificationRuleService : IVerificationRuleService
    {
        private readonly DesignReviewDbContext _db;

        public VerificationRuleService(DesignReviewDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<VerificationRuleDto>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _db.VerificationRules.AsNoTracking();
            if (activeOnly)
                query = query.Where(r => r.IsActive);
            var list = await query.OrderBy(r => r.DisplayOrder).ThenBy(r => r.RuleName).ToListAsync(cancellationToken);
            return list.Select(MapToDto).ToList();
        }

        public async Task<VerificationRuleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var rule = await _db.VerificationRules.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            return rule == null ? null : MapToDto(rule);
        }

        public async Task<VerificationRuleDto> CreateAsync(CreateVerificationRuleDto dto, CancellationToken cancellationToken = default)
        {
            var rule = new VerificationRule
            {
                Id = Guid.NewGuid(),
                RuleName = dto.RuleName,
                Description = dto.Description,
                RuleType = dto.RuleType,
                SearchPatterns = string.IsNullOrWhiteSpace(dto.SearchPatterns) ? "[]" : dto.SearchPatterns,
                ValidationPattern = dto.ValidationPattern,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };
            _db.VerificationRules.Add(rule);
            await _db.SaveChangesAsync(cancellationToken);
            return MapToDto(rule);
        }

        public async Task<VerificationRuleDto?> UpdateAsync(Guid id, UpdateVerificationRuleDto dto, CancellationToken cancellationToken = default)
        {
            var rule = await _db.VerificationRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            if (rule == null)
                return null;
            rule.RuleName = dto.RuleName;
            rule.Description = dto.Description;
            rule.RuleType = dto.RuleType;
            rule.SearchPatterns = string.IsNullOrWhiteSpace(dto.SearchPatterns) ? "[]" : dto.SearchPatterns;
            rule.ValidationPattern = dto.ValidationPattern;
            rule.IsActive = dto.IsActive;
            rule.DisplayOrder = dto.DisplayOrder;
            rule.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return MapToDto(rule);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var rule = await _db.VerificationRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            if (rule == null)
                return false;
            _db.VerificationRules.Remove(rule);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static VerificationRuleDto MapToDto(VerificationRule r) => new()
        {
            Id = r.Id,
            RuleName = r.RuleName,
            Description = r.Description,
            RuleType = r.RuleType,
            SearchPatterns = r.SearchPatterns,
            ValidationPattern = r.ValidationPattern,
            IsActive = r.IsActive,
            DisplayOrder = r.DisplayOrder,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}
