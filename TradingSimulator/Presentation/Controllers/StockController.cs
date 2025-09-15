using Microsoft.AspNetCore.Mvc;
using TradingSimulator.Application.Services;

namespace TradingSimulator.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly StockPriceService _stockPriceService;
    private readonly ILogger<StockController> _logger;

    public StockController(StockPriceService stockPriceService, ILogger<StockController> logger)
    {
        _stockPriceService = stockPriceService;
        _logger = logger;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentPrices()
    {
        try
        {
            var prices = await _stockPriceService.GetAllCurrentPricesAsync();
            return Ok(prices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current prices");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{symbol}/current")]
    public async Task<IActionResult> GetCurrentPrice(string symbol)
    {
        try
        {
            var stock = await _stockPriceService.GetStockHistoryAsync(symbol.ToUpper());
            if (stock == null)
                return NotFound($"Stock {symbol} not found");

            return Ok(new { symbol = stock.Symbol, price = stock.CurrentPrice, timestamp = stock.LastUpdated });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current price for {Symbol}", symbol);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{symbol}/history")]
    public async Task<IActionResult> GetPriceHistory(string symbol)
    {
        try
        {
            var history = await _stockPriceService.GetStockHistoryAsync(symbol.ToUpper());
            if (history == null)
                return NotFound($"Stock {symbol} not found");

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history for {Symbol}", symbol);
            return StatusCode(500, "Internal server error");
        }
    }
}