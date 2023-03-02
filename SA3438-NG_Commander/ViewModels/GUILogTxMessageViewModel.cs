using System;
using System.Collections.Generic;
using System.Linq;
using NG_Commander.Services;

namespace NG_Commander.ViewModels;

public class GUILogTxMessageViewModel
{
    public GUILogTxMessageViewModel(Byte[] Message, Boolean IsEscaped = false)
    {
        if (IsEscaped)
        {
            CMD        = "[" + BitConverter.ToString(NGProtocol.EscapeData(Message.Take(2).ToArray()))                              + "]";
            DataLength = "[" + BitConverter.ToString(NGProtocol.EscapeData(Message.Skip(2).Take(2).ToArray()))                      + "]";
            Data       = "[" + BitConverter.ToString(NGProtocol.EscapeData(Message.Skip(4).Take(Message.Length - 2 - 4).ToArray())) + "]";
            CRC        = "[" + BitConverter.ToString(NGProtocol.EscapeData(Message.TakeLast(2).ToArray()))                                     + "]";
        }
        else
        {
            CMD        = "[" + BitConverter.ToString(Message.Take(2).ToArray())                              + "]";
            DataLength = "[" + BitConverter.ToString(Message.Skip(2).Take(2).ToArray())                      + "]";
            Data       = "[" + BitConverter.ToString(Message.Skip(4).Take(Message.Length - 2 - 4).ToArray()) + "]";
            CRC        = "[" + BitConverter.ToString(Message.TakeLast(2).ToArray())                          + "]";
        }
    }
    public String CMD        { get;}
    public String DataLength { get;}
    public String Data       { get;}
    public String CRC        { get;}
    
    public Boolean HasData
    {
        get => Data.Length > 2;
    }
}