using Spotify4Unity.Dtos;
using Spotify4Unity.Enums;
using Spotify4Unity.Events;
using Spotify4Unity.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spotify4Unity
/// Example Controller script for saved tracks
/// (Make sure you read the Wiki documentation for all information, explainations & help!)
/// </summary>
public class ExampleTracksController : LayoutGroupBase<Track>
{
    [SerializeField, Tooltip("Column header button which contains an image to represent it's current sorting method")]
    private Button m_sortByTitleBtn = null;

    [SerializeField, Tooltip("Column header button which contains an image to represent it's current sorting method")]
    private Button m_sortByArtistBtn = null;

    [SerializeField, Tooltip("Column header button which contains an image to represent it's current sorting method")]
    private Button m_sortByAlbumBtn = null;

    [SerializeField, Tooltip("Should this example use async loading or not")]
    private bool m_loadAsync = true;

    [SerializeField, Tooltip("UI to display when nothing is loaded")]
    private GameObject m_noTracksUI = null;

    [SerializeField, Tooltip("The UI game object to display when list is being loaded async")]
    private GameObject m_loadingUI = null;

    private Sort m_currentSort = Sort.Unsorted;
    private bool m_isSortInverted = false;

    private List<Track> m_tracks = null;

    protected override void Start()
    {
        base.Start();

        if (m_sortByTitleBtn != null)
        {
            m_sortByTitleBtn.onClick.AddListener(OnSortByTitle);
            m_sortByTitleBtn.transform.Find("Icon").gameObject.SetActive(false);
        }
        if (m_sortByArtistBtn != null)
        {
            m_sortByArtistBtn.onClick.AddListener(OnSortByArtist);
            m_sortByArtistBtn.transform.Find("Icon").gameObject.SetActive(false);
        }
        if (m_sortByAlbumBtn != null)
        {
            m_sortByAlbumBtn.onClick.AddListener(OnSortByAlbum);
            m_sortByAlbumBtn.transform.Find("Icon").gameObject.SetActive(false);
        }

        m_loadingUI.SetActive(false);
        m_noTracksUI.SetActive(true);
    }

    protected override void SetPrefabInfo(GameObject instantiatedPrefab, Track track)
    {
        SetChildText(instantiatedPrefab, "Title", track.Title);
        SetChildText(instantiatedPrefab, "Artist", String.Join(", ", track.Artists.Select(x => x.Name)));
        SetChildText(instantiatedPrefab, "Album", track.Album);

        SetBtnListener(instantiatedPrefab, track);
    }

    private void SetBtnListener(GameObject prefab, Track track)
    {
        Button playBtn = prefab.transform.Find("PlayBtn").GetComponent<Button>();
        Button queueBtn = prefab.transform.Find("QueueBtn").GetComponent<Button>();
        if (m_loadAsync)
        {
            playBtn.onClick.AddListener(async () => await OnPlayTrackAsync(track));
            queueBtn.onClick.AddListener(async () => await OnQueueTrackAsync(track));
        }
        else
        {
            playBtn.onClick.AddListener(() => OnPlayTrack(track));
            queueBtn.onClick.AddListener(() => OnQueueTrack(track));
        }
    }

    private void SetChildText(GameObject parent, string childName, string content)
    {
        parent.transform.Find(childName).GetComponent<Text>().text = content;
    }

    private async Task OnPlayTrackAsync(Track t)
    {
        await SpotifyService.PlayTrackAsync(t);
    }

    private async Task OnQueueTrackAsync(Track t)
    {
        SpotifyService.QueueTrack(t);
    }

    private void OnPlayTrack(Track t)
    {
        SpotifyService.PlayTrack(t);
    }

    private void OnQueueTrack(Track t)
    {
        SpotifyService.QueueTrack(t);
    }


    protected override void OnConnectedChanged(ConnectedChanged e)
    {
        if (!e.IsConnected)
            UpdateUI(null);
    }

    protected override void OnSavedTracksLoaded(SavedTracksLoaded e)
    {
        base.OnSavedTracksLoaded(e);

        m_tracks = e.SavedTracks;
        if(m_tracks != null && m_tracks.Count > 0)
        {
            if (m_noTracksUI != null)
                m_noTracksUI.SetActive(false);
            if (m_loadingUI != null)
                m_loadingUI.SetActive(true);
            if (m_resizeCanvas != null)
                m_resizeCanvas.gameObject.SetActive(false);

            if (m_loadAsync)
                UpdateUICoroutine(m_tracks);
            else
                UpdateUI(m_tracks);
        }
        else
        {
            // No tracks loaded or no tracks in users library
            if (m_noTracksUI != null)
                m_noTracksUI.SetActive(true);
        }
    }

    protected override void OnUIUpdateFinished()
    {
        base.OnUIUpdateFinished();

        m_resizeCanvas.gameObject.SetActive(true);
        m_loadingUI.SetActive(false);
    }

    public void OnSortByTitle()
    {
        if (m_tracks == null || m_tracks?.Count <= 0)
            return;
        DisableAll();
        GenericSort(Sort.Title, m_sortByTitleBtn);
    }

    public void OnSortByArtist()
    {
        if (m_tracks == null || m_tracks?.Count <= 0)
            return;
        DisableAll();
        GenericSort(Sort.Artist, m_sortByArtistBtn);
    }

    public void OnSortByAlbum()
    {
        if (m_tracks == null || m_tracks?.Count <= 0)
            return;
        DisableAll();
        GenericSort(Sort.Album, m_sortByAlbumBtn);
    }

    private void GenericSort(Sort sortByMode, Button btn)
    {
        if (m_currentSort == sortByMode)
        {
            if (m_isSortInverted)
            {
                //Is inverted & sorted, not restore to unsorted
                m_currentSort = Sort.Unsorted;
                m_isSortInverted = false;
                m_tracks = SpotifyService.SavedTracks;

                btn.transform.Find("Icon").gameObject.SetActive(false);
            }
            else
            {
                //Is sorted but not inverted
                m_tracks.Reverse();
                m_isSortInverted = true;

                btn.transform.Find("Icon").gameObject.SetActive(true);
                btn.transform.Find("Icon").transform.localRotation = Quaternion.Euler(180, 0, 0);
            }
        }
        else
        {
            if (m_isSortInverted)
                m_isSortInverted = false;

            //Is unsorted, sort list
            m_tracks = SpotifyService.GetSavedTracksSorted(sortByMode);
            m_currentSort = sortByMode;

            btn.transform.Find("Icon").gameObject.SetActive(true);
            btn.transform.Find("Icon").transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        UpdateUI(m_tracks);
    }

    private void DisableAll()
    {
        m_sortByTitleBtn.transform.Find("Icon").gameObject.SetActive(false);
        m_sortByArtistBtn.transform.Find("Icon").gameObject.SetActive(false);
        m_sortByAlbumBtn.transform.Find("Icon").gameObject.SetActive(false);
    }
}
