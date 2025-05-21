using System.Net.Sockets;
using System.Text;
using Modbus.Device;
using Tomlyn.Model;

namespace PlcConnect.Program.Classes
{
    class PlcConnection(string ipAddress, int port) : IDisposable
    {
        private readonly string _ipAddress = ipAddress;
        private readonly int _port = port;
        private bool _disposed;

        public async Task<string> SendCommandAsyncMc(string command, bool isAscii = true)
        {
            try
            {
                using TcpClient client = new(_ipAddress, _port);
                using NetworkStream stream = client.GetStream();

                // Set timeout values for read and write
                stream.ReadTimeout = 5000;  // 5 seconds timeout for reading
                stream.WriteTimeout = 5000; // 5 seconds timeout for writing

                byte[] cmd = isAscii switch
                {
                    false => HexStringToBytes(command),   // If isAscii is false, convert the command from hex string to byte array.
                    _ => Encoding.ASCII.GetBytes(AscciiToHexstring(command)) // If isAscii is true, convert to hex string and then to bytes using ASCII encoding.
                };

                // Send the command to the PLC
                await stream.WriteAsync(cmd);

                // Read the response
                var response = new byte[256];
                int bytesRead = await stream.ReadAsync(response);

                // Return the response as a hex string
                return BitConverter.ToString(response, 0, bytesRead).Replace("-", "");
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> readWriteMc(string head, string? value)
        {
            try
            {
                using TcpClient client = new(_ipAddress, _port);
                using NetworkStream stream = client.GetStream();

                // Set timeout values for read and write
                stream.ReadTimeout = 5000;  // 5 seconds timeout for reading
                stream.WriteTimeout = 5000; // 5 seconds timeout for writing

                var deviceAddr = head[1..];
                var action = value != null? "write": "read";

                var headCmd = (string)((TomlTable)Config.config["default"]!)["subheader"];
                var networkCmd = (string)((TomlTable)Config.config["default"]!)["network_no"];
                var pcNoCmd = (string)((TomlTable)Config.config["default"]!)["pc_no"];
                var ioNoCmd = (string)((TomlTable)Config.config["default"]!)["io_no"];
                var stationNoCmd = (string)((TomlTable)Config.config["default"]!)["station_no"];
                var requestLengthCmd =
                        (string)((TomlTable)((TomlTable)Config.config["default"])[action])["request_length"];
                var MonitoringTimerCmd = (string)((TomlTable)Config.config["default"]!)["monitoring_timer"];
                var commandCmd = (string)((TomlTable)((TomlTable)Config.config["default"])[action])["command"];
                var headDeviceCmd = head[0..1];
                var subcommandCmd =
                    (string)((TomlTable)((TomlTable)((TomlTable)Config.config["default"])["write"])[headDeviceCmd])["subcommand"];
                var deviceCodeCmd = ushort.Parse(deviceAddr).ToString("X4");
                var numOfPointsCmd = (string)((TomlTable)Config.config["default"]!)["number_of_points"];

                var cmdToml = $"{headCmd}{networkCmd}{pcNoCmd}{ioNoCmd}{stationNoCmd}" +
                    $"{requestLengthCmd}{MonitoringTimerCmd}{commandCmd}{subcommandCmd}" +
                    $"{deviceCodeCmd}{headCmd}{numOfPointsCmd}";
                
                if (value != null)
                    cmdToml += headDeviceCmd == "M"? value != "0"? 1: 0: ushort.Parse(value).ToString("X4");

                var hexTomlCmd = AscciiToHexstring(cmdToml);
                var bytesTomlCmd = HexStringToBytes(hexTomlCmd);

              
                await stream.WriteAsync(bytesTomlCmd);

                // Read the response
                var response = new byte[256];
                int bytesRead = await stream.ReadAsync(response);

                // Return the response as a hex string
                return BitConverter.ToString(response, 0, bytesRead).Replace("-", "");
                // return "Command sent";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private static string AscciiToHexstring(string asciis)
        {
            return string.Concat(asciis.Select(c => ((int)c).ToString("X2"))).ToUpper();
        }

        private static byte[] HexStringToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length / 2)
                             .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
                             .ToArray();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here
                }

                // Dispose unmanaged resources here

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
