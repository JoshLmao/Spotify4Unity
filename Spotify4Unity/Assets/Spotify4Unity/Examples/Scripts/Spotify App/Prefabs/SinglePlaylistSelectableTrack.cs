using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlaylistSelectableTrack : MonoBehaviour
{
    [SerializeField]
    Button _playTrackBtn;

    [SerializeField]
    Text _trackNameText, _trackArtistsText, _albumText, _durationText;

    [SerializeField]
    Button _addToQueueBtn;

    private string _contextUri;
    private FullTrack _track;

    private void Start()
    {
        // Add btn listeners on start
        if (_playTrackBtn != null)
        {
            _playTrackBtn.onClick.AddListener(this.OnPlayTrack);
        }

        if (_addToQueueBtn != null)
        {
            _addToQueueBtn.onClick.AddListener(this.OnAddToQueue);
        }
    }

    public void SetTrack(FullTrack t, string contextUri)
    {
        _contextUri = contextUri;
        // Set track and Update
        _track = t;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_track != null)
        {
            if (_trackNameText != null)
            {
                _trackNameText.text = _track.Name;
            }
            if (_trackArtistsText != null)
            {
                _trackArtistsText.text = S4UUtility.ArtistsToSeparatedString(", ", _track.Artists);
            }
            if (_albumText != null)
            {
                _albumText.text = _track.Album.Name;
            }
            if (_durationText != null)
            {
                _durationText.text = S4UUtility.MsToTimeString(_track.DurationMs);
            }
        }
    }

    private async void OnPlayTrack()
    {
        if (_track != null)
        {
            SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
            if (client != null)
            {
                // Play track in context of the playlist
                PlayerResumePlaybackRequest request = new PlayerResumePlaybackRequest()
                {
                    ContextUri = _contextUri,
                    OffsetParam = new PlayerResumePlaybackRequest.Offset() { Uri = _track.Uri },
                };
                await client.Player.ResumePlayback(request);

                LogTrackChange("Playing track");
            }
        }
    }


    private void OnAddToQueue()
    {
        if (_track != null)
        {
            SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
            if (client != null)
            {
                PlayerAddToQueueRequest request = new PlayerAddToQueueRequest(_track.Uri);
                client.Player.AddToQueue(request);

                LogTrackChange("Added to queue");
            }
        }
    }

    private void LogTrackChange(string source)
    {
        Debug.Log($"Spotify App | {source} '{S4UUtility.ArtistsToSeparatedString(", ", _track.Artists)} - {_track.Name}'");
    }
}
