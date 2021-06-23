using SpotifyAPI.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Artist widget for taking an artist's Spotify Id (not URI) and displaying their information
/// This is an example script of how/what you could/should implement
/// </summary>
public class ArtistWidgetController : SpotifyServiceListener
{
    [Tooltip("Id of the artist to display in thw widget. ")]
    public string ArtistId;

    [SerializeField]
    private UnityEngine.UI.Image _icon;

    [SerializeField]
    private Button _followBtn;

    [SerializeField]
    private Text _nameText, _idText, _uriText, _followersText, _genresText, _popularityText, _typeText;

    private FullArtist _artistInfo;

    protected override async void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        if (client != null)
        {
            _artistInfo = await client.Artists.Get(ArtistId);
        }
        else
        {
            _artistInfo = null;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_artistInfo != null)
        {
            DownloadUpdateSprite(_icon, _artistInfo.Images);

            UpdateTextElement(_nameText, $"Name: {_artistInfo.Name}");
            UpdateTextElement(_idText, $"Id: {_artistInfo.Id}");
            UpdateTextElement(_uriText, $"URI: {_artistInfo.Uri}");
            UpdateTextElement(_followersText, $"Followers: {_artistInfo.Followers.Total.ToString()}");
            UpdateTextElement(_genresText, $"Genres: {string.Join(", ", _artistInfo.Genres.ToArray())}");
            UpdateTextElement(_popularityText, $"Popularity: {_artistInfo.Popularity}");
            UpdateTextElement(_typeText, $"Type: {_artistInfo.Type}");
        }
        else
        {
            UpdateTextElement(_nameText, string.Empty);
            UpdateTextElement(_idText, string.Empty);
            UpdateTextElement(_uriText, string.Empty);
            UpdateTextElement(_followersText, string.Empty);
            UpdateTextElement(_genresText, string.Empty);
            UpdateTextElement(_popularityText, string.Empty);
            UpdateTextElement(_typeText, string.Empty);
            _icon.sprite = null;
        }

        // If follow btn set, add listener to on click to follow the current artist
        if (_followBtn != null)
        {
            _followBtn.onClick.AddListener(() =>
            {
                SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
                if (client != null)
                {
                    List<string> allArtistIdsList = new List<string>() { ArtistId };
                    FollowRequest followRequest = new FollowRequest(FollowRequest.Type.Artist, allArtistIdsList);
                    client.Follow.Follow(followRequest);
                }
            });
        }
    }

    private void UpdateTextElement(Text element, string content)
    {
        if (element != null)
        {
            element.text = content;
        }
    }

    private void DownloadUpdateSprite(Image img, List<SpotifyAPI.Web.Image> images)
    {
        if (img != null && img.sprite == null)
        {
            SpotifyAPI.Web.Image icon = S4UUtility.GetLowestResolutionImage(images);
            if (icon != null)
            {
                StartCoroutine(S4UUtility.LoadImageFromUrl(icon.Url, (loadedSprite) =>
                {
                    _icon.sprite = loadedSprite;
                }));
            }
        }
    }
}
