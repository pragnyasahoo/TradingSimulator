namespace CsvFormatter.Plugin;

public class CsvFormatter : IDataFormatter
{
    public string FormatPrice(string symbol, decimal price, DateTime timestamp)
    {
        return $"{symbol},{price:F2},{timestamp:HH:mm:ss}";
    }
}