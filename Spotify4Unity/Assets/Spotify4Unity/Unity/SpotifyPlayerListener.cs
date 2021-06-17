using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// Listener class for listening to the current playing context on Spotify 
/// and providing callbacks related to the context
/// </summary>
public class SpotifyPlayerListener : SpotifyServiceListener
{
    /// <summary>
    /// Amount of milliseconds for the internal player updater to poll at
    /// </summary>
    public float UpdateFrequencyMS = 1000;

    /// <summary>
    /// Triggered when a new Track or Episode is playing in the player
    /// </summary>
    public event Action<IPlayableItem> OnPlayingItemChanged;

    // Current connected spotify client
    private SpotifyClient _client;

    // The last retrieved context from API
    private CurrentlyPlayingContext _currentContext;
    // Current playing item within context
    private IPlayableItem _currentItem;
    // Is the internal update loop being invoked?
    private bool _isInvoking = false;

    protected override void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        _client = client;

        // Start internal update loop
        if (_client != null && UpdateFrequencyMS > 0)
        {
            if (SpotifyService.Instance.AreScopesAuthorized(Scopes.UserReadPlaybackState))
            {
                InvokeRepeating(nameof(FetchLatestPlayer), 0, UpdateFrequencyMS / 1000);
                _isInvoking = true;
            }
            else
            {
                Debug.LogError($"Not authorized to access '{Scopes.UserReadPlaybackState}'");
            }
        }
        else if (_client == null && _isInvoking)
        {
            CancelInvoke(nameof(FetchLatestPlayer));
            _isInvoking = false;

            // Invoke playing item changed, no more client, no more context
            OnPlayingItemChanged?.Invoke(null);
        }
    }

    private async void FetchLatestPlayer()
    {
        if (_client != null)
        {
            // get the current context on this run
            CurrentlyPlayingContext newContext = await _client.Player.GetCurrentPlayback();

            // Check if not null
            if (newContext != null)
            {
                if (newContext.Item != null)
                {
                    // Check and cast the item to the correct type
                    if (newContext.Item.Type == ItemType.Track)
                    {
                        FullTrack currentTrack = newContext.Item as FullTrack;

                        // No previous track or previous item was different type 
                        if (_currentItem == null || (_currentItem != null && _currentItem is FullEpisode episode))
                        {
                            Debug.Log($"No prev track or new type | -> '{S4UUtility.GetTrackString(currentTrack)}'");
                            _currentItem = currentTrack;
                            OnPlayingItemChanged?.Invoke(_currentItem);
                        }
                        else if (_currentItem != null && _currentItem is FullTrack lastTrack)
                        {
                            // Check if track name & artists aren't the same
                            if (lastTrack.Name != currentTrack.Name || IsArtistsChanged(lastTrack.Artists, currentTrack.Artists))
                            {
                                Debug.Log($"Track to new Track | '{S4UUtility.GetTrackString(lastTrack)}' -> '{S4UUtility.GetTrackString(currentTrack)}'");
                                _currentItem = currentTrack;
                                OnPlayingItemChanged?.Invoke(_currentItem);
                            }
                        }
                    }
                    else if (newContext.Item.Type == ItemType.Episode)
                    {
                        FullEpisode currentEpisode = newContext.Item as FullEpisode;

                        // If no previous item or current item is different type
                        if (_currentItem == null || (_currentItem != null && _currentItem is FullTrack track))
                        {
                            Debug.Log($"No prev episode or new type | -> '{currentEpisode.Show.Publisher} {currentEpisode.Name}'");
                            _currentItem = currentEpisode;
                            OnPlayingItemChanged?.Invoke(_currentItem);
                        }
                        else if (_currentItem != null && _currentItem is FullEpisode lastEpisode)
                        {
                            if (lastEpisode.Name != currentEpisode.Name || lastEpisode.Show?.Publisher != currentEpisode.Show?.Publisher)
                            {
                                Debug.Log($"Episode to new Episode | '{lastEpisode.Show.Publisher} {lastEpisode.Name}' -> '{currentEpisode.Show.Publisher} {currentEpisode.Name}'");
                                _currentItem = currentEpisode;
                                OnPlayingItemChanged?.Invoke(_currentItem);
                            }
                        }
                    }
                }
                else
                {
                    if (_currentItem != null)
                    {
                        Debug.Log($"Item to no item | '{(_currentItem.Type == ItemType.Track ? (_currentItem as FullTrack).Name : (_currentItem as FullEpisode).Name )}' -> ?");
                        _currentItem = null;
                        OnPlayingItemChanged?.Invoke(null);
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

    /// <summary>
    /// Gets the current context of the spotify player
    /// </summary>
    /// <returns></returns>
    protected CurrentlyPlayingContext GetCurrentContext()
    {
        return _currentContext;
    }
}
