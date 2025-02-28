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
    /// <response code="200">Validation result, including whether the number is valid.</response>
    /// <response code="400">Invalid request format, such as missing `card_number`.</response>
    [HttpPost("luhn")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResponse<ValidatorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public IActionResult ValidateByLuhn([FromBody] ValidatorRequest? request)
    {
        if (request == null)
        {
            _logger.LogWarning("The request body is required.");
            return BadRequest(new ApiResponse<ValidatorResponse>(
                StatusCodes.Status400BadRequest,
                error: new ErrorDetails("missing_body", "Please provide a valid JSON request body.")
            ));
        }

        if (string.IsNullOrWhiteSpace(request.CardNumber))
        {
            _logger.LogWarning("The 'card_number' field is required.");
            return BadRequest(new ApiResponse<ValidatorResponse>(
                StatusCodes.Status400BadRequest,
                error: new ErrorDetails("missing_field", "Please provide a valid card number.")
            ));
        }

        string cardNumber = request.CardNumber.Trim();
        _logger.LogInformation("Validating card number: {CardNumber}", cardNumber);

        if (cardNumber.Length < 13 || cardNumber.Length > 19)
        {
            _logger.LogWarning("Invalid card number length.");
            return Ok(new ApiResponse<ValidatorResponse>(
                StatusCodes.Status200OK,
                data: new ValidatorResponse() { IsValid = false },
                error: new ErrorDetails("invalid_length", "Card number length must be between 13 and 19 digits.")
            ));
        }

        if (!cardNumber.All(char.IsDigit))
        {
            _logger.LogWarning("Invalid card number format.");
            return Ok(new ApiResponse<ValidatorResponse>(
                StatusCodes.Status200OK,
                data: new ValidatorResponse() { IsValid = false },
                error: new ErrorDetails("invalid_format", "Card number must contain only numeric digits (0-9).")
            ));
        }

        bool isValid = _service.ValidateCardNumber(cardNumber);
        if (!isValid)
        {
            _logger.LogWarning("Invalid card number.");
            return Ok(new ApiResponse<ValidatorResponse>(
                StatusCodes.Status200OK,
                data: new ValidatorResponse() { IsValid = isValid },
                error: new ErrorDetails("invalid_number", "The card number does not pass the Luhn algorithm validation, please try again.")
            ));
        }

        return Ok(new ApiResponse<ValidatorResponse>(
            StatusCodes.Status200OK,
            data: new ValidatorResponse() { IsValid = isValid }));
    }
}
