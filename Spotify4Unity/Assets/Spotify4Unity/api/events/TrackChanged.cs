using Spotify4Unity.Dtos;

namespace Spotify4Unity.Events
{
    public class TrackChanged : GameEventBase
    {
        public Track NewTrack { get; set; }
        public TrackChanged(Track newT)
        {
            NewTrack = newT;
        }
    }
}