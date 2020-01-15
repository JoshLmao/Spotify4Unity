using Spotify4Unity;
using Spotify4Unity.Dtos;
using Spotify4Unity.Enums;
using Spotify4Unity.Events;
using Spotify4Unity.Helpers;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spotify4Unity
/// Example Controller script for a Simple Player with media controls
/// (Make sure you read the Wiki documentation for all information & help!)
/// </summary>
public class ExamplePlayerController : SpotifyUIBase
{
    [SerializeField, Tooltip("The root parent holding the connect UI, gets disabled when connected")]
    private GameObject m_connectCanvas = null;

    [SerializeField, Tooltip("Button to trigger a connect to spotify")]
    private Button m_connectBtn = null;

    [SerializeField, Tooltip("Text to display the current track's artists")]
    private Text m_artistText = null;

    [SerializeField, Tooltip("Text to display the current track name")]
    private Text m_trackText = null;

    [SerializeField, Tooltip("Text to display the current track's album name")]
    private Text m_albumText = null;

    [SerializeField, Tooltip("Slider to control and display current track's position")]
    private Slider m_playingSlider = null;

    [SerializeField, Tooltip("Text to display the current track position")]
    private Text m_trackPositionText = null;

    [SerializeField, Tooltip("Slider to display and control the current volume")]
    private Slider m_volumeSlider = null;

    [SerializeField, Tooltip("Button used to mute Spotify sound")]
    private Button m_muteBtn = null;

    [SerializeField, Tooltip("Button to unmute Spotify's sound")]
    private Button m_unmuteBtn = null;

    [SerializeField, Tooltip("Button to change the track to the previous track")]
    private Button m_previousBtn = null;

    [SerializeField, Tooltip("Button to change the track to the next track")]
    private Button m_nextBtn = null;

    [SerializeField, Tooltip("Button to Play the track when paused")]
    private Button m_playBtn = null;

    [SerializeField, Tooltip("Button to pause the track when playing")]
    private Button m_pauseBtn = null;

    [SerializeField, Tooltip("Image to display the current track's album art")]
    private Image m_albumArt = null;

    [SerializeField]
    private Button m_shuffleBtn = null;

    [SerializeField]
    private Button m_repeatBtn = null;

    [SerializeField]
    private GameObject m_connectingSpinner = null;

    [SerializeField, Tooltip("Should match the amount of repeat states (2 - Check the enum!).")]
    private Sprite[] m_repeatSprites = null;

    [SerializeField, Tooltip("Should match the amount of shuffle states (3 - Check the enum!)")]
    private Sprite[] m_shuffleSprites = null;

    [SerializeField, Tooltip("Sprite icon for when advert is playing. Has media for Tracks and Episodes")]
    private Sprite AdvertSprite = null;

    [SerializeField, Tooltip("How much to increase or decrease the current track position by on mouse scroll (in seconds)")]
    private float m_positionScrollAmount = 5f;

    [SerializeField, Tooltip("How much to increase or decrease the volume by on mouse scroll")]
    private float m_volScrollAmount = 5f;

    [SerializeField, Tooltip("The resolution to load album arts at")]
    private Spotify4Unity.Enums.Resolution m_albumArtResolution = Spotify4Unity.Enums.Resolution.Original;

    [SerializeField, Tooltip("Set the 'Pressed Button' color for all playback buttons when the user isn't Premium")]
    private Color m_errorColor = new Color(1f, 0f, 0f, 0.5f);

    [SerializeField, Tooltip("Button to add the current track to  the user's saved tracks. Requires an image as a child")]
    private Button m_saveTrackBtn = null;

    [SerializeField, Tooltip("All save track icons in order of 'Add save track to  Library' and 'Track already in library'")]
    private Sprite[] m_saveTrackIcons = null;

    [SerializeField]
    private Button m_disconnectBtn = null;

    private bool m_isDraggingTrackPositionSlider = false;
    private float m_lastTrackPosMsSliderValue = -1f;

    private bool m_isDraggingVolumeSlider = false;
    private float m_lastVolumeSliderValue = -1f;

    const string TIME_SPAN_FORMAT = @"mm\:ss";

