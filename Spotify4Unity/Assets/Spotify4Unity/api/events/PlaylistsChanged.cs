using Spotify4Unity.Dtos;
using System.Collections.Generic;

namespace Spotify4Unity.Events
{
    public class PlaylistsChanged : GameEventBase
    {
        public List<Playlist> Playlists { get; set; }
        public PlaylistsChanged(List<Playlist> playlists)
        {
            Playlists = playlists;
        }
    }
}