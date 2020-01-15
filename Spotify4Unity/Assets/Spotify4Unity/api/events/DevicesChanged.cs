using Spotify4Unity.Dtos;
using System.Collections.Generic;

namespace Spotify4Unity.Events
{
    public class DevicesChanged : GameEventBase
    {
        public List<Device> Devices { get; set; }
        public DevicesChanged(List<Device> devices)
        {
            Devices = devices;
        }
    }
}
