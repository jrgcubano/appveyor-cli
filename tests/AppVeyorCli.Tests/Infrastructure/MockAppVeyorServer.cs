using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AppVeyorCli.Tests.Infrastructure;

public sealed class MockAppVeyorServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly Dictionary<string, (int StatusCode, string Body)> _routes = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly int _port;

    public string BaseUrl => $"http://localhost:{_port}/api/";

    public MockAppVeyorServer()
    {
        _listener = new HttpListener();
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                _port = Random.Shared.Next(49152, 65535);
                _listener.Prefixes.Clear();
                _listener.Prefixes.Add($"http://localhost:{_port}/");
                _listener.Start();
                break;
            }
            catch (HttpListenerException) when (attempt < 9)
            {
                _listener = new HttpListener();
            }
        }
        Task.Run(ListenAsync);
    }

    /// <summary>
    /// Register a response with a raw string body.
    /// Path is the exact path the HttpListener will see (e.g. "/projects").
    /// </summary>
    public void RegisterRawResponse(string method, string path, int statusCode, string body = "")
    {
        var key = $"{method.ToUpperInvariant()}:{path}";
        _routes[key] = (statusCode, body);
    }

    /// <summary>
    /// Register a response using source-gen serialization.
    /// Path is the exact path the HttpListener will see (e.g. "/projects").
    /// </summary>
    public void RegisterJsonResponse<T>(string method, string path, int statusCode, T body, JsonTypeInfo<T> typeInfo)
    {
        var key = $"{method.ToUpperInvariant()}:{path}";
        var json = JsonSerializer.Serialize(body, typeInfo);
        _routes[key] = (statusCode, json);
    }

    private async Task ListenAsync()
    {
        while (!_cts.Token.IsCancellationRequested && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                var method = context.Request.HttpMethod;
                var rawPath = context.Request.Url?.PathAndQuery ?? "";

                // Try exact match (including query string)
                var key = $"{method}:{rawPath}";
                if (!_routes.TryGetValue(key, out var route))
                {
                    // Try without query string
                    var pathOnly = context.Request.Url?.AbsolutePath ?? "";
                    key = $"{method}:{pathOnly}";
                    _routes.TryGetValue(key, out route);
                }

                if (route.Body is not null)
                {
                    context.Response.StatusCode = route.StatusCode;
                    context.Response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(route.Body);
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer);
                }
                else
                {
                    context.Response.StatusCode = 404;
                    var msg = $"Route not found: {method}:{rawPath}";
                    var notFound = Encoding.UTF8.GetBytes($"{{\"error\":\"{msg}\"}}");
                    context.Response.ContentLength64 = notFound.Length;
                    await context.Response.OutputStream.WriteAsync(notFound);
                }
                context.Response.Close();
            }
            catch (ObjectDisposedException) { break; }
            catch (HttpListenerException) { break; }
        }
    }

    public HttpClient CreateHttpClient()
    {
        return new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        _listener.Close();
        _cts.Dispose();
    }
}
