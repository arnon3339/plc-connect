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
using System.Text;

namespace WriteDPlcPackage
{
    //Add additional attributes as required:
    //[ResponsibleOfMILAllocations]
    [Description("This is a WriteDPlc step.")]
    [UIEditor("WriteDPlcPackage.WriteDPlcUIEditor")]   
    public class WriteDPlc : Step
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

        [Input]
        [Linkable]
        public int InValue => (int)GetInputValue("InValue");

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

            var valueCommand = InValue.ToString("X2");
            var headCommand = $"{HeadDevice[0]}*00{HeadDevice.Substring(1)}";
            var command = $"500000FF03FF00002C0004140200000101{headCommand}00{valueCommand}{headCommand}000000{valueCommand}";
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

            var resAscii = Encoding.ASCII.GetString(response, 0, bytesRead);

            if (resAscii[..2] != "D0")
            {
                throw new Exception("Invalid response");
            }
            
            _valueOutput = Convert.ToInt32(resAscii[^2..], 16);
        }

    }

}