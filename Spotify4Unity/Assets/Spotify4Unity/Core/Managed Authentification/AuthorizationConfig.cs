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
}
