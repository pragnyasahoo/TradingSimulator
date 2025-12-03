using TradingSimulator.Application.DTOs;
using TradingSimulator.Domain.Entities;
using TradingSimulator.Domain.Interfaces;

namespace TradingSimulator.Application.Services;

public class StockPriceService
{
    private readonly IStockRepository _stockRepository;
    private readonly ILogger<StockPriceService> _logger;
    private readonly Random _random = new();
    private readonly string[] _symbols = { "AAPL", "MSFT", "GOOGL", "TSLA", "AMZN" };

    public StockPriceService(IStockRepository stockRepository, ILogger<StockPriceService> logger)
    {
        _stockRepository = stockRepository;
        _logger = logger;
    }

    public async Task InitializeStocksAsync()
    {
        var initialPrices = new Dictionary<string, decimal>
        {
            { "AAPL", 150.00m },
            { "MSFT", 300.00m },
            { "GOOGL", 2500.00m },
            { "TSLA", 800.00m },
            { "AMZN", 3200.00m }
        };

        foreach (var symbol in _symbols)
        {
            var existingStock = await _stockRepository.GetBySymbolAsync(symbol);
            if (existingStock == null)
            {
                var stock = new Stock
                {
                    Symbol = symbol,
                    CurrentPrice = initialPrices[symbol],
                    LastUpdated = DateTime.UtcNow
                };
                await _stockRepository.AddAsync(stock);
                _logger.LogInformation("Initialized stock {Symbol} with price {Price}", symbol, initialPrices[symbol]);
            }
        }
    }

    public async Task<StockPriceDto> UpdateStockPriceAsync(string symbol)
    {
        var stock = await _stockRepository.GetBySymbolAsync(symbol);
        if (stock == null) throw new ArgumentException($"Stock {symbol} not found");

        var changePercent = (_random.NextDouble() - 0.5) * 0.04; // ±2%
        var newPrice = stock.CurrentPrice * (1 + (decimal)changePercent);
        newPrice = Math.Round(newPrice, 2);

        stock.CurrentPrice = newPrice;
        stock.LastUpdated = DateTime.UtcNow;
        await _stockRepository.UpdateAsync(stock);

        var priceHistory = new PriceHistory
        {
            StockId = stock.Id,
            Symbol = symbol,
            Price = newPrice,
            Timestamp = DateTime.UtcNow
        };
        await _stockRepository.AddPriceHistoryAsync(priceHistory);

        return new StockPriceDto
        {
            Symbol = symbol,
            Price = newPrice,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<StockPriceDto>> GetAllCurrentPricesAsync()
    {
        var stocks = await _stockRepository.GetAllAsync();
        return stocks.Select(s => new StockPriceDto
        {
            Symbol = s.Symbol,
            Price = s.CurrentPrice,
            Timestamp = s.LastUpdated
        });
    }

    public async Task<StockHistoryDto?> GetStockHistoryAsync(string symbol)
    {
        var stock = await _stockRepository.GetBySymbolAsync(symbol);
        if (stock == null) return null;

        var history = await _stockRepository.GetPriceHistoryAsync(symbol, 10);
        
        return new StockHistoryDto
        {
            Symbol = symbol,
            CurrentPrice = stock.CurrentPrice,
            LastUpdated = stock.LastUpdated,
            History = history.Select(h => new PriceHistoryDto
            {
                Price = h.Price,
                Timestamp = h.Timestamp
            }).ToList()
        };
    }
}