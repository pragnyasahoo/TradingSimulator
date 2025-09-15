namespace TradingSimulator.Domain.Entities;

public class PriceHistory
{
    public int Id { get; set; }
    public int StockId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
    public Stock Stock { get; set; } = null!;
}