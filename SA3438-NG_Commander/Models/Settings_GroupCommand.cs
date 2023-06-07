using System;
using System.ComponentModel.DataAnnotations;

namespace NG_Commander.Models;

public class Settings_GroupCommand
{
    [Required] public String? Description { get; set; } = "UNDEFINED";
    public            Byte    SubCommandId { get; set; }
    public            String? ToolTipText  { get; set; }
    public            String? TxType       { get; set; }
    public            String? RxType       { get; set; }
    public            String? RxUnit       { get; set; }
    public            float   RxMultiplier { get; set; }
    public            UInt32  Timoutms     { get; set; } = 0;
}