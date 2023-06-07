using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;


namespace SA3438_NG_Commander.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void MnuExportLogs_OnClick(object? sender, RoutedEventArgs e)
    {
        //todo implement async export
    }
    private void MnuExit_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.Shutdown();
        }
        else
        {
            Close();    
        }
    }

    private void onDebugScollChanged(object? Sender, AvaloniaPropertyChangedEventArgs E)
    {
        var scrollViewer = Sender as ScrollViewer;
        Dispatcher.UIThread.Invoke(() =>
        {
            scrollViewer.ScrollToHome();
            scrollViewer.InvalidateVisual();
        });
        //scrollViewer.ScrollToEnd();
        //scrollViewer.ScrollToHome();
        
        //scrollViewer.InvalidateVisual();
        //foreach (var child in scrollViewer.GetVisualChildren())
        //{
          //  child.InvalidateVisual();
//        }
        //scrollViewer.Inb
        
    }

    private void AvaloniaObject_OnPropertyChanged(object? Sender, AvaloniaPropertyChangedEventArgs E)
    {
        var scrollViewer = Sender as ScrollViewer;
        Dispatcher.UIThread.Invoke(() =>
        {
            scrollViewer.ScrollToEnd();
            scrollViewer.InvalidateVisual();
        });
    }
}