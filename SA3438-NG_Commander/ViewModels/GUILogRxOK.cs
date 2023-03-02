using System;

namespace NG_Commander.ViewModels;

public class GUILogRxOK : GUILogRxBase
{
    public GUILogRxAckViewModel UnescapedMessage { get; set; }
    public GUILogRxAckViewModel EscapedMessage   { get; set; }

}