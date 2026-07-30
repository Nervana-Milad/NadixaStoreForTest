using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces;
using Nadixa.Core.DTOS.Auth;
using Nadixa.Web.Helpers;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }


        // =========================================
        // REGISTER
        // =========================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new RegisterDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                Address = model.Address,
                City = model.City
            };

            var result =
                await _authService.RegisterAsync(dto);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error);
                }

                return View(model);
            }

            TempData["Success"] =
                AppMessages.RegisterSuccess;

            return RedirectToAction(
                "Login",
                "Auth");
        }


        // =========================================
        // LOGIN
        // =========================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new LoginDto
            {
                Email = model.Email,
                Password = model.Password
            };

            var result =
                await _authService.LoginAsync(dto);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    "",
                    AppMessages.InvalidLogin);

                return View(model);
            }

            TempData["Success"] =
                AppMessages.LoginSuccess;

            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================================
        // LOGOUT
        // =========================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();

            TempData["Success"] =
                AppMessages.LogoutSuccess;

            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================================
        // ACCESS DENIED
        // =========================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // =========================================
        // GOOGLE LOGIN
        // =========================================

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl =
                Url.Action(
                    "GoogleResponse",
                    "Auth");

            var properties =
                new AuthenticationProperties
                {
                    RedirectUri = redirectUrl
                };

            return Challenge(
                properties,
                "Google");
        }


        // =========================================
        // GOOGLE RESPONSE
        // =========================================

        [HttpGet]
        public async Task<IActionResult>
            GoogleResponse()
        {
            var result =
                await _authService
                    .GoogleResponseAsync();

            if (!result.Succeeded)
            {
                TempData["Error"] =
                    result.Errors.FirstOrDefault();

                return RedirectToAction(
                    "Login");
            }

            TempData["Success"] =
                AppMessages.GoogleLoginSuccess;

            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================================
        // FORGOT PASSWORD CONFIRMATION
        // =========================================

        [HttpGet]
        public IActionResult
            ForgotPasswordConfirmation()
        {
            return View();
        }


        // =========================================
        // FORGOT PASSWORD - GET
        // =========================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        // =========================================
        // FORGOT PASSWORD - POST
        // =========================================
        // AuthController.cs
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new ForgotPasswordDto
            {
                Email = model.Email
            };

            var result = await _authService.ForgotPasswordAsync(dto, token =>
                Url.Action("ResetPassword", "Auth",
                    new { token, email = dto.Email },
                    Request.Scheme)!);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error);

                return View(model);
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // =========================================
        // RESET PASSWORD - GET
        // =========================================

        [HttpGet]
        public IActionResult ResetPassword(
            string token,
            string email)
        {
            if (token == null ||
                email == null)
            {
                return BadRequest();
            }

            var model =
                new ResetPasswordViewModel
                {
                    Token = token,
                    Email = email
                };

            return View(model);
        }


        // =========================================
        // RESET PASSWORD - POST
        // =========================================

        [HttpPost]
        public async Task<IActionResult>
            ResetPassword(
                ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new ResetPasswordDto
            {
                Email = model.Email,
                Token = model.Token,
                Password = model.Password
            };

            var result =
                await _authService
                    .ResetPasswordAsync(dto);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error);
                }

                return View(model);
            }

            return RedirectToAction(
                "Login");
        }
    }
}