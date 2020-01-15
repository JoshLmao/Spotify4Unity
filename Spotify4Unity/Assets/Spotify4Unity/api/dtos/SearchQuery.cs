using Spotify4Unity.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotify4Unity.Dtos
{
    public class SearchQuery
    {
        public List<Track> Tracks { get; set; }
        public List<Artist> Artists { get; set; }
        public List<Playlist> Playlists { get; set; }
        public List<Album> Albums { get; set; }
    }
}
