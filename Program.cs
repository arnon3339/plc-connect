using System.CommandLine;
using System.Net.Sockets;
using PlcConnect.Program.Classes;

if (args is null)
{
    throw new ArgumentNullException(nameof(args));
}

var rootCommand = new RootCommand("A simple PLC communication.");

var ipAddressOption = new Option<string>(
    "--address",
    "IP address of PLC"
);

var portOption = new Option<string>(
    "--port",
    "Port of PLC"
);

var commandOption = new Option<string>(
    "--command",
    "Command for sending to PLC"
);

var headOption = new Option<string>(
    "--head",
    "head of PLC memory"
);

var valueOption = new Option<string>(
    "--value",
    "value sent to PLC"
);

var binaryOption = new Option<bool>(
    "--binary",
    "binary mode PLC"
);

rootCommand.AddOption(ipAddressOption);
rootCommand.AddOption(portOption);
rootCommand.AddOption(commandOption);
rootCommand.AddOption(headOption);
rootCommand.AddOption(valueOption);

rootCommand.SetHandler(static async (address, port, command, head, value, binary) =>
{
    if (address == null || port == null)
    {
        throw new Exception(message: "Missing IP address or port.");
    }

    using PlcConnection plcConnection = new(address, int.Parse(port));
    if (command != null)
    {
        Console.WriteLine(await plcConnection.SendCommandAsyncMc(command, !binary));
    }
    else if (command == null && head != null)
    {
        try
        {
            Console.WriteLine(plcConnection.readWriteMc(head, value));
        }
        catch (Exception ex)
        {
            throw new Exception(message: ex.Message);
        }
    }
    else
    {
        throw new Exception(message: "Missing command line arguments.");
    }

}, ipAddressOption, portOption, commandOption, headOption, valueOption, binaryOption);

return await rootCommand.InvokeAsync(args);
