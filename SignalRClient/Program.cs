using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:44308/pricehub")
    .Build();

connection.On<object>("PriceUpdate", (priceData) =>
{
    var json = JsonSerializer.Serialize(priceData, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Price Update: {json}");
});

try
{
    await connection.StartAsync();
    Console.WriteLine("Connected to SignalR Hub. Press any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"Error connecting to SignalR Hub: {ex.Message}");
}
finally
{
    await connection.DisposeAsync();
}