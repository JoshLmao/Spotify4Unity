using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SpotifyPlayerListener : UIListener
{
    public float UpdateFrequencyMS = 1000;

    public event Action<FullTrack> OnTrackChanged;

    private SpotifyClient _client;

    private CurrentlyPlayingContext _currentContext;
    private FullTrack _currentTrack;

    protected override void OnSpotifyConnected(SpotifyClient client)
    {
        base.OnSpotifyConnected(client);

        _client = client;

        if (_client != null)
        {
            InvokeRepeating("FetchLatestPlayer", 0, UpdateFrequencyMS / 1000);
        }
    }

    private void OnDestroy()
    {
    }

    private async void FetchLatestPlayer()
    {
        if (_client != null)
        {
            // get the current context on this run
            CurrentlyPlayingContext newContext = await _client.Player.GetCurrentPlayback();

            // Check if not null and is a track
            if (newContext != null && newContext.Item.Type == ItemType.Track)
            {
                FullTrack itm = newContext.Item as FullTrack;

                // No previous track, set to newest track
                if (_currentTrack == null)
                {
                    _currentTrack = itm;
                    OnTrackChanged?.Invoke(itm);
                }
                else
                {
                    // Check if track name & artists aren't the same
                    if (_currentTrack.Name != itm.Name || IsArtistsChanged(_currentTrack.Artists, itm.Artists))
                    {
                        Debug.Log($"Context track changed: '{_currentTrack.Name}' -> '{itm.Name}'");
                        _currentTrack = itm;
                        OnTrackChanged?.Invoke(itm);
                    }
                }
            }

            _currentContext = newContext;
        }
    }

    /// <summary>
    /// Checks if the two lists of artists have any difference in names
    /// </summary>
    /// <param name="a">First list of artists</param>
    /// <param name="b">Second list of artists</param>
    /// <returns></returns>
    private bool IsArtistsChanged(List<SimpleArtist> a, List<SimpleArtist> b)
    {
        // If lists are different size, it's changed
        if (a.Count != b.Count)
            return true;

        // Iterate through equal length lists for name difference
        for(int i = 0; i < a.Count; i++)
        {
            if (a[i].Name != b[i].Name)
                return true;    // Name differs, changed
        }

        // No change
        return false;
    }

    protected CurrentlyPlayingContext GetCurrentContext()
    {
        return _currentContext;
    }
}
