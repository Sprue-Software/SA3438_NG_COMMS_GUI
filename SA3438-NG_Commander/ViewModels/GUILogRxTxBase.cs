using System;

namespace NG_Commander.ViewModels;

public class GUILogRxTxBase : GUILogBase
{

    public DateTime Time { get; set; } = DateTime.MinValue;
    
    public String? TimeString
    {
        get => (Time == DateTime.MinValue) ? "" : Time.TimeOfDay.ToString();
    }
    

}