using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NG_Commander.ViewModels;

public class ProductProtocolCommandGroupViewModel : ObservableObject
{
    [Required] public string?                       Name                    { get; set; }
    public Byte                         CommandGroup            { get; set; }
    public ObservableCollection<ProductProtocolCommandViewModel> ProductProtocolCommands { get; set; } = new ();
}