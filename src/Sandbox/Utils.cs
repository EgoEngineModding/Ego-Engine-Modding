namespace Sandbox;

public static class Utils
{
    public static IEnumerable<string> GetFiles(string filter, params string[] gameFolders)
    {
        var files = Enumerable.Empty<string>();
        foreach (var folder in gameFolders)
        {
            var folderFiles = Directory.EnumerateFiles(folder, filter, SearchOption.AllDirectories);
            files = files.Concat(folderFiles);
        }

        return files;
    }
}
