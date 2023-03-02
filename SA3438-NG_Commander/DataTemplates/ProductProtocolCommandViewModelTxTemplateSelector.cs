using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using NG_Commander.ViewModels;

namespace NG_Commander.DataTemplates;

public class ProductProtocolCommandViewModelTxTemplateSelector : IDataTemplate
{
    [Content] public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new();

    // Build the DataTemplate here
    public IControl Build(object param)
    {
        string? TxValueType = ((ProductProtocolCommandViewModel)param).TxValueType;

        if (TxValueType is null)
            return new DataTemplate() { Content = null, DataType = null }.Build(param);

        if (!AvailableTemplates.ContainsKey(TxValueType))
            return AvailableTemplates["UnknownDataType"].Build(param);


        return AvailableTemplates[TxValueType].Build(param);
    }

    public bool Match(object data)
    {
        return data is ProductProtocolCommandViewModel;
    }
}