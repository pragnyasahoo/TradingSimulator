using System.Net.Sockets;
using System.Text;

try
{
    using var client = new TcpClient();
    await client.ConnectAsync("localhost", 8080);
    Console.WriteLine("Connected to TCP Server. Listening for price updates...");
    
    using var stream = client.GetStream();
    var buffer = new byte[1024];
    
    while (client.Connected)
    {
        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead > 0)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message.Trim()}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();