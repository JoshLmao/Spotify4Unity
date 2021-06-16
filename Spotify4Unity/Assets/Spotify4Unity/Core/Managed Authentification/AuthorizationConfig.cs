using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base configurable variables for authorization methods
/// </summary>
[System.Serializable]
public class AuthorizationConfig : MonoBehaviour
{
    [Tooltip("Your client id, found in your Spotify Dashboard. Don't have an id? Go here: https://developer.spotify.com/dashboard/")]
    public string ClientID = "";

    [Tooltip("All API scopes required for your functionality. By default, all scopes will be selected. " +
        "You should only select the scopes you aim to use in your app. Users are more likely to accept authorization the less API scopes you use")]
    public List<string> APIScopes = S4UUtility.GetAllScopes();

    [Tooltip("The redirect uri used to pass Spotify authentification onto your app. This uri needs to be in your Spotify Dashboard. " +
        "Dont change this if you don't know what you are doing.")]
    public string RedirectUri = "http://localhost:5000/callback";

    [Tooltip("Port number to use for recieving Spotify auth from the browser. Should be the same value in your Redirect uri." +
        " Don't change this if you don't know what you are doing")]
    public int ServerPort = 5000;
}
