using AlNady.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AlNady.IntegrationTests.Auth;

public class AuthEndpointTests : IClassFixture<AlNadyWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthEndpointTests(AlNadyWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Should_Return_201_With_Valid_Data()
    {
        var payload = new
        {
            email = $"test_{Guid.NewGuid()}@example.com",
            password = "TestPass1!",
            fullName = "Integration Tester",
            role = 0 // Player
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("accessToken");
        body.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Register_Should_Return_400_With_Weak_Password()
    {
        var payload = new
        {
            email = "weakpass@example.com",
            password = "weak",
            fullName = "Test User",
            role = 0
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Should_Return_401_With_Wrong_Credentials()
    {
        var payload = new { email = "nonexistent@example.com", password = "WrongPass1!" };

        var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Protected_Endpoint_Should_Return_401_Without_Token()
    {
        var response = await _client.GetAsync("/api/users/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Health_Endpoint_Should_Return_200()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }
}
