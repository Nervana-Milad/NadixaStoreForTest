using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Core.Entities;
using Nadixa.Web.Helpers;
using Nadixa.Web.Models.ViewModels;
using Nadixa.Web.Services;

namespace Nadixa.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EmailSender _emailSender;

        public AuthController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager , EmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Address = model.Address,
                    City = model.City
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                //If User created successfully
                if (result.Succeeded)
                {
                    //If the role exist in DB
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, true);

                    TempData["Success"] = AppMessages.RegisterSuccess;
                    return RedirectToAction("Login", "Auth");
                }
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        ModelState.AddModelError("", "This email is already registered.");
                    }
                    else
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

            }

            return View(model);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if(user == null)
                {
                    ModelState.AddModelError("", AppMessages.InvalidLogin);
                    return View(model);
                }
                var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if(!signInResult.Succeeded)
                {
                    ModelState.AddModelError("", AppMessages.InvalidLogin);
                    return View(model);
                }
                TempData["Success"] = AppMessages.LoginSuccess;
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = AppMessages.LogoutSuccess;
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth");

            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
                return RedirectToAction("Login");

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false);

            if (result.Succeeded)
            {
                TempData["Success"] = AppMessages.GoogleLoginSuccess;
                return RedirectToAction("Index", "Home");
            }

            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = AppMessages.LoginWithEmailFaild;
                return RedirectToAction("Login");
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email
            };

            var identityResult = await _userManager.CreateAsync(user);

            if (!identityResult.Succeeded)
            {
                TempData["Error"] = string.Join(" | ",
                    identityResult.Errors.Select(e => e.Description));

                return RedirectToAction("Login");
            }

            await _userManager.AddLoginAsync(user, info);

            await _signInManager.SignInAsync(user, false);
            TempData["Success"] = AppMessages.GoogleLoginSuccess;
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email not found.");
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                "ResetPassword",
                "Auth",
                new
                {
                    token = token,
                    email = user.Email
                },
                Request.Scheme);


            _emailSender.SendEmail(
    "Nadixa",
    "your-email@gmail.com",
    user.UserName ?? user.Email,
    user.Email,
    "Reset Password",
    $"Reset your password using this link: {resetLink}");

            _emailSender.SendEmail(
                "Nadixa",
                "your-email@gmail.com",
                user.UserName ?? user.Email,
                user.Email,
                "Reset Password",
                $"Click the following link to reset your password:\n{resetLink}"
            );

            return RedirectToAction("ForgotPasswordConfirmation");
        }


        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return BadRequest();

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}
