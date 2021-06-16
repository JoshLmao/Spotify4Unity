using SpotifyAPI.Web;
using UnityEngine;

public class AppMainContentController : MonoBehaviour
{
    [SerializeField]
    private Transform _viewsParent;

    [SerializeField]
    private GameObject _playlistViewPrefab;

    private MonoBehaviour _currentView;

    private void Start()
    {
        // Destroy any initial children
        if (_viewsParent.transform.childCount > 0)
        {
            foreach(Transform child in _viewsParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // ToDo: Make a default main view
        // SetContent( defaultMainView );
    }

    public void SetContent(object expectedObject)
    {
        if (expectedObject is SimplePlaylist playlist)
        {
            // Set main view to playlist
            GameObject playlistViewGO = Instantiate(_playlistViewPrefab, _viewsParent);
            PlaylistViewController playlistView = playlistViewGO.GetComponent<PlaylistViewController>();
            playlistView.SetPlaylist(playlist);

            _currentView = playlistView;
        }
    }
}
