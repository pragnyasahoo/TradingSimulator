using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TradingSimulator.Application.Services;
using TradingSimulator.Presentation.Controllers;
using Xunit;

namespace TradingSimulator.Tests;

public class StockControllerTests
{
    private readonly Mock<StockPriceService> _mockStockService;
    private readonly Mock<ILogger<StockController>> _mockLogger;
    private readonly StockController _controller;

    public StockControllerTests()
    {
        _mockStockService = new Mock<StockPriceService>(Mock.Of<TradingSimulator.Domain.Interfaces.IStockRepository>(), Mock.Of<ILogger<StockPriceService>>());
        _mockLogger = new Mock<ILogger<StockController>>();
        _controller = new StockController(_mockStockService.Object, _mockLogger.Object);
    }




}