using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Authorizer of the PKCE auth flow, implemented with IServiceAuthenticator for access/callbacks.
/// Should be added as a Unity component.
/// </summary>
public class PKCE_Authentification : MonoBehaviour, IServiceAuthenticator
{
    public event Action<IAuthenticator> OnAuthenticatorComplete;

    // Custom config for PKCE
    public PKCE_Config PKCEConfig;

    // Loaded client ID to use
    private string _clientID;
    
    // Current PKCE authorization token
    private PKCETokenResponse _pkceToken;

    // Current PKCE authenticator
    private PKCEAuthenticator _pkceAuthenticator;

    // Local EmbedIO authorization server
    private static readonly EmbedIOAuthServer _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

    public void Configure(object config)
    {
        if (config is AuthConfig authConfig)
        {
            _clientID = authConfig.ClientID;
        }

        if (config is PKCE_Config pkceConfig)
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
            _pkceToken = LoadPKCEToken(PKCEConfig?.TokenPath);
            if (_pkceToken != null)
            {
                // Set API authenticator
                SetAuthenticator(_pkceToken);

                // if not expired, output expire time
                if (!_pkceToken.IsExpired)
                {
                    DateTime expireDT = GetTokenExpireDT(_pkceToken.CreatedAt, _pkceToken.ExpiresIn);
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

    public void RemoveAuthentification()
    {
        // Delete any previous PKCE saved auth
        if (PKCEConfig != null && File.Exists(PKCEConfig.TokenPath))
        {
            File.Delete(PKCEConfig.TokenPath);
        }

        // Dispose server
        if (_server != null)
        {
            _server.Dispose();
        }

        _pkceToken = null;
    }

    public bool HasPreviousAuthentification()
    {
        if (PKCEConfig != null && File.Exists(PKCEConfig.TokenPath))
        {
            _pkceToken = LoadPKCEToken(PKCEConfig.TokenPath);
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
        _server.AuthorizationCodeReceived += (sender, response) => this.OnAuthCodeRecieved(sender, response, verifier);

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

            // Write to system locally, ready for next sign in
            File.WriteAllText(PKCEConfig.TokenPath, JsonConvert.SerializeObject(_pkceToken));

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
        DateTime expireDT = GetTokenExpireDT(token.CreatedAt, token.ExpiresIn);
        Debug.Log($"PKCE token refreshed | Expires at '{expireDT.ToLocalTime()}'");

        bool triggerEvent = _pkceToken.IsExpired && !token.IsExpired;
        _pkceToken = token;

        if (PKCEConfig != null)
        {
            string json = JsonConvert.SerializeObject(token);
            File.WriteAllText(PKCEConfig.TokenPath, json);
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
    /// <param name="tokenFilePath">File path of the saved token</param>
    /// <returns></returns>
    private PKCETokenResponse LoadPKCEToken(string tokenFilePath)
    {
        if (!string.IsNullOrEmpty(tokenFilePath))
        {
            string previousToken = File.ReadAllText(tokenFilePath);
            if (string.IsNullOrEmpty(previousToken))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<PKCETokenResponse>(previousToken);
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the current PKCE token used to authorize the service
    /// </summary>
    /// <returns></returns>
    public PKCETokenResponse GetPKCEToken()
    {
        return _pkceToken;
    }

    /// <summary>
    /// Gets the main API authenticator
    /// </summary>
    /// <returns></returns>
    public IAuthenticator GetAPIAuthenticator()
    {
        return _pkceAuthenticator;
    }

    private DateTime GetTokenExpireDT(DateTime createdAt, int expiresIn)
    {
        return createdAt.AddSeconds(expiresIn);
    }
}
