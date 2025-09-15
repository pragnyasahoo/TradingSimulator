using TradingSimulator.Domain.Entities;
using TradingSimulator.Infrastructure.Repositories;
using Xunit;

namespace TradingSimulator.Tests;

public class StockRepositoryTests
{
    private readonly InMemoryStockRepository _repository;

    public StockRepositoryTests()
    {
        _repository = new InMemoryStockRepository();
    }

    [Fact]
    public async Task AddAsync_ValidStock_AddsToDatabase()
    {
        // Arrange
        var stock = new Stock { Symbol = "TEST", CurrentPrice = 100.00m, LastUpdated = DateTime.UtcNow };

        // Act
        var result = await _repository.AddAsync(stock);

        // Assert
        Assert.Equal("TEST", result.Symbol);
        
        var retrievedStock = await _repository.GetBySymbolAsync("TEST");
        Assert.NotNull(retrievedStock);
        Assert.Equal("TEST", retrievedStock.Symbol);
    }

    [Fact]
    public async Task GetBySymbolAsync_ExistingStock_ReturnsStock()
    {
        // Arrange
        var stock = new Stock { Symbol = "AAPL", CurrentPrice = 150.00m, LastUpdated = DateTime.UtcNow };
        await _repository.AddAsync(stock);

        // Act
        var result = await _repository.GetBySymbolAsync("AAPL");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AAPL", result.Symbol);
        Assert.Equal(150.00m, result.CurrentPrice);
    }

    [Fact]
    public async Task GetBySymbolAsync_NonExistingStock_ReturnsNull()
    {
        // Act
        var result = await _repository.GetBySymbolAsync("NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddPriceHistoryAsync_ValidHistory_AddsAndLimitsRecords()
    {
        // Add 12 price history records
        for (int i = 0; i < 12; i++)
        {
            var history = new PriceHistory
            {
                Symbol = "AAPL",
                Price = 150.00m + i,
                Timestamp = DateTime.UtcNow.AddMinutes(-i)
            };
            await _repository.AddPriceHistoryAsync(history);
        }

        // Act
        var result = await _repository.GetPriceHistoryAsync("AAPL", 15);

        // Assert
        Assert.Equal(10, result.Count()); // Should be limited to 10 records
    }

    [Fact]
    public async Task GetAllAsync_MultipleStocks_ReturnsAll()
    {
        // Arrange
        await _repository.AddAsync(new Stock { Symbol = "AAPL", CurrentPrice = 150.00m, LastUpdated = DateTime.UtcNow });
        await _repository.AddAsync(new Stock { Symbol = "MSFT", CurrentPrice = 300.00m, LastUpdated = DateTime.UtcNow });

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, s => s.Symbol == "AAPL");
        Assert.Contains(result, s => s.Symbol == "MSFT");
    }

}