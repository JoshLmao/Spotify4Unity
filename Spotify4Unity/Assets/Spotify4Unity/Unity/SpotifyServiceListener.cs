using SpotifyAPI.Web;
using UnityEngine;

/// <summary>
/// Base class for listening to service specific events and overriding their callbacks
/// </summary>
public class SpotifyServiceListener : MonoBehaviour
{
    protected virtual void Awake()
    {
        // Get instance and listen to connected event
        SpotifyService.Instance.OnClientConnectionChanged += OnSpotifyConnectionChanged;
    }

    protected virtual void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        // Override me and take my client, i'll keep an ear out for any changes.
    }
}
