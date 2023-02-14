using System;
using RJCP.IO.Ports;

namespace NG_Commander.Models;

public class ProductProtocolSerialConfig
{
    public Int32  BaudRate { get; set; } = 9600;
    public Parity Parity   { get; set; } = Parity.None;
    public StopBits StopBits { get; set; } = StopBits.One;
    public int   DataBits { get; set; } = 8;
}