    #region MonoBehavious
    protected override void Awake()
    {
        base.Awake();

        if (m_connectBtn != null)
            m_connectBtn.onClick.AddListener(OnConnect);

        if (m_nextBtn != null)
            m_nextBtn.onClick.AddListener(OnPlayMedia);

        if (m_pauseBtn != null)
            m_pauseBtn.onClick.AddListener(OnPauseMedia);

        if (m_playBtn != null)
            m_playBtn.onClick.AddListener(OnPlayMedia);

        if (m_previousBtn != null)
            m_previousBtn.onClick.AddListener(OnPreviousMedia);

        if (m_nextBtn != null)
            m_nextBtn.onClick.AddListener(OnNextMedia);

        if (m_muteBtn != null)
            m_muteBtn.onClick.AddListener(OnUnmuteSound);
        if (m_unmuteBtn != null)
            m_unmuteBtn.onClick.AddListener(OnMuteSound);

        if (m_volumeSlider != null)
            m_volumeSlider.onValueChanged.AddListener(OnSetVolumeChanged);

        if (m_playingSlider != null)
            m_playingSlider.onValueChanged.AddListener(OnSetTrackPosition);

        if (m_repeatBtn != null)
            m_repeatBtn.onClick.AddListener(OnClickRepeat);

        if (m_shuffleBtn != null)
            m_shuffleBtn.onClick.AddListener(OnClickShuffle);

        if (m_saveTrackBtn != null)
            m_saveTrackBtn.onClick.AddListener(OnSaveTrack);

        if (m_disconnectBtn != null)
            m_disconnectBtn.onClick.AddListener(OnDisconnect);
    }

    private void Start()
    {
        // Hide Connect & Connecting UI
        m_connectCanvas.SetActive(true);
        m_connectingSpinner.SetActive(false);
    }
    #endregion

    private void OnConnect()
    {
        if (!SpotifyService.IsConnected)
        {
            m_connectCanvas.SetActive(false);
            m_connectingSpinner.SetActive(true);

            bool didAttempt = SpotifyService.Connect();
            // If an attempt to authorize failed, show connect btn, hide spinner
            if (!didAttempt)
            {
                m_connectCanvas.SetActive(true);
                m_connectingSpinner.SetActive(false);
            }
        }
    }

    protected async void OnNextMedia()
    {
        await SpotifyService.NextSongAsync();
    }

    protected async void OnPreviousMedia()
    {
        await SpotifyService.PreviousSongAsync();
    }

    protected async void OnPauseMedia()
    {
        await SpotifyService.PauseAsync();
    }

    protected async void OnPlayMedia()
    {
        await SpotifyService.PlayAsync();
    }

    protected override void OnConnectingChanged(ConnectingChanged e)
    {
        base.OnConnectingChanged(e);

        // Enable Connect Canvas when NOT connecting and NOT connected
        m_connectCanvas.SetActive(!e.IsConnecting && !SpotifyService.IsConnected);
        // Enable Spinner when connecting and NOT connected
        m_connectingSpinner.SetActive(e.IsConnecting && !SpotifyService.IsConnected);
    }

    protected override void OnConnectedChanged(ConnectedChanged e)
    {
        base.OnConnectedChanged(e);

        // Re-enable connect UI if not connected to Spotify
        if (m_connectCanvas != null)
            m_connectCanvas.SetActive(!e.IsConnected);

        // Always disable spinner
        if (m_connectingSpinner != null && m_connectingSpinner.activeInHierarchy)
            m_connectingSpinner.SetActive(false);
    }

    protected override void OnTrackTimeChanged(TrackTimeChanged e)
    {
        base.OnTrackTimeChanged(e);

        if (m_playingSlider != null)
        {
            //Dont update when dragging slider
            if (m_isDraggingTrackPositionSlider)
                return;

            m_playingSlider.value = e.CurrentPositionMs;
            m_playingSlider.maxValue = e.TotalTimeMs;
        }

        if(m_trackPositionText != null)
        {
            SetTrackPositionText(e.CurrentPositionMs, e.TotalTimeMs);
        }
    }

    protected override void OnPlayStatusChanged(PlayStatusChanged e)
    {
        base.OnPlayStatusChanged(e);

        // If a Play btn & Pause btn is configured, set it's correct displaying state
        if (m_playBtn != null && m_playBtn.isActiveAndEnabled != !e.IsPlaying)
        {
            m_playBtn.gameObject.SetActive(!e.IsPlaying);
        }
        if (m_pauseBtn != null && m_pauseBtn.isActiveAndEnabled != e.IsPlaying)
        {
            m_pauseBtn.gameObject.SetActive(e.IsPlaying);
        }
    }

    protected override void OnTrackChanged(TrackChanged e)
    {
        if(e != null)
        {
            // Load the Album Art for the new Track
            LoadAlbumArt(e.NewTrack, m_albumArtResolution);
            SetTrackInfo(e.NewTrack.Title, e.NewTrack.Album, String.Join(", ", e.NewTrack.Artists.Select(x => x.Name)));

            UpdateSaveTrackBtn();
        }
    }

    private void LoadAlbumArt(Track t, Spotify4Unity.Enums.Resolution resolution)
    {
        // Get the URL and load on a Coroutine
        string url = t.GetAlbumArtUrl();
        if (!string.IsNullOrEmpty(url))
        {
            if (this.isActiveAndEnabled)
                StartCoroutine(Utility.LoadImageFromUrl(url, resolution, sprite => OnAlbumArtLoaded(sprite)));
            else
                Utility.RunCoroutineEmptyObject(Utility.LoadImageFromUrl(url, resolution, sprite => OnAlbumArtLoaded(sprite)));
        }
    }

