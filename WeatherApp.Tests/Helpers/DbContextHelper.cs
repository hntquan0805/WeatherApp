using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;

namespace WeatherApp.Tests.Helpers;

public static class DbContextHelper
{
    // Mỗi test dùng một DB riêng — tránh data leak giữa các test
    public static WeatherContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<WeatherContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new WeatherContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}