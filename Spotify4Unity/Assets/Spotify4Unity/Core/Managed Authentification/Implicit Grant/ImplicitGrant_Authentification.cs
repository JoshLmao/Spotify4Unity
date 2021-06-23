using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ImplicitGrant_Authentification : MonoBehaviour, IServiceAuthenticator
{
    public event Action<object> OnAuthenticatorComplete;

    // Current config for method
    private AuthorizationConfig _authConfig;

    // Last gained auth token
    private ImplictGrantResponse _lastAuthToken;

    // Server for local callback
    private static EmbedIOAuthServer _server;

    public void Configure(object config)
    {
        if (config is AuthorizationConfig authConfig)
        {
            _authConfig = authConfig;
        }
    }

    public bool HasPreviousAuthentification()
    {
        // Never has previous authentification.
        // Implicit grant requires user auth every time.
        return false;
    }

    public async void StartAuthentification()
    {
        // Validate config values
        if (_authConfig.RedirectUri == string.Empty && _authConfig.ServerPort <= 0)
        {
            // Problem with user's config, use default values
            _authConfig.RedirectUri = "http://localhost:5000/callback";
            _authConfig.ServerPort = 5000;
            return;
        }

        // Start server
        _server = new EmbedIOAuthServer(new Uri(_authConfig.RedirectUri), _authConfig.ServerPort);
        await _server.Start();

        // Await token recieved
        _server.ImplictGrantReceived += this.OnImplicitGrantReceived;

        // Create request
        LoginRequest request = new LoginRequest(_server.BaseUri, _authConfig.ClientID, LoginRequest.ResponseType.Token)
        {
            Scope = _authConfig.APIScopes,
        };
        BrowserUtil.Open(request.ToUri());
    }

    private async Task OnImplicitGrantReceived(object sender, ImplictGrantResponse response)
    {
        // Stop server
        await _server.Stop();

        _lastAuthToken = response;

        // Trigger complete with auth token
        OnAuthenticatorComplete?.Invoke(_lastAuthToken);
    }

    public void RemoveSavedAuth()
    {
        // N/A
    }

    public DateTime GetExpiryDateTime()
    {
        if (_lastAuthToken != null)
        {
            return S4UUtility.GetTokenExpiry(_lastAuthToken.CreatedAt, _lastAuthToken.ExpiresIn);
        }
        return DateTime.MinValue;
    }

    public void DeauthorizeUser()
    {
        if (_server != null)
        {
            _server.Dispose();
        }

        if (_lastAuthToken != null)
        {
            _lastAuthToken = null;
        }
    }
}
