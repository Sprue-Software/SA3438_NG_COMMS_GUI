using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NG_Commander.Models;

namespace NG_Commander.ViewModels;

public class ProductProtocolNackReasonCodeViewModel : ObservableObject
{
    public String                 Name  { get; set; } = "";
    public Byte                   Value { get; set; } = 0;
    public ProtocolNackSeverity_e Level { get; set; } = ProtocolNackSeverity_e.Error;
}