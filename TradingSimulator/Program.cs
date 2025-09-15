using TradingSimulator.Application.Services;
using TradingSimulator.Domain.Interfaces;
using TradingSimulator.Infrastructure.HealthChecks;
using TradingSimulator.Infrastructure.Plugins;
using TradingSimulator.Infrastructure.Repositories;
using TradingSimulator.Presentation.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add repositories
builder.Services.AddSingleton<IStockRepository, InMemoryStockRepository>();

// Add services
builder.Services.AddSingleton<StockPriceService>();
builder.Services.AddSingleton<TcpServerService>();
builder.Services.AddSingleton<IPluginManager, PluginManager>();
builder.Services.AddHostedService<PriceUpdateService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<TradingSimulatorHealthCheck>("trading-simulator");

// Add CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PriceHub>("/pricehub");
app.MapHealthChecks("/health");

// Load plugins
var pluginManager = app.Services.GetRequiredService<IPluginManager>();
await pluginManager.LoadPluginsAsync();

// Start TCP server
var tcpServer = app.Services.GetRequiredService<TcpServerService>();
await tcpServer.StartAsync(8080);

app.Run();
