namespace TradingSimulator.Domain.Interfaces;

public interface IPluginManager
{
    Task LoadPluginsAsync();
    IEnumerable<IDataFormatter> GetFormatters();
    void UnloadPlugins();
}