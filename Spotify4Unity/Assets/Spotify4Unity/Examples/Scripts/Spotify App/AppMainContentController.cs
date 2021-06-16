using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public enum Views
{
    Landing = 0,
    Playlist = 1,
}

public class AppMainContentController : MonoBehaviour
{
    /// <summary>
    /// All prefabs for every view. Need to be ordered by their number in Views enum
    /// </summary>
    public List<GameObject> ViewPrefabs;

    [SerializeField]
    private Transform _viewsParent;

    private Object _currentView;

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

        // Set default view
        GameObject prefab = ViewPrefabs[(int)Views.Landing];
        if (prefab)
        {
            _currentView = Instantiate(prefab, _viewsParent);
        }
    }

    public void SetContent(object expectedObject)
    {
        if (expectedObject is SimplePlaylist playlist)
        {
            GameObject prefab = ViewPrefabs[(int)Views.Playlist];
            if (prefab)
            {
                // Set main view to playlist
                GameObject playlistViewGO = Instantiate(prefab, _viewsParent);
                PlaylistViewController playlistView = playlistViewGO.GetComponent<PlaylistViewController>();
                playlistView.SetPlaylist(playlist);

                _currentView = playlistView;
            }
        }
    }
}
