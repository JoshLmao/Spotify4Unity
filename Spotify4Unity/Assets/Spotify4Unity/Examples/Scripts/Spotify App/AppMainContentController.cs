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
    Search = 2,
    LikedSongs = 3,
}

public class AppMainContentController : MonoBehaviour
{
    /// <summary>
    /// All prefabs for every view. Need to be ordered by their number in Views enum
    /// </summary>
    public List<GameObject> ViewPrefabs;

    // Parent the views should be children of
    [SerializeField]
    private Transform _viewsParent;

    // Current view's controller
    private ViewControllerBase _currentViewController;

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
        SetViewFromEnum(Views.Landing);
    }

    public void SetContent(object expectedObject)
    {
        if (expectedObject is SimplePlaylist playlist)
        {
            MonoBehaviour viewController = SetViewFromEnum(Views.Playlist, (view) =>
            {
                (view as PlaylistViewController).SetPlaylist(playlist);
            });
        }
        else if (expectedObject is Views setView)
        {
            SetViewFromEnum(setView, null);
        }
        else
        {
            // Else if unknonwn object or null, display home screen
            SetViewFromEnum(Views.Landing);
        }
    }

    private MonoBehaviour SetViewFromEnum(Views viewEnum, Action<ViewControllerBase> intermediateActn = null)
    {
        if (_currentViewController != null)
        {
            Destroy(_currentViewController.gameObject);
            _currentViewController = null;
        }

        GameObject prefab = ViewPrefabs[(int)viewEnum];
        if (prefab)
        {
            // Set main view to playlist
            GameObject viewGO = Instantiate(prefab, _viewsParent);

            // Get view controller base, invoke any action
            _currentViewController = viewGO.GetComponent<ViewControllerBase>();
            if (!_currentViewController)
            {
                Debug.LogError($"View '{viewEnum}' doesn't inherit from ViewControllerBase!");
            }
            intermediateActn?.Invoke(_currentViewController);

            return _currentViewController;
        }

        return null;
    }
}
