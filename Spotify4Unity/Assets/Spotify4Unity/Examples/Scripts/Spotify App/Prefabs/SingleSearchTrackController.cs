using SpotifyAPI.Web;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleSearchTrackController : MonoBehaviour
{
    [SerializeField]
    private Text _name, _artist, _duration;

    [SerializeField]
    private Button _playBtn;

    private FullTrack _track;

    public void SetTrack(FullTrack t)
    {
        _track = t;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_name != null)
        {
            _name.text = _track.Name;
        }
        if (_artist != null)
        {
            _artist.text = S4UUtility.ArtistsToSeparatedString(", ", _track.Artists);
        }
        if (_duration != null)
        {
            _duration.text = S4UUtility.MsToTimeString(_track.DurationMs);
        }
        if (_playBtn != null)
        {
            _playBtn.onClick.AddListener(() =>
            {
                var client = SpotifyService.Instance.GetSpotifyClient();
                if (client != null)
                {
                    PlayerResumePlaybackRequest request = new PlayerResumePlaybackRequest()
                    {
                        //ContextUri = ,        // Play within context of just the individual track
                        Uris = new List<string>() { _track.Uri },
                    };
                    client.Player.ResumePlayback(request);

                    Debug.Log($"Spotify App | Playing searched song '{S4UUtility.ArtistsToSeparatedString(", ", _track.Artists)} - {_track.Name}'");
                }
            });
        }
    }
}
