using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using NG_Commander.Services;

namespace NG_Commander.Models;

public class ProductProtocolCommandGroup
{
    [Required] public String? Name         { get; set; }
    public            Byte    CommandGroup { get; set; }
    [Required] public List<ProductProtocolCommand> ProductProtocolCommands { get; set; } = new ();
}