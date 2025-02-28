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
            "missing_field");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901234567890")]
    public void ValidateByLuhn_WhenCardNumberIsInvalidLength_ReturnsOk(string inputs)
    {
        // Arrange
        _validatorService.Setup(v => v.ValidateCardNumber(It.IsAny<string>())).Returns(false);

        // Act
        var result = _controller.ValidateByLuhn(new ValidatorRequest() { CardNumber = inputs });

        // Assert
        AssertErrorResponse<OkObjectResult>(
            result,
            StatusCodes.Status200OK,
            "invalid_length");
    }

    [Theory]
    [InlineData("123$#^789%$&-())")]
    [InlineData("!!!!!!!!!!!!!")]
    public void ValidateByLuhn_WhenCardNumberIsInvalidFormat_ReturnsOk(string inputs)
    {
        // Arrange
        _validatorService.Setup(v => v.ValidateCardNumber(It.IsAny<string>())).Returns(false);

        // Act
        var result = _controller.ValidateByLuhn(new ValidatorRequest() { CardNumber = inputs });

        // Assert
        AssertInvalidResponse<OkObjectResult>(
            result,
            StatusCodes.Status200OK,
            "invalid_format");
    }

    [Fact]
    public void ValidateByLuhn_WhenCardNumberIsInvalid_ReturnsOk()
    {
        // Arrange
        const string CardNumber = "4111111111111112";
        _validatorService.Setup(v => v.ValidateCardNumber(It.IsAny<string>())).Returns(false);

        // Act
        var result = _controller.ValidateByLuhn(new ValidatorRequest() { CardNumber = CardNumber });

        // Assert
        AssertInvalidResponse<OkObjectResult>(
            result,
            StatusCodes.Status200OK,
            "invalid_number");
    }

    [Fact]
    public void ValidateByLuhn_WhenCardNumberIsValid_ReturnsOk()
    {
        // Arrange
        const string CardNumber = "4111111111111111";
        _validatorService.Setup(v => v.ValidateCardNumber(It.IsAny<string>()))
            .Returns(true)
            .Verifiable();

        // Act
        var result = _controller.ValidateByLuhn(new ValidatorRequest() { CardNumber = CardNumber });

        // Assert
        _validatorService.Verify(v => v.ValidateCardNumber(It.IsAny<string>()), Times.Once);

        AssertValidResponse<OkObjectResult>(
            result,
            StatusCodes.Status200OK);
    }

    private static void AssertErrorResponse<T>(
        IActionResult result,
        int expectedStatus,
        string expectedErrorCode)
        where T : ObjectResult
    {
        var request = Assert.IsType<T>(result);
        var response = Assert.IsType<ApiResponse<ValidatorResponse>>(request.Value);
        Assert.Equal(expectedStatus, response.Status);
        Assert.NotNull(response.Error);
        Assert.Equal(expectedErrorCode, response.Error.Code);
    }

    private static void AssertInvalidResponse<T>(
        IActionResult result,
        int expectedStatus,
        string expectedErrorCode)
        where T : ObjectResult
    {
        var request = Assert.IsType<T>(result);
        var response = Assert.IsType<ApiResponse<ValidatorResponse>>(request.Value);
        Assert.Equal(expectedStatus, response.Status);
        Assert.NotNull(response.Error);
        Assert.Equal(expectedErrorCode, response.Error.Code);
        Assert.NotNull(response.Data);
        Assert.False(response.Data.IsValid);
    }

    private static void AssertValidResponse<T>(
        IActionResult result,
        int expectedStatus)
        where T : ObjectResult
    {
        var request = Assert.IsType<T>(result);
        var response = Assert.IsType<ApiResponse<ValidatorResponse>>(request.Value);
        Assert.Equal(expectedStatus, response.Status);
        Assert.Null(response.Error);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.IsValid);
    }
}
