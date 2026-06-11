using AutoMapper;
using WeatherApp.API.Mappings;

namespace WeatherApp.Tests.Helpers;

public static class MapperHelper
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance
        );

        return config.CreateMapper();
    }
}