#:sdk Cake.Sdk@6.0.0

var solution = "./AppVeyorCli.slnx";

////////////////////////////////////////////////////////////////
// Arguments

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

////////////////////////////////////////////////////////////////
// Tasks

Task("Clean")
    .Does(ctx =>
{
    ctx.CleanDirectory("./.artifacts");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(ctx =>
{
    ctx.DotNetBuild(solution, new DotNetBuildSettings
    {
        Configuration = configuration,
        Verbosity = DotNetVerbosity.Minimal,
        NoLogo = true,
        NoIncremental = ctx.HasArgument("rebuild"),
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(ctx =>
{
    ctx.DotNetTest(solution, new DotNetTestSettings
    {
        Configuration = configuration,
        Verbosity = DotNetVerbosity.Minimal,
        NoLogo = true,
        NoRestore = true,
        NoBuild = true,
    });
});

Task("Publish")
    .IsDependentOn("Test")
    .Does(ctx =>
{
    var runtimes = new[] { "win-x64", "linux-x64", "osx-x64", "osx-arm64" };

    foreach (var runtime in runtimes)
    {
        ctx.DotNetPublish("./src/AppVeyorCli/AppVeyorCli.csproj", new DotNetPublishSettings
        {
            Configuration = configuration,
            Runtime = runtime,
            SelfContained = true,
            OutputDirectory = $"./.artifacts/publish/{runtime}",
            MSBuildSettings = new DotNetMSBuildSettings()
                .SetVersion(Argument<string>("version", "0.0.0"))
        });
    }
});

////////////////////////////////////////////////////////////////
// Targets

Task("Default")
    .IsDependentOn("Test");

////////////////////////////////////////////////////////////////
// Execution

RunTarget(target);
