using System.Text.Json;
using Xunit;

namespace TradingSimulator.Tests;

public class PluginFormatterTests
{
    [Fact]
    public void JsonFormatter_FormatPrice_ReturnsValidJson()
    {
        // Arrange
        var formatter = new JsonFormatter.Plugin.JsonFormatter();
        var symbol = "AAPL";
        var price = 150.25m;
        var timestamp = new DateTime(2024, 1, 15, 10, 30, 0);

        // Act
        var result = formatter.FormatPrice(symbol, price, timestamp);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("AAPL", result);
        Assert.Contains("150.25", result);
        
        // Verify it's valid JSON
        var jsonDoc = JsonDocument.Parse(result);
        Assert.Equal("AAPL", jsonDoc.RootElement.GetProperty("symbol").GetString());
        Assert.Equal(150.25m, jsonDoc.RootElement.GetProperty("price").GetDecimal());
    }

    [Fact]
    public void CsvFormatter_FormatPrice_ReturnsValidCsv()
    {
        // Arrange
        var formatter = new CsvFormatter.Plugin.CsvFormatter();
        var symbol = "MSFT";
        var price = 300.50m;
        var timestamp = new DateTime(2024, 1, 15, 14, 45, 30);

        // Act
        var result = formatter.FormatPrice(symbol, price, timestamp);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MSFT,300.50,14:45:30", result);
    }


}