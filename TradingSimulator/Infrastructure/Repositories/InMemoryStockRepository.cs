using System.Collections.Concurrent;
using TradingSimulator.Domain.Entities;
using TradingSimulator.Domain.Interfaces;

namespace TradingSimulator.Infrastructure.Repositories;

public class InMemoryStockRepository : IStockRepository
{
    private readonly ConcurrentDictionary<string, Stock> _stocks = new();
    private readonly ConcurrentDictionary<string, List<PriceHistory>> _priceHistories = new();

    public Task<Stock?> GetBySymbolAsync(string symbol)
    {
        _stocks.TryGetValue(symbol, out var stock);
        return Task.FromResult(stock);
    }

    public Task<IEnumerable<Stock>> GetAllAsync()
    {
        return Task.FromResult(_stocks.Values.AsEnumerable());
    }

    public Task<Stock> AddAsync(Stock stock)
    {
        _stocks[stock.Symbol] = stock;
        return Task.FromResult(stock);
    }

    public Task<Stock> UpdateAsync(Stock stock)
    {
        _stocks[stock.Symbol] = stock;
        return Task.FromResult(stock);
    }

    public Task<IEnumerable<PriceHistory>> GetPriceHistoryAsync(string symbol, int count = 10)
    {
        if (_priceHistories.TryGetValue(symbol, out var history))
        {
            return Task.FromResult(history.OrderByDescending(h => h.Timestamp).Take(count));
        }
        return Task.FromResult(Enumerable.Empty<PriceHistory>());
    }

    public Task AddPriceHistoryAsync(PriceHistory priceHistory)
    {
        var history = _priceHistories.GetOrAdd(priceHistory.Symbol, _ => new List<PriceHistory>());
        
        lock (history)
        {
            history.Add(priceHistory);
            if (history.Count > 10)
            {
                history.RemoveRange(0, history.Count - 10);
            }
        }
        
        return Task.CompletedTask;
    }
}