using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MatchaLab.App.Services;
using MatchaLab.App.ViewModels;
using MatchaLab.App.Views;
using MatchaLab.Core;

namespace MatchaLab.App;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var settings = AppSettings.Load();
        ThemeManager.Apply(settings.Theme);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var api = new ApiClient(http);
            var secret = new SecretStore();
            var sub = new SubscriptionStore(api, secret);
            var split = new SplitTunnelStore(api);
            ITunnelController tunnel = OperatingSystem.IsWindows()
                ? new TunnelRouter(
                    new WindowsTunnelController(),
                    new SingBoxTunnelController())
                : new StubTunnelController();
            var vm = new AppViewModel(api, sub, split, tunnel, settings);

            if (desktop.Args?.Contains("--min") == true)
            {
                var win = new MainWindow { DataContext = vm, WindowState = Avalonia.Controls.WindowState.Minimized };
                desktop.MainWindow = win;
            }
            else
            {
                var splash = new SplashWindow();
                desktop.MainWindow = splash;
                Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                {
                    var win = new MainWindow { DataContext = vm };
                    await splash.WaitAsync();
                    desktop.MainWindow = win;
                    win.Show();
                    splash.Close();
                }, Avalonia.Threading.DispatcherPriority.Background);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
