using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistWidgetController : SpotifyServiceListener
{
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
            PopulateListUI();
            _isPopulated = true;
        }
    }

    protected override async void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        // Check if we have permission to access logged in user's playlists
        if (SpotifyService.Instance.AreScopesAuthorized(Scopes.PlaylistReadPrivate))
        {
            // Get first page from client
            Paging<SimplePlaylist> page = await client.Playlists.CurrentUsers();
            // Get rest of pages from utility function and set variable to run on main thread
            _allPlaylists = await S4UUtility.GetAllOfPagingAsync(client, page);
            _isPopulated = false;
        }
        else
        {
            Debug.LogError($"Not authorized to access '{Scopes.PlaylistReadPrivate}'");
        }
    }

    private void PopulateListUI()
    {
        // Destroy any previous children, blank list
        if (_listViewParent.transform.childCount > 0)
        {
            foreach(Transform child in _listViewParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Iterate through all playlists and instantiate & set playlist
        foreach (SimplePlaylist playlist in _allPlaylists)
        {
            GameObject playlistPrefabGO = Instantiate(_playlistPrefab, _listViewParent);
            playlistPrefabGO.name = $"Playlist {playlist.Name}";

            SinglePlaylistController controller = playlistPrefabGO.GetComponent<SinglePlaylistController>();
            controller.SetPlaylist(playlist);
        }

        if (_allPlaylists.Count() > 0)
        {
            // Get first inst prefab for it's height/width
            RectTransform t = _listViewParent.transform.GetChild(0).GetComponent<RectTransform>();
            float singlePrefabWidth = t.rect.width;
            float singlePrefabHeight = t.rect.height;

            // Get GridLayout & parent rect
            RectTransform parentRect = _listViewParent.GetComponent<RectTransform>();
            GridLayoutGroup group = _listViewParent.GetComponent<GridLayoutGroup>();

            // Determine how many rows/cols there are
            int amtInRow = (int)(parentRect.rect.width / (singlePrefabWidth + (group.spacing.x * 2)));
            int amtOfRows = _allPlaylists.Count() / amtInRow;

            // Add spacing to single height and multiple amount of rows
            float height = (singlePrefabHeight + group.spacing.y) * amtOfRows;

            // Set parent's new height for scrolling
            parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
