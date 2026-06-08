using AutoMapper;
using WeatherApp.API.DTOs;
using WeatherApp.Models;
using WeatherApp.API.Repositories.Interfaces;
using WeatherApp.API.Services.Interfaces;

namespace WeatherApp.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IMapper         _mapper;
    private readonly JwtTokenService _jwtService;

    public AuthService(
        IUserRepository  userRepo,
        IMapper          mapper,
        JwtTokenService  jwtService)
    {
        _userRepo   = userRepo;
        _mapper     = mapper;
        _jwtService = jwtService;
    }

    // ── Register ──────────────────────────────────────
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Kiểm tra trùng email / username
        if (await _userRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        if (await _userRepo.UsernameExistsAsync(dto.Username))
            throw new InvalidOperationException("Username đã được sử dụng.");

        // Map DTO → Entity, hash password
        var user = _mapper.Map<User>(dto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _userRepo.AddAsync(user);

        // Tạo JWT
        var (token, expiresAt) = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token     = token,
            ExpiresAt = expiresAt,
            User      = _mapper.Map<UserDto>(user)
        };
    }

    // ── Login ─────────────────────────────────────────
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Email hoặc password không đúng.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc password không đúng.");

        var (token, expiresAt) = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token     = token,
            ExpiresAt = expiresAt,
            User      = _mapper.Map<UserDto>(user)
        };
    }

    // ── Get Profile ───────────────────────────────────
    public async Task<UserDto> GetProfileAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy user.");

        return _mapper.Map<UserDto>(user);
    }

    // ── Update Profile ────────────────────────────────
    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy user.");

        // Cập nhật username nếu có và chưa bị trùng
        if (!string.IsNullOrWhiteSpace(dto.Username) &&
            dto.Username != user.Username)
        {
            if (await _userRepo.UsernameExistsAsync(dto.Username))
                throw new InvalidOperationException("Username đã được sử dụng.");

            user.Username = dto.Username;
        }

        // Cập nhật email nếu có và chưa bị trùng
        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            dto.Email != user.Email)
        {
            if (await _userRepo.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("Email đã được sử dụng.");

            user.Email = dto.Email;
        }

        // Đổi password nếu có
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Password hiện tại không đúng.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        await _userRepo.UpdateAsync(user);
        return _mapper.Map<UserDto>(user);
    }
}