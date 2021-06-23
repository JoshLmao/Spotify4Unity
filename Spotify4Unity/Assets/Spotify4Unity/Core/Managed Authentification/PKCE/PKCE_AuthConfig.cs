using UnityEngine;

/// <summary>
/// Enum containing all method types used to save PKCE authentification
/// </summary>
public enum PKCETokenSaveType
{
    File = 0,
    PlayerPrefs = 1,
}

/// <summary>
/// All configurable parameters for the PKCE authorization method
/// </summary>
[System.Serializable]
public class PKCE_AuthConfig : AuthorizationConfig
{
    /// <summary>
    /// Length of the automatically generated verifier string. Value defaults to 100, only change if you know what you're doing
    /// </summary>
    public int Length = 100;
    /// <summary>
    /// Your own custom verifier string. If this is set, it takes priority over the length. Value defaults to empty, only change if you know what you're doing
    /// </summary>
    public string Verifier = "";

    /// <summary>
    /// Method to use for saving PKCE token.
    /// </summary>
    public PKCETokenSaveType TokenSaveType;
    /// <summary>
    /// If TokenSaveType is File, use this local path to store the PKCE authorization token
    /// </summary>
    public string TokenPath = "PKCE-credentials.json";
    /// <summary>
    /// If TokenSaveType is PlayerPrefs, store the current credentials in player preferences using this key
    /// </summary>
    public string PlayerPrefsKey = "PKCE-credentials";
}
