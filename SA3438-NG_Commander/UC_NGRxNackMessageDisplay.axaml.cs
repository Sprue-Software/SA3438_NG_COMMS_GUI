using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NG_Commander;

public partial class UC_NGRxNackMessageDisplay : UserControl
{
    public UC_NGRxNackMessageDisplay()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}