using AppVeyorCli.Api;
using AppVeyorCli.Configuration;
using AppVeyorCli.Infrastructure;
using AppVeyorCli.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AppVeyorCli.Commands.Config;

public sealed class ConfigGetCommand(IConfigService configService, IConsoleProvider consoleProvider) : AsyncCommand<GlobalSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings)
    {
        var config = configService.Load();
        var renderer = OutputRendererFactory.Create(settings.Json, consoleProvider.Console);

        if (settings.Json)
        {
            renderer.RenderJson(config, AppVeyorJsonContext.Default.AppVeyorConfig);
        }
        else
        {
            var maskedToken = config.Token is not null
                ? "***" + config.Token[^Math.Min(4, config.Token.Length)..]
                : "(not set)";

            renderer.RenderDetail("Configuration",
                ("Config file", configService.GetConfigFilePath()),
                ("Token", maskedToken),
                ("Account", config.AccountName ?? "(not set)"),
                ("API URL", config.ApiUrl));
        }

        return Task.FromResult(0);
    }
}
