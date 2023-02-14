using System;
using System.ComponentModel.DataAnnotations;

namespace NG_Commander.Models;

public class Settings_ProtocolNackReasonCodes //Old NackReasonCode
{
    [Required] public String? Name { get; set; }
    public Byte Value { get; set; }
}