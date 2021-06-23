using SpotifyAPI.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SinglePlaylistController : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Image _icon;

    [SerializeField]
    private Text _nameText, _creatorText;

    [SerializeField]
    private Button _playPlaylistBtn;

    private SimplePlaylist _playlist;

    public void SetPlaylist(SimplePlaylist playlist)
    {
        _playlist = playlist;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_playlist != null)
        {
            if (_nameText != null)
            {
                _nameText.text = _playlist.Name;
            }

            if (_creatorText != null)
            {
                _creatorText.text = "By " + _playlist.Owner.DisplayName;
            }

            if (_icon != null)
            {
                SpotifyAPI.Web.Image image = S4UUtility.GetLowestResolutionImage(_playlist.Images);
                if(image != null)
                {
                    StartCoroutine(S4UUtility.LoadImageFromUrl(image.Url, (loadedSprite) =>
                    {
                        _icon.sprite = loadedSprite;
                    }));
                }
            }

            if (_playPlaylistBtn != null)
            {
                _playPlaylistBtn.onClick.AddListener(() =>
                {
                    SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

                    PlayerResumePlaybackRequest request = new PlayerResumePlaybackRequest
                    {
                        ContextUri = _playlist.Uri
                    };
                    client.Player.ResumePlayback(request);
                });
            }
        }
    }
}
