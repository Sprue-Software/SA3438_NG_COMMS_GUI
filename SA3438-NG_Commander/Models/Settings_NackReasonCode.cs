using System;

namespace NG_Commander.Models;

public enum ProtocolNackSeverity_e {None, Error, Warning}
public class Settings_NackReasonCode
{
    public  String                 Name  { get; set; } = "";
    public  Byte                   Value { get; set; } = 0;
    
    public ProtocolNackSeverity_e Level { get; set; } = ProtocolNackSeverity_e.Error;
}