    private void OnAlbumArtLoaded(Sprite s)
    {
        if (m_albumArt != null)
        {
            m_albumArt.sprite = s;
        }
    }

    protected override void OnVolumeChanged(VolumeChanged e)
    {
        base.OnVolumeChanged(e);

        if(m_volumeSlider != null && !m_isDraggingVolumeSlider)
        {
            m_volumeSlider.value = e.Volume;
            m_volumeSlider.maxValue = e.MaxVolume;
        }
    }

    protected void OnSetVolumeChanged(float value)
    {
        m_lastVolumeSliderValue = value;
    }

    protected async void OnUnmuteSound()
    {
        if (SpotifyService.IsMuted)
        {
            await SpotifyService.SetMuteAsync(false);
        }
    }

    protected async void OnMuteSound()
    {
        if (!SpotifyService.IsMuted)
        {
            await SpotifyService.SetMuteAsync(true);
        }
    }

    protected override void OnMuteChanged(MuteChanged e)
    {
        base.OnMuteChanged(e);

        m_muteBtn.gameObject.SetActive(e.IsMuted);
        m_unmuteBtn.gameObject.SetActive(!e.IsMuted);
    }

    protected void OnSetTrackPosition(float sliderValue)
    {
        m_lastTrackPosMsSliderValue = sliderValue;

        SetTrackPositionText(m_lastTrackPosMsSliderValue, m_playingSlider.maxValue);
    }

    public void OnMouseDownTrackTimeSlider()
    {
        m_isDraggingTrackPositionSlider = true;
    }

    public async void OnMouseUpTrackTimeSlider()
    {
        if(m_lastTrackPosMsSliderValue > 0f)
        {
            await SpotifyService.SetTrackPositionAsync(m_lastTrackPosMsSliderValue);
        }

        m_isDraggingTrackPositionSlider = false;
        m_lastTrackPosMsSliderValue = -1f;
    }

    protected async void OnClickShuffle()
    {
        Shuffle state = SpotifyService.ShuffleState;
        if (state == 0)
            state = (Shuffle)1;
        else
            state = (Shuffle)0;

        await SpotifyService.SetShuffleAsync(state);
    }

    protected async void OnClickRepeat()
    {
        //Repeat button acts as a toggle through 3 items
        Repeat state = SpotifyService.RepeatState;
        if (SpotifyService.RepeatState == Repeat.Disabled)
            state = Repeat.Playlist;
        else if (SpotifyService.RepeatState == Repeat.Playlist)
            state = Repeat.Track;
        else if (SpotifyService.RepeatState == Repeat.Track)
            state = Repeat.Disabled;

        await SpotifyService.SetRepeatAsync(state);
    }

    protected override void OnRepeatChanged(RepeatChanged e)
    {
        base.OnRepeatChanged(e);

        if(m_repeatBtn != null && m_repeatSprites != null)
        {
            SetSprite((int)e.State,
                m_repeatSprites,
                m_repeatBtn.transform.Find("Icon").GetComponent<Image>(),
                "Missing sprite icons for Repeat! Make sure you have the same amount of sprites as Repeat States (Check the enum!)");
        }
    }

    protected override void OnShuffleChanged(ShuffleChanged e)
    {
        base.OnShuffleChanged(e);

        if(m_shuffleBtn != null && m_shuffleSprites != null)
        {
            SetSprite((int)e.State,
                m_shuffleSprites,
                m_shuffleBtn.transform.Find("Icon").GetComponent<Image>(),
                "Missing sprite icons for Shuffle! Make sure you have the same amount of sprites as Repeat States (Check the enum!)");
        }
    }

    protected override void OnMediaTypeChanged(MediaTypeChanged e)
    {
        if(e.MediaType == MediaType.Advert)
        {
            if (m_albumArt != null)
                m_albumArt.sprite = AdvertSprite;

            SetTrackInfo("Advert", "Unknown", "Unknown");
        }
    }

    protected override void OnUserInformationLoaded(UserInfoLoaded e)
    {
        base.OnUserInformationLoaded(e);

        // If the user is't Premium, show red error when trying to set playback
        // (Spotify doesn't allow Free users to control their playback through the WebAPI)
        if (!e.Info.IsPremium)
        {
            SetBtnPressedTint(ref m_playBtn);
            SetBtnPressedTint(ref m_pauseBtn);
            SetBtnPressedTint(ref m_previousBtn);
            SetBtnPressedTint(ref m_nextBtn);
            SetBtnPressedTint(ref m_shuffleBtn);
            SetBtnPressedTint(ref m_repeatBtn);
            SetBtnPressedTint(ref m_muteBtn);
            SetBtnPressedTint(ref m_unmuteBtn);
        }
    }

