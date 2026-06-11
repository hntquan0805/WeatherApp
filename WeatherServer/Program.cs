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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Expand %VAR% → giá trị thực từ Environment
ExpandEnvVars(builder.Configuration);


// ── Rate Limiting ─────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429; // Too Many Requests

    // ── Policy 1: Auth endpoints (login/register) ─────
    // Chống brute-force: 5 request / 1 phút / IP
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit      = 5,
                Window           = TimeSpan.FromMinutes(1),
                QueueLimit       = 0, // không queue, reject thẳng
            }));

    // ── Policy 2: Weather search ──────────────────────
    // 30 request / 1 phút / IP (đã login hay chưa)
    options.AddPolicy("weather", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit          = 30,
                Window               = TimeSpan.FromMinutes(1),
                SegmentsPerWindow    = 6,  // chia nhỏ thành 10s/segment
                QueueLimit           = 0,
            }));

    // ── Policy 3: Global fallback ─────────────────────
    // 100 request / 1 phút / IP cho mọi endpoint còn lại
    options.AddPolicy("global", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window      = TimeSpan.FromMinutes(1),
                QueueLimit  = 0,
            }));

    // Callback khi bị reject — log để monitor
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;

        // Trả về thời gian còn phải chờ nếu có
        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }

        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"success":false,"message":"Quá nhiều yêu cầu. Vui lòng thử lại sau.","data":null,"errors":[]}""",
            cancellationToken);
    };
});

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
app.UseRateLimiter();
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