using SpotifyAPI.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Image = UnityEngine.UI.Image;
using System;
using UnityEngine.Events;

public class SpotifyPlayerController : SpotifyPlayerListener
{
    [SerializeField]
    private Image _trackIcon;

    [SerializeField]
    private Text _trackName, _artistsNames;

    [SerializeField]
    private Text _currentProgressText, _totalProgressText;
    [SerializeField]
    private Slider _currentProgressSlider;

    [SerializeField]
    private Button _playPauseButton, _previousButton, _nextButton, _shuffleButton, _repeatButton;

    [SerializeField]
    private Slider _volumeSlider;
    [SerializeField]
    private Button _muteButton;

    private void Start()
    {
        this.OnTrackChanged += this.TrackChanged;

        if (_playPauseButton != null)
        {
            _playPauseButton.onClick.AddListener(() => this.OnPlayPauseClicked());
        }
        if (_previousButton != null)
        {
            _previousButton.onClick.AddListener(() => this.OnPreviousClicked());
        }
        if (_nextButton != null)
        {
            _nextButton.onClick.AddListener(() => this.OnNextClicked());
        }
        if (_shuffleButton != null)
        {
            _shuffleButton.onClick.AddListener(() => this.OnToggleShuffle());
        }
        if (_repeatButton != null)
        {
            _repeatButton.onClick.AddListener(() => this.OnToggleRepeat());
        }

        if (_muteButton != null)
        {
            _muteButton.onClick.AddListener(() => this.OnToggleMute());
        }
    }

    private void Update()
    {
        var context = GetCurrentContext();
        if (context != null)
        {
            var track = context.Item as FullTrack;

            if (_currentProgressText != null)
            {
                _currentProgressText.text = S4UUtility.MsToTimeString(context.ProgressMs);
            }
            if (_totalProgressText != null)
            {
                _totalProgressText.text = S4UUtility.MsToTimeString(track.DurationMs);
            }
            if (_currentProgressSlider != null)
            {
                _currentProgressSlider.minValue = 0;
                _currentProgressSlider.maxValue = track.DurationMs;
                _currentProgressSlider.value = context.ProgressMs;
            }
            if (_volumeSlider != null)
            {
                _volumeSlider.minValue = 0;
                _volumeSlider.maxValue = 100;
                _volumeSlider.value = context.Device.VolumePercent.Value;
            }
        }
    }

    private void TrackChanged(FullTrack newTrack)
    {
        if (newTrack == null)
            return;

        if (_trackName != null)
        {
            _trackName.text = newTrack.Name;
        }
        if (_artistsNames != null)
        {
            string allArtists = string.Join(", ", newTrack.Artists.Select((x) => x.Name));
            _artistsNames.text = allArtists;
        }
        if (_trackIcon != null)
        {
            string firstImgUrl = newTrack.Album.Images.FirstOrDefault()?.Url;
            StartCoroutine(S4UUtility.LoadImageFromUrl(firstImgUrl, (loadedSprite) =>
            {
                _trackIcon.sprite = loadedSprite;
            }));
        }
    }

    private void OnPlayPauseClicked()
    {
        var context = GetCurrentContext();
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

        if (context.IsPlaying)
        {
            client.Player.PausePlayback();
        }
        else
        {
            client.Player.ResumePlayback();
        }
    }

    private void OnPreviousClicked()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        client.Player.SkipPrevious();
    }

    private void OnNextClicked()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        client.Player.SkipNext();
    }

    private void OnToggleShuffle()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

        // get current shuffle state
        bool currentShuffleState = GetCurrentContext().ShuffleState;
        // Create request, invert state
        PlayerShuffleRequest request = new PlayerShuffleRequest(!currentShuffleState);

        client.Player.SetShuffle(request);
    }

    private void OnToggleRepeat()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

        // Get current shuffle state
        string currentShuffleState = GetCurrentContext().RepeatState;
        
        // Determine next shuffle state
        PlayerSetRepeatRequest.State newState = PlayerSetRepeatRequest.State.Off;
        switch (currentShuffleState)
        {
            case "off":
                newState = PlayerSetRepeatRequest.State.Track;
                break;
            case "track":
                newState = PlayerSetRepeatRequest.State.Context;
                break;
            case "context":
                newState = PlayerSetRepeatRequest.State.Off;
                break;
            default:
                Debug.LogError($"Unknown Shuffle State '{currentShuffleState}'");
                break;
        }

        // Build request and send
        PlayerSetRepeatRequest request = new PlayerSetRepeatRequest(newState);
        client.Player.SetRepeat(request);
    }

    private void OnToggleMute()
    {
        var context = GetCurrentContext();
        int? volume = context.Device.VolumePercent;

        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

        int targetVolume;
        if (volume.HasValue && volume > 0)
        {
            targetVolume = 0;
        }
        else
        {
            // Default volume when un-muting
            targetVolume = 50;
        }

        PlayerVolumeRequest request = new PlayerVolumeRequest(targetVolume);
        client.Player.SetVolume(request);
    }
}
