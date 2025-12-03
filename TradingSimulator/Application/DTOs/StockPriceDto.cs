namespace TradingSimulator.Application.DTOs;

public class StockPriceDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
}

public class StockHistoryDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<PriceHistoryDto> History { get; set; } = new();
}

public class PriceHistoryDto
{
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
}