using AppVeyorCli.Infrastructure;

namespace AppVeyorCli.Tests.Infrastructure;

public class ReadOnlyGuardTests
{
    [Fact]
    public void ThrowIfReadOnly_WhenReadOnly_Throws()
    {
        var settings = new GlobalSettings { ReadOnly = true };
        Assert.Throws<InvalidOperationException>(() => ReadOnlyGuard.ThrowIfReadOnly(settings));
    }

    [Fact]
    public void ThrowIfReadOnly_WhenNotReadOnly_DoesNotThrow()
    {
        var settings = new GlobalSettings();
        ReadOnlyGuard.ThrowIfReadOnly(settings); // Should not throw
    }
}
