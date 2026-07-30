using Nadixa.Application.DTOS;
using Nadixa.Core.DTOS.Auth;

namespace Nadixa.Application.Interfaces
{
    public interface IAuthService
    {
        // Register
        Task<AuthResult> RegisterAsync(RegisterDto dto);

        // Login
        Task<AuthResult> LoginAsync(LoginDto dto);

        // Logout
        Task LogoutAsync();

        // Google Login
        Task<string> GetGoogleLoginUrlAsync();

        Task<AuthResult> GoogleResponseAsync();

        // Forgot Password
        Task<AuthResult> ForgotPasswordAsync(
            ForgotPasswordDto dto,
            Func<string, string> buildResetLink);   
        // Reset Password
        Task<AuthResult> ResetPasswordAsync(
            ResetPasswordDto dto);
    }
}