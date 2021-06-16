using UnityEngine;

/// <summary>
/// All configurable parameters for the PKCE authorization method
/// </summary>
[System.Serializable]
public class PKCE_AuthConfig : AuthorizationConfig
{
    [Tooltip("Length of the automatically generated verifier string. Value defaults to 100, only change if you know what you're doing")]
    public int Length = 100;
    [Tooltip("Your own custom verifier string. If this is set, it takes priority over the length. Value defaults to empty, only change if you know what you're doing")]
    public string Verifier = "";

    [Tooltip("Local path to a file to store the PKCE authorization token. Token is able to persist if saved to a file.")]
    public string TokenPath = "PKCE-credentials.json";
}
