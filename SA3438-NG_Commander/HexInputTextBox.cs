using Avalonia.Input.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Utils;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Utilities;
using Avalonia.Controls.Metadata;
using Avalonia.Styling;

namespace NG_Commander;


public class HexInputTextBox : TextBox, IStyleable
{
    Type IStyleable.StyleKey => typeof(TextBox);
    private int     SizeInBytes = 0;
    private int SizeInBytesX2
    {
        get => SizeInBytes * 2;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (IsVisible)
        {
            SizeInBytes = Marshal.SizeOf(Value.GetType());
            SetStringFromValue();
        }

        Console.WriteLine("Constructor TextBox");
        
        //Text      = $"{Value" + FormatString + "}";
    }

    public static readonly DirectProperty<HexInputTextBox, Object> ValueProperty =
        AvaloniaProperty.RegisterDirect<HexInputTextBox, Object>(nameof(Value), hexin => hexin.Value,
                                                                 (hexin, v) => hexin.Value = v, defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    private Object _value;
    public  Object Value { get => _value;
        set
        {
            SetAndRaise(ValueProperty, ref _value, value);  
        } }
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
     Console.WriteLine("OnGetFocus");   
        base.OnGotFocus(e);
        //e.Handled = false;
        if (!AcceptsReturn && Text?.Length > 0)
        {
            SelectionStart = 0;
            SelectionEnd   = Text.Length;
        }
    }
    
    private void SetStringFromValue()
    {
        String Temp = "";
        switch (SizeInBytes)
        {
            case 1:
                Text = $"{Value:X2}";
                break;
            case 2:
                Text = $"{Value:X4}";
                break;
            case 4: 
                Text = $"{Value:X8}";
                break;
            case 8:
                Text = $"{Value:X16}";
                break;
        }

        Text = Regex.Replace(Text, @".{2}", "$0 ");
    }
    private void StringToValue(String source)
    {
        
        switch (Type.GetTypeCode(Value.GetType()))
        {
            case TypeCode.Byte:
                Byte tempByte;
                Byte.TryParse(source, NumberStyles.HexNumber, null, out tempByte);
                Value = tempByte;
                break;
            case TypeCode.UInt16:
                UInt16 tempUInt16;
                UInt16.TryParse(source, NumberStyles.HexNumber, null, out tempUInt16);
                Value = tempUInt16;
                break;
            case TypeCode.UInt32:
                UInt32 tempUInt32;
                UInt32.TryParse(source, NumberStyles.HexNumber, null, out tempUInt32);
                Value = tempUInt32;
                break;
            case TypeCode.UInt64:
                UInt64 tempUInt64;
                UInt64.TryParse(source, NumberStyles.HexNumber, null, out tempUInt64);
                Value = tempUInt64;
                break;
        }
    }
    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        
        if (e.Text != null)
        {
            var upperText = Text.ToUpper();
            var tempText = Regex.Replace(upperText, @"[^A-F0-9]", "");
            //todo handle caretindex correctly when invalid key is pressed
            if (tempText.Length > SizeInBytesX2)
            {
                tempText   = tempText.Substring(tempText.Length - SizeInBytesX2);
                CaretIndex = SizeInBytesX2 + 1;
            }
            
            StringToValue(tempText);
            
            var PreChangeTextLength = Text.Length;
            var PreChangeCaretIndex = CaretIndex;
            
            SetStringFromValue();
            
            if((PreChangeTextLength < SizeInBytesX2) || PreChangeCaretIndex == (SizeInBytesX2 + 1))
                CaretIndex = SizeInBytesX2 + 1;
            else
            {
                CaretIndex -= 1;
            }
        }
    }
}