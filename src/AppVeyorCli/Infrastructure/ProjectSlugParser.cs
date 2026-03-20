namespace AppVeyorCli.Infrastructure;

internal static class ProjectSlugParser
{
    public static (string Account, string Slug) Parse(string projectSlug)
    {
        var parts = projectSlug.Split('/', 2);
        return parts.Length == 2
            ? (parts[0], parts[1])
            : throw new ArgumentException("Project must be in 'account/slug' format.", nameof(projectSlug));
    }
}
