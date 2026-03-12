using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

public sealed record PluginStatus(bool IsRunning, bool OperationInProgress, string? CurrentOperation, int QueueDepth);

public sealed class JsonRpcDispatchException : Exception
{
  public JsonRpcDispatchException(string code, string message) : base(message)
  {
    Code = code;
  }

  public string Code { get; }
}

public static class PluginRuntime
{
  public const int Port = 8080;

  private static readonly object Sync = new();
  private static RpcTcpServer? _server;
  private static int _queueDepth;
  private static int _activeOperations;
  private static string? _currentOperation;

  public static void StartServer()
  {
    lock (Sync)
    {
      if (_server != null)
      {
        return;
      }

      _server = new RpcTcpServer(Port, HandleRawRequestAsync);
      _server.Start();
    }
  }

  public static void StopServer()
  {
    lock (Sync)
    {
      _server?.Stop();
      _server = null;
      _currentOperation = null;
      _activeOperations = 0;
      _queueDepth = 0;
    }
  }

  public static PluginStatus GetStatus()
  {
    lock (Sync)
    {
      return new PluginStatus(_server != null, _activeOperations > 0, _currentOperation, _queueDepth);
    }
  }

  public static async Task<string> HandleRawRequestAsync(string rawRequest, CancellationToken cancellationToken)
  {
    JsonNode? parsed;
    try
    {
      parsed = JsonNode.Parse(rawRequest);
    }
    catch (Exception ex)
    {
      return SerializeError(null, "CIVIL3D.INVALID_INPUT", $"Invalid JSON request: {ex.Message}");
    }

    if (parsed is not JsonObject request)
    {
      return SerializeError(null, "CIVIL3D.INVALID_INPUT", "JSON-RPC request must be an object.");
    }

    var id = request["id"]?.DeepClone();
    var method = request["method"]?.GetValue<string>();
    var parameters = request["params"] as JsonObject;

    if (string.IsNullOrWhiteSpace(method))
    {
      return SerializeError(id, "CIVIL3D.INVALID_INPUT", "JSON-RPC request is missing method.");
    }

    lock (Sync)
    {
      _queueDepth++;
    }

    try
    {
      lock (Sync)
      {
        _queueDepth--;
        _activeOperations++;
        _currentOperation = method;
      }

      var result = await CommandDispatcher.DispatchAsync(method, parameters, cancellationToken);
      return SerializeResult(id, result);
    }
    catch (JsonRpcDispatchException ex)
    {
      return SerializeError(id, ex.Code, ex.Message);
    }
    catch (Exception ex)
    {
      return SerializeError(id, "CIVIL3D.TRANSACTION_FAILED", ex.Message);
    }
    finally
    {
      lock (Sync)
      {
        _activeOperations = Math.Max(0, _activeOperations - 1);
        _currentOperation = _activeOperations == 0 ? null : _currentOperation;
      }
    }
  }

  public static object? GetParameter(JsonObject? parameters, string name)
  {
    if (parameters == null)
    {
      return null;
    }

    return parameters.TryGetPropertyValue(name, out var value) ? value : null;
  }

  public static string GetRequiredString(JsonObject? parameters, string name)
  {
    var value = GetParameter(parameters, name) as JsonNode;
    if (value == null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Missing required parameter '{name}'.");
    }

    var stringValue = value.GetValue<string>();
    if (string.IsNullOrWhiteSpace(stringValue))
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Parameter '{name}' must be a non-empty string.");
    }

    return stringValue;
  }

  public static double GetRequiredDouble(JsonObject? parameters, string name)
  {
    var value = GetParameter(parameters, name) as JsonNode;
    if (value == null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Missing required parameter '{name}'.");
    }

    return value.GetValue<double>();
  }

  public static int GetRequiredInt(JsonObject? parameters, string name)
  {
    var value = GetParameter(parameters, name) as JsonNode;
    if (value == null)
    {
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Missing required parameter '{name}'.");
    }

    return value.GetValue<int>();
  }

  public static string? GetOptionalString(JsonObject? parameters, string name)
  {
    var value = GetParameter(parameters, name) as JsonNode;
    return value == null ? null : value.GetValue<string?>();
  }

  public static double? GetOptionalDouble(JsonObject? parameters, string name)
  {
    var value = GetParameter(parameters, name) as JsonNode;
    return value == null ? null : value.GetValue<double>();
  }

  public static int? GetOptionalInt(JsonObject? parameters, string name)
  {
    var value = GetParameter(parameters, name) as JsonNode;
    return value == null ? null : value.GetValue<int>();
  }

  private static string SerializeResult(JsonNode? id, object? result)
  {
    var response = new JsonObject
    {
      ["jsonrpc"] = "2.0",
      ["id"] = id,
      ["result"] = result == null ? null : JsonSerializer.SerializeToNode(result, JsonSerializerOptions),
    };

    return response.ToJsonString(JsonSerializerOptions);
  }

  private static string SerializeError(JsonNode? id, string code, string message)
  {
    var response = new JsonObject
    {
      ["jsonrpc"] = "2.0",
      ["id"] = id,
      ["error"] = new JsonObject
      {
        ["code"] = code,
        ["message"] = message,
      },
    };

    return response.ToJsonString(JsonSerializerOptions);
  }

  private static readonly JsonSerializerOptions JsonSerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false,
  };
}
