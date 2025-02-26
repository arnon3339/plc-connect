using System.CommandLine;
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

var portOption = new Option<int>(
    "--port",
    "Port of PLC"
);

var commandOption = new Option<string>(
    "--command",
    "Command for sending to PLC"
);

rootCommand.AddOption(ipAddressOption);
rootCommand.AddOption(portOption);
rootCommand.AddOption(commandOption);

rootCommand.SetHandler(async (address, port, command) =>
{
    if (address != null && port != 0 && command != null)
    {
        using PlcConnection plcConnection = new PlcConnection(address, port, command);
        Console.WriteLine(await plcConnection.SendCommandAsyncMc());
    }
    else
    {
        Console.WriteLine($"Address: {address}, Port: {port}, Command: {command}");
        throw new Exception(message: "Missing <address> or <port> or <command>");
    }

}, ipAddressOption, portOption, commandOption);

return await rootCommand.InvokeAsync(args);
