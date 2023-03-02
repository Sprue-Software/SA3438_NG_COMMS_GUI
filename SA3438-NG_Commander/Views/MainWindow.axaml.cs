using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using NG_Commander.Models;
using NG_Commander.Services;
using NG_Commander.ViewModels;
using RJCP.IO.Ports;

namespace NG_Commander.Views
{
    public partial class MainWindow : Window
    {
        private bool m_ScreenCentered = false;

        public MainWindow()
        {
            InitializeComponent();
            var iv = this.GetObservable(Window.IsVisibleProperty);
            iv.Subscribe(value =>
            {
                if (value && !m_ScreenCentered)
                {
                    m_ScreenCentered = true;
                    CenterWindow();
                }
            });
        }

        private async void CenterWindow()
        {
            if (this.WindowStartupLocation == WindowStartupLocation.Manual)
                return;

            Screen? screen = null;
            while (screen == null)
            {
                await Task.Delay(1);
                screen = this.Screens.ScreenFromVisual(this);
            }

            if (this.WindowStartupLocation == WindowStartupLocation.CenterScreen)
            {
                var x = (int)Math.Floor(screen.Bounds.Width  / 2.0 - this.Bounds.Width         / 2);
                var y = (int)Math.Floor(screen.Bounds.Height / 2.0 - (this.Bounds.Height + 30) / 2);

                this.Position = new PixelPoint(x, y);
            }
            else if (this.WindowStartupLocation == WindowStartupLocation.CenterOwner)
            {
                var pw = this.Owner as Window;
                if (pw != null)
                {
                    var x = (int)Math.Floor(pw.Bounds.Width  / 2 - this.Bounds.Width         / 2 + pw.Position.X);
                    var y = (int)Math.Floor(pw.Bounds.Height / 2 - (this.Bounds.Height + 30) / 2 + pw.Position.Y);

                    this.Position = new PixelPoint(x, y);
                }
            }
        }

        private void MnuExit_OnClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}