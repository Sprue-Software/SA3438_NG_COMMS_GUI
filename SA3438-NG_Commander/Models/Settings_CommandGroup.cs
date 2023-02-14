using System;
using System.Collections.Generic;
using NG_Commander.Services;

namespace NG_Commander.Models;

public class Settings_CommandGroup //Old Config_CommandGroup
{
    public String Group { get; set; } = "";
    public Byte GroupCode { get; set; } = 0;
    public List<Settings_GroupCommand> GroupCommands { get; set; } = new();
}