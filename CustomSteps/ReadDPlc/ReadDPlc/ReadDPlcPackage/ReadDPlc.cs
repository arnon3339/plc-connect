using System;
using System.Collections.Generic;
using Matrox.DesignAssistant.Core;
using Matrox.DesignAssistant.Core.CommonTypes;
using Matrox.DesignAssistant.Core.Expressions;
using Matrox.DesignAssistant.Core.Steps;
using Matrox.DesignAssistant.Core.Steps.Attributes;
using Matrox.DesignAssistant.Core.Steps.Annotations;
using Matrox.MatroxImagingLibrary;
using System.Linq;
using System.Net.Sockets;
using System.Net;

namespace ReadDPlcPackage
{
    //Add additional attributes as required:
    //[ResponsibleOfMILAllocations]
    [Description("This is a ReadDPlc step.")]
    [UIEditor("ReadDPlcPackage.ReadDPlcUIEditor")]   
    public class ReadDPlc : Step
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
        public int Value
        {
            get
            {
                ValidateOutputAvailability(nameof(Value));
                return _valueOutput;
            }
        }
        private int _valueOutput;

        protected override void Run()
        {
            using TcpClient client = new(IpAddress, Port);
            using NetworkStream stream = client.GetStream();

            stream.ReadTimeout = 5000;
            stream.WriteTimeout = 5000;

            var preCommand = $"{HeadDevice[0]}*00{HeadDevice.Substring(1)}";
            var command = $"500000FF03FF0000200004040300000101{preCommand}{preCommand}";
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
            if (outStr[..2] != "D0")
            {
                throw new Exception("Invalid response");
            }
            _valueOutput = Convert.ToInt32(outStr[^2..], 16);
        }
    }
}