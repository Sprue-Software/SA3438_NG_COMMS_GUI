using System;
using System.Collections.Generic;

namespace NG_Commander.Models;

public class Settings_ProductProtocol
{
    public String?         Name            { get; set; }
    public String?         ProtocolHandler { get; set; }
    public List<UInt16>?   CommandSet      { get; set; }
    public Settings_Serial SerialConfig    { get; set; }
        
}