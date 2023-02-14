using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NG_Commander.ViewModels;

public class ProductProtocolNackReasonCodeViewModel : ObservableObject
{
    public String Name  { get; set; } = "";
    public Byte   Value { get; set; } = 0;
}