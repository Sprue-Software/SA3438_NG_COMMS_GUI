using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NG_Commander.Models;

public class ProductProtocolCommandGroup
{
    [Required] public String?                      Name                    { get; set; }
    public            Byte                         CommandGroup            { get; set; }
    [Required] public List<ProductProtocolCommand> ProductProtocolCommands { get; set; } = new();
}