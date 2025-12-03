using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TradingSimulator.Application.Services;

public class TcpServerService : IDisposable
{
    private readonly ILogger<TcpServerService> _logger;
    private TcpListener? _tcpListener;
    private readonly List<TcpClient> _clients = new();
    private readonly object _clientsLock = new();
    private CancellationTokenSource? _cancellationTokenSource;
    
    public bool IsRunning => _tcpListener != null && _cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested;

    public TcpServerService(ILogger<TcpServerService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(int port = 8080)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _tcpListener = new TcpListener(IPAddress.Any, port);
        _tcpListener.Start();
        
        _logger.LogInformation("TCP Server started on port {Port}", port);
        
        var clientAcceptorTask = Task.Run(async () => await AcceptClientsAsync(_cancellationTokenSource.Token));
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _tcpListener != null)
        {
            try
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                lock (_clientsLock)
                {
                    _clients.Add(tcpClient);
                }
                _logger.LogInformation("TCP Client connected: {Client}", tcpClient.Client.RemoteEndPoint);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting TCP client");
            }
        }
    }

    public async Task BroadcastAsync(string message)
    {
        var data = Encoding.UTF8.GetBytes(message + "\n");
        List<TcpClient> clientsToRemove = new();

        lock (_clientsLock)
        {
            foreach (var client in _clients)
            {
                try
                {
                    if (client.Connected)
                    {
                        client.GetStream().WriteAsync(data, 0, data.Length);
                    }
                    else
                    {
                        clientsToRemove.Add(client);
                    }
                }
                catch
                {
                    clientsToRemove.Add(client);
                }
            }

            foreach (var client in clientsToRemove)
            {
                _clients.Remove(client);
                client.Close();
            }
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _tcpListener?.Stop();
        
        lock (_clientsLock)
        {
            foreach (var client in _clients)
            {
                client.Close();
            }
            _clients.Clear();
        }
        
        _logger.LogInformation("TCP Server stopped");
    }

    public void Dispose()
    {
        Stop();
        _cancellationTokenSource?.Dispose();
    }
}