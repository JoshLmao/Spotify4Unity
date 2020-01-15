using System;
using System.Collections.Generic;

namespace Spotify4Unity.Dtos
{
    /// <summary>
    /// Base class for sharing properties between artists and users
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// Display name of the account
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Unique Id of the account
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// How many followers the account has
        /// </summary>
        public int Followers { get; set; }
        /// <summary>
        /// The URL of the first profile picture for the user, can be empty
        /// </summary>
        public string ProfilePictureURL { get; set; }
    }

    /// <summary>
    /// All available information for artists
    /// </summary>
    public class ArtistInfo : AccountInfo
    {
        /// <summary>
        /// A number between 0 and 100 that defines the current popularity
        /// </summary>
        public int Popularity { get; set; }
        /// <summary>
        /// List of display names of genres the artist is part of
        /// </summary>
        public List<string> Genres { get; set; }
        /// <summary>
        /// First url that can be used to share the artist
        /// </summary>
        public string ShareURL { get; set; }
    }

    /// <summary>
    /// All information available for users
    /// </summary>
    public class UserInfo : AccountInfo
    {
        /// <summary>
        /// The username of the user, different from their (display) name
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Is the user a premium user
        /// </summary>
        public bool IsPremium { get; set; }
        /// <summary>
        /// A code of the country for the user. For example, if the user is in Great Britain, it will be "GB"
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// The birthdate of the user. Will be DateTime.Min if invalid or unavailable
        /// </summary>
        public DateTime Birthdate { get; set; }
    }
}
