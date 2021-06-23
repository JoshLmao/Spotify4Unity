using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SingleArtistWidgetController : SpotifyServiceListener
{
    public string ArtistId = "";

    public Vector2 ImageSize = new Vector2(75, 75);

    [SerializeField]
    private Text _artistName;

    [SerializeField]
    private UnityEngine.UI.Image _icon;

    [SerializeField]
    private Button _playButton, _followButton;

    private FullArtist _artist;

    private List<Action> _dispatcher = new List<Action>();

    private void Start()
    {
        if (_playButton != null)
        {
            _playButton.onClick.AddListener(() => this.OnPlayArtist());
        }
        if (_followButton != null)
        {
            _followButton.onClick.AddListener(() => this.OnFollowArtist());
        }

        SetUIActive(_artist != null);
    }

    private void Update()
    {
        if (_dispatcher.Count > 0)
        {
            foreach (Action actn in _dispatcher)
                actn.Invoke();
            _dispatcher.Clear();
        }
    }

    protected override async void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        // On connect, if artist id set, then retrieve from API
        if (client != null && !string.IsNullOrEmpty(ArtistId))
        {
            _artist = await client.Artists.Get(ArtistId);

            _dispatcher.Add(() =>
            {
                UpdateUI();
            });
        }
        else if (_artist == null)
        {
            // Set to inactive if no artist set
            SetUIActive(false);
        }
    }

    private void UpdateUI()
    {
        if (_artist != null)
        {
            SetUIActive(true);

            if (_artistName != null)
            {
                _artistName.text = _artist.Name;
            }
            if (_icon != null)
            {
                // Update to target image size
                RectTransform rect = _icon.GetComponent<RectTransform>();
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ImageSize.x);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ImageSize.y);

                SpotifyAPI.Web.Image lowestResImg = S4UUtility.GetLowestResolutionImage(_artist.Images);
                StartCoroutine(S4UUtility.LoadImageFromUrl(lowestResImg?.Url, (sprite) =>
                {
                    _icon.sprite = sprite;
                }));
            }
        }
        else
        {
            SetUIActive(false);
        }
    }

    private void OnPlayArtist()
    {
        var client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            PlayerResumePlaybackRequest request = new PlayerResumePlaybackRequest()
            {
                ContextUri = _artist.Uri,
            };
            client.Player.ResumePlayback(request);
        }
    }

    private void OnFollowArtist()
    {
        var client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            FollowRequest request = new FollowRequest(FollowRequest.Type.Artist, new List<string>() { ArtistId });
            client.Follow.Follow(request);
        }
    }

    private void SetUIActive(bool isActive)
    {
        if (_artistName != null)
            _artistName.gameObject.SetActive(isActive);
        if (_icon != null)
            _icon.gameObject.SetActive(isActive);
        if (_playButton != null)
            _playButton.gameObject.SetActive(isActive);
        if (_followButton != null)
            _followButton.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Set the full artist and populate the UI
    /// </summary>
    /// <param name="artist"></param>
    public void SetArtist(FullArtist artist)
    {
        _artist = artist;

        UpdateUI();
    }
}
