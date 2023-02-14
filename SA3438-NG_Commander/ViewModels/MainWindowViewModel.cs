using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NG_Commander.Models;
using NG_Commander.Services;
using RJCP.IO.Ports;

namespace NG_Commander.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public SerialPortStream m_SerialPortStream = new SerialPortStream();// = new SerialPortStream("/dev/ttyS4");
        public MainWindowViewModel()
        {
            SettingsService SettingProvider = new();
            foreach (var ProductProtocol in SettingProvider.ProductProtocols)
            {
                ProductProtocolViewModel ProtocolViewModel = new() { Name = ProductProtocol.Name, ProductProtocolSerialConfig = ProductProtocol.SettingsSerial};
                foreach (var ProductProtocolCommandGroup in ProductProtocol.ProductProtocolCommandGroups)
                {
                    ProductProtocolCommandGroupViewModel ProtocolCommandGroup = new()
                        { Name = ProductProtocolCommandGroup.Name, CommandGroup = ProductProtocolCommandGroup.CommandGroup };
                    foreach (var ProductProtocolCommand in ProductProtocolCommandGroup.ProductProtocolCommands)
                    {
                        ProductProtocolCommandViewModel vm = new()
                        {
                            Name       = ProductProtocolCommand.Name,
                            Command    = ProductProtocolCommand.Command, 
                            Unit       = ProductProtocolCommand.Unit,
                            Multiplier = ProductProtocolCommand.Multiplier,
                            Timeout_ms = ProductProtocolCommand.Timeout_ms
                        };
                        if (!String.IsNullOrEmpty(ProductProtocolCommand.ToolTipText))
                        {
                            vm.ToolTipText = ProductProtocolCommand.ToolTipText;
                        }
                        if (ProductProtocolCommand.TxType != null)
                        {
                            if (ProductProtocolCommand.TxType.StartsWith("System."))
                            {
                                Type? TxType = Type.GetType(ProductProtocolCommand.TxType);
                                if (TxType != null)
                                {
                                    vm.TxValue = Activator.CreateInstance(TxType);
                                }
                                else
                                {
                                    Console.WriteLine($"[ERROR] Unknown TxType [{ProductProtocolCommand.TxType}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[ERROR] Custom TxType [{ProductProtocolCommand.TxType} not implemented");
                            }
                        }

                        if (ProductProtocolCommand.RxType != null)
                        {
                            if (ProductProtocolCommand.RxType.StartsWith("System."))
                            {
                                Type? RxType = Type.GetType(ProductProtocolCommand.RxType);
                                if (RxType != null)
                                {
                                    vm.RxValue = Activator.CreateInstance(RxType);
                                }
                                else
                                {
                                    Console.WriteLine($"[ERROR] Unknown RxType [{ProductProtocolCommand.RxType}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[ERROR] Custom RxType [{ProductProtocolCommand.RxType} not implemented");
                            }
                        }

                        ProtocolCommandGroup.ProductProtocolCommands.Add(vm);
                    }

                    ProtocolViewModel.ProductProtocolCommandGroups.Add(ProtocolCommandGroup);
                }

                ProtocolViewModel.NackReasonCodes = new();
                foreach (var NackReason in ProductProtocol.NackReasonCodes)
                {
                    ProtocolViewModel.NackReasonCodes.Add(new()
                                                              { Name = NackReason.Name, Value = NackReason.Value });
                }

                Protocols.Add(ProtocolViewModel);
            }
            //Start task to update serial ports in background every 1s
            Task.Factory.StartNew(new Action(() => {
                while (true)
                {
                    //no need to update serial ports while we are connected
                    if (!m_SerialPortStream.IsOpen && (IsPortsComboboxDown || SerialPorts.Count == 0))
                    {
                        SerialPorts = new ObservableCollection<String>(SerialPortStream.GetPortNames().ToList());
                    }
                    Thread.Sleep(1000);
                }            
            }));
            
            selectedProductProtocol = Protocols.First();
            
        }

        [ObservableProperty] public Boolean isPortsComboboxDown;

        [ObservableProperty] public ProductProtocolViewModel                       selectedProductProtocol;
        public                      ObservableCollection<ProductProtocolViewModel> Protocols { get; set; } = new();

        [ObservableProperty] public String connectDisconnect = "Connect";

        private                     Boolean                      isConnected = false;

        public Boolean IsConnected
        {
            get =>m_SerialPortStream. IsOpen;
            set => SetProperty(ref isConnected, value);
        }
        private                     ObservableCollection<String> serialPorts = new();
        public                      ObservableCollection<String> SerialPorts { get => serialPorts; set => SetProperty(ref serialPorts, value); }
        [ObservableProperty] public int                       selectedSerialPortIndex = 0;


        [RelayCommand]
        public void OpenClosePort()
        {
            if (m_SerialPortStream.IsOpen)
            {
                m_SerialPortStream.Close();
            }
            else
            {
                try
                {
                    
                    m_SerialPortStream.BaudRate = SelectedProductProtocol.ProductProtocolSerialConfig.BaudRate;
                    m_SerialPortStream.DataBits = SelectedProductProtocol.ProductProtocolSerialConfig.DataBits;
                    m_SerialPortStream.StopBits = selectedProductProtocol.ProductProtocolSerialConfig.StopBits;
                    m_SerialPortStream.Parity   = selectedProductProtocol.ProductProtocolSerialConfig.Parity;
                    m_SerialPortStream.PortName = SerialPorts[SelectedSerialPortIndex];
                    m_SerialPortStream.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[ERROR] Error Opening serial port [{m_SerialPortStream.PortName}]. => {e.Message}");
                }
            }
            IsConnected       = m_SerialPortStream.IsOpen;
            ConnectDisconnect = IsConnected ? "Disconnect" : "Connect";
        }
        
    }
}
