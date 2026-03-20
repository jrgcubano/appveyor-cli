using Spectre.Console;
using Spectre.Console.Rendering;

namespace AppVeyorCli.Output;

public sealed class ConsoleRenderer(IAnsiConsole console) : IOutputRenderer
{
    public void RenderTable<T>(string title, IEnumerable<T> items, params ColumnDefinition[] columns)
    {
        var table = new Table()
            .Title(title)
            .Border(TableBorder.Rounded)
            .Expand();

        foreach (var col in columns)
            table.AddColumn(new TableColumn(col.Header));

        foreach (var item in items)
        {
            var cells = columns.Select(col =>
            {
                var value = col.Accessor(item!);
                if (col.Colorize)
                {
                    value = ColorizeStatus(value);
                }
                return value;
            }).ToArray();

            table.AddRow(cells.Select(c => (IRenderable)new Markup(c)).ToArray());
        }

        console.Write(table);
    }

    public void RenderDetail(string title, params (string Label, string Value)[] fields)
    {
        var table = new Table()
            .Title(title)
            .Border(TableBorder.Rounded)
            .HideHeaders()
            .AddColumn("Field")
            .AddColumn("Value");

        foreach (var (label, value) in fields)
            table.AddRow($"[bold]{Markup.Escape(label)}[/]", Markup.Escape(value));

        console.Write(table);
    }

    public void RenderJson<T>(T data, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data, typeInfo);
        console.WriteLine(json);
    }

    public void RenderSuccess(string message)
        => console.MarkupLine($"[green]V[/] {Markup.Escape(message)}");

    public void RenderError(string message)
        => console.MarkupLine($"[red]X[/] {Markup.Escape(message)}");

    public void RenderText(string text)
        => console.WriteLine(text);

    private static string ColorizeStatus(string status) => status.ToLowerInvariant() switch
    {
        "success" => "[green]success[/]",
        "failed" => "[red]failed[/]",
        "running" => "[blue]running[/]",
        "queued" => "[yellow]queued[/]",
        "cancelled" => "[grey]cancelled[/]",
        _ => status
    };
}
