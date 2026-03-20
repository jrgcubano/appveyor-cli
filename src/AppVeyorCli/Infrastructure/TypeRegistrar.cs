using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AppVeyorCli.Infrastructure;

public sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());

#pragma warning disable IL2067 // Spectre.Console.Cli ITypeRegistrar interface lacks trim annotations
    public void Register(Type service, Type implementation)
        => services.AddSingleton(service, implementation);
#pragma warning restore IL2067

    public void RegisterInstance(Type service, object implementation)
        => services.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> factory)
        => services.AddSingleton(service, _ => factory());
}
