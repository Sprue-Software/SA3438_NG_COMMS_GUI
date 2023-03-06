using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NG_Commander.Models;
using RJCP.IO.Ports;

namespace NG_Commander.Services;

public class SettingsService
{
    public ObservableCollection<ProductProtocol> ProductProtocols  { get; private set; } = new ();
    public List<Settings_LowLevelProtocol>       LowLevelProtocols { get; private set; } = new();

    public SettingsService()
    {
        Settings_ToolConfig ToolConfig        = new();
        var                 ConfigBuilder     = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        var                 ConfigurationRoot = ConfigBuilder.Build();
        ToolConfig.ProductProtocols  = ConfigurationRoot.GetSection("ProductProtocols").Get<List<Settings_ProductProtocol>>();
        ToolConfig.LowLevelProtocols = ConfigurationRoot.GetSection("LowLevelProtocols").Get<List<Settings_LowLevelProtocol>>();
        LowLevelProtocols            = ToolConfig.LowLevelProtocols;
        //Check System Types
        //todo add check for protocol handler are Rx/Tx Types
        Boolean IsProtocolHandlerUnique                   = (ToolConfig.LowLevelProtocols.GroupBy(Llp => Llp.Name).Count() == ToolConfig.LowLevelProtocols.Count);
        Boolean AreProtocolHandlerGroupNamesUnique        = true;
        Boolean AreProtocolHandlerGroupCodesUnique        = true;
        Boolean AreProtocolHandlerGroupCommandNamesUnique = true;
        Boolean AreProtocolHandlerGroupSubCommandsUnique  = true;
        foreach (var LowLevelProtocol in ToolConfig.LowLevelProtocols)
        {
            AreProtocolHandlerGroupNamesUnique &= LowLevelProtocol.Commands.GroupBy(LlpCommand => LlpCommand.Group).Count()     == LowLevelProtocol.Commands.Count;
            AreProtocolHandlerGroupCodesUnique &= LowLevelProtocol.Commands.GroupBy(LlpCommand => LlpCommand.GroupCode).Count() == LowLevelProtocol.Commands.Count;
            foreach (var CommandGroup in LowLevelProtocol.Commands)
            {
                AreProtocolHandlerGroupCommandNamesUnique &= CommandGroup.GroupCommands.GroupBy(GroupCommand => GroupCommand.Description).Count()  == CommandGroup.GroupCommands.Count;
                AreProtocolHandlerGroupSubCommandsUnique  &= CommandGroup.GroupCommands.GroupBy(GroupCommand => GroupCommand.SubCommandId).Count() == CommandGroup.GroupCommands.Count;
                if (!AreProtocolHandlerGroupSubCommandsUnique)
                {
                    Console.WriteLine($"[ERROR] Subcommands NOT unique in {LowLevelProtocol.Name} - Group {CommandGroup.Group}");
                }
            }
            //Check NACK types
        }

        //Check Product Protocols
        Boolean IsProductProtocolNameUnique          = ToolConfig.ProductProtocols.GroupBy(ProductProtocol => ProductProtocol.Name).Count() == ToolConfig.ProductProtocols.Count;
        Boolean AreProductProtocolCommandsSetsUnique = true;
        foreach (var ProductProtocol in ToolConfig.ProductProtocols)
        {
            AreProductProtocolCommandsSetsUnique &= (ProductProtocol.CommandSet?.GroupBy(CommandSet => CommandSet).Count() == ProductProtocol.CommandSet?.Count);
            if (!AreProductProtocolCommandsSetsUnique)
            {
                var DoubleCommand = ProductProtocol.CommandSet?.GroupBy(CommandSet => CommandSet).Where(Command => Command.Count() > 1).Select(z => z.Key).ToList().First();
                Console.WriteLine($"[ERROR] Product Protocol \"{ProductProtocol.Name}\" command set is not unique command [0x{DoubleCommand:X4}]");
            }
        }

        Boolean ProtocolHandlersExist = true; //todo: add check if protocol handler exists
        Boolean RxTxTypesExist        = true; //todo: add Rx/Tx type checks
        Boolean HasOtherError         = false;

        //Configuration file is valid if all checks are true
        if (IsProtocolHandlerUnique                  && AreProtocolHandlerGroupNamesUnique && AreProtocolHandlerGroupCodesUnique   && AreProtocolHandlerGroupCommandNamesUnique &&
            AreProtocolHandlerGroupSubCommandsUnique && IsProductProtocolNameUnique        && AreProductProtocolCommandsSetsUnique && ProtocolHandlersExist && RxTxTypesExist)
        {
            //All basic config file checks are executed, Now we need to create the protocol lists for the GUI and check if the product protocol commands exist in the low level protocol commands.
            //There is no need to check of ProductProtocol is already in the list as we checked they are unique
            foreach (var ConfigProductProtocol in ToolConfig.ProductProtocols)
            {
                //Check if the Lowlevel protocol exists
                if (ToolConfig.LowLevelProtocols.Exists(Llp => Llp.Name == ConfigProductProtocol.ProtocolHandler))
                {
                    Settings_LowLevelProtocol lowLevelProtocol = ToolConfig.LowLevelProtocols.Find(Llp => Llp.Name == ConfigProductProtocol.ProtocolHandler);
                     
                    //todo add ProtocolHandler type
                    if (lowLevelProtocol != null)
                    {
                        ProductProtocol productProtocol = new ProductProtocol() { Name = ConfigProductProtocol.Name, NackReasonCodes = lowLevelProtocol.NackReasonCodes };
                        ProductProtocols.Add(productProtocol);
                        //Try parse the Serial Port Configuration
                        if (ConfigProductProtocol.SerialConfig != null)
                        {
                            if (!String.IsNullOrEmpty(ConfigProductProtocol.SerialConfig.Parity?.Trim()))
                            {
                                Parity tempParity;

                                if (Parity.TryParse(ConfigProductProtocol.SerialConfig.Parity, out tempParity))
                                {
                                    productProtocol.SettingsSerial.Parity = tempParity;
                                }
                                else
                                {
                                    Console.WriteLine($"[ERROR] invalid Parity configuration [{ConfigProductProtocol.SerialConfig.Parity}]");
                                }
                            }

                            if (!String.IsNullOrEmpty(ConfigProductProtocol.SerialConfig.Parity?.Trim()))
                            {

                                StopBits tempStopBits;
                                if (StopBits.TryParse(ConfigProductProtocol.SerialConfig.StopBits, out tempStopBits))
                                {
                                    productProtocol.SettingsSerial.StopBits = tempStopBits;
                                }
                                else
                                {
                                    Console.WriteLine($"[ERROR] invalid StopBits configuration [{ConfigProductProtocol.SerialConfig.StopBits}]");
                                }
                            }

                            //todo: Add check for data bits and baud-rate? Howto ensure valid values
                            if (ConfigProductProtocol.SerialConfig.BaudRate > 0)
                            {
                                productProtocol.SettingsSerial.BaudRate = ConfigProductProtocol.SerialConfig.BaudRate;
                            }

                            if ((ConfigProductProtocol.SerialConfig.DataBits >= 5) && (ConfigProductProtocol.SerialConfig.DataBits <= 9))
                            {
                                productProtocol.SettingsSerial.DataBits = ConfigProductProtocol.SerialConfig.DataBits;
                            }
                        }

                        if (ConfigProductProtocol.CommandSet != null)
                            foreach (var configProductCommand in ConfigProductProtocol.CommandSet)
                            {
                                Byte GroupCode       = (Byte)((configProductCommand >> 8) & 0xFF); //Get Group Code from command
                                Byte GroupSubCommand = (Byte)(configProductCommand        & 0xFF);
                                if (lowLevelProtocol.Commands.Exists(x => x.GroupCode == GroupCode))
                                {
                                    var CommandGroup = lowLevelProtocol.Commands.First(x => x.GroupCode == GroupCode);
                                    //Check if subcommand exist in the lowLevelProtocol
                                    if (lowLevelProtocol.Commands.First(x => x.GroupCode == GroupCode).GroupCommands.Exists(x => x.SubCommandId == GroupSubCommand))
                                    {
                                        //Check if the command group is in the current productProtocol
                                        try
                                        {
                                            var temp = productProtocol.ProductProtocolCommandGroups.First(x => x.CommandGroup == GroupCode);
                                        }
                                        catch
                                        {
                                            //Add new command group to the Tool Configuration
                                            productProtocol.ProductProtocolCommandGroups.Add(new ProductProtocolCommandGroup() { CommandGroup = GroupCode, Name = CommandGroup.Group });
                                        }

                                        ProductProtocolCommandGroup ProductProtocolCommandGroup = productProtocol.ProductProtocolCommandGroups.First(x => x.CommandGroup == GroupCode);
                                        Settings_GroupCommand ConfigGroupCommand = lowLevelProtocol.Commands.First(x => x.GroupCode == GroupCode).GroupCommands
                                                                                                   .First(x => x.SubCommandId       == GroupSubCommand);
                                        ProductProtocolCommandGroup.ProductProtocolCommands.Add(new ProductProtocolCommand()
                                        {
                                            Name   = ConfigGroupCommand.Description, Command = configProductCommand, TxType = ConfigGroupCommand.TxType,
                                            RxType = ConfigGroupCommand.RxType,
                                            ToolTipText = ConfigGroupCommand.ToolTipText,
                                            RxUnit   = ConfigGroupCommand.RxUnit, RxMultiplier = ConfigGroupCommand.RxMultiplier, Timeout_ms = ConfigGroupCommand.Timoutms
                                        });
                                    }
                                    else
                                    {
                                        Console.WriteLine(
                                                          $"[ERROR] SubCommand [0x{GroupSubCommand:X2}] does not exists in command group \"{CommandGroup.Group}\" [0x{CommandGroup.GroupCode}]");
                                        HasOtherError = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    HasOtherError = true;
                                    Console.WriteLine($"Unknown Groupcode [0x{GroupCode:X2}] for Product Protocol {productProtocol.Name} in Low Level Protocol {lowLevelProtocol.Name}");
                                }
                            }
                    }
                }
                else
                {
                    HasOtherError = true;
                    Console.WriteLine($"[ERROR] Unknown Low Level Protocol [{ConfigProductProtocol.ProtocolHandler}]");
                }
            }
        }
        else
        {
            Console.WriteLine("[ERROR] Invalid appsettings.json file");
        }

        //Sort the protocol lists
        ProductProtocols = new ObservableCollection<ProductProtocol>(ProductProtocols.OrderBy(i => i.Name).ToList());
        foreach (var Protocol in ProductProtocols)
        {
            Protocol.ProductProtocolCommandGroups = new List<ProductProtocolCommandGroup>(Protocol.ProductProtocolCommandGroups.OrderBy(i => i.Name).ToList());
            foreach (var Command in Protocol.ProductProtocolCommandGroups)
            {
                Command.ProductProtocolCommands = Command.ProductProtocolCommands.OrderBy(i => i.Name).ToList();

            }
        }

        Console.WriteLine("[SUCCESS] appsetting.json corectly loaded");
    }
}


