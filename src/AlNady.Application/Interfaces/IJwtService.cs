namespace AlNady.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(int userId, string email, string role);
    string GenerateRefreshToken();
    (int userId, string email, string role)? ValidateAccessToken(string token);
}
