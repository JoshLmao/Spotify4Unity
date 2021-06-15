using System;
using UnityEngine;

[System.Serializable]
public class PKCE_Config : AuthConfig
{
    [Tooltip("Length of the automatically generated verifier string. Value defaults to 100, only change if you know what you're doing")]
    public int Length = 100;
    [Tooltip("Your own custom verifier string. If this is set, it takes priority over the length. Value defaults to empty, only change if you know what you're doing")]
    public string Verifier = "";

    [Tooltip("Local path file to store the PKCE authorization token")]
    public string TokenPath = "PKCE-credentials.json";
}
