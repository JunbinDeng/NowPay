using ValidatorService.Controllers;
using ValidatorService.Data;
using Moq;
using Microsoft.AspNetCore.Mvc;
using ValidatorService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ValidatorService.UnitTests;

public class LuhnControllerTests
{
    private readonly Mock<IValidatorService> _validatorService;
    private readonly Mock<ILogger<ValidatorController>> _logger;
    private readonly ValidatorController _controller;

    public LuhnControllerTests()
    {
        _validatorService = new Mock<IValidatorService>();
        _logger = new Mock<ILogger<ValidatorController>>();
        _controller = new ValidatorController(_validatorService.Object, _logger.Object);
    }

    [Fact]
    public void ValidateByLuhn_WhenRequestIsNull_ReturnsBadRequest()
    {
        // Act
        var result = _controller.ValidateByLuhn(null);

        // Assert
        AssertErrorResponse<BadRequestObjectResult>(
            result,
            StatusCodes.Status400BadRequest,
            "The request body is required.",
            "missing_body");
    }

    [Fact]
    public void ValidateByLuhn_WhenCardNumberIsEmpty_ReturnsBadRequest()
    {
        // Act
        var result = _controller.ValidateByLuhn(
            new ValidatorRequest() { CardNumber = string.Empty });

        // Assert
        AssertErrorResponse<BadRequestObjectResult>(
            result,
            StatusCodes.Status400BadRequest,
            "The 'card_number' field is required.",
            "missing_field");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901234567890")]
    public void ValidateByLuhn_WhenCardNumberIsInvalidLength_ReturnsUnprocessable(string inputs)
    {
        // Arrange
        _validatorService.Setup(v => v.ValidateCardNumber(It.IsAny<string>())).Returns(false);

        // Act
        var result = _controller.ValidateByLuhn(new ValidatorRequest() { CardNumber = inputs });

        // Assert
        AssertErrorResponse<UnprocessableEntityObjectResult>(
            result,
            StatusCodes.Status422UnprocessableEntity,
            "Invalid card number length.",
            "invalid_length");
    }

    [Theory]
    [InlineData("123$#^789%$&-())")]
    [InlineData("!!!!!!!!!!!!!")]
    public void ValidateByLuhn_WhenCardNumberIsInvalidFormat_ReturnsUnprocessable(string inputs)
    {
        // Arrange
        _validatorService.Setup(v => v.ValidateCardNumber(It.IsAny<string>())).Returns(false);

        // Act
        var result = _controller.ValidateByLuhn(new ValidatorRequest() { CardNumber = inputs });

        // Assert
        AssertErrorResponse<UnprocessableEntityObjectResult>(
            result,
            StatusCodes.Status422UnprocessableEntity,
            "Invalid card number format.",
            "invalid_format");
    }

    [Fact]
    public void ValidateByLuhn_WhenCardNumberIsInvalid_ReturnsUnprocessable()
    {
        // Arrange
        const string CARD_NUMBER = "4111111111111112";
        _validatorService.Setup(v => v.ValidateCardNumber(It.IsAny<string>())).Returns(false);

        // Act
        var result = _controller.ValidateByLuhn(new ValidatorRequest() { CardNumber = CARD_NUMBER });

        // Assert
        AssertErrorResponse<UnprocessableEntityObjectResult>(
            result,
            StatusCodes.Status422UnprocessableEntity,
            "Invalid card number.",
            "invalid_number");
    }

    [Fact]
    public void ValidateByLuhn_WhenCardNumberIsValid_ReturnsOk()
    {
        // Arrange
        const string CARD_NUMBER = "4111111111111111";
        _validatorService.Setup(v => v.ValidateCardNumber(It.IsAny<string>()))
            .Returns(true)
            .Verifiable();

        // Act
        var result = _controller.ValidateByLuhn(new ValidatorRequest() { CardNumber = CARD_NUMBER });

        // Assert
        _validatorService.Verify(v => v.ValidateCardNumber(It.IsAny<string>()), Times.Once);

        AssertValidResponse<OkObjectResult>(
            result,
            StatusCodes.Status200OK,
            "Valid card number.");
    }

    private static void AssertErrorResponse<T>(
        IActionResult result,
        int expectedStatus,
        string expectedMessage,
        string expectedErrorCode)
        where T : ObjectResult
    {
        var request = Assert.IsType<T>(result);
        var response = Assert.IsType<ApiResponse<object>>(request.Value);
        Assert.Equal(expectedStatus, response.Status);
        Assert.Equal(expectedMessage, response.Message);
        Assert.NotNull(response.Error);
        Assert.Equal(expectedErrorCode, response.Error.Code);
    }

    private static void AssertValidResponse<T>(
        IActionResult result,
        int expectedStatus,
        string expectedMessage)
        where T : ObjectResult
    {
        var request = Assert.IsType<T>(result);
        var response = Assert.IsType<ApiResponse<object>>(request.Value);
        Assert.Equal(expectedStatus, response.Status);
        Assert.Equal(expectedMessage, response.Message);
    }
}
