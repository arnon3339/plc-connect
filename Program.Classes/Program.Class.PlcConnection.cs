using System.Net.Sockets;
using System.Text;
using Modbus.Device;

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
                // Log exceptions (e.g., connection issues, timeouts)
                // Assuming a logger is available
                // Logger.LogError(ex, "Error sending command to PLC");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> WriteRelayMc(string head, bool value)
        {
            try
            {
                using TcpClient client = new(_ipAddress, _port);
                using NetworkStream stream = client.GetStream();

                // Set timeout values for read and write
                stream.ReadTimeout = 5000;  // 5 seconds timeout for reading
                stream.WriteTimeout = 5000; // 5 seconds timeout for writing

                var preCommand = "500000FF03FF00001800041402000101";
                var postCommand = $"{head[0]}*00{head.Substring(1)}0{(value ? "1" : "0")}";
                var command = preCommand + postCommand;
                var hexCommand = AscciiToHexstring(command);
                var bytesCommand = HexStringToBytes(hexCommand);

                Console.WriteLine($"Command: {hexCommand}");

                await stream.WriteAsync(bytesCommand);

                // Read the response
                var response = new byte[256];
                int bytesRead = await stream.ReadAsync(response);

                // Return the response as a hex string
                return BitConverter.ToString(response, 0, bytesRead).Replace("-", "");
            }
            catch (Exception ex)
            {
                // Log exceptions (e.g., connection issues, timeouts)
                // Assuming a logger is available
                // Logger.LogError(ex, "Error sending command to PLC");
                return $"Error: {ex.Message}";
            }
        }

        public string ReadRegistersModbus(ushort startAddress, ushort count)
        {
            try
            {
                using TcpClient client = new(_ipAddress, _port);
                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);
                ushort[] registers = master.ReadHoldingRegisters(startAddress, count);

                return "Registers: " + string.Join(", ", registers);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public string WriteRegistersModbus(ushort startAddress, ushort[] values)
        {
            try
            {
                byte slaveId = 1;
                using TcpClient client = new(_ipAddress, _port);
                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);

                master.WriteMultipleRegisters(slaveId, startAddress, values);

                return $"Wrote multiple values to registers starting at {startAddress}.";

            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public string WriteRegisterModbus(ushort startAddress, ushort value)
        {
            try
            {
                byte slaveId = 1;
                using TcpClient client = new(_ipAddress, _port);
                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);
                master.WriteSingleRegister(slaveId, startAddress, value);

                return $"Wrote value {value} to register at {startAddress}.";

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
