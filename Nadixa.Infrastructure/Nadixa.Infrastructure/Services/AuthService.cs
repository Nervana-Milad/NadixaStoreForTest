using Microsoft.AspNetCore.Identity;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces;
using Nadixa.Application.DTOS.Auth;
using Nadixa.Core.Entities;

namespace Nadixa.Infrastructure.Services
{
    public class AuthService : Application.Interfaces.IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EmailSender _emailSender;

        public AuthService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager,
            EmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }


        // =========================================
        // REGISTER
        // =========================================

        public async Task<AuthResult> RegisterAsync(
            RegisterDto dto)
        {
            var user = new AppUser
            {
                UserName = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Address = dto.Address,
                City = dto.City
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    dto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(
                    error =>
                        error.Code == "DuplicateUserName"
                            ? "This email is already registered."
                            : error.Description
                );

                return AuthResult.Fail(errors);
            }

            // Check if User role exists
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole("User"));
            }

            // Add User role
            await _userManager.AddToRoleAsync(
                user,
                "User");

            // Sign in user
            await _signInManager.SignInAsync(
                user,
                isPersistent: true);

            return AuthResult.Success();
        }


        // =========================================
        // LOGIN
        // =========================================

        public async Task<AuthResult> LoginAsync(
            LoginDto dto)
        {
            var user =
                await _userManager.FindByEmailAsync(
                    dto.Email);

            if (user == null)
            {
                return AuthResult.Fail(
                    "Invalid login attempt.");
            }

            var signInResult =
                await _signInManager.PasswordSignInAsync(
                    user,
                    dto.Password,
                    isPersistent: false,
                    lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                return AuthResult.Fail(
                    "Invalid login attempt.");
            }

            return AuthResult.Success();
        }


        // =========================================
        // LOGOUT
        // =========================================

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }


        // =========================================
        // GOOGLE LOGIN
        // =========================================

        public async Task<string>
            GetGoogleLoginUrlAsync()
        {
            return "Google";
        }


        // =========================================
        // GOOGLE RESPONSE
        // =========================================

        public async Task<AuthResult>
            GoogleResponseAsync()
        {
            var info =
                await _signInManager
                    .GetExternalLoginInfoAsync();

            if (info == null)
            {
                return AuthResult.Fail(
                    "Unable to load Google login information.");
            }

            // Try login with Google
            var result =
                await _signInManager
                    .ExternalLoginSignInAsync(
                        info.LoginProvider,
                        info.ProviderKey,
                        isPersistent: false);

            // Existing Google user
            if (result.Succeeded)
            {
                return AuthResult.Success();
            }

            // Get Email from Google
            var email =
                info.Principal
                    .FindFirst(
                        System.Security.Claims.ClaimTypes.Email)
                    ?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return AuthResult.Fail(
                    "Unable to get email from Google.");
            }

            // Create new user
            var user = new AppUser
            {
                UserName = email,
                Email = email
            };

            var identityResult =
                await _userManager.CreateAsync(user);

            if (!identityResult.Succeeded)
            {
                return AuthResult.Fail(
                    identityResult.Errors.Select(
                        e => e.Description));
            }

            // Link Google account
            var loginResult =
                await _userManager.AddLoginAsync(
                    user,
                    info);

            if (!loginResult.Succeeded)
            {
                return AuthResult.Fail(
                    loginResult.Errors.Select(
                        e => e.Description));
            }

            // Sign in
            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return AuthResult.Success();
        }


        // =========================================
        // FORGOT PASSWORD
        // =========================================


        public async Task<AuthResult> ForgotPasswordAsync(
         ForgotPasswordDto dto,
         Func<string, string> buildResetLink)   // بدل string resetLink
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return AuthResult.Fail("Email not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = buildResetLink(token);

            var sent = _emailSender.SendEmail(
                "Nadixaaa", "nermenhosny9@gmail.com",
                user.UserName ?? user.Email, user.Email,
                "Reset Password",
                $"Click the following link to reset your password:\n{resetLink}");

            if (!sent)
                return AuthResult.Fail("Failed to send email. Please try again later.");

            return AuthResult.Success();
        }
        

        // =========================================
        // RESET PASSWORD
        // =========================================

        public async Task<AuthResult>
            ResetPasswordAsync(
                ResetPasswordDto dto)
        {
            var user =
                await _userManager.FindByEmailAsync(
                    dto.Email);

            if (user == null)
            {
                return AuthResult.Fail(
                    "User not found.");
            }

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    dto.Token,
                    dto.Password);

            if (!result.Succeeded)
            {
                return AuthResult.Fail(
                    result.Errors.Select(
                        e => e.Description));
            }

            return AuthResult.Success();
        }
    }
}