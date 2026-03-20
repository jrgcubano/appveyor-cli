using System.ComponentModel;
using AppVeyorCli.Configuration;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Config;

public sealed class ConfigSetSettings : GlobalSettings
{
    [CommandOption("--token <TOKEN>")]
    [Description("AppVeyor API token")]
    public string? Token { get; init; }

    [CommandOption("--account <ACCOUNT>")]
    [Description("AppVeyor account name")]
    public string? Account { get; init; }

    [CommandOption("--api-url <URL>")]
    [Description("AppVeyor API base URL (for self-hosted)")]
    public string? ApiUrl { get; init; }
}

public sealed class ConfigSetCommand(IConfigService configService, IConsoleProvider consoleProvider) : AsyncCommand<ConfigSetSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, ConfigSetSettings settings)
    {
        ReadOnlyGuard.ThrowIfReadOnly(settings);
        var config = configService.Load();
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);

        if (!string.IsNullOrWhiteSpace(settings.Token))
        {
            config.Token = settings.Token;
        }
        else if (!settings.Json)
        {
            config.Token = consoleProvider.Console.Ask<string>("Enter your AppVeyor API [green]token[/]:");
        }

        if (!string.IsNullOrWhiteSpace(settings.Account))
        {
            config.AccountName = settings.Account;
        }
        else if (!settings.Json && consoleProvider.Console.Confirm("Do you want to set an account name?", false))
        {
            config.AccountName = consoleProvider.Console.Ask<string>("Enter your AppVeyor [green]account name[/]:");
        }

        if (!string.IsNullOrWhiteSpace(settings.ApiUrl))
        {
            config.ApiUrl = settings.ApiUrl;
        }

        configService.Save(config);
        renderer.RenderSuccess($"Configuration saved to {configService.GetConfigFilePath()}");

        return Task.FromResult(0);
    }
}
