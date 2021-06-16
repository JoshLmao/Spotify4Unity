using UnityEngine;

[System.Serializable]
public class ClientCredentials_AuthConfig : AuthorizationConfig
{
    [Tooltip("Secret Id of your app, can be located in your Spotify dashboard. Don't have one? Don't have an id? Go here: https://developer.spotify.com/dashboard/")]
    public string ClientSecret = "";
}
