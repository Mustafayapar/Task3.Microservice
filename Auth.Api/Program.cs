using Auth.Api.Application.Abstractions;
using Auth.Api.Application.Handlers;
using Auth.Api.Domain.Entities;
using Auth.Api.Infrastructure.Identity;
using Auth.Api.Infrastructure.Persistence;
using Auth.Api.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR (Application assembly)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(LoginHandler).Assembly,
        typeof(RegisterHandler).Assembly,
        typeof(RefreshTokenHandler).Assembly,
        typeof(RevokeRefreshTokenHandler).Assembly
    );
});

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

// Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

// JWT Ayarları
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


// Repositories
 builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });

    // 🔑 JWT Tanımı
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
