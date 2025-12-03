using Microsoft.Extensions.Logging;
using Moq;
using TradingSimulator.Application.Services;
using TradingSimulator.Domain.Entities;
using TradingSimulator.Domain.Interfaces;
using Xunit;

namespace TradingSimulator.Tests;

public class StockPriceServiceTests
{
    private readonly Mock<IStockRepository> _mockRepository;
    private readonly Mock<ILogger<StockPriceService>> _mockLogger;
    private readonly StockPriceService _service;

    public StockPriceServiceTests()
    {
        _mockRepository = new Mock<IStockRepository>();
        _mockLogger = new Mock<ILogger<StockPriceService>>();
        _service = new StockPriceService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateStockPriceAsync_ValidSymbol_UpdatesPrice()
    {
        // Arrange
        var stock = new Stock { Id = 1, Symbol = "AAPL", CurrentPrice = 150.00m, LastUpdated = DateTime.UtcNow };
        _mockRepository.Setup(r => r.GetBySymbolAsync("AAPL")).ReturnsAsync(stock);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Stock>())).ReturnsAsync(stock);

        // Act
        var result = await _service.UpdateStockPriceAsync("AAPL");

        // Assert
        Assert.Equal("AAPL", result.Symbol);
        Assert.True(result.Price > 0);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Stock>()), Times.Once);
        _mockRepository.Verify(r => r.AddPriceHistoryAsync(It.IsAny<PriceHistory>()), Times.Once);
    }



    [Fact]
    public async Task InitializeStocksAsync_CreatesAllStocks()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetBySymbolAsync(It.IsAny<string>())).ReturnsAsync((Stock?)null);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Stock>())).ReturnsAsync((Stock s) => s);

        // Act
        await _service.InitializeStocksAsync();

        // Assert
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Stock>()), Times.Exactly(5));
    }
}