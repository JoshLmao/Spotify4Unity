using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.IO;
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
        if (HasPreviousAuthentification())
        {
            _pkceToken = LoadPKCEToken(PKCEConfig?.TokenPath);
            if (_pkceToken != null)
            {
                // Check if loaded token isn't expired
                if (_pkceToken.IsExpired)
                {
                    // testing
                    Debug.LogError("PKCE Auth is expired");
                }

                SetAuthenticator(_pkceToken);
            }
        }
        else
        {
            GetFreshAuth();
        }
    }

    private void OnTokenRefreshed(object sender, PKCETokenResponse token)
    {
        _pkceToken = token;

        if (PKCEConfig != null)
        {
            string json = JsonConvert.SerializeObject(token);
            File.WriteAllText(PKCEConfig.TokenPath, json);
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

    public void RemoveAuthentification()
    {
        if (PKCEConfig != null && File.Exists(PKCEConfig.TokenPath))
        {
            File.Delete(PKCEConfig.TokenPath);
        }

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

    public IAuthenticator GetAPIAuthenticator()
    {
        return _pkceAuthenticator;
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
        _server.AuthorizationCodeReceived += async (sender, response) =>
        {
            await _server.Stop();

            _pkceToken = await new OAuthClient().RequestToken(
                new PKCETokenRequest(_clientID, response.Code, _server.BaseUri, verifier)
            );

            File.WriteAllText(PKCEConfig.TokenPath, JsonConvert.SerializeObject(_pkceToken));

            SetAuthenticator(_pkceToken);
        };

        LoginRequest request = new LoginRequest(_server.BaseUri, _clientID, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = PKCEConfig.APIScopes,
        };

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

    private void SetAuthenticator(PKCETokenResponse token)
    {
        // Set API authentification once recieved
        _pkceAuthenticator = new PKCEAuthenticator(_clientID, token);
        _pkceAuthenticator.TokenRefreshed += this.OnTokenRefreshed;

        OnAuthenticatorComplete?.Invoke(_pkceAuthenticator);
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
}
