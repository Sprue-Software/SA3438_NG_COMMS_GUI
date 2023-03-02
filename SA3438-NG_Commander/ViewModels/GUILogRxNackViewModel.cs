using System;
using System.Collections.Generic;
using System.Linq;
using NG_Commander.Services;

namespace NG_Commander.ViewModels;

public class GUILogRxNackViewModel
{
    public GUILogRxNackViewModel(Byte[] Message, Boolean IsEscaped = false)
    {
        if (IsEscaped)
        {
            NACK       = "[" + BitConverter.ToString(NGProtocol.EscapeData(Message.Take(2).ToArray()))                              + "]";
            DataLength = "[" + BitConverter.ToString(NGProtocol.EscapeData(Message.Skip(2).Take(2).ToArray()))                      + "]";
            CMD        = "[" + BitConverter.ToString(NGProtocol.EscapeData(Message.Skip(4).Take(2).ToArray()))                      + "]";
            Reason     = "[" + BitConverter.ToString(NGProtocol.EscapeData(Message.Skip(4).Take(Message.Length - 2 - 6).ToArray())) + "]";
            CRC        = "[" + BitConverter.ToString(NGProtocol.EscapeData(Message.TakeLast(2).ToArray()))                          + "]";
        }
        else
        {
            NACK       = "[" + BitConverter.ToString(Message.Take(2).ToArray())                              + "]";
            DataLength = "[" + BitConverter.ToString(Message.Skip(2).Take(2).ToArray())                      + "]";
            CMD        = "[" + BitConverter.ToString(Message.Skip(4).Take(2).ToArray())                      + "]";
            Reason     = "[" + BitConverter.ToString(Message.Skip(6).Take(Message.Length - 2 - 6).ToArray()) + "]";
            CRC        = "[" + BitConverter.ToString(Message.TakeLast(2).ToArray())                          + "]";
        }
        
    }
    public String NACK        { get;}
    public String CMD        { get;}
    public String DataLength { get;}
    public String Reason       { get;}
    public String CRC        { get;}
}