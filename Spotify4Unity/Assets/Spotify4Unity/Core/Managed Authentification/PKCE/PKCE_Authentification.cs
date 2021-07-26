using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Authorizer of the PKCE auth flow, implemented with IServiceAuthenticator for access/callbacks.
/// Should be added as a Unity component.
/// </summary>
public class PKCE_Authentification : MonoBehaviour, IServiceAuthenticator
{
    public event Action<object> OnAuthenticatorComplete;

    // Custom config for PKCE
    public PKCE_AuthConfig PKCEConfig;

    // Loaded client ID to use
    private string _clientID;
    
    // Current PKCE authorization token
    private PKCETokenResponse _pkceToken;

    // Current PKCE authenticator
    private PKCEAuthenticator _pkceAuthenticator;

    // Local EmbedIO authorization server
    private static EmbedIOAuthServer _server;

    private List<Action> _dispatcher = new List<Action>();

    private void Update()
    {
        if (_dispatcher.Count > 0)
        {
            foreach (Action actn in _dispatcher)
                actn.Invoke();

            _dispatcher.Clear();
        }
    }

    public void Configure(object config)
    {
        if (config is AuthorizationConfig authConfig)
        {
            _clientID = authConfig.ClientID;

            // Start server with config values
            _server = new EmbedIOAuthServer(new Uri(authConfig.RedirectUri), authConfig.ServerPort);
        }

        if (config is PKCE_AuthConfig pkceConfig)
        {
            PKCEConfig = pkceConfig;
        }
    }

    public void StartAuthentification()
    {
        // Check if previous auth
        if (HasPreviousAuthentification())
        {
            // Load local pkce saved token
            _pkceToken = LoadPKCEToken();
            if (_pkceToken != null)
            {
                // Set API authenticator
                SetAuthenticator(_pkceToken);

                // if not expired, output expire time
                if (!_pkceToken.IsExpired)
                {
                    DateTime expireDT = S4UUtility.GetTokenExpiry(_pkceToken.CreatedAt, _pkceToken.ExpiresIn);
                    Debug.Log($"PKCE token loaded | Expires at '{expireDT.ToLocalTime()}'");
                }
            }
        }
        else
        {
            // No previous auth, first time, get new
            GetFreshAuth();
        }
    }

    public void DeauthorizeUser()
    {
        // Dispose server
        if (_server != null)
        {
            _server.Dispose();
        }

        _pkceToken = null;
    }

    public void RemoveSavedAuth()
    {
        // Delete any previous PKCE saved auth
        if (PKCEConfig != null)
        {
            // Delete local file is set & exists
            if (!string.IsNullOrEmpty(PKCEConfig.TokenPath) && File.Exists(PKCEConfig.TokenPath))
            {
                File.Delete(PKCEConfig.TokenPath);
            }

            // Delete player prefs if set & exists
            if (!string.IsNullOrEmpty(PKCEConfig.PlayerPrefsKey) && !string.IsNullOrEmpty(PlayerPrefs.GetString(PKCEConfig.PlayerPrefsKey)))
            {
                PlayerPrefs.SetString(PKCEConfig.PlayerPrefsKey, string.Empty);
            }
        }
    }

    public bool HasPreviousAuthentification()
    {
        if (PKCEConfig != null)
        {
            _pkceToken = LoadPKCEToken();
            return _pkceToken != null;
        }
        return false;
    }

    private async void GetFreshAuth()
    {
        if (PKCEConfig == null)
        {
            return;
        }

        // Load PKCE verifier/challenge with any config changes
        var (verifier, challenge) = LoadConfigPKCECodes();

        await _server.Start();

        // On auth is recieved, save and start service
        try
        {
            _server.AuthorizationCodeReceived += (sender, response) => this.OnAuthCodeRecieved(sender, response, verifier);
        }
        catch(Exception e)
        {
            Debug.LogError(e.ToString());
        }

        // Create login request
        LoginRequest request = new LoginRequest(_server.BaseUri, _clientID, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = PKCEConfig.APIScopes,
        };

        // Build Uri and open in browser
        Uri uri = request.ToUri();
        try
        {
            BrowserUtil.Open(uri);
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception opening browser for auth: '{e.ToString()}'");
        }
    }

