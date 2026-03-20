using System.Text.Json;
using AppVeyorCli.Api;
using Spectre.Console;

namespace AppVeyorCli.Output;

public sealed class JsonRenderer(IAnsiConsole console) : IOutputRenderer
{
    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        WriteIndented = true,
        TypeInfoResolver = AppVeyorJsonContext.Default
    };

    public void RenderTable<T>(string title, IEnumerable<T> items, params ColumnDefinition[] columns)
    {
        // Fallback — uses source-gen context resolver
#pragma warning disable IL2026
        var json = JsonSerializer.Serialize(items, IndentedOptions);
#pragma warning restore IL2026
        console.WriteLine(json);
    }

    public void RenderDetail(string title, params (string Label, string Value)[] fields)
    {
        var dict = fields.ToDictionary(f => f.Label, f => f.Value);
        var json = JsonSerializer.Serialize(dict, AppVeyorJsonContext.Default.DictionaryStringString);
        console.WriteLine(json);
    }

    public void RenderJson<T>(T data, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo)
    {
        var json = JsonSerializer.Serialize(data, typeInfo);
        console.WriteLine(json);
    }

    public void RenderSuccess(string message)
    {
        // In JSON mode, success is silent — the data speaks for itself
    }

    public void RenderError(string message)
    {
        var dict = new Dictionary<string, string> { ["error"] = message };
        var json = JsonSerializer.Serialize(dict, AppVeyorJsonContext.Default.DictionaryStringString);
        console.WriteLine(json);
    }

    public void RenderText(string text)
        => console.WriteLine(text);
}
