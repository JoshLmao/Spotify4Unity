using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PKCEConfig
{
    [Tooltip("Length of the automatically generated verifier string. Value defaults to 100, only change if you know what you're doing")]
    public int Length = 100;
    [Tooltip("Your own custom verifier string. If this is set, it takes priority over the length. Value defaults to empty, only change if you know what you're doing")]
    public string Verifier = "";
}

public class SpotifyService : SceneSingleton<SpotifyService>
{
    // Spotify Dashboard client id
    public string ClientID = "";
    // Local path file to store the PKCE authorization token
    public string AuthPath = "PKCE-credentials.json";
    // Should the service attempt to authorize the user on MonoBehaviour.Start()
    public bool AuthorizeUserOnStart = true;
    // Custom config for PKCE
    public PKCEConfig PKCEConfig;

    /// <summary>
    /// Is the service connected to Spotify with user authentification
    /// </summary>
    public bool IsConnected { get { return _client != null; } }

    /// <summary>
    /// Triggered when the SpotifyService changes connection. For example, losing user authentification, calling DeauthorizeUser().
    /// </summary>
    public event Action<SpotifyClient> OnClientConnectionChanged;

    // Current SpotifyClient
    private SpotifyClient _client;

    // Local EmbedIO authorization server
    private static readonly EmbedIOAuthServer _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

    // Current PKCE authorization token
    private PKCETokenResponse _pkceToken;

    protected virtual void Start()
    {
        if (AuthorizeUserOnStart)
        {
            StartService();
        }
    }

    /// <summary>
    /// Starts the service, either reusing previous authentification or gathering new authentification
    /// </summary>
    public void StartService()
    {
        BeginService();
    }

    /// <summary>
    /// Begin to get authorization from the current user and connects to Spotify, creating a SpotifyClient
    /// </summary>
    public void AuthorizeUser()
    {
        // Dont need to authorize if already done
        if (_pkceToken != null && _client != null)
            return;

        StartAuthentication();
    }

    /// <summary>
    /// Signs the current user out, removes any saved authorization and requires user to re-authorize next time
    /// </summary>
    public void DeauthorizeUser()
    {
        RemoveAuthorization();
    }

    private void BeginService()
    {
        // Attempt to load a previous PKCE token
        _pkceToken = LoadPKCEToken();
        if (_pkceToken != null)
        {
            // Check if loaded token isn't expired
            if (_pkceToken.IsExpired)
            {
                Debug.LogError("PKCE Auth is expired. Getting new auth");
                _pkceToken = null;

                StartAuthentication();
            }
            else
            {
                PKCEAuthenticator pkceAuthenticator = new PKCEAuthenticator(ClientID, _pkceToken);
                pkceAuthenticator.TokenRefreshed += this.OnTokenRefreshed;

                SpotifyClientConfig config = SpotifyClientConfig.CreateDefault().WithAuthenticator(pkceAuthenticator);

                SpotifyClient client = new SpotifyClient(config);

                if (client != null)
                {
                    _client = client;
                    OnClientConnectionChanged?.Invoke(_client);

                    Debug.Log("Successfully connected using PKCE authentification");
                }
                else
                {
                    Debug.LogError("Error creating SpotifyAPI client!");
                }
            }
        }
        else
        {
            StartAuthentication();
        }
    }

    private void OnTokenRefreshed(object sender, PKCETokenResponse token)
    {
        _pkceToken = token;

        string json = JsonConvert.SerializeObject(token);
        File.WriteAllText(AuthPath, json);
    }

    private async void StartAuthentication()
    {
        // Load PKCE verifier/challenge with any config changes
        var (verifier, challenge) = LoadCustomPKCEProperties();

        await _server.Start();

        // On auth is recieved, save and start service
        _server.AuthorizationCodeReceived += async (sender, response) =>
        {
            await _server.Stop();
            PKCETokenResponse token = await new OAuthClient().RequestToken(
                new PKCETokenRequest(ClientID, response.Code, _server.BaseUri, verifier)
            );

            File.WriteAllText(AuthPath, JsonConvert.SerializeObject(token));

            StartService();
        };

        LoginRequest request = new LoginRequest(_server.BaseUri, ClientID, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string> { Scopes.UserReadEmail, Scopes.UserReadPrivate, Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative }
        };

        Uri uri = request.ToUri();
        try
        {
            BrowserUtil.Open(uri);
        }
        catch(Exception e)
        {
            Debug.LogError($"Exception opening browser for auth: '{e.ToString()}'");
        }
    }

    private void RemoveAuthorization()
    {
        if (_client != null)
        {
            _client = null;
        }

        if (_server != null)
        {
            _server.Dispose();
        }

        if (File.Exists(AuthPath))
        {
            File.Delete(AuthPath);
        }

        _pkceToken = null;

        // Client no longer connected
        OnClientConnectionChanged?.Invoke(_client);
    }

    /// <summary>
    /// Gets the current SpotifyClient service from SpotifyAPI.NET. Can return null if not connected
    /// </summary>
    /// <returns></returns>
    public SpotifyClient GetSpotifyClient()
    {
        return _client;
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
    /// Check if the current user has previous authentification and is valid and returns the current token
    /// </summary>
    /// <returns></returns>
    public bool HadPreviousAuthentification()
    {
        if (File.Exists(AuthPath))
        {
            PKCETokenResponse token = LoadPKCEToken();
            return token != null && !token.IsExpired;
        }
        return false;
    }

    private PKCETokenResponse LoadPKCEToken()
    {
        string previousToken = File.ReadAllText(AuthPath);
        if (string.IsNullOrEmpty(previousToken))
        {
            return null;
        }
        else
        {
            return JsonConvert.DeserializeObject<PKCETokenResponse>(previousToken);
        }
    }

    private (string, string) LoadCustomPKCEProperties()
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
}
