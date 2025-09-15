using TradingSimulator.Domain.Entities;

namespace TradingSimulator.Domain.Interfaces;

public interface IStockRepository
{
    Task<Stock?> GetBySymbolAsync(string symbol);
    Task<IEnumerable<Stock>> GetAllAsync();
    Task<Stock> AddAsync(Stock stock);
    Task<Stock> UpdateAsync(Stock stock);
    Task<IEnumerable<PriceHistory>> GetPriceHistoryAsync(string symbol, int count = 10);
    Task AddPriceHistoryAsync(PriceHistory priceHistory);
}