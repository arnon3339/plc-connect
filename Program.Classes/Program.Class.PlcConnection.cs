﻿using System.Net.Sockets;

namespace PlcConnect.Program.Classes
{
    class PlcConnection(string ipAddress, int port, string command) : IDisposable
    {
        private readonly string _ipAddress = ipAddress;
        private readonly int _port = port;
        private readonly string _command = command;
        private bool _disposed;

        private static byte[] HexStringToBytes(string hex)
        {
            return [.. Enumerable.Range(0, hex.Length / 2).Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))];
        }

        public async Task<string> SendCommandAsync()
        {
            try
            {
                using TcpClient client = new(_ipAddress, _port);
                using NetworkStream stream = client.GetStream();

                // Set timeout values for read and write
                stream.ReadTimeout = 5000;  // 5 seconds timeout for reading
                stream.WriteTimeout = 5000; // 5 seconds timeout for writing

                byte[] command = HexStringToBytes(_command);

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
