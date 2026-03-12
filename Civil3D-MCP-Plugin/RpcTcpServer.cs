using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Civil3DMcpPlugin;

public sealed class RpcTcpServer
{
  private readonly int _port;
  private readonly Func<string, CancellationToken, Task<string>> _handler;
  private readonly CancellationTokenSource _cts = new();
  private TcpListener? _listener;
  private Task? _acceptLoop;

  public RpcTcpServer(int port, Func<string, CancellationToken, Task<string>> handler)
  {
    _port = port;
    _handler = handler;
  }

  public void Start()
  {
    _listener = new TcpListener(IPAddress.Loopback, _port);
    _listener.Start();
    _acceptLoop = Task.Run(AcceptLoopAsync, _cts.Token);
  }

  public void Stop()
  {
    try
    {
      _cts.Cancel();
      _listener?.Stop();
      _acceptLoop?.Wait(TimeSpan.FromSeconds(1));
    }
    catch
    {
    }
  }

  private async Task AcceptLoopAsync()
  {
    while (!_cts.IsCancellationRequested)
    {
      TcpClient? client = null;
      try
      {
        client = await _listener!.AcceptTcpClientAsync(_cts.Token);
        _ = Task.Run(() => ProcessClientAsync(client, _cts.Token), _cts.Token);
      }
      catch (OperationCanceledException)
      {
        break;
      }
      catch
      {
        client?.Dispose();
      }
    }
  }

  private async Task ProcessClientAsync(TcpClient client, CancellationToken cancellationToken)
  {
    using (client)
    await using (var stream = client.GetStream())
    {
      var request = await ReadSingleJsonObjectAsync(stream, cancellationToken);
      if (string.IsNullOrWhiteSpace(request))
      {
        return;
      }

      var response = await _handler(request, cancellationToken);
      var responseBytes = Encoding.UTF8.GetBytes(response);
      await stream.WriteAsync(responseBytes, cancellationToken);
      await stream.FlushAsync(cancellationToken);
    }
  }

  private static async Task<string> ReadSingleJsonObjectAsync(NetworkStream stream, CancellationToken cancellationToken)
  {
    var buffer = new byte[8192];
    var builder = new StringBuilder();

    while (!cancellationToken.IsCancellationRequested)
    {
      var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
      if (bytesRead <= 0)
      {
        break;
      }

      builder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
      var text = builder.ToString();

      try
      {
        using var _ = JsonDocument.Parse(text);
        return text;
      }
      catch (JsonException)
      {
      }
    }

    return builder.ToString();
  }
}
