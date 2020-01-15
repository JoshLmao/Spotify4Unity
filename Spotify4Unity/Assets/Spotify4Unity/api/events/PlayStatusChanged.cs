namespace Spotify4Unity.Events
{
    public class PlayStatusChanged : GameEventBase
    {
        public bool IsPlaying { get; set; }
        public PlayStatusChanged(bool isPlaying)
        {
            IsPlaying = isPlaying;
        }
    }
}