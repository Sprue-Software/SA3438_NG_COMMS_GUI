using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NG_Commander;

public partial class UC_NGTxMessageDisplay : UserControl
{
    public UC_NGTxMessageDisplay()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}