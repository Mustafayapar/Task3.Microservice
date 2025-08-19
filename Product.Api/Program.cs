using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Product.Api.Application.Abstractions;
using Product.Api.Application.Commands;
using Product.Api.Application.Handlers;
using Product.Api.Infrastructure.Cache;
using Product.Api.Infrastructure.Messaging;
using Product.Api.Infrastructure.Persistence;
using Serilog;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);


// 12-Factor: Config from env
var sql = builder.Configuration.GetConnectionString("ProductDb")
          ?? Environment.GetEnvironmentVariable("PRODUCT_SQL")
          ?? "Server=localhost;Database=ProductDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;";
var redisConn = builder.Configuration["Redis:Connection"]
          ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION")
          ?? "localhost:6379";
var jwtSecret = builder.Configuration["Jwt:Secret"]
          ?? Environment.GetEnvironmentVariable("JWT_SECRET")
          ?? "superSuperSecretKey_1234567890_ABCDEF_123456";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "AuthService";
var audience = builder.Configuration["Jwt:Audience"] ?? "ApiUsers";
// 🔹 Serilog ayarları
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration) // appsettings.json'dan okur
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .Enrich.WithProperty("ServiceName", "Product.Api") // servis ismini sabitliyoruz
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter()) // JSON structured logging
                                                                  //.WriteTo.Seq("http://localhost:5341")        // opsiyonel: Seq
                                                                  //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200")) // opsiyonel: ELK
                                                                  //{
                                                                  //    AutoRegisterTemplate = true,
                                                                  //    IndexFormat = "logs-{0:yyyy.MM.dd}"
                                                                  //})
);
// DbContext
builder.Services.AddDbContext<ProductDbContext>(opt => opt.UseSqlServer(sql));

// CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(CreateProductHandler).Assembly,
        typeof(UpdateProductHandler).Assembly,
        typeof(GetProductByIdHandler).Assembly,
        typeof(GetProductsHandler).Assembly
    );
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ProductApi_";
});

// Event bus (RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        var mq = builder.Configuration["RabbitMQ:Host"]
                 ?? Environment.GetEnvironmentVariable("RABBIT_HOST")
                 ?? "rabbitmq://localhost";
        cfg.Host(new Uri(mq), h =>
        {
            var user = builder.Configuration["RabbitMQ:User"] ?? "guest";
            var pass = builder.Configuration["RabbitMQ:Pass"] ?? "guest";
            h.Username(user);
            h.Password(pass);
        });
    });
});

// Auth(JWT) – Update endpoint için gerekli
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();




// Repos & Infra
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddStackExchangeRedisCache(opt => { opt.Configuration = redisConn; });



// Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Product API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { new OpenApiSecurityScheme{ Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id="Bearer"}}, Array.Empty<string>() }
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
