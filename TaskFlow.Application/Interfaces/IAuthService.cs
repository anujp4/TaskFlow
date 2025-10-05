using TaskFlow.Application.DTOs.Auth;
using TaskFlow.Application.DTOs.Common;

namespace TaskFlow.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ResponseDto<UserDto>> GetUserByIdAsync(string userId);
    }
}