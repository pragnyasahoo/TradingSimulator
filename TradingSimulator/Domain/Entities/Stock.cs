namespace TradingSimulator.Domain.Entities;

public class Stock
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<PriceHistory> PriceHistory { get; set; } = new();
}