using System.Reflection;
using System.Runtime.Loader;
using TradingSimulator.Domain.Interfaces;

namespace TradingSimulator.Infrastructure.Plugins;

public class PluginManager : IPluginManager, IDisposable
{
    private readonly ILogger<PluginManager> _logger;
    private readonly List<AssemblyLoadContext> _loadContexts = new();
    private readonly List<IDataFormatter> _formatters = new();
    private readonly string _pluginsPath;

    public PluginManager(ILogger<PluginManager> logger, IConfiguration configuration)
    {
        _logger = logger;
        _pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
    }

    public async Task LoadPluginsAsync()
    {
        if (!Directory.Exists(_pluginsPath))
        {
            Directory.CreateDirectory(_pluginsPath);
            _logger.LogWarning("Plugins directory created at {Path}", _pluginsPath);
            return;
        }

        var pluginFiles = Directory.GetFiles(_pluginsPath, "*.dll");
        
        foreach (var pluginFile in pluginFiles)
        {
            try
            {
                var loadContext = new AssemblyLoadContext($"Plugin_{Path.GetFileNameWithoutExtension(pluginFile)}", true);
                _loadContexts.Add(loadContext);
                
                var assembly = loadContext.LoadFromAssemblyPath(pluginFile);
                
                var formatterTypes = assembly.GetTypes()
                    .Where(t => typeof(IDataFormatter).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                
                foreach (var type in formatterTypes)
                {
                    if (Activator.CreateInstance(type) is IDataFormatter formatter)
                    {
                        _formatters.Add(formatter);
                        _logger.LogInformation("Loaded plugin formatter: {Type} from {Assembly}", type.Name, pluginFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin from {File}", pluginFile);
            }
        }
    }

    public IEnumerable<IDataFormatter> GetFormatters()
    {
        return _formatters.AsReadOnly();
    }

    public void UnloadPlugins()
    {
        _formatters.Clear();
        
        foreach (var context in _loadContexts)
        {
            try
            {
                context.Unload();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unloading plugin context");
            }
        }
        
        _loadContexts.Clear();
        _logger.LogInformation("All plugins unloaded");
    }

    public void Dispose()
    {
        UnloadPlugins();
    }
}