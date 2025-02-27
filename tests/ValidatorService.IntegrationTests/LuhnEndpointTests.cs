using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using ValidatorService.Models;

namespace ValidatorService.IntegrationTests;

public class LuhnEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string Path = "/api/validator/luhn";

    public LuhnEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("4111111111111111", true)]  // Valid Luhn card number
    [InlineData("4012888888881881", true)]  // Valid Visa test card
    [InlineData("5500000000000004", true)]  // Valid MasterCard test card
    [InlineData("4111111111111112", false)] // Invalid Luhn number
    [InlineData("1234567890123456", false)] // Random invalid number
    [InlineData("000000000000", false)]    // Too short (12 digits)
    [InlineData("12345678901234567890", false)] // Too long (20 digits)
    public async Task ValidateByLuhn_ForValidatesInputs_ReturnsExpectedResult(string number, bool expectedValid)
    {
        // Arrange
        var content = new StringContent(
            JsonSerializer.Serialize(new { cardNumber = number }),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync(Path, content);

        // Assert
        if (expectedValid)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        else
        {
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(responseBody);
            Assert.NotNull(responseBody.Error);
            if (number.Length >= 13 && number.Length <= 19)
            {
                Assert.Equal("invalid_number", responseBody.Error.Code);
            }
            else
            {
                Assert.Equal("invalid_length", responseBody.Error.Code);
            }
        }
    }

    [Theory]
    [InlineData("abcd", "invalid_length")]  // Non-numeric characters
    [InlineData("1234 5678 9012 3456", "invalid_format")] // Spaces in number
    [InlineData("123-4567-8901-2345", "invalid_format")] // Hyphens in number
    [InlineData("1234567", "invalid_length")] // Too short
    [InlineData("123456789012345678901", "invalid_length")] // Too long
    public async Task ValidateByLuhn_InvalidNumbers_ReturnsUnprocessableEntity(string number, string expectedErrorCode)
    {
        // Arrange
        var content = new StringContent(
            JsonSerializer.Serialize(new { cardNumber = number }),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync(Path, content);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Error);
        Assert.Equal(expectedErrorCode, responseBody.Error.Code);
    }

    [Fact]
    public async Task ValidateByLuhn_WhenRequestBodyIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(Path, content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Error);
        Assert.Equal("missing_field", responseBody.Error.Code);
    }

    [Fact]
    public async Task ValidateByLuhn_WhenNoBodyProvided_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsync(Path, null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseBody = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Error);
        Assert.Equal("missing_body", responseBody.Error.Code);
    }
}
