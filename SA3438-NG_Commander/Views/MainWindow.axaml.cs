using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using DynamicData;
using NG_Commander.Models;
using NG_Commander.Services;
using NG_Commander.ViewModels;
using RJCP.IO.Ports;

namespace NG_Commander.Views
{
    public partial class MainWindow : Window
    {
        private bool m_ScreenCentered = false;

        public MainWindow()
        {
            InitializeComponent();
            var iv = this.GetObservable(Window.IsVisibleProperty);
            iv.Subscribe(value =>
            {
                if (value && !m_ScreenCentered)
                {
                    m_ScreenCentered = true;
                    CenterWindow();
                }
            });
        }

        private async void CenterWindow()
        {
            if (this.WindowStartupLocation == WindowStartupLocation.Manual)
                return;

            Screen? screen = null;
            while (screen == null)
            {
                await Task.Delay(1);
                screen = this.Screens.ScreenFromVisual(this);
            }

            if (this.WindowStartupLocation == WindowStartupLocation.CenterScreen)
            {
                var x = (int)Math.Floor(screen.Bounds.Width  / 2.0 - this.Bounds.Width         / 2);
                var y = (int)Math.Floor(screen.Bounds.Height / 2.0 - (this.Bounds.Height + 30) / 2);

                this.Position = new PixelPoint(x, y);
            }
            else if (this.WindowStartupLocation == WindowStartupLocation.CenterOwner)
            {
                var pw = this.Owner as Window;
                if (pw != null)
                {
                    var x = (int)Math.Floor(pw.Bounds.Width  / 2 - this.Bounds.Width         / 2 + pw.Position.X);
                    var y = (int)Math.Floor(pw.Bounds.Height / 2 - (this.Bounds.Height + 30) / 2 + pw.Position.Y);

                    this.Position = new PixelPoint(x, y);
                }
            }
        }

        private void MnuExit_OnClick(object? sender, RoutedEventArgs e)
        {
            
            Close();
        }

        private async void MnuExportLogs_OnClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine(this.DataContext.GetType().ToString());
            
            var SaveFileBox = new SaveFileDialog();
            SaveFileBox.Filters?.Add(new FileDialogFilter(){Name = "CSV File", Extensions = new List<string>()});
            SaveFileBox.Filters?[0].Extensions.Add("csv");
            
