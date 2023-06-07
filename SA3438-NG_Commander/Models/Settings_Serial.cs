using System;
using RJCP.IO.Ports;

namespace NG_Commander.Models;

public class Settings_Serial
{
    public Int32  BaudRate             { get; set; } = 9600;
    public String Parity               { get; set; } 
    public String StopBits             { get; set; }
    public int    DataBits             { get; set; } = 8;
    public int    XModemEnterDelay     { get; set; } = 500;
    public int    XModemMessageTimeout { get; set; } = 100;
}