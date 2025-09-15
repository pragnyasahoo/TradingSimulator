using Microsoft.AspNetCore.SignalR;
using TradingSimulator.Domain.Interfaces;
using TradingSimulator.Presentation.Hubs;

namespace TradingSimulator.Application.Services;

public class PriceUpdateService : BackgroundService
{
    private readonly StockPriceService _stockPriceService;
    private readonly IHubContext<PriceHub> _hubContext;
    private readonly TcpServerService _tcpServerService;
    private readonly IPluginManager _pluginManager;
    private readonly ILogger<PriceUpdateService> _logger;
    private readonly string[] _symbols = { "AAPL", "MSFT", "GOOGL", "TSLA", "AMZN" };

    public PriceUpdateService(
        StockPriceService stockPriceService,
        IHubContext<PriceHub> hubContext,
        TcpServerService tcpServerService,
        IPluginManager pluginManager,
        ILogger<PriceUpdateService> logger)
    {
        _stockPriceService = stockPriceService;
        _hubContext = hubContext;
        _tcpServerService = tcpServerService;
        _pluginManager = pluginManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _stockPriceService.InitializeStocksAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var allUpdates = new List<Application.DTOs.StockPriceDto>();
                var formatters = _pluginManager.GetFormatters();
                
                foreach (var symbol in _symbols)
                {
                    var updatedPrice = await _stockPriceService.UpdateStockPriceAsync(symbol);
                    allUpdates.Add(updatedPrice);
                    
                    // Send via TCP with plugin formatting
                    foreach (var formatter in formatters)
                    {
                        var formattedData = formatter.FormatPrice(updatedPrice.Symbol, updatedPrice.Price, updatedPrice.Timestamp);
                        await _tcpServerService.BroadcastAsync(formattedData);
                    }
                    
                    _logger.LogDebug("Updated price for {Symbol}: {Price}", symbol, updatedPrice.Price);
                }
                
                // Batch broadcast via SignalR
                await _hubContext.Clients.All.SendAsync("BatchPriceUpdate", allUpdates, stoppingToken);
                
                await Task.Delay(5000, stoppingToken); // 5 seconds
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock prices");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}