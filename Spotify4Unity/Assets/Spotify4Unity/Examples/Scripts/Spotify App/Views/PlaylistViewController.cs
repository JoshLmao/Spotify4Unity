using SpotifyAPI.Web;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class PlaylistViewController : MonoBehaviour
{
    public float HeaderHeight = 300;

    [SerializeField]
    private GameObject _singleTrackPrefab;
    
    [SerializeField]
    private Transform _listViewParent;

    [SerializeField]
    private Transform _tracksListParent;

    [SerializeField]
    private Text _headerTitle, _headerDescription, _headerDetails, _headerType;
    [SerializeField]
    private Image _headerImg;

    private SimplePlaylist _playlist;
    private FullPlaylist _fullPlaylist;

    bool _doUpdateUI = false;
    List<PlaylistTrack<IPlayableItem>> _allTracks;

    private void Start()
    {
        _doUpdateUI = false;
    }

    private void Update()
    {
        // Update on main thread
        if (_doUpdateUI)
        {
            UpdateUI();

            _doUpdateUI = false;
        }
    }

    public async void SetPlaylist(SimplePlaylist playlist)
    {
        _playlist = playlist;

        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

        // Get full details of simple playlist
        _fullPlaylist = await client.Playlists.Get(_playlist.Id);

        // Get all tracks inside full playlist
        _allTracks = await GetAllTracks(client);

        // Require ui update on main thread
        _doUpdateUI = true;
    }

    private void UpdateUI()
    {
        UpdatePlaylistDetailsUI();
        UpdatePlaylistTracksUI();
    }

    private void UpdatePlaylistDetailsUI()
    {
        if (_playlist != null)
        {
            if (_headerImg != null)
            {
                StartCoroutine(S4UUtility.LoadImageFromUrl(_playlist.Images.FirstOrDefault()?.Url, (loadedSprite) =>
                {
                    _headerImg.sprite = loadedSprite;
                }));
            }
            if (_headerTitle != null)
            {
                _headerTitle.text = _playlist.Name;
            }
            if (_headerDescription != null)
            {
                _headerDescription.text = _playlist.Description;
            }
            if (_headerDetails != null)
            {
                _headerDetails.text = $"{ _playlist.Owner.DisplayName} • {_playlist.Tracks.Total.Value} songs";
            }
            if (_headerType != null)
            {
                _headerType.text = _playlist.Type.ToUpper();
            }
        }
    }

    private void UpdatePlaylistTracksUI()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

        if (_allTracks != null && client != null)
        {
            // Destroy any previous track children, blank list
            if (_tracksListParent.transform.childCount > 0)
            {
                foreach (Transform child in _tracksListParent.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            // Get all tracks from client, cast to a List
            foreach (PlaylistTrack<IPlayableItem> track in _allTracks)
            {
                // Instantiate prefab
                GameObject singleTrackGo = Instantiate(_singleTrackPrefab, _tracksListParent);
                // Get prefab controller
                SinglePlaylistSelectableTrack singleTrack = singleTrackGo.GetComponent<SinglePlaylistSelectableTrack>();
                // Set track and it's context uri
                singleTrack.SetTrack(track.Track as FullTrack, _playlist.Uri);
            }


            // Get height of prefab
            RectTransform t = _tracksListParent.transform.GetChild(0).GetComponent<RectTransform>();
            float singlePrefabHeight = t.rect.height;

            // Determine total new height of parent
            float totalHeight = (_allTracks.Count() * singlePrefabHeight) + HeaderHeight;

            RectTransform parentRect = _listViewParent.GetComponent<RectTransform>();
            // Set parent's new height for scrolling
            parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }
    }

    private async Task<List<PlaylistTrack<IPlayableItem>>> GetAllTracks(SpotifyClient client)
    {
        List<PlaylistTrack<IPlayableItem>> allTracks = new List<PlaylistTrack<IPlayableItem>>();

        Paging<PlaylistTrack<IPlayableItem>> pUserTracks = await client.Playlists.GetItems(_fullPlaylist.Id, new PlaylistGetItemsRequest { Offset = 0 });
        allTracks.AddRange(pUserTracks.Items);

        int currentOffset = 0;
        int pagingAmount = 100;

        while (currentOffset <= pUserTracks.Total.Value)
        {
            pUserTracks = await client.Playlists.GetItems(_fullPlaylist.Id, new PlaylistGetItemsRequest { Offset = currentOffset + pagingAmount });
            allTracks.AddRange(pUserTracks.Items);

            // Increment by amount + 1 for next segment of tracks
            currentOffset += pagingAmount + 1;
        }

        return allTracks;
    }
}
