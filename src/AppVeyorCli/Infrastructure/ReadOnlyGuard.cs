namespace AppVeyorCli.Infrastructure;

public static class ReadOnlyGuard
{
    public static void ThrowIfReadOnly(GlobalSettings settings)
    {
        if (settings.IsReadOnly)
        {
            throw new InvalidOperationException(
                "Write operation blocked — running in read-only mode. " +
                "Remove --read-only flag or unset APPVEYOR_READ_ONLY to allow writes.");
        }
    }
}
