using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NG_Commander.Models;

public class Settings_LowLevelProtocol
{
    [Required] public String?                        Name            { get; set; }
    [Required] public List<Settings_NackReasonCode>? NackReasonCodes { get; set; }
    [Required] public List<Settings_CommandGroup>?   Commands { get; set; }
}