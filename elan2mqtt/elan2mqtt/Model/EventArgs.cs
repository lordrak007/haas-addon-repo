using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elan2mqtt
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public string Payload { get; }
        public string Address { get; }
        public MessageReceivedEventArgs(string payload, string address)
        {
            Payload = payload;
            Address = address;
        }
    }
    public class ElanStatusEventReceivedEventArgs : EventArgs
    {
        public string Payload { get; }
        public string Device { get; }
        /// <summary>
        /// raise information for mqtt topic update
        /// </summary>
        /// <param name="payload">Data to send</param>
        /// <param name="device">Elan device address</param>
        public ElanStatusEventReceivedEventArgs(string payload, string device)
        {
            Payload = payload;
            Device = device;
        }
    }
    public class ElanDeviceDiscoveryEventReceivedEventArgs : EventArgs
    {
        public string Topic { get; }
        public byte[] Payload { get; }
        public ElanDeviceDiscoveryEventReceivedEventArgs(string topic, byte[] payload)
        {
            Payload = payload;
            Topic = topic;
        }
    }
}
