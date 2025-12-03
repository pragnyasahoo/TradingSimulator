using Microsoft.Extensions.Diagnostics.HealthChecks;
using TradingSimulator.Application.Services;
using TradingSimulator.Domain.Interfaces;

namespace TradingSimulator.Infrastructure.HealthChecks;

public class TradingSimulatorHealthCheck : IHealthCheck
{
    private readonly StockPriceService _stockPriceService;
    private readonly TcpServerService _tcpServerService;
    private readonly IPluginManager _pluginManager;

    public TradingSimulatorHealthCheck(
        StockPriceService stockPriceService,
        TcpServerService tcpServerService,
        IPluginManager pluginManager)
    {
        _stockPriceService = stockPriceService;
        _tcpServerService = tcpServerService;
        _pluginManager = pluginManager;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        try
        {
            // Check if stocks are initialized
            var stocks = await _stockPriceService.GetAllCurrentPricesAsync();
            var stockCount = stocks.Count();
            data["StockCount"] = stockCount;
            data["StocksHealthy"] = stockCount == 5;

            // Check TCP server status
            var tcpStatus = _tcpServerService.IsRunning;
            data["TcpServerRunning"] = tcpStatus;

            // Check plugins loaded
            var formatters = _pluginManager.GetFormatters();
            var pluginCount = formatters.Count();
            data["PluginCount"] = pluginCount;
            data["PluginsLoaded"] = pluginCount > 0;

            var isHealthy = stockCount == 5 && tcpStatus && pluginCount > 0;

            if (isHealthy)
                return HealthCheckResult.Healthy("Trading simulator is running normally");
            else
                return HealthCheckResult.Degraded("Trading simulator has issues");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Trading simulator is unhealthy", ex);
        }
    }
}