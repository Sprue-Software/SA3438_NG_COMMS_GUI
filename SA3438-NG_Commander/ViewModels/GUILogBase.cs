using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NG_Commander.ViewModels;

public class GUILogBase
{
    public DateTime Time { get; set; } = DateTime.MinValue;
    
    public String? TimeString
    {
        get => (Time == DateTime.MinValue) ? "" : Time.TimeOfDay.ToString();
    }
    
}