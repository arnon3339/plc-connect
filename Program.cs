using System.CommandLine;

using PlcConnect.Program.Classes;

class Program
{
    static async Task<int> Main(string[] args)
    {
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

        rootCommand.SetHandler(static (address, port, command) =>
        {
            if (address != null && port != 0 && command != null)
            {
                var plcConnection = new PlcConnection(address, port, command);
                Console.WriteLine(plcConnection.SendCommand());
            }
            else
            {
                throw new Exception(message: "Missing <address> or <port> or <comand>");
            }

        }, ipAddressOption, portOption, commandOption);

        return await rootCommand.InvokeAsync(args);
    }
}
