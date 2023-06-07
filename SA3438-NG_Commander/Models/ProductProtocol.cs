using System;
using System.Collections.Generic;

namespace NG_Commander.Models;

public class ProductProtocol
{
    public String                            Name                         { get; set; } = "Warning Undefined";
    public List<Settings_NackReasonCode>     NackReasonCodes              { get; set; } = new();
    public List<ProductProtocolCommandGroup> ProductProtocolCommandGroups { get; set; } = new();
    public Settings_Serial                   SettingsSerial               { get; set; } = new();
}