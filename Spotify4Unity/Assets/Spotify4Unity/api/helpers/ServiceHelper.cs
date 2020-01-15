using Spotify4Unity;
using Spotify4Unity.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAPIModels = SpotifyAPI.Web.Models;

public class ServiceHelper
{
    /// <summary>
    /// Converts the SpotifyAPI.NET model to my own
    /// </summary>
    /// <param name="fullTracksList"></param>
    /// <returns></returns>
    public static async Task<List<Track>> ConvertFullTracksAsync(List<SAPIModels.FullTrack> fullTracksList)
    {
        List<Track> tracks = new List<Track>();
        await Task.Run(() =>
        {
            foreach (SAPIModels.FullTrack track in fullTracksList)
            {
                string spotifyShareURLKey = "spotify";
                string trackUrl = track.ExternUrls.ContainsKey(spotifyShareURLKey) ? track.ExternUrls[spotifyShareURLKey] : null;
                tracks.Add(new Track()
                {
                    Title = track.Name,
                    Artists = track.Artists.Select(x => new Artist()
                    {
                        Name = x.Name,
                        Uri = x.Uri,
                    }).ToList(),
                    Album = track.Album.Name,
                    TrackURL = trackUrl,
                    TrackId = track.Id,
                    TotalTimeMs = track.DurationMs,
                    Popularity = track.Popularity,

                    TrackUri = track.Uri,
                    AlbumUri = track.Album.Uri,
                });
            }
        });
        return tracks;
    }


    public static async Task<List<Playlist>> ConvertSimplePlaylist(List<SAPIModels.SimplePlaylist> playlistList, bool getTracks, SpotifyAPI.Web.SpotifyWebAPI api, string userId)
    {
        List<Playlist> playlists = new List<Playlist>();
        foreach (SAPIModels.SimplePlaylist p in playlistList)
        {
            List<Track> ts = null;
            if (getTracks)
            {
                List<SAPIModels.PlaylistTrack> tracks = null;
                if (api != null && !string.IsNullOrEmpty(userId))
                {
                    SAPIModels.Paging<SAPIModels.PlaylistTrack> currentTracks = await api.GetPlaylistTracksAsync(userId, p.Id, "", 100);
                    tracks = await GetAllFromPagingAsync(api, currentTracks);
                }
                
                try
                {
                    ts = tracks?.Select(x => new Track(x.Track)).ToList();
                }
                catch (Exception e)
                {
                    Analysis.LogError($"Service Playlist Track Error on '{p.Name}' - {e}", Analysis.LogLevel.Vital);
                }
            }

            playlists.Add(new Playlist()
            {
                Name = p.Name,
                Uri = p.Uri,
                Tracks = ts,
                ImageUrl = p.Images != null && p.Images.Count > 0 ? p.Images.FirstOrDefault().Url : null,
                Author = string.IsNullOrEmpty(p.Owner.DisplayName) ? p.Owner.Id : p.Owner.DisplayName,
            });
        }
        return playlists;
    }

    public static async Task<List<Track>> ConvertSavedTracksAsync(List<SAPIModels.SavedTrack> savedTrackList)
    {
        List<SAPIModels.FullTrack> fullTracks = savedTrackList.Select(x => x.Track).ToList();
        return await ServiceHelper.ConvertFullTracksAsync(fullTracks);
    }

    public static DateTime ParseBirthdate(string birthdate)
    {
        //Format should come through as "Year-Month-Day". Simple parse
        string[] split = birthdate.Split('-');
        if (split.Length >= 3)
        {
            int year = int.Parse(split[0]);
            int month = int.Parse(split[1]);
            int day = int.Parse(split[2]);
            return new DateTime(year, month, day);
        }
        else
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// Get all elements from a paging list
    /// </summary>
    /// <typeparam name="T">The type the paging list contains</typeparam>
    /// <param name="pagingList">The original paging list</param>
    /// <param name="getNext">Code to retrieve more from the paging list. Argument is offset. Should return the next block of items in the paging list</param>
    /// <returns></returns>
    private async Task<List<T>> GetAllFromPagingList<T>(SAPIModels.Paging<T> pagingList, Func<int, Task<SAPIModels.Paging<T>>> getNext) where T : class
    {
        if (pagingList == null || pagingList?.Items == null)
        {
            return null;
        }

        List<T> list = new List<T>();
        list.AddRange(pagingList.Items.Select(x => x).ToList());

        while (pagingList.Next != null && pagingList.Items != null)
        {
            pagingList = await getNext(pagingList.Offset + pagingList.Limit);
            if (pagingList.Items != null)
                list.AddRange(pagingList.Items.Select(x => x));
        }

        return list;
    }

    /// <summary>
    /// Gets all elements of a paging list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="api">A reference to the api</param>
    /// <param name="pagingList">The list to get the rest of</param>
    /// <returns></returns>
    public static async Task<List<T>> GetAllFromPagingAsync<T>(SpotifyAPI.Web.SpotifyWebAPI api, SAPIModels.Paging<T> pagingList) where T : class
    {
        List<T> returnList = new List<T>(pagingList.Items);
        while (pagingList.Next != null)
        {
            pagingList = await api.GetNextPageAsync(pagingList);
            returnList.AddRange(pagingList.Items);
        }
        return returnList;
    }
}
