namespace NG_Commander.ViewModels;

public class GUILogRxError : GUILogRxBase
{
    public string               ReasonString     { get; set; } = "";
    public GUILogRxNackViewModel UnescapedMessage { get; set; }
    public GUILogRxNackViewModel EscapedMessage   { get; set; }
}