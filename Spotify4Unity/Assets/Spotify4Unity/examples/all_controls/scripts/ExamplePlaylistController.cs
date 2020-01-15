using Spotify4Unity;
using Spotify4Unity.Dtos;
using Spotify4Unity.Events;
using Spotify4Unity.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spotify4Unity
/// Example Controller script for playlists
/// (Make sure you read the Wiki documentation for all information & help!)
/// </summary>
public class ExamplePlaylistController : LayoutGroupBase<Playlist>
{
    [SerializeField, Tooltip("Should the playlist image's aspect ratio be used in the UI")]
    private bool m_preserveAspectRatio = true;

    [SerializeField, Tooltip("The target resolution to display the playlist preview images")]
    private Spotify4Unity.Enums.Resolution m_playlistImageResolution = Spotify4Unity.Enums.Resolution.x128;

    [SerializeField, Tooltip("Should this example use async loading or not")]
    private bool m_loadAsync = true;

    [SerializeField, Tooltip("UI to display when nothing is loaded")]
    private GameObject m_noPlaylistsUI = null;

    [SerializeField, Tooltip("The UI game object to display when list is being loaded")]
    private GameObject m_loadingUI = null;

    private List<Playlist> m_playlists = null;

    protected override void Start()
    {
        base.Start();

        m_loadingUI.SetActive(false);
    }

    protected override void ConfigureLayoutGroup(LayoutGroup layoutGroup)
    {
        base.ConfigureLayoutGroup(layoutGroup);

        GridLayoutGroup gridGroup = layoutGroup as GridLayoutGroup;
        Rect prefabRect = m_prefab.GetComponent<RectTransform>().rect;
        gridGroup.cellSize = new Vector2(prefabRect.width, prefabRect.height);
    }

    protected override void OnPlaylistsChanged(PlaylistsChanged e)
    {
        base.OnPlaylistsChanged(e);

        m_playlists = e.Playlists;
        if(m_playlists != null && m_playlists.Count > 0)
        {
            if (m_noPlaylistsUI != null)
                m_noPlaylistsUI.SetActive(false);
            if (m_loadingUI != null)
                m_loadingUI.SetActive(true);
            if (m_resizeCanvas != null)
                m_resizeCanvas.gameObject.SetActive(false);

            if (m_loadAsync)
                UpdateUICoroutine(m_playlists);
            else
                UpdateUI(m_playlists);
        }
        else
        {
            // No playlists loaded or has no playlists
            if (m_noPlaylistsUI != null)
                m_noPlaylistsUI.SetActive(true);
        }
    }

    protected override void SetPrefabInfo(GameObject instantiatedPrefab, Playlist playlist)
    {
        //Set playlist name in prefab
        instantiatedPrefab.transform.Find("Name").GetComponent<Text>().text = playlist.Name;
        //Add listener to btn to play the playlist

        Button playBtn = instantiatedPrefab.transform.Find("Image/PlayBtn").GetComponent<Button>();
        if (m_loadAsync)
            playBtn.onClick.AddListener(async () => await OnPlayPlaylistAsync(playlist));
        else
            playBtn.onClick.AddListener(() => OnPlayPlaylistAsync(playlist));

        instantiatedPrefab.transform.Find("Author").GetComponent<Text>().text = playlist.Author;

        //Load the playlist icon on a routine
        if (!string.IsNullOrEmpty(playlist.ImageUrl))
        {
            StartCoroutine(LoadImage(instantiatedPrefab, playlist));
        }
    }

    private System.Collections.IEnumerator LoadImage(GameObject prefab, Playlist playlist)
    {
        Image img = prefab.transform.Find("Image").GetComponent<Image>();
        img.preserveAspect = m_preserveAspectRatio;
        yield return Utility.LoadImageFromUrl(playlist.ImageUrl, m_playlistImageResolution, (sprite) =>
        {
            img.sprite = sprite;
        });
    }

    protected override void OnUIUpdateFinished()
    {
        base.OnUIUpdateFinished();

        m_loadingUI.SetActive(false);
        m_resizeCanvas.gameObject.SetActive(true);
    }

    private async Task OnPlayPlaylistAsync(Playlist playlist)
    {
        Analysis.Log($"Playing playlist '{playlist.Name}'", Analysis.LogLevel.All);

        await SpotifyService.PlayPlaylistAsync(playlist);
    }
}
