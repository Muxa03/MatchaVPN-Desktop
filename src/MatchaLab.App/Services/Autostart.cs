using System.Diagnostics;
using Microsoft.Win32;

namespace MatchaLab.App.Services;

public static class Autostart
{
    private const string TaskName = "MatchaLab";
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsEnabled()
    {
        if (!OperatingSystem.IsWindows()) return false;
        return Schtasks($"/Query /TN \"{TaskName}\"") == 0;
    }

    public static void Set(bool on)
    {
        if (!OperatingSystem.IsWindows()) return;
        RemoveLegacyRunValue();
        if (on)
        {
            var exe = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exe)) return;
            Schtasks($"/Create /F /RL HIGHEST /SC ONLOGON /TN \"{TaskName}\" /TR \"\\\"{exe}\\\" --min\"");
        }
        else
            Schtasks($"/Delete /F /TN \"{TaskName}\"");
    }

    private static int Schtasks(string args)
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo("schtasks", args)
            { CreateNoWindow = true, UseShellExecute = false });
            if (p is null) return -1;
            p.WaitForExit(10000);
            return p.HasExited ? p.ExitCode : -1;
        }
        catch { return -1; }
    }

    private static void RemoveLegacyRunValue()
    {
        if (!OperatingSystem.IsWindows()) return;
        try
        {
            using var k = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            k?.DeleteValue(TaskName, throwOnMissingValue: false);
        }
        catch {  }
    }
}
