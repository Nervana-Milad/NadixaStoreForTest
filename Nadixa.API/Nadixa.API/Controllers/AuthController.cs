//using Microsoft.AspNetCore.Mvc;
//using Nadixa.Core.DTOs;
//using Nadixa.Core.DTOS.Auth;
// Replace
//using Nadixa.Web.Controllers;

//namespace Nadixa.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AuthController : ControllerBase
//    {
//        private readonly IAuthService _authService;

//        public AuthController(
//            IAuthService authService)
//        {
//            _authService = authService;
//        }

//        [HttpPost("register")]
//        public async Task<IActionResult> Register(
//            [FromBody] RegisterDto dto)
//        {
//            var result =
//                await _authService.RegisterAsync(dto);

//            if (!result.Success)
//                return BadRequest(result);

//            return Ok(result);
//        }

//        [HttpPost("login")]
//        public async Task<IActionResult> Login(
//            [FromBody] LoginDto dto)
//        {
//            var result =
//                await _authService.LoginAsync(dto);

//            if (!result.Success)
//                return Unauthorized(result);

//            return Ok(result);
//        }

//        [HttpPost("google")]
//        public async Task<IActionResult> GoogleLogin(
//            [FromBody] GoogleLoginDto dto)
//        {
//            var result =
//                await _authService.GoogleLoginAsync(
//                    dto.Email,
//                    dto.FirstName,
//                    dto.LastName);

//            if (!result.Success)
//                return BadRequest(result);

//            return Ok(result);
//        }

//        [HttpPost("forgot-password")]
//        public async Task<IActionResult> ForgotPassword(
//            [FromBody] ForgotPasswordDto dto)
//        {
//            var result =
//                await _authService
//                    .ForgotPasswordAsync(dto.Email);

//            if (!result)
//                return NotFound(new
//                {
//                    Success = false,
//                    Message = "Email not found."
//                });

//            return Ok(new
//            {
//                Success = true,
//                Message = "Password reset email sent."
//            });
//        }

//        [HttpPost("reset-password")]
//        public async Task<IActionResult> ResetPassword(
//            [FromBody] ResetPasswordDto dto)
//        {
//            var result =
//                await _authService
//                    .ResetPasswordAsync(dto);

//            if (!result.Success)
//                return BadRequest(result);

//            return Ok(result);
//        }
//    }
//}