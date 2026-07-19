namespace Infrastructure.Views;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Application.Views;

public sealed class BrowserViewLauncher : IViewLauncher
{
    private static readonly TimeSpan MaxTempFileAge = TimeSpan.FromHours(24);

    private readonly string _tempDirectory;

    public BrowserViewLauncher()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "axoloop-calculator-views");
        Directory.CreateDirectory(_tempDirectory);
        CleanupOldFiles();
    }

    public void Open(string content, string extension)
    {
        var fileName = $"{Guid.NewGuid():N}.{extension.TrimStart('.')}";
        var path = Path.Combine(_tempDirectory, fileName);
        File.WriteAllText(path, content);

        if (!TryLaunch(path))
            Console.WriteLine($"Could not open a viewer automatically. The generated file is at: {path}");
    }

    private static bool TryLaunch(string path)
    {
        try
        {
            ProcessStartInfo startInfo;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                startInfo = new ProcessStartInfo(path) { UseShellExecute = true };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                startInfo = new ProcessStartInfo("open") { UseShellExecute = false };
                startInfo.ArgumentList.Add(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo = new ProcessStartInfo("xdg-open") { UseShellExecute = false };
                startInfo.ArgumentList.Add(path);
            }
            else
            {
                return false;
            }

            using var process = Process.Start(startInfo);
            return process is not null;
        }
        catch
        {
            return false;
        }
    }

    private void CleanupOldFiles()
    {
        try
        {
            var cutoff = DateTime.UtcNow - MaxTempFileAge;
            foreach (var file in Directory.EnumerateFiles(_tempDirectory))
            {
                if (File.GetLastWriteTimeUtc(file) >= cutoff) continue;
                try { File.Delete(file); } catch { /* best effort; another process may still hold it */ }
            }
        }
        catch
        {
            // Cleanup is best-effort and must never block a launch.
        }
    }
}
