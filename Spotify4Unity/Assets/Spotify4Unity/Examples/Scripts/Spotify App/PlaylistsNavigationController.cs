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
    private Button _homeNavBtn, _searchNavBtn, _likedSongsNavBtn;

    [SerializeField]
    private GameObject _playlistPrefab;

    [SerializeField]
    private Transform _listViewParent;

    [SerializeField]
    private GameObject _loadingSpinnerPrefab;

    private GameObject _liveSpinner;

    private IEnumerable<SimplePlaylist> _allPlaylists = null;

    private List<Action> _dispatcher = new List<Action>();

    private void Start()
    {
        if (_homeNavBtn != null)
        {
            _homeNavBtn.onClick.AddListener(() =>
            {
                if (_mainContentController)
                    _mainContentController.SetContent(Views.Landing);
            });
        }

        if (_searchNavBtn != null)
        {
            _searchNavBtn.onClick.AddListener(() =>
            {
                if (_mainContentController)
                    _mainContentController.SetContent(Views.Search);
            });
        }

        if (_likedSongsNavBtn != null)
        {
            _likedSongsNavBtn.onClick.AddListener(() =>
            {
                if (_mainContentController)
                    _mainContentController.SetContent(Views.LikedSongs);
            });
        }
    }

    private void Update()
    {
        if (_dispatcher.Count > 0)
        {
            foreach(Action actn in _dispatcher)
            {
                actn.Invoke();
            }
            _dispatcher.Clear();
        }
    }

    protected override async void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        if (_loadingSpinnerPrefab != null)
        {
            _dispatcher.Add(() =>
            {
                // Create loading spinner
                _liveSpinner = Instantiate(_loadingSpinnerPrefab, _listViewParent);
            });
        }

        if (client != null)
        {
            // Get first page from client
            Paging<SimplePlaylist> page = await client.Playlists.CurrentUsers();
            // Get rest of pages from utility function and set variable to run on main thread
            _allPlaylists = await S4UUtility.GetAllOfPagingAsync(client, page);

            _dispatcher.Add(() =>
            {
                // Delete loading spinner
                if (_liveSpinner != null)
                    Destroy(_liveSpinner.gameObject);

                UpdateUI();
            });
        }
        else
        {
            // No client, set playlists to empty
            _allPlaylists = new List<SimplePlaylist>();
            UpdateUI();
        }
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
