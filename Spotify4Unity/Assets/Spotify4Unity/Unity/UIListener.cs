using SpotifyAPI.Web;
using UnityEngine;

/// <summary>
/// Base class for listening to service specific events and overriding their callbacks
/// </summary>
public class UIListener : MonoBehaviour
{
    protected virtual void Awake()
    {
        // Get instance and listen to connected event
        SpotifyService.Instance.OnClientConnected += OnSpotifyConnected;
    }

    protected virtual void OnSpotifyConnected(SpotifyClient client)
    {
    }
}
