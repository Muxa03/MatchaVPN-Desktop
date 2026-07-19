using Avalonia;
using MatchaLab.App.Services;

namespace MatchaLab.App;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length >= 2 && args[0] == "/service")
        {
            try
            {
                var logPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(args[1]) ?? ".", "service.log");
                RedirectStdErr(logPath);
                var conf = System.IO.File.ReadAllText(args[1]);
                var name = System.IO.Path.GetFileNameWithoutExtension(args[1]);
                return NativeTunnel.WireGuardTunnelService(conf, name) ? 0 : 1;
            }
            catch (Exception ex)
            {
                Crash(ex);
                return 2;
            }
        }

        AppDomain.CurrentDomain.UnhandledException += (_, e) => Crash(e.ExceptionObject as Exception);
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }
        catch (Exception ex)
        {
            Crash(ex);
            return 1;
        }
    }

    private static void RedirectStdErr(string path)
    {
        try
        {
            var fs = new System.IO.FileStream(path, System.IO.FileMode.Append,
                System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite);
            SetStdHandle(-12, fs.SafeFileHandle.DangerousGetHandle());
        }
        catch {  }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

    private static void Crash(Exception? ex)
    {
        try
        {
            var dir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MatchaLab");
            System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(System.IO.Path.Combine(dir, "crash.txt"),
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex?.ToString() ?? "unknown error"}");
        }
        catch {  }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .AfterPlatformServicesSetup(_ => Use144HzRenderTimer());

    private static void Use144HzRenderTimer()
    {
        try
        {
            var baseAsm = typeof(AvaloniaLocator).Assembly;
            object? timer = null;
            foreach (var name in new[]
            {
                "Avalonia.Rendering.SleepLoopRenderTimer",
                "Avalonia.Rendering.UiThreadRenderTimer",
                "Avalonia.Rendering.DefaultRenderTimer",
            })
            {
                if (baseAsm.GetType(name) is { } t)
                {
                    try { timer = Activator.CreateInstance(t, 144); break; }
                    catch {  }
                }
            }
            if (timer is null) return;

            var locator = typeof(AvaloniaLocator).GetProperty("CurrentMutable")?.GetValue(null);
            if (locator is null) return;
            var bind = locator.GetType().GetMethods()
                .First(m => m.Name == "Bind" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(baseAsm.GetType("Avalonia.Rendering.IRenderTimer")!);
            var helper = bind.Invoke(locator, null)!;
            helper.GetType().GetMethod("ToConstant")!.Invoke(helper, new[] { timer });
        }
        catch {  }
    }
}
