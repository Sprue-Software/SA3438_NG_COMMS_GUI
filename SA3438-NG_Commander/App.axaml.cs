using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NG_Commander.Services;
using NG_Commander.ViewModels;
using NG_Commander.Views;

namespace NG_Commander;

public partial class App : Application
{
    private SettingsService SettingsProvider = new();
    private NGProtocol      NGProtocol;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        NGProtocol = new NGProtocol(SettingsProvider);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(NGProtocol, SettingsProvider),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}