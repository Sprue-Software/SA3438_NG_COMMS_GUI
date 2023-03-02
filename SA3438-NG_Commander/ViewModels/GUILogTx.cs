using System;
using System.Collections.Generic;

namespace NG_Commander.ViewModels;

public class GUILogTx : GUILogBase
{
    public GUILogTxMessageViewModel UnescapedMessage { get; set; }

    public GUILogTxMessageViewModel EscapedMessage { get; set; }
}