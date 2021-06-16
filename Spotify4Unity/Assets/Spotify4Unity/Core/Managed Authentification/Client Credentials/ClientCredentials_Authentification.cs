using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientCredentials_Authorization : MonoBehaviour, IServiceAuthenticator
{
    public event Action<object> OnAuthenticatorComplete;

    private ClientCredentials_AuthConfig _authConfig;

    private IAuthenticator _ccAuthenticator;

    public void Configure(object config)
    {
        if (config is ClientCredentials_AuthConfig ccConfig)
        {
            _authConfig = ccConfig;
        }
    }

    public bool HasPreviousAuthentification()
    {
        return false;
    }

    public void StartAuthentification()
    {
        // Create CC authenticator to manage auth
        _ccAuthenticator = new ClientCredentialsAuthenticator(_authConfig.ClientID, _authConfig.ClientSecret);

        // Trigger complete
        OnAuthenticatorComplete?.Invoke(_ccAuthenticator);
    }

    public void RemoveAuthentification()
    {
        if (_ccAuthenticator != null)
        {
            _ccAuthenticator = null;
        }
    }
}
