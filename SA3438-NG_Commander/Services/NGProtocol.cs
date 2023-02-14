using System;
using NG_Commander.Models;
using RJCP.IO.Ports;

namespace NG_Commander.Services;

public class NGProtocol : IDisposable
{
    private SerialPortStream m_SerialPortStream = new ();
    public  Boolean          IsConnected        = false;
    public NGProtocol()
    {
        
    }

    public bool Connect(ProductProtocolSerialConfig SerialConfig)
    {
        if (m_SerialPortStream.IsOpen)
        {
            m_SerialPortStream.Close();
        }
        else
        {
            try
            {
                    
                m_SerialPortStream.BaudRate = SerialConfig.BaudRate;
                m_SerialPortStream.DataBits = SerialConfig.DataBits;
                m_SerialPortStream.StopBits = SerialConfig.StopBits;
                m_SerialPortStream.Parity   = SerialConfig.Parity;
                //m_SerialPortStream.PortName = SerialPorts[SelectedSerialPortIndex];
                m_SerialPortStream.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Error Opening serial port [{m_SerialPortStream.PortName}]. => {e.Message}");
            }
        }
        return m_SerialPortStream.IsOpen;
    }
    ///public void Send(UInt16 CommandId)
    public void Dispose()
    {
        m_SerialPortStream.Close();
    }
}