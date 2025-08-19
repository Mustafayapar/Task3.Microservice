using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy;


var builder = WebApplication.CreateBuilder(args);


// Serilog (JSON console)
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "ApiGateway")
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
);

// JWT Auth (Gateway tarafı da doğrulasın)
var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"] ?? "");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

// Authorization Policies
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("JwtRequired", p => p.RequireAuthenticatedUser());
    // "JwtOptional": authenticated olursa ne ala, yoksa da izin ver (auth servisindeki /register, /login gibi)
    o.AddPolicy("JwtOptional", p => p.RequireAssertion(_ => true));
});



// Rate Limiting Policies
builder.Services.AddRateLimiter(options =>
{
    // Token Bucket (per-IP burst)
    options.AddPolicy("per-ip-burst", httpContext =>
        RateLimitPartition.GetTokenBucketLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown", // partition key
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 30,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                AutoReplenishment = true,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    // Fixed Window (per-IP strict)
    options.AddPolicy("per-ip-strict", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Sıra: RateLimit → Auth → AuthZ → YARP
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// YARP endpointlerini konfig metadata’sına göre authorize/ratelimit uygular
app.MapReverseProxy()
   .RequireRateLimiting("per-ip-burst") // route bazlı policy `appsettings.json`'dan okunur
   .WithMetadata();       // YARP route metadata'sını koru


app.MapGet("/", () => "Hello World!");

app.Run();

