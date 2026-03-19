using DesignReview.Application.Interfaces;
using DesignReview.Application.Models;
using DesignReview.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignReview.API.Controllers
{
    /// <summary>
    /// Verification rule templates. Reviewers can read rules; only Admin can create/update/delete.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Reviewer")]
    public class RulesController : BaseApiController<RulesController>
    {
        private readonly IVerificationRuleService _ruleService;

        public RulesController(
            ILogger<RulesController> logger,
            IVerificationRuleService ruleService,
            IUserHelperService userHelper)
            : base(logger, userHelper)
        {
            _ruleService = ruleService;
        }

        /// <summary>
        /// List all verification rules. Optional filter: activeOnly.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<VerificationRuleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<VerificationRuleDto>>> GetList(
            [FromQuery] bool activeOnly = false,
            CancellationToken cancellationToken = default)
        {
            var list = await _ruleService.GetAllAsync(activeOnly, cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Get a single rule by id.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(VerificationRuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VerificationRuleDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var rule = await _ruleService.GetByIdAsync(id, cancellationToken);
            if (rule == null)
                return NotFound(new { message = "Rule not found." });
            return Ok(rule);
        }

        /// <summary>
        /// Create a new verification rule. Admin only.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(typeof(VerificationRuleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VerificationRuleDto>> Create(
            [FromBody] CreateVerificationRuleDto dto,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.RuleName))
                return BadRequest(new { message = "RuleName is required." });
            var rule = await _ruleService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
        }

        /// <summary>
        /// Update an existing verification rule. Admin only.
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(typeof(VerificationRuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VerificationRuleDto>> Update(
            Guid id,
            [FromBody] UpdateVerificationRuleDto dto,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.RuleName))
                return BadRequest(new { message = "RuleName is required." });
            var rule = await _ruleService.UpdateAsync(id, dto, cancellationToken);
            if (rule == null)
                return NotFound(new { message = "Rule not found." });
            return Ok(rule);
        }

        /// <summary>
        /// Delete a verification rule. Admin only. Existing verification results that reference this rule will have RuleId set to null.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _ruleService.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return NotFound(new { message = "Rule not found." });
            return NoContent();
        }
    }
}
