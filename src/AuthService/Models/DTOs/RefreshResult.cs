namespace AuthService.Models.DTOs
{
    public record RefreshResult(
        bool Succeeded,
        string? JwtToken,
        string? RefreshToken
        );
}