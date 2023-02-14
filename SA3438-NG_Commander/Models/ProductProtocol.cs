using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NG_Commander.Services;

namespace NG_Commander.Models;

public class ProductProtocol
{
    public String                            Name                         { get; set; } = "Warning Undefined";
    public ProtocolHandler?                  ProtocolHandler              { get; set; }
    public List<Settings_NackReasonCode>     NackReasonCodes              { get; set; } = new ();
    public List<ProductProtocolCommandGroup> ProductProtocolCommandGroups { get; set; } = new ();
    public ProductProtocolSerialConfig       SettingsSerial               { get; set; } = new();
}