using System;
using System.Collections.Generic;

namespace NG_Commander.Models;

public class Settings_CommandGroup
{
    public String                      Group         { get; set; } = "";
    public Byte                        GroupCode     { get; set; } = 0;
    public List<Settings_GroupCommand> GroupCommands { get; set; } = new();
}