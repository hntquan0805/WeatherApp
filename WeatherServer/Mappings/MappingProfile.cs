using AutoMapper;
using WeatherApp.API.DTOs;
using WeatherApp.Models;

namespace WeatherApp.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── User ──────────────────────────────────────────────
        CreateMap<User, UserDto>();

        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role,         opt => opt.MapFrom(_ => "User"))
            .ForMember(dest => dest.CreatedAt,    opt => opt.MapFrom(_ => DateTime.UtcNow));

        // ── WeatherLog ────────────────────────────────────────
        CreateMap<Weatherlog, WeatherLogDto>()
            .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon));

        // WeatherDto → WeatherLog (lưu lịch sử)
        // UserId sẽ được set thủ công trong Service
        CreateMap<WeatherDto, Weatherlog>()
            .ForMember(dest => dest.Id,         opt => opt.Ignore())
            .ForMember(dest => dest.UserId,     opt => opt.Ignore())
            .ForMember(dest => dest.User,       opt => opt.Ignore())
            .ForMember(dest => dest.SearchedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}