using SFB;

public static class FileSelectionHelper
{
    public static string SelectFile(ExtensionFilter extensionFilter)
    {
        var extensions = new[] {
            extensionFilter
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open data file...", "", extensions, false);

        if (paths.Length == 0)
            return null;

        return paths[0];
    }
}
