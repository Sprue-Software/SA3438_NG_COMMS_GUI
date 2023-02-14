using System;
using System.ComponentModel.DataAnnotations;

namespace NG_Commander.Models;

public class Settings_GroupCommand
{
    [Required] public String? Description  { get; set; }
    public            Byte    SubCommandId { get; set; }
    public            String? ToolTipText  { get; set; }
    public            String? TxType       { get; set; }
    public            String? RxType       { get; set; }
    public            String? Unit         { get; set; }
    public            float   Multiplier   { get; set; } = 1.0f;
    public            UInt32  Timoutms     { get; set; } = 0;
}