using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NG_Commander;
using NG_Commander.Models;
using RJCP.IO.Ports;
using SA3438_NG_Commander.Views;

namespace SA3438_NG_Commander.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private NGProtocolComms             m_ProtocolComms        = new NGProtocolComms();
    private ProductProtocolSerialConfig m_ProtocolSerialConfig = new ProductProtocolSerialConfig();

    #region DebugLog
    [ObservableProperty] public ObservableCollection<String> _debug = new();

    void OnDebugMessage(object? sender, DebugMessageEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (e.B == '\n')
            {
                Debug.Insert(0, $"{Convert.ToChar(e.B)}");
                //Debug = new ObservableCollection<string>(Debug); //Needed for reverse view otherwise the GUI is not updated correctly
            }
            else
            {
                if (Debug.Count > 0)
                {
                    Debug[0] += $"{Convert.ToChar(e.B)}";
                }
                else
                {
                    Debug.Insert(0, $"{Convert.ToChar(e.B)}");
                }
            }

        }, DispatcherPriority.Normal);
        Console.WriteLine("DebugMessage");
    }
    
    [RelayCommand]
    public void ClearDebugMessages()
    {
        Debug.Clear();
    }
    #endregion

    #region Comms
    [ObservableProperty] public String connectDisconnect = "Connect";

    [ObservableProperty] private Boolean isConnected = false;

    public Boolean SerialPortIsConnected
    {
        get => m_ProtocolComms.IsConnected;
        set => SetProperty(ref isConnected, value);
    }

    private ObservableCollection<String> serialPorts = new();

    public ObservableCollection<String> SerialPorts
    {
        get => serialPorts;
        set => SetProperty(ref serialPorts, value);
    }

    [ObservableProperty] public int     selectedSerialPortIndex = 0;
    [ObservableProperty] public Boolean isPortsComboboxDown     = false;
    [RelayCommand]
    public void OpenClosePort()
    {
        if (!IsConnected)
        {
            //IsConnected = m_ProtocolComms.Connect(SelectedProductProtocol.ProductProtocolSerialConfig, SerialPorts[SelectedSerialPortIndex]);
            IsConnected = m_ProtocolComms.Connect(m_ProtocolSerialConfig, SerialPorts[SelectedSerialPortIndex]);
        }
        else
        {
            m_ProtocolComms.Disconnect();
            IsConnected = false;
        }

        ConnectDisconnect = IsConnected ? "Disconnect" : "Connect";
    }
    #endregion

    private Task            m_SerialPortUpdateTask;
    CancellationTokenSource m_CancelSerialPortUpdateTask = new CancellationTokenSource();
    public MainWindowViewModel()
    {
        //Start task to update serial ports in background every 1s
        m_SerialPortUpdateTask = Task.Factory.StartNew(() =>
        {
            while (true)
            {
                //no need to update serial ports while we are connected
                if (!SerialPortIsConnected && (IsPortsComboboxDown || SerialPorts.Count == 0))
                {
                    SerialPorts = new ObservableCollection<String>(SerialPortStream.GetPortNames().ToList().OrderBy(i => i));
                }
                if (m_CancelSerialPortUpdateTask.IsCancellationRequested)
                {
                    // Terminate the infinite loop.
                    break;
                }
                Thread.Sleep(1000);
            }
        },m_CancelSerialPortUpdateTask.Token);
        m_ProtocolComms.DebugMessage += OnDebugMessage;

     /*   Task.Factory.StartNew(new Action(() =>
        {
            while (true)
            {
                if (x < 20)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        if (NewString)
                        {
                            Debug.Insert(0, $"{x}  - Test1\n");
                            Debug = new ObservableCollection<string>(Debug); //Needed for reverse view otherwise the GUI is not updated correctly
                        }
                        else
                        {
                            if (Debug.Count > 0)
                            {
                                //Debug[0] = Debug[0] + (2);
                                Debug[0] += " (2)";
                            }
                        }

                        NewString = !NewString;
                    }, DispatcherPriority.Normal);
                    //Debug.Insert(0, $"{x}  - Test1\n");
                    x++;
                }

                Thread.Sleep(1000);
            }
        }));*/
        /*_debug.CollectionChanged += (o, args) =>
        {
            // Do something when the collection changes
            Console.WriteLine($"Debug changed ({x}");
        };*/
    }

    

    public void Dispose()
    {
        m_CancelSerialPortUpdateTask.Cancel();
        m_SerialPortUpdateTask.Wait();
        m_ProtocolComms.Dispose();
    }
}