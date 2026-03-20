using System.ComponentModel;
using Spectre.Console.Cli;

namespace AppVeyorCli.Infrastructure;

public class GlobalSettings : CommandSettings
{
    [CommandOption("--json")]
    [Description("Output as JSON for machine/AI consumption")]
    public bool Json { get; init; }

    [CommandOption("--verbose")]
    [Description("Show detailed output")]
    public bool Verbose { get; init; }

    [CommandOption("--read-only")]
    [Description("Block all write operations (safe for exploration)")]
    public bool ReadOnly { get; init; }

    /// <summary>
    /// Returns true if read-only mode is active (via flag or APPVEYOR_READ_ONLY env var).
    /// </summary>
    public bool IsReadOnly =>
        ReadOnly || string.Equals(
            Environment.GetEnvironmentVariable("APPVEYOR_READ_ONLY"), "true",
            StringComparison.OrdinalIgnoreCase);
}
