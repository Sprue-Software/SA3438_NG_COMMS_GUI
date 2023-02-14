using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NG_Commander.Models;

namespace NG_Commander.ViewModels;

public class ProductProtocolViewModel : ObservableObject
{
    public String                                        Name            { get; set; } = "Warning Undefined";
    //public ProtocolHandler?                              ProtocolHandler { get; set; }
    public ObservableCollection<ProductProtocolNackReasonCodeViewModel> NackReasonCodes              { get; set; } = new ();
    public ObservableCollection<ProductProtocolCommandGroupViewModel>   ProductProtocolCommandGroups { get; set; } = new ();
    public ProductProtocolSerialConfig                                  ProductProtocolSerialConfig  { get; set; } = new ();
}