    private void SetBtnPressedTint(ref Button btn)
    {
        if (btn == null)
            return;
        ColorBlock colors = btn.colors;
        colors.pressedColor = m_errorColor;
        btn.colors = colors;
    }

    /// <summary>
    /// Set the correct sprite in the UI button
    /// </summary>
    /// <param name="stateIndex">The state as an int</param>
    /// <param name="spritesArray">The array of sprites for that state</param>
    /// <param name="image">The image inside the button that will change and display the current state</param>
    /// <param name="errorMsg">The output error message if stateIndex is too high</param>
    protected void SetSprite(int stateIndex, Sprite[] spritesArray, Image image, string errorMsg)
    {
        if (stateIndex >= spritesArray.Length)
        {
            Analysis.LogError(errorMsg, Analysis.LogLevel.All);
            return;
        }

        if (image != null)
            image.sprite = spritesArray[stateIndex];
    }

    private void SetTrackPositionText(float currentMs, float maxMs)
    {
        string currentPositionFormat = TimeSpan.FromMilliseconds(currentMs).ToString(TIME_SPAN_FORMAT);
        string totalTimeFormat = TimeSpan.FromMilliseconds(maxMs).ToString(TIME_SPAN_FORMAT);
        m_trackPositionText.text = $"{currentPositionFormat}/{totalTimeFormat}";
    }

    public void OnMouseDownVolumeSlider()
    {
        m_isDraggingVolumeSlider = true;
    }

    public async void OnMouseUpVolumeSlider()
    {
        if (m_lastVolumeSliderValue > 0f)
        {
            await SpotifyService.SetVolumeAsync((int)m_lastVolumeSliderValue);
        }

        m_lastVolumeSliderValue = -1f;
    }

    public async void OnScrollUpVolume(float scrollYDelta)
    {
        await SpotifyService.SetVolumeAsync((int)(SpotifyService.Volume.CurrentVolume + (m_volScrollAmount * scrollYDelta)));
    }

    public async void OnScrollDownVolume(float scrollYDelta)
    {
        await SpotifyService.SetVolumeAsync((int)(SpotifyService.Volume.CurrentVolume - (m_volScrollAmount * -scrollYDelta)));
    }

    public async void OnScrollUpPosition(float scrollYDelta)
    {
        float msAmount = (m_positionScrollAmount * scrollYDelta) * 1000f;
        await SpotifyService.SetTrackPositionAsync(SpotifyService.CurrentTrackTimeMs + msAmount);
    }

    public async void OnScrollDownPosition(float scrollYDelta)
    {
        float msAmount = (m_positionScrollAmount * -scrollYDelta) * 1000f;
        await SpotifyService.SetTrackPositionAsync(SpotifyService.CurrentTrackTimeMs - msAmount);
    }

    /// <summary>
    /// Sets the Track, Album and Artist name
    /// </summary>
    /// <param name="track"></param>
    /// <param name="artist"></param>
    /// <param name="album"></param>
    private void SetTrackInfo(string track, string artist, string album)
    {
        if (m_artistText != null)
            m_artistText.text = artist;

        if (m_trackText != null)
            m_trackText.text = track;

        if (m_albumText != null)
            m_albumText.text = album;
    }

    protected override void OnSavedTracksLoaded(SavedTracksLoaded e)
    {
        base.OnSavedTracksLoaded(e);

        UpdateSaveTrackBtn();
    }

    private void UpdateSaveTrackBtn()
    {
        if (SpotifyService.CurrentTrack == null)
            return;

        Track currentTrack = SpotifyService.CurrentTrack;
        if (currentTrack != null && SpotifyService.SavedTracks != null)
        {
            if(SpotifyService.SavedTracks.Exists(x => x.TrackId == SpotifyService.CurrentTrack.TrackId))
                m_saveTrackBtn.GetComponentsInChildren<Image>()[1].sprite = m_saveTrackIcons[1];
            else
                m_saveTrackBtn.GetComponentsInChildren<Image>()[1].sprite = m_saveTrackIcons[0];
        }
    }

    private void OnSaveTrack()
    {
        if (SpotifyService.CurrentTrack == null)
            return;

        bool hasTrackSaved = SpotifyService.SavedTracks.Exists(x => x.TrackId == SpotifyService.CurrentTrack.TrackId);
        bool result = false;
        if (hasTrackSaved)
        {
            result = SpotifyService.UnsaveTracks(SpotifyService.CurrentTrack);
        }
        else
        {
            result = SpotifyService.SaveTracks(SpotifyService.CurrentTrack);
        }

        UpdateSaveTrackBtn();
    }

    private void OnDisconnect()
    {
        SpotifyService.Disconnect();
    }
}
