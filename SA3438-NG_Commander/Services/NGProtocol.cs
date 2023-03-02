using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using DynamicData;
using NG_Commander.Models;
using NG_Commander.ViewModels;
using RJCP.IO.Ports;

namespace NG_Commander.Services;

public class NGProtocol : IDisposable
{
    private const Byte STX        = 0x02;
    private const Byte ETX        = 0x03;
    private const Byte ESC        = 0x1B;
    private const Byte ESC_Offset = 0x20;

    enum NACKReason_e : Byte
    {
        NG_NACK_REASON_OK                    = 0x00,
        NG_NACK_REASON_CRCError              = 0x01,
        NG_NACK_REASON_InvalidDataByteLength = 0x02,
        NG_NACK_REASON_InvalidMessageLength  = 0x03,
        NG_NACK_REASON_MessageToLong         = 0x04,
        NG_NACK_REASON_UnknownCommand        = 0x05,
        NG_NACK_REASON_InvalidDeviceState    = 0x06,
        NG_NACK_REASON_InvalidData           = 0x07,
        NG_NACK_REASON_CommandNotImplemented = 0x08,
        NG_NACK_REASON_TestFailed            = 0x09,
        NG_NACK_REASON_TestNotFinished       = 0x0A,
        NG_NACK_REASON_NVMError              = 0x0B,
        NG_NACK_REASON_Timeout               = 0x0C,
        NG_NACK_REASON_BufferOverflow        = 0x0D,
        NG_NACK_REASON_HardwareError         = 0x0E,
        NG_NACK_REASON_SDKError              = 0xFD,
        NG_NACK_REASON_InternalError         = 0xFE
    }

    private DateTime         m_LastTxTime       = DateTime.MinValue;
    private SerialPortStream m_SerialPortStream = new();

    public Boolean IsConnected
    {
        get => m_SerialPortStream.IsOpen;
    }

    private Thread RxThread;

    private ConcurrentQueue<Byte[]> m_RxPackets = new();

    public ConcurrentQueue<GUILogBase> NewRxTxLogs = new();

    private readonly SettingsService ProtocolInfo;

