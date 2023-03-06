using System;
using System.ComponentModel.DataAnnotations;

namespace NG_Commander.Models;

public class ProductProtocolCommand
{
    [Required] public String  Name        { get; set; }
    [Required] public UInt16  Command     { get; set; }
    public            String? ToolTipText { get; set; }
    public            String? TxType  { get; set; }
    public            String? RxType  { get; set; }
    public            String? RxUnit       { get; set; }
    public float  RxMultiplier { get; set; } = 1.0f;
    public UInt32 Timeout_ms { get; set; } = 0;
}
