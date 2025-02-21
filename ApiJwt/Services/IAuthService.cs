using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiJwt.Entities;
using ApiJwt.Models;

namespace ApiJwt.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto reqData);
        Task<TokenResponseDto?> LoginAsync(UserDto reqData);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
    }
}