    public NGProtocol(SettingsService Settings)
    {
        ProtocolInfo = Settings;

        m_SerialPortStream.DataReceived += RXHandler;

        RxThread = new Thread(() =>
        {
            for (;;)
            {
                Byte[]     Packet;
                List<Byte> DeEscapedPacket = new();
                if (m_RxPackets.TryDequeue(out Packet))
                {
                    DateTime Now = DateTime.Now;

                    bool PreviousWasEscape = false;
                    foreach (var b in Packet)
                    {
                        if (b == ESC)
                        {
                            PreviousWasEscape = true;
                        }
                        else if (PreviousWasEscape)
                        {
                            DeEscapedPacket.Add((byte)(b - ESC_Offset));
                            PreviousWasEscape = false;
                        }
                        else
                        {
                            DeEscapedPacket.Add(b);
                        }
                    }

                    var ReverversedPacket = DeEscapedPacket.ToArray().Reverse().ToArray();
                    Console.WriteLine();
                    UInt16 PacketCRC = BitConverter.ToUInt16(ReverversedPacket, 0);

                    UInt16 CRCCalculated = CalculateCRC(DeEscapedPacket.Take(DeEscapedPacket.Count - 2).ToArray());
                    Console.WriteLine($"CRC CALCULATED = [{CRCCalculated:X4}], CRC PACKET = [{PacketCRC:X4}]");

                    Console.WriteLine("");
                    String TimeDiffString = "";
                    if (m_LastTxTime != DateTime.MinValue)
                    {
                        TimeSpan X = Now - m_LastTxTime;

                        TimeDiffString = "Δ  " + ((double)X.TotalMicroseconds / 1000.0).ToString("N3") + "ms";
                        Console.WriteLine($"Now {Now} - LastTx {m_LastTxTime} - Timespan {X.ToString()} - {TimeDiffString}");
                    }

                    UInt16 AckNack = BitConverter.ToUInt16(DeEscapedPacket.Take(2).Reverse().ToArray());

                    if (AckNack == 0x0006) //Ack
                    {
                        NewRxTxLogs.Enqueue(new GUILogRxOK()
                        {
                            Time             = Now,
                            TimeDiff         = TimeDiffString,
                            UnescapedMessage = new GUILogRxAckViewModel(DeEscapedPacket.ToArray(), false),
                            EscapedMessage   = new GUILogRxAckViewModel(DeEscapedPacket.ToArray(), true)
                        });
                    }
                    else //Nack
                    {
                        Byte NackReason = DeEscapedPacket[6];
                        Settings_NackReasonCode Reason =
                            ProtocolInfo.LowLevelProtocols[0].NackReasonCodes.Find(i => i.Value == NackReason);
                        if (Reason != null)
                        {
                            Console.WriteLine(Reason.Level.ToString());
                            Console.WriteLine(Reason.Name);
                            if (Reason.Level == ProtocolNackSeverity_e.Warning)
                            {
                                NewRxTxLogs.Enqueue(new GUILogRxWarning()
                                {
                                    Time             = Now,
                                    TimeDiff         = TimeDiffString,
                                    UnescapedMessage = new GUILogRxNackViewModel(DeEscapedPacket.ToArray(), false),
                                    EscapedMessage   = new GUILogRxNackViewModel(DeEscapedPacket.ToArray(), true),
                                    ReasonString     = Reason.Name
                                });
                            }
                            else //Error
                            {
                                NewRxTxLogs.Enqueue(new GUILogRxError()
                                {
                                    Time             = Now,
                                    TimeDiff         = TimeDiffString,
                                    UnescapedMessage = new GUILogRxNackViewModel(DeEscapedPacket.ToArray(), false),
                                    EscapedMessage   = new GUILogRxNackViewModel(DeEscapedPacket.ToArray(), true),
                                    ReasonString     = Reason.Name
                                });
                            }
                        }
                    }

                    m_LastTxTime = DateTime.MinValue;
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        });
        RxThread.Start();
    }

    public bool Connect(ProductProtocolSerialConfig SerialConfig, String PortName)
    {
        if (m_SerialPortStream.IsOpen)
        {
            m_SerialPortStream.Close();
        }


        try
        {
            m_SerialPortStream.BaudRate = SerialConfig.BaudRate;
            m_SerialPortStream.DataBits = SerialConfig.DataBits;
            m_SerialPortStream.StopBits = SerialConfig.StopBits;
            m_SerialPortStream.Parity   = SerialConfig.Parity;
            m_SerialPortStream.PortName = PortName;
            m_SerialPortStream.Open();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] Error Opening serial port [{m_SerialPortStream.PortName}]. => {e.Message}");
        }


        return m_SerialPortStream.IsOpen;
    }

    public void Disconnect()
    {
        if (m_SerialPortStream.IsOpen)
        {
            m_SerialPortStream.Close();
        }
    }

    public void Dispose()
    {
        if (m_SerialPortStream.IsOpen)
        {
            m_SerialPortStream.Close();
        }

        m_SerialPortStream.Dispose();
    }

    public void SendMessage(ProductProtocolCommandViewModel i_Command)
    {
        if (!IsConnected)
        {
            //NewRxTxLogs.Enqueue(new GUILogError() { Text = "Unable to send message. Serial port not open" });
        }
        else
        {
            List<Byte> TxMessage = new List<byte>();

            TxMessage.Add((byte)((i_Command.Command >> 8) & 0xFF));
            TxMessage.Add((byte)(i_Command.Command        & 0xFF));

            TxMessage.Add((byte)0xFF);
            TxMessage.Add((byte)0xFF); //Temp Size
            if (i_Command.HasTxData)
            {
                if (i_Command.TxValue != null)
                {
                    foreach (FieldInfo f in i_Command.TxValue.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                    {
                        if (f.FieldType.IsValueType)
                        {
                            switch (f.FieldType.ToString())
                            {
                                case "System.Boolean":
                                    TxMessage.Add((Byte)(((Boolean)i_Command.TxValue) == true ? 1 : 0));
                                    break;
                                case "System.Byte":
                                    TxMessage.Add((Byte)i_Command.TxValue);
                                    break;
                                case "System.UInt16":
                                case "System.Int16":
                                    UInt16 V16 = (UInt16)i_Command.TxValue;
                                    TxMessage.Add((Byte)((V16 >> 8) & 0xFF));
                                    TxMessage.Add((Byte)(V16        & 0xFF));
                                    break;
                                case "System.UInt32":
                                case "System.Int32":
                                    UInt32 V32 = (UInt32)i_Command.TxValue;
                                    TxMessage.Add((Byte)((V32 >> 24) & 0xFF));
                                    TxMessage.Add((Byte)((V32 >> 16) & 0xFF));
                                    TxMessage.Add((Byte)((V32 >> 8)  & 0xFF));
                                    TxMessage.Add((Byte)(V32         & 0xFF));
                                    break;
                                case "System.UInt64":
                                case "System.Int64":
                                    UInt64 V64 = (UInt64)i_Command.TxValue;
                                    TxMessage.Add((Byte)((V64 >> 56) & 0xFF));
                                    TxMessage.Add((Byte)((V64 >> 48) & 0xFF));
                                    TxMessage.Add((Byte)((V64 >> 40) & 0xFF));
                                    TxMessage.Add((Byte)((V64 >> 32) & 0xFF));
                                    TxMessage.Add((Byte)((V64 >> 24) & 0xFF));
                                    TxMessage.Add((Byte)((V64 >> 16) & 0xFF));
                                    TxMessage.Add((Byte)((V64 >> 8)  & 0xFF));
                                    TxMessage.Add((Byte)(V64         & 0xFF));
                                    break;
                            }
                        }
                    }
                }
            }

            UInt16 DataByteCount = (UInt16)(TxMessage.Count() - 4U); //Sunstract cmd and size
            TxMessage[2] = ((byte)((DataByteCount >> 8) & 0xFF));
            TxMessage[3] = ((byte)(DataByteCount        & 0xFF));


            UInt16 CRC = CalculateCRC(TxMessage.ToArray());

            TxMessage.Add((byte)((CRC >> 8) & 0xFF));
            TxMessage.Add((byte)(CRC        & 0xFF));


            Byte[] MessageData_ = TxMessage.ToArray();

            TxMessage.Clear();

            UInt16 ByteCounter = 0;

            //Create the full escaped message
            TxMessage.Add(STX);
            foreach (Byte b in MessageData_)
            {
                if ((b == ESC) || (b == STX) || (b == ETX))
                {
                    TxMessage.Add(ESC);
                    TxMessage.Add((byte)(b + ESC_Offset));
                }
                else
                {
                    TxMessage.Add(b);
                }
            }

            TxMessage.Add(ETX);

            m_LastTxTime = DateTime.Now;
            m_SerialPortStream.Write(TxMessage.ToArray());


            NewRxTxLogs.Enqueue(new GUILogTx()
            {
                Time             = m_LastTxTime,
                UnescapedMessage = new(MessageData_, false),
                EscapedMessage   = new(MessageData_, true)
            });
        }
    }


    private List<Byte> m_RxMessageCurrent = new();
    private Boolean    m_RXInprogress     = false;

    private void RXHandler(object arg1, SerialDataReceivedEventArgs arg2)
    {
        Int32  BytesToRead = m_SerialPortStream.BytesToRead;
        Byte[] Buffer      = new Byte[BytesToRead];
        m_SerialPortStream.Read(Buffer, 0, BytesToRead);
        foreach (var b in Buffer)
        {
            if (b == STX)
            {
                m_RxMessageCurrent.Clear();
                m_RXInprogress = true;
            }
            else if (b == ETX)
            {
                Byte[] RxMessage = m_RxMessageCurrent.ToArray();
                m_RxPackets.Enqueue(RxMessage);
                m_RxMessageCurrent.Clear();
                m_RXInprogress = false;
            }
            else if (m_RXInprogress)
            {
                m_RxMessageCurrent.Add(b);
            }
        }
    }

    private const UInt32 XMODEM_CRC16_POLY = 0x11021;

    public static UInt16 CalculateCRC(Byte[] i_Message)
    {
        UInt32 CRCResult = 0;
        for (int i = 0; i < i_Message.Length; i++)
        {
            CRCResult ^= (UInt32)(i_Message[i] << 8);
            for (int bitCounter = 0; bitCounter < 8; bitCounter++)
            {
                if ((CRCResult & 0x8000) == 0x8000)
                {
                    CRCResult <<= 1;
                    CRCResult ^=  XMODEM_CRC16_POLY;
                }
                else
                {
                    CRCResult <<= 1;
                }
            }
        }

        return (UInt16)(CRCResult & 0xFFFF);
    }

    public static Byte[] EscapeData(List<Byte> Source)
    {
        List<Byte> Destination = new();
        foreach (Byte b in Source)
        {
            if ((b == ESC) || (b == STX) || (b == ETX))
            {
                Destination.Add(ESC);
                Destination.Add((byte)(b + ESC_Offset));
            }
            else
            {
                Destination.Add(b);
            }
        }

        return Destination.ToArray();
    }

    public static Byte[] EscapeData(Byte[] Source)
    {
        List<Byte> Destination = new();
        foreach (Byte b in Source)
        {
            if ((b == ESC) || (b == STX) || (b == ETX))
            {
                Destination.Add(ESC);
                Destination.Add((byte)(b + ESC_Offset));
            }
            else
            {
                Destination.Add(b);
            }
        }

        return Destination.ToArray();
    }
}