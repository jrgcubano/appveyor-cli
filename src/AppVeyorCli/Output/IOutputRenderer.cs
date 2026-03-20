namespace AppVeyorCli.Output;

public record ColumnDefinition(string Header, Func<object, string> Accessor, bool Colorize = false);
public record FieldDefinition(string Label, Func<object, string> Accessor);

public interface IOutputRenderer
{
    void RenderTable<T>(string title, IEnumerable<T> items, params ColumnDefinition[] columns);
    void RenderDetail(string title, params (string Label, string Value)[] fields);
    void RenderJson<T>(T data, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo);
    void RenderSuccess(string message);
    void RenderError(string message);
    void RenderText(string text);
}
