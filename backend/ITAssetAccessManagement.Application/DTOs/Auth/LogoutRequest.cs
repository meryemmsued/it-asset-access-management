namespace ITAssetAccessManagement.Application.DTOs.Auth;

public sealed class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}