using Matrox.DesignAssistant.Core.Steps;
using Matrox.DesignAssistant.Core.Steps.Attributes;
using Matrox.DesignAssistant.Core.Steps.Annotations;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System;

namespace ReadMPlcPackage
{
    //Add additional attributes as required:
    //[ResponsibleOfMILAllocations]
    [Description("This is a ReadMPlc step.")]
    [UIEditor("ReadMPlcPackage.ReadMPlcUIEditor")]   
    public class ReadMPlc : Step
    {
        [Input]
        [Linkable]
        public string IpAddress => (string)GetInputValue("IpAddress");

        [Input]
        [Linkable]
        public int Port => (int)GetInputValue("Port");


        [Input]
        [Linkable]
        public string HeadDevice => (string)GetInputValue("HeadDevice");


        [Output]     
        public string Value
        {
            get
            {
                ValidateOutputAvailability(nameof(Value));
                return _valueOutput;
            }
        }
        private string _valueOutput;

        protected override void Run()
        {
            using TcpClient client = new(IpAddress, Port);
            using NetworkStream stream = client.GetStream();

            stream.ReadTimeout = 5000;
            stream.WriteTimeout = 5000;

            var command = $"500000FF03FF000018000404010001{HeadDevice[0]}*00{HeadDevice.Substring(1)}0001";
            var commandHex = "";
            foreach (var c in command)
            {
                commandHex += ((int)c).ToString("X2");
            }
            commandHex = commandHex.ToUpper();
            var commandBytes = Enumerable.Range(0, commandHex.Length / 2)
                             .Select(i => Convert.ToByte(commandHex.Substring(i * 2, 2), 16))
                             .ToArray();


            stream.Write(commandBytes);
            var response = new byte[256];
            int bytesRead = stream.Read(response);

            var outStr = BitConverter.ToString(response, 0, bytesRead).Replace("-", "");
            _valueOutput = outStr;

        }
    }
}