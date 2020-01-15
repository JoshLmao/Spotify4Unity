using System.Collections.Generic;

namespace Spotify4Unity.Dtos
{
    public class Playlist
    {
        /// <summary>
        /// The name of the playlist
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The creator of the playlist
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// The amount of followers the playlist has
        /// </summary>
        public int Followers { get; set; }
        /// <summary>
        /// The url of the image that represents the playlist
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// The internal Spotify Uri of the playlist
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// All tracks inside the playlist
        /// </summary>
        public List<Track> Tracks { get; set; }
    }
}