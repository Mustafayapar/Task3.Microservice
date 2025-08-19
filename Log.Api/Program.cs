using Log.Api.Application.Abstractions;
using Log.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MassTransit;
using MediatR;
 using Microsoft.OpenApi.Models;
using Log.Api.Infrastructure.Messaging;
using Serilog;
using Serilog.Enrichers.CorrelationId; // bu şart

var builder = WebApplication.CreateBuilder(args);

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
var cs = builder.Configuration.GetConnectionString("LogDb")
         ?? "Server=localhost;Database=LogDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";
builder.Services.AddDbContext<LogDbContext>(opt => opt.UseSqlServer(cs));

// Repos
builder.Services.AddScoped<ILogRepository, LogRepository>();

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<LogEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq://localhost";
        var user = builder.Configuration["RabbitMQ:User"] ?? "guest";
        var pass = builder.Configuration["RabbitMQ:Pass"] ?? "guest";

        cfg.Host(new Uri(host), h =>
        {
            h.Username(user);
            h.Password(pass);
        });

        cfg.ReceiveEndpoint("log-service", e =>
        {
            e.ConfigureConsumer<LogEventConsumer>(context);
        });
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Log API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
