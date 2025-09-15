using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TradingSimulator.Infrastructure.Plugins;
using Xunit;

namespace TradingSimulator.Tests;

public class PluginManagerTests : IDisposable
{
    private readonly Mock<ILogger<PluginManager>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly PluginManager _pluginManager;
    private readonly string _testPluginsPath;

    public PluginManagerTests()
    {
        _mockLogger = new Mock<ILogger<PluginManager>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _pluginManager = new PluginManager(_mockLogger.Object, _mockConfiguration.Object);
        _testPluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
    }

    [Fact]
    public async Task LoadPluginsAsync_CreatesPluginsDirectory_WhenNotExists()
    {
        // Arrange
        if (Directory.Exists(_testPluginsPath))
            Directory.Delete(_testPluginsPath, true);

        // Act
        await _pluginManager.LoadPluginsAsync();

        // Assert
        Assert.True(Directory.Exists(_testPluginsPath));
    }

    [Fact]
    public void GetFormatters_ReturnsEmptyCollection_WhenNoPluginsLoaded()
    {
        // Act
        var formatters = _pluginManager.GetFormatters();

        // Assert
        Assert.Empty(formatters);
    }

    [Fact]
    public void UnloadPlugins_DoesNotThrow_WhenNoPluginsLoaded()
    {
        // Act & Assert
        var exception = Record.Exception(() => _pluginManager.UnloadPlugins());
        Assert.Null(exception);
    }



    [Fact]
    public void Dispose_UnloadsPlugins_Successfully()
    {
        // Act & Assert
        var exception = Record.Exception(() => _pluginManager.Dispose());
        Assert.Null(exception);
    }

    public void Dispose()
    {
        _pluginManager.Dispose();
        if (Directory.Exists(_testPluginsPath))
        {
            try
            {
                Directory.Delete(_testPluginsPath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}