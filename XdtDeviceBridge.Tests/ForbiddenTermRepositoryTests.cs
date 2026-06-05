namespace XdtDeviceBridge.Tests;

public sealed class ForbiddenTermRepositoryTests
{
    [Fact]
    public void RepositoryTextFiles_ShouldNotContainForbiddenExternalMarkers()
    {
        var root = FindRepositoryRoot();
        var forbiddenMarkers = CreateForbiddenMarkers();
        var failures = new List<string>();

        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                     .Where(ShouldScanFile))
        {
            var text = File.ReadAllText(path);

            foreach (var marker in forbiddenMarkers)
            {
                if (text.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    failures.Add(Path.GetRelativePath(root, path));
                    break;
                }
            }
        }

        Assert.True(
            failures.Count == 0,
            "Forbidden external marker found in repository text file(s): "
            + string.Join(", ", failures.OrderBy(value => value, StringComparer.OrdinalIgnoreCase)));
    }

    private static string[] CreateForbiddenMarkers()
    {
        return new[]
        {
            string.Concat("Kai", " ", "Zirle", "wagen"),
            string.Concat("Zirle", "wagen"),
            string.Concat("T", "2", "W"),
            string.Concat("Team", "2", "Work")
        };
    }

    private static bool ShouldScanFile(string path)
    {
        var normalized = path.Replace('\\', '/');
        if (normalized.Contains("/.git/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/.vs/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var extension = Path.GetExtension(path);
        return extension.Equals(".cs", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".json", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".xml", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".md", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".targets", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".props", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".sln", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".wxs", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".iss", StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "XdtDeviceBridge.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root could not be found from test output directory.");
    }
}
