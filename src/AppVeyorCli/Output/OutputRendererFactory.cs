using Spectre.Console;

namespace AppVeyorCli.Output;

public static class OutputRendererFactory
{
    public static IOutputRenderer Create(bool json, IAnsiConsole console)
    {
        // APPVEYOR_OUTPUT env var can also force JSON mode
        var envOutput = Environment.GetEnvironmentVariable("APPVEYOR_OUTPUT");
        if (json || string.Equals(envOutput, "json", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonRenderer(console);
        }

        return new ConsoleRenderer(console);
    }
}
