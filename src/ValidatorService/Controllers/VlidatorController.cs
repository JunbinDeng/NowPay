using Microsoft.AspNetCore.Mvc;
using ValidatorService.Data;
using ValidatorService.Models;

namespace ValidatorService.Controllers;

/// <summary>
/// Controller responsible for validating credit card numbers.
/// </summary>
/// <remarks>
/// Provides endpoints to check the validity of credit card numbers using the **Luhn algorithm**.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public partial class ValidatorController : ControllerBase
{
    private readonly IValidatorService _service;
    private readonly ILogger<ValidatorController> _logger;

    public ValidatorController(IValidatorService service, ILogger<ValidatorController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Validates a credit card number using the Luhn algorithm.
    /// </summary>
    /// <remarks>
    /// This endpoint verifies if a provided credit card number is valid using the **Luhn algorithm**.
    /// - The number must be **between 13 and 19 digits**.
    /// - Only **numeric characters (0-9)** are allowed.
    /// - A valid number **passes Luhn validation**.
    /// </remarks>
    /// <param name="request">The request containing the card number.</param>
    /// <returns>
    /// A response indicating whether the card number is valid or not.
    /// </returns>
    /// <response code="200">Valid card number.</response>
    /// <response code="400">Invalid request format, such as missing `card_number`.</response>
    /// <response code="422">Invalid card number format or Luhn validation failure.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("luhn")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public IActionResult ValidateByLuhn([FromBody] ValidatorRequest? request)
    {
        throw new NotImplementedException();
    }
}
