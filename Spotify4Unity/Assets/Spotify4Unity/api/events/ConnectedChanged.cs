namespace Spotify4Unity.Events
{
    public class ConnectedChanged : GameEventBase
    {
        public bool IsConnected { get; set; }
        public ConnectedChanged(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
