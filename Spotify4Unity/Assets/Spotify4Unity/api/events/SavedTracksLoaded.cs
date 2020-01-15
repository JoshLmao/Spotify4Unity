using Spotify4Unity.Dtos;
using System.Collections.Generic;

namespace Spotify4Unity.Events
{
    public class SavedTracksLoaded : GameEventBase
    {
        public List<Track> SavedTracks { get; set; }
        public SavedTracksLoaded(List<Track> t)
        {
            SavedTracks = t;
        }
    }
}