using Spotify4Unity;
using Spotify4Unity.Dtos;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleSearchController : SpotifyUIBase
{
    [SerializeField, Tooltip("The amount of results to return after search. Maximum is 50!")]
    private int m_maxCount = 50;

    /// <summary>
    /// The text input for the search query
    /// </summary>
    [SerializeField, Tooltip("Text input for the search query")]
    private InputField m_input = null;

    /// <summary>
    /// Button used to search with the current query. If not set, will use OnTextChanged event of InputField
    /// </summary>
    [SerializeField, Tooltip("Button used to search with the current query. If not set, will use OnTextChanged event of InputField")]
    private Button m_searchBtn = null;

    /// <summary>
    /// The list container for displaying tracks
    /// </summary>
    [SerializeField, Tooltip("The child list for displaying tracks")]
    private TracksContainer m_tracks = null;

    /// <summary>
    /// The list container for displaying artists
    /// </summary>
    [SerializeField, Tooltip("The child list for displaying arists")]
    private ArtistsContainer m_artists = null;

    /// <summary>
    /// The list container for displaying albums
    /// </summary>
    [SerializeField, Tooltip("The child list for displaying album")]
    private AlbumsContainer m_albums = null;

    /// <summary>
    /// Should the returned results be sorted by popularity
    /// </summary>
    [SerializeField, Tooltip("Sorts all results by popularity")]
    private bool m_soryByPopularity = true;

    private void Start()
    {
        if(m_searchBtn != null)
        {
            m_searchBtn.onClick.AddListener(OnSearch);
        }
        else
        {
            if (m_input != null)
                m_input.onValueChanged.AddListener(OnQueryChanged);
        }
    }

    private void OnSearch()
    {
        string query = m_input.text;
        OnQueryChanged(query);
    }

    private void OnQueryChanged(string q)
    {
        if (m_maxCount > 50)
            m_maxCount = 50;

        SearchQuery items = null;
        items = SpotifyService.Search(q, m_maxCount);

        Analysis.Log($"Search for '{q}'- Found '{items.Tracks.Count}' tracks, '{items.Artists.Count}' artists and '{items.Albums.Count}' albums", Analysis.LogLevel.All);

        if (items != null)
        {
            if (m_tracks != null && items.Tracks != null)
            {
                List<Track> tracks = items.Tracks;
                if (m_soryByPopularity)
                    tracks.Sort((x, y) => x.Popularity > y.Popularity ? y.Popularity : x.Popularity);
                m_tracks.Populate(tracks);
            }

            if (m_artists != null && items.Artists != null)
            {
                List<Artist> artists = items.Artists;
                if (m_soryByPopularity)
                    artists.Sort((x, y) => x.Popularity > y.Popularity ? y.Popularity : x.Popularity);
                m_artists.Populate(items.Artists);
            }

            if (m_albums != null && items.Albums != null)
                m_albums.Populate(items.Albums);
        }
    }
}
