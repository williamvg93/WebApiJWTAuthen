using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiJwt.Entities;
using ApiJwt.Models;
using ApiJwt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ApiJwt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("SignUp")]
        public async Task<ActionResult<User>> SignUp(UserDto userDto)
        {
            var user = await _authService.RegisterAsync(userDto);

            if (user == null) return BadRequest("Username Already Exist !!!");

            return Ok(user);
        }

        [HttpPost("LogIn")]
        public async Task<ActionResult<TokenResponseDto>> LogIn(UserDto userDto)
        {
            var result = await _authService.LoginAsync(userDto);

            if (result is null) return BadRequest("Invalid UserName or Password !!!");

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndPoint()
        {
            return Ok("You Are Authenticated");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("OnlyAdmin")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You Are at Admin !!!");
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken == null || result.RefreshToken == null) return Unauthorized("Invalid Refresh Token !!!");

            return Ok(result);
        }
    }
}