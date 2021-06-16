using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistsNavigationController : SpotifyServiceListener
{
    [SerializeField]
    private AppMainContentController _mainContentController;

    [SerializeField]
    private GameObject _playlistPrefab;

    [SerializeField]
    private Transform _listViewParent;

    private IEnumerable<SimplePlaylist> _allPlaylists = null;
    bool _isPopulated = false;

    private void Update()
    {
        if (_allPlaylists != null && _allPlaylists.Count() > 0 && !_isPopulated)
        {
            UpdateUI();
            _isPopulated = true;
        }
    }

    protected override async void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        // Get first page from client
        Paging<SimplePlaylist> page = await client.Playlists.CurrentUsers();
        // Get rest of pages from utility function and set variable to run on main thread
        _allPlaylists = await S4UUtility.GetAllOfPagingAsync(client, page);
    }

    private void UpdateUI()
    {
        // Destroy any previous children, blank list
        if (_listViewParent.transform.childCount > 0)
        {
            foreach (Transform child in _listViewParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Iterate through all playlists and instantiate & set playlist
        foreach (SimplePlaylist playlist in _allPlaylists)
        {
            GameObject playlistPrefabGO = Instantiate(_playlistPrefab, _listViewParent);
            playlistPrefabGO.name = $"Playlist {playlist.Name}";

            SingleNavPlaylistController controller = playlistPrefabGO.GetComponent<SingleNavPlaylistController>();
            controller.SetPlaylist(playlist);

            controller.OnPlaylistSelected += this.OnSetPlaylistMainContent;
        }

        if (_allPlaylists.Count() > 0)
        {
            // Get first inst prefab for it's height/width
            RectTransform t = _listViewParent.transform.GetChild(0).GetComponent<RectTransform>();
            float singlePrefabWidth = t.rect.width;
            float singlePrefabHeight = t.rect.height;

            // Get GridLayout & parent rect
            RectTransform parentRect = _listViewParent.GetComponent<RectTransform>();
            VerticalLayoutGroup group = _listViewParent.GetComponent<VerticalLayoutGroup>();

            // Height is amount of entries + spacing + padding
            float allPaddingSpacingPixels = group.padding.top + group.padding.bottom + group.spacing;
            float height = (singlePrefabHeight + allPaddingSpacingPixels) * _allPlaylists.Count();

            // Set parent's new height for scrolling
            parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }

    private void OnSetPlaylistMainContent(SimplePlaylist mainPlaylist)
    {
        // nav selected new playlist, change main content to display it

        Debug.Log($"Spotify App | Set main content to playlist '{mainPlaylist.Name}'");

        if (_mainContentController)
        {
            _mainContentController.SetContent(mainPlaylist);
        }
    }
}
