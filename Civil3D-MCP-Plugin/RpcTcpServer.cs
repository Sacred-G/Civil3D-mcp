using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Civil3DMcpPlugin;

public sealed class RpcTcpServer
{
  // Must match MAX_REQUEST_BYTES in src/utils/SocketClient.ts: an oversized
  // request is dropped here without a response, so the Node client refuses it
  // before sending. Change both together.
  private const int MaxRequestBytes = 1024 * 1024;
  private static readonly TimeSpan RequestReadTimeout = TimeSpan.FromSeconds(30);

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
    try
    {
      using (client)
      await using (var stream = client.GetStream())
      {
        var request = await ReadSingleJsonObjectAsync(stream, cancellationToken);
        if (string.IsNullOrWhiteSpace(request))
        {
          return;
        }

        using var requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var disconnectMonitorCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var disconnectMonitor = MonitorClientDisconnectAsync(
          stream,
          requestCancellation,
          disconnectMonitorCancellation.Token);

        var response = await _handler(request, requestCancellation.Token);
        disconnectMonitorCancellation.Cancel();
        var clientDisconnected = await disconnectMonitor;
        if (clientDisconnected)
        {
          PluginLog.Debug("RpcTcpServer", "Skipped response write because the client disconnected.");
          return;
        }

        var responseBytes = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(responseBytes, cancellationToken);
        await stream.FlushAsync(cancellationToken);
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      // Normal plugin shutdown.
    }
    catch (TimeoutException)
    {
      PluginLog.Warn("RpcTcpServer", $"Closed a client that did not send a complete request within {RequestReadTimeout.TotalSeconds:0} seconds.");
    }
    catch (OperationCanceledException)
    {
      PluginLog.Debug("RpcTcpServer", "Client disconnected and its queued request was cancelled.");
    }
    catch (InvalidDataException ex)
    {
      PluginLog.Warn("RpcTcpServer", ex.Message);
    }
    catch (Exception ex)
    {
      PluginLog.Error("RpcTcpServer", "Client request processing failed.", ex);
    }
  }

  private static async Task<string> ReadSingleJsonObjectAsync(NetworkStream stream, CancellationToken cancellationToken)
  {
    var buffer = new byte[8192];
    using var requestBuffer = new MemoryStream();
    using var readTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    readTimeout.CancelAfter(RequestReadTimeout);

    try
    {
      while (!readTimeout.IsCancellationRequested)
      {
        var bytesRead = await stream.ReadAsync(buffer, readTimeout.Token);
        if (bytesRead <= 0)
        {
          break;
        }

        if (requestBuffer.Length + bytesRead > MaxRequestBytes)
        {
          throw new InvalidDataException($"RPC request exceeds the {MaxRequestBytes}-byte limit.");
        }

        await requestBuffer.WriteAsync(buffer.AsMemory(0, bytesRead), readTimeout.Token);
        var requestBytes = requestBuffer.GetBuffer();
        var requestLength = checked((int)requestBuffer.Length);

        try
        {
          using var _ = JsonDocument.Parse(
            new ReadOnlyMemory<byte>(requestBytes, 0, requestLength));
          return Encoding.UTF8.GetString(requestBytes, 0, requestLength);
        }
        catch (JsonException)
        {
        }
      }
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && readTimeout.IsCancellationRequested)
    {
      throw new TimeoutException("RPC request read timed out.");
    }

    return Encoding.UTF8.GetString(
      requestBuffer.GetBuffer(),
      0,
      checked((int)requestBuffer.Length));
  }

  private static async Task<bool> MonitorClientDisconnectAsync(
    NetworkStream stream,
    CancellationTokenSource requestCancellation,
    CancellationToken monitorCancellation)
  {
    var probe = new byte[1];
    try
    {
      while (!monitorCancellation.IsCancellationRequested)
      {
        var bytesRead = await stream.ReadAsync(probe, monitorCancellation);
        if (bytesRead == 0)
        {
          requestCancellation.Cancel();
          return true;
        }

        // The protocol permits one JSON request per connection. Ignore any
        // trailing bytes while continuing to watch for the peer's FIN.
      }
    }
    catch (OperationCanceledException) when (monitorCancellation.IsCancellationRequested)
    {
      return false;
    }
    catch (IOException)
    {
      requestCancellation.Cancel();
      return true;
    }
    catch (SocketException)
    {
      requestCancellation.Cancel();
      return true;
    }

    return false;
  }
}
