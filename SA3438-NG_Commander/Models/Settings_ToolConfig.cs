using System.Collections.Generic;

namespace NG_Commander.Models;

public class Settings_ToolConfig
{
    public List<Settings_LowLevelProtocol> LowLevelProtocols { get; set; } = new();
    public List<Settings_ProductProtocol> ProductProtocols { get; set; } = new();
}