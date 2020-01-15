using System.Collections.Generic;
using System.Linq;

namespace Spotify4Unity.Dtos
{
    public class Track
    {
        /// <summary>
        /// All artist(s) that has created the track
        /// </summary>
        public List<Artist> Artists { get; set; }
        /// <summary>
        /// Title of the track
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Name of the album the song is from
        /// </summary>
        public string Album { get; set; }
        /// <summary>
        /// Url of the song
        /// </summary>
        public string TrackURL { get; set; }

        /// <summary>
        /// The URI of the track
        /// </summary>
        public string TrackUri { get; set; }
        /// <summary>
        /// The URI of the album
        /// </summary>
        public string AlbumUri { get; set; }
        /// <summary>
        /// The Spotify ID of the current track
        /// </summary>
        public string TrackId { get; set; }

        /// <summary>
        /// Total time in milliseconds the song is
        /// </summary>
        public float TotalTimeMs { get; set; }

        /// <summary>
        /// Name of the first artist on the track
        /// </summary>
        public Artist Artist { get { return Artists != null ? Artists.FirstOrDefault() : null; } }
        /// <summary>
        /// Amount of popularity the track has, from X to X
        /// </summary>
        public int Popularity { get; set; }

        private SpotifyAPI.Web.Models.FullTrack m_track = null;

        public Track()
        {

        }

        public Track(SpotifyAPI.Web.Models.FullTrack t)
        {
            m_track = t;
            if (m_track == null)
                return;

            Title = t.Name;
            TrackUri = t.Uri;
            TrackId = t.Id;
            Popularity = t.Popularity;

            if (t.Artists != null)
            {
                Artists = t.Artists.Select(x => new Artist()
                {
                    Name = x.Name,
                    Uri = x.Uri,
                }).ToList();
            }
            if (t.Album != null)
            {
                Album = t.Album.Name;
                AlbumUri = t.Album.Uri;
            }

            TotalTimeMs = t.DurationMs;
        }

        public string GetAlbumArtUrl()
        {
            if (m_track != null && m_track.Album != null && m_track.Album.Images != null)
            {
                SpotifyAPI.Web.Models.Image i = m_track.Album.Images.FirstOrDefault();
                return i.Url;
            }
            return null;
        }
    }
}