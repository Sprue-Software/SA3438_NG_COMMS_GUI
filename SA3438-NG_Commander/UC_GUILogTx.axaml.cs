using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NG_Commander;

public partial class UC_GUILogTx : UserControl
{
    public UC_GUILogTx()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}