            var FileName =  await SaveFileBox.ShowAsync(this);
            if (FileName != null)
            {
                using StreamWriter CSVFile = new StreamWriter(FileName);
                var                vm      = DataContext as MainWindowViewModel;
                
                await CSVFile.WriteLineAsync("Type;DateTime;Delta T;Command;Command Description;Value;");
                foreach (var i in vm.Logs.Reverse())
                {

                    //var x = i as GUILogRxAckViewModel;
                    Console.WriteLine(i.Time);
                    if (i.GetType() == typeof(GUILogRxOK))
                    {

                        var    rx          = i as GUILogRxOK;
                        UInt16 FullCommand = UInt16.Parse(rx.UnescapedMessage.CMD.Replace("[", "").Replace("]", "").Replace("-", ""), System.Globalization.NumberStyles.HexNumber);
                        Byte   Command     = (Byte)((FullCommand >> 8) & 0xFF);
                        Byte   SubCommand  = (Byte)(FullCommand        & 0xFF);

                        var commandGroup  = vm.SelectedProductProtocol.ProductProtocolCommandGroups.First(i => i.CommandGroup == Command);
                        var CommandDefine = commandGroup.ProductProtocolCommands.First(c => c.Command                         == FullCommand);

                        String ValueHexString = rx.UnescapedMessage.Data.Replace("[", "").Replace("]", "").Replace("-", "");
                        var    bytes          = new byte[ValueHexString.Length / 2];
                        for (var ic = 0; ic < bytes.Length; ic++)
                        {
                            bytes[ic] = Convert.ToByte(ValueHexString.Substring(ic * 2, 2), 16);
                        }

                        bytes = bytes.Reverse().ToArray();
                        object Value;
                        float  Multiplier = (CommandDefine.RxMultiplier != 0.0f) ? CommandDefine.RxMultiplier : 1.0f;

                        switch (CommandDefine.RxValueType)
                        {
                            case "System.Int16":

                                Value = Multiplier * BitConverter.ToInt16(bytes, 0);

                                break;
                            case "System.UInt16":
                                Value = Multiplier * BitConverter.ToUInt16(bytes, 0);
                                break;
                            case "System.Int32":
                                Value = Multiplier * BitConverter.ToInt32(bytes, 0);
                                break;
                            case "System.UInt32":
                                Value = Multiplier * BitConverter.ToUInt32(bytes, 0);
                                break;
                            default:
                                Value = "";
                                break;
                        }

                        /*Value.GetType().
                        if(CommandDefine.RxValue)*/
                        await CSVFile.WriteLineAsync($"RX_ACK;"                                                                             +
                                                     $"{rx.Time.ToString()};"                                                           +
                                                     $"{rx.TimeDiff};"                                                                  +
                                                     $"0x{rx.UnescapedMessage.CMD.Replace("[", "").Replace("]", "").Replace("-", "")};" +
                                                     $"{commandGroup.Name} / {CommandDefine.Name};"                                     +
                                                     $"{Value}{CommandDefine.RxUnit};");
                    }
                    else if (i.GetType() == typeof(GUILogRxError))
                    {
                            var rx = i as GUILogRxError;
                            UInt16 FullCommand = UInt16.Parse(rx.UnescapedMessage.CMD.Replace("[", "").Replace("]", "").Replace("-", ""), System.Globalization.NumberStyles.HexNumber);
                            Byte   Command     = (Byte)((FullCommand >> 8) & 0xFF);
                            Byte   SubCommand  = (Byte)(FullCommand        & 0xFF);

                            var commandGroup  = vm.SelectedProductProtocol.ProductProtocolCommandGroups.First(i => i.CommandGroup == Command);
                            var CommandDefine = commandGroup.ProductProtocolCommands.First(c => c.Command                         == FullCommand);
                            await CSVFile.WriteLineAsync($"RX_NACK;"                                                                        +
                                                         $"{rx.Time.ToString()};"                                                           +
                                                         $"{rx.TimeDiff};"                                                                  +
                                                         $"0x{rx.UnescapedMessage.CMD.Replace("[", "").Replace("]", "").Replace("-", "")};" +
                                                         $"{commandGroup.Name} / {CommandDefine.Name};"                                     +
                                                         $";");
                        
                        /*await CSVFile.WriteLineAsync($"RX_NACK;"                                                                             +
                                                     $"{rx.Time.ToString()};"                                                           +
                                                     $"{rx.TimeDiff};"                                                                  +
                                                     $"0x{rx.UnescapedMessage.CMD.Replace("[", "").Replace("]", "").Replace("-", "")};" +
                                                     $"{commandGroup.Name} / {CommandDefine.Name};"                                     +
                                                     $"{Value}{CommandDefine.RxUnit};");*/
                    }
                    else if (i.GetType() == typeof(GUILogRxWarning))
                    {
                        var    rx          = i as GUILogRxWarning;
                        UInt16 FullCommand = UInt16.Parse(rx.UnescapedMessage.CMD.Replace("[", "").Replace("]", "").Replace("-", ""), System.Globalization.NumberStyles.HexNumber);
                        Byte   Command     = (Byte)((FullCommand >> 8) & 0xFF);
                        Byte   SubCommand  = (Byte)(FullCommand        & 0xFF);

                        var commandGroup  = vm.SelectedProductProtocol.ProductProtocolCommandGroups.First(i => i.CommandGroup == Command);
                        var CommandDefine = commandGroup.ProductProtocolCommands.First(c => c.Command                         == FullCommand);
                        await CSVFile.WriteLineAsync($"RX_NACK;"                                                                        +
                                                     $"{rx.Time.ToString()};"                                                           +
                                                     $"{rx.TimeDiff};"                                                                  +
                                                     $"0x{rx.UnescapedMessage.CMD.Replace("[", "").Replace("]", "").Replace("-", "")};" +
                                                     $"{commandGroup.Name} / {CommandDefine.Name};"                                     +
                                                     $";");
                    }
                    else if (i.GetType() == typeof(GUILogTx))
                    {
                        var    rx          = i as GUILogTx;
                        UInt16 FullCommand = UInt16.Parse(rx.UnescapedMessage.CMD.Replace("[", "").Replace("]", "").Replace("-", ""), System.Globalization.NumberStyles.HexNumber);
                        Byte   Command     = (Byte)((FullCommand >> 8) & 0xFF);
                        Byte   SubCommand  = (Byte)(FullCommand        & 0xFF);

                        var commandGroup  = vm.SelectedProductProtocol.ProductProtocolCommandGroups.First(i => i.CommandGroup == Command);
                        var CommandDefine = commandGroup.ProductProtocolCommands.First(c => c.Command                         == FullCommand);
                        await CSVFile.WriteLineAsync($"TX;"                                                                        +
                                                     $"{rx.Time.ToString()};"                                                           +
                                                     $";"                                                                  +
                                                     $"0x{rx.UnescapedMessage.CMD.Replace("[", "").Replace("]", "").Replace("-", "")};" +
                                                     $"{commandGroup.Name} / {CommandDefine.Name};"                                     +
                                                     $";");
                    }
               
                }
                CSVFile.Flush();
                CSVFile.Close();
                Console.WriteLine(FileName);    
            }
            
        }
    }
}