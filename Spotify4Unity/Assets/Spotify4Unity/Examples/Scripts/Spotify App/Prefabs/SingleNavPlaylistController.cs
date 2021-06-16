using SpotifyAPI.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleNavPlaylistController : MonoBehaviour
{
    public event System.Action<SimplePlaylist> OnPlaylistSelected;

    [SerializeField]
    Button _selectPlaylistBtn;

    [SerializeField]
    Text _playlistNameText;

    private SimplePlaylist _playlist;

    private void Start()
    {
    }

    public void SetPlaylist(SimplePlaylist p)
    {
        _playlist = p;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_playlist != null)
        {
            if (_playlistNameText != null)
            {
                _playlistNameText.text = _playlist.Name;
            }
            if (_selectPlaylistBtn != null)
            {
                _selectPlaylistBtn.onClick.AddListener(() => OnPlaylistSelected?.Invoke(_playlist));
            }
        }
    }
}
