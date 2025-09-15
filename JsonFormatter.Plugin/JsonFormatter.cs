using System.Text.Json;

namespace JsonFormatter.Plugin;

public class JsonFormatter : IDataFormatter
{
    public string FormatPrice(string symbol, decimal price, DateTime timestamp)
    {
        var data = new
        {
            symbol = symbol,
            price = price,
            timestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
        };
        
        // Returns: {"symbol":"AAPL","price":150.25,"timestamp":"2024-01-15T14:30:15.123Z"}
        return JsonSerializer.Serialize(data);
    }
}