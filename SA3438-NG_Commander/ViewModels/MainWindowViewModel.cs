using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using NG_Commander.Models;
using NG_Commander.Services;
using RJCP.IO.Ports;

namespace NG_Commander.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NGProtocol      m_NgProtocol;
        private readonly SettingsService m_SettingProvider;
        
        public MainWindowViewModel(NGProtocol Protocol, SettingsService Settings)
        {
            m_NgProtocol      = Protocol;
            m_SettingProvider = Settings;

            foreach (var ProductProtocol in m_SettingProvider.ProductProtocols)
            {
                ProductProtocolViewModel ProtocolViewModel = new() { Name = ProductProtocol.Name, ProductProtocolSerialConfig = ProductProtocol.SettingsSerial };
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
                                                              { Name = NackReason.Name, Value = NackReason.Value, Level = NackReason.Level});
                }

                Protocols.Add(ProtocolViewModel);
            }
            selectedProductProtocol = Protocols.First();
            
            
            //Start task to update serial ports in background every 1s
            Task.Factory.StartNew(new Action(() =>
            {
                while (true)
                {
                    //no need to update serial ports while we are connected
                    if (!SerialPortIsConnected && (IsPortsComboboxDown || SerialPorts.Count == 0))
                    {
                        Console.WriteLine($"Dropdown open? [{IsPortsComboboxDown}");
                        SerialPorts = new ObservableCollection<String>(SerialPortStream.GetPortNames().ToList().OrderBy(i => i));
                    }

                    Thread.Sleep(1000);
                }
            }));

            async Task UpdateGuiLogs()
            {
                GUILogBase Msg;
                Boolean    HasChanged = false;
                while (m_NgProtocol.NewRxTxLogs.TryDequeue(out Msg))
                {
                    Logs.Insert(0, Msg);
                    HasChanged = true;
                    //Logs.Add(Msg);
                }

                
                while (Logs.Count > 5000)
                {
                    Logs.RemoveAt(0);
                }

                if (HasChanged)
                {
                    //NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    
                    //Logs.OnCollectionChanged(args);
                    //todo force update as this is not always executed, implement thread safe observablecollection
                    
                    HasChanged = false;
                }
            }
            Task.Factory.StartNew(new Action(() =>
            {
                while (true)
                {
                    Dispatcher.UIThread.Post(() => UpdateGuiLogs(), DispatcherPriority.Background);
                    
                    Thread.Sleep(50);
                }
            }));
            
            
        }

        [ObservableProperty] public Boolean isPortsComboboxDown;

        [ObservableProperty] public ProductProtocolViewModel                       selectedProductProtocol;
        public                      ObservableCollection<ProductProtocolViewModel> Protocols { get; set; } = new();
        
        public ObservableCollection<GUILogBase> Logs { get; set; } = new();

        [ObservableProperty] public String connectDisconnect = "Connect";

        [ObservableProperty] private Boolean isConnected = false;

        public Boolean SerialPortIsConnected
        {
            get => m_NgProtocol.IsConnected;
            set => SetProperty(ref isConnected, value);
        }

        private ObservableCollection<String> serialPorts = new();

        public ObservableCollection<String> SerialPorts
        {
            get => serialPorts;
            set => SetProperty(ref serialPorts, value);
        }

        [ObservableProperty] public int selectedSerialPortIndex = 0;


        [RelayCommand]
        public void OpenClosePort()
        {
            if (!IsConnected)
            {
                IsConnected = m_NgProtocol.Connect(SelectedProductProtocol.ProductProtocolSerialConfig, SerialPorts[SelectedSerialPortIndex]);
            }
            else
            {
                m_NgProtocol.Disconnect();
                IsConnected = false;
            }

            ConnectDisconnect = IsConnected ? "Disconnect" : "Connect";
        }

        [RelayCommand]
        public void SendCommand(ProductProtocolCommandViewModel Data)
        {
            m_NgProtocol.SendMessage(Data);
            Console.WriteLine("Buttom Pressed");
        }

        [RelayCommand]
        public void ClearLogs()
        {
            Logs.Clear();
        }

        [RelayCommand]
        public void ExportLogs()
        {
            //var SaveFileBox = new SaveFileDialog();
            //SaveFileBox.Filters.Add(new);
        }
    }
}