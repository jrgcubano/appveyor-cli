using Spectre.Console;

namespace AppVeyorCli.Output;

public interface IConsoleProvider
{
    IAnsiConsole Console { get; }
}

public sealed class ConsoleProvider(IAnsiConsole console) : IConsoleProvider
{
    public IAnsiConsole Console { get; } = console;
}
