using Spotify4Unity.Events;

namespace Spotify4Unity.Events
{
    public class ConnectingChanged : GameEventBase
    {
        public bool IsConnecting { get; set; }
        public ConnectingChanged(bool isConnecting)
        {
            IsConnecting = isConnecting;
        }
    }
}
