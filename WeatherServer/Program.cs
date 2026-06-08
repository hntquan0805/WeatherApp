using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using WeatherApp.API.Mappings;
using WeatherApp.API.Repositories;
using WeatherApp.API.Repositories.Interfaces;
using WeatherApp.API.Services;
using WeatherApp.API.Services.Interfaces;
using WeatherApp.Models;
using WeatherApp.API.Middleware;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Expand %VAR% → giá trị thực từ Environment
ExpandEnvVars(builder.Configuration);

// ── Database ──────────────────────────────────────────
builder.Services.AddDbContext<WeatherContext>(options =>
    options.UseMySQL(builder.Configuration
        .GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// ── JWT Authentication ────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

// ── AutoMapper ────────────────────────────────────────
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// ── Dependency Injection ──────────────────────────────
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddHttpClient(); // cho WeatherService

// ── CORS (cho React frontend) ─────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite default port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── Swagger với JWT support ───────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "WeatherApp API",
        Version = "v1"
    });

    // Thêm nút Authorize trong Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Nhập JWT token: Bearer {token}"
    });

    c.AddSecurityRequirement(document => new()
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication(); // phải trước UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();

// ── Helper function ───────────────────────────────────
static void ExpandEnvVars(IConfiguration config)
{
    foreach (var kvp in config.AsEnumerable())
    {
        if (kvp.Value is not null && kvp.Value.StartsWith("%") 
                                  && kvp.Value.EndsWith("%"))
        {
            var varName = kvp.Value.Trim('%');
            var value   = Environment.GetEnvironmentVariable(varName);
            if (value is not null)
                config[kvp.Key] = value;
        }
    }
}