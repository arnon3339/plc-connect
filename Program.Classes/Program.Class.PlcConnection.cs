using System.Net.Sockets;
using System.Text;

namespace PlcConnect.Program.Classes
{
    class PlcConnection
    {
        private string _ipAddress;
        private int _port;
        private string _command;

        public PlcConnection(string ipAddress, int port, string command)
        {
            _ipAddress = ipAddress;
            _port = port;
            _command = command;
        }

        public string SendCommand()
        {
            using (TcpClient client = new TcpClient(this._ipAddress, this._port))
            using (NetworkStream stream = client.GetStream())
            {
                var command = Encoding.ASCII.GetBytes(_command);
                stream.Write(command, 0, command.Length);
                var response = new byte[256];
                stream.Read(response, 0, response.Length);
                return Encoding.ASCII.GetString(response);
            }
        }

    }

}