    private async Task OnAuthCodeRecieved(object sender, AuthorizationCodeResponse response, string verifier)
    {
        // Check response and & is valid
        if (response != null && !string.IsNullOrEmpty(response.Code))
        {
            await _server.Stop();

            _pkceToken = await new OAuthClient().RequestToken(
                new PKCETokenRequest(_clientID, response.Code, _server.BaseUri, verifier)
            );

            // Save PKCE token first
            SavePKCEToken(_pkceToken);

            Debug.Log("PKCE: Recieved Auth Code");
            SetAuthenticator(_pkceToken);
        }
    }

    private void SetAuthenticator(PKCETokenResponse token)
    {
        // Set API authentification once recieved
        _pkceAuthenticator = new PKCEAuthenticator(_clientID, token);
        _pkceAuthenticator.TokenRefreshed += this.OnTokenRefreshed;

        OnAuthenticatorComplete?.Invoke(_pkceAuthenticator);
    }

    private void OnTokenRefreshed(object sender, PKCETokenResponse token)
    {
        DateTime expireDT = S4UUtility.GetTokenExpiry(token.CreatedAt, token.ExpiresIn);
        Debug.Log($"PKCE token refreshed | Expires at '{expireDT.ToLocalTime()}'");

        bool triggerEvent = _pkceToken.IsExpired && !token.IsExpired;
        _pkceToken = token;

        if (PKCEConfig != null)
        {
            SavePKCEToken(_pkceToken);
        }

        if (triggerEvent)
        {
            Debug.Log("PKCE: Success in refreshing expired token into new token");
            OnAuthenticatorComplete?.Invoke(_pkceAuthenticator);
        }
    }

    /// <summary>
    /// Loads the PKCE validator and challenge strings with PKCEConfig properties
    /// </summary>
    /// <returns></returns>
    private (string, string) LoadConfigPKCECodes()
    {
        if (PKCEConfig != null)
        {
            // Check config for a custom verifier
            if (!string.IsNullOrEmpty(PKCEConfig.Verifier))
            {
                return PKCEUtil.GenerateCodes(PKCEConfig.Verifier);
            }
            else if (PKCEConfig.Length > 0) // or use custom length
            {
                return PKCEUtil.GenerateCodes(PKCEConfig.Length);
            }
        }

        // Default values
        return PKCEUtil.GenerateCodes();
    }

    /// <summary>
    /// Attempts to load the previous saved PKCE token from a given file path
    /// </summary>
    /// <returns></returns>
    private PKCETokenResponse LoadPKCEToken()
    {
        if (PKCEConfig.TokenSaveType == PKCETokenSaveType.File)
        {
            // Load token from File
            if (!string.IsNullOrEmpty(PKCEConfig.TokenPath))
            {
                if (!File.Exists(PKCEConfig.TokenPath))
                {
                    return null;
                }
                string previousToken = File.ReadAllText(PKCEConfig.TokenPath);
                if (string.IsNullOrEmpty(previousToken))
                {
                    return null;
                }
                else
                {
                    return JsonConvert.DeserializeObject<PKCETokenResponse>(previousToken);
                }
            }
        }
        else if (PKCEConfig.TokenSaveType == PKCETokenSaveType.PlayerPrefs)
        {
            // Load token from PlayerPrefs
            string tokenStr = PlayerPrefs.GetString(PKCEConfig.PlayerPrefsKey);
            if (string.IsNullOrEmpty(tokenStr))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<PKCETokenResponse>(tokenStr);
            }
        }

        return null;
    }

    /// <summary>
    /// Saves the PKCE token using the current PKECTokenSaveType in config
    /// </summary>
    /// <param name="token">Current token to save</param>
    private void SavePKCEToken(PKCETokenResponse token)
    {
        if (token != null)
        {
            string json = JsonConvert.SerializeObject(token);
            if (PKCEConfig.TokenSaveType == PKCETokenSaveType.File)
            {
                File.WriteAllText(PKCEConfig.TokenPath, json);
            }
            else if (PKCEConfig.TokenSaveType == PKCETokenSaveType.PlayerPrefs)
            {
                // Save to player prefs on main thread
                _dispatcher.Add(() =>
                {
                    PlayerPrefs.SetString(PKCEConfig.PlayerPrefsKey, json);
                });
            }
        }
    }

    /// <summary>
    /// Gets the current PKCE token used to authorize the service
    /// </summary>
    /// <returns></returns>
    public PKCETokenResponse GetPKCEToken()
    {
        return _pkceToken;
    }

    public DateTime GetExpiryDateTime()
    {
        if (_pkceToken != null)
        {
            return S4UUtility.GetTokenExpiry(_pkceToken.CreatedAt, _pkceToken.ExpiresIn);
        }
        return DateTime.MinValue;
    }
}
