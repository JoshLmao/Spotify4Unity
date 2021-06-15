using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpotifyAPI.Web;
using System.IO;
using Newtonsoft.Json;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;

public class SpotifyService : SceneSingleton<SpotifyService>
{
    // Spotify Dashboard client id
    public string ClientID;
    public string AuthPath = "PKCE-credentials.json";

    public event Action<SpotifyClient> OnClientConnected;

    private SpotifyClient _client;

    private static readonly EmbedIOAuthServer _server = new EmbedIOAuthServer(new Uri("http://localhost:5000/callback"), 5000);

    private void Start()
    {
        StartService();
    }

    private void StartService()
    {
        if (File.Exists(AuthPath))
        {
            string previousToken = File.ReadAllText(AuthPath);
            var token = JsonConvert.DeserializeObject<PKCETokenResponse>(previousToken);

            PKCEAuthenticator pkceAuthenticator = new PKCEAuthenticator(ClientID, token);
            pkceAuthenticator.TokenRefreshed += this.OnTokenRefreshed;

            SpotifyClientConfig config = SpotifyClientConfig.CreateDefault().WithAuthenticator(pkceAuthenticator);

            SpotifyClient client = new SpotifyClient(config);

            if (client != null)
            {
                _client = client;
                OnClientConnected?.Invoke(_client);

                Debug.Log("Successfully connected using PKCE authentification");
            }
            else
            {
                Debug.LogError("Error creating SpotifyAPI client!");
            }
        }
        else
        {
            StartAuthentication();
        }
    }

    private void OnTokenRefreshed(object sender, PKCETokenResponse token)
    {
        string json = JsonConvert.SerializeObject(token);
        File.WriteAllText(AuthPath, json);
    }

    private async void StartAuthentication()
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        await _server.Start();
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

    /// <summary>
    /// Gets the current SpotifyClient service from SpotifyAPI.NET. Can return null if not connected
    /// </summary>
    /// <returns></returns>
    public SpotifyClient GetSpotifyClient()
    {
        return _client;
    }
}
