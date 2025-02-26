using System.Net.Sockets;
using System.Text;
using Modbus.Device;

namespace PlcConnect.Program.Classes
{
    class PlcConnection(string ipAddress, int port, string command) : IDisposable
    {
        private readonly string _ipAddress = ipAddress;
        private readonly int _port = port;
        private readonly string _command = command;
        private bool _disposed;

        private bool _isAscii = true;

        public bool IsAscii
        {
            get => _isAscii;
            set => _isAscii = value;
        }

        public async Task<string> SendCommandAsyncMc()
        {
            try
            {
                using TcpClient client = new(_ipAddress, _port);
                using NetworkStream stream = client.GetStream();

                // Set timeout values for read and write
                stream.ReadTimeout = 5000;  // 5 seconds timeout for reading
                stream.WriteTimeout = 5000; // 5 seconds timeout for writing

                byte[] command = _isAscii switch 
                {
                    false => HexStringToBytes(_command),
                    _ => Encoding.ASCII.GetBytes(_command)
                };

                // Send the command to the PLC
                await stream.WriteAsync(command);

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

        private static byte[] HexStringToBytes(string hex)
        {
            return [.. Enumerable.Range(0, hex.Length / 2).Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))];
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
