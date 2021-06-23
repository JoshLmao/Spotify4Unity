using SpotifyAPI.Web;
using System;
using UnityEngine;

public class ClientCredentials_Authorization : MonoBehaviour, IServiceAuthenticator
{
    public event Action<object> OnAuthenticatorComplete;

    private ClientCredentials_AuthConfig _authConfig;

    private ClientCredentialsTokenResponse _token;

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

    public void RemoveSavedAuth()
    {
        // No saved auth, not valid
    }

    public DateTime GetExpiryDateTime()
    {
        if (_ccAuthenticator != null && _ccAuthenticator is ClientCredentialsAuthenticator ccAuthenticator)
        {
            return S4UUtility.GetTokenExpiry(ccAuthenticator.Token.CreatedAt, ccAuthenticator.Token.ExpiresIn);
        }
        return DateTime.MinValue;
    }

    public void DeauthorizeUser()
    {
        if (_ccAuthenticator != null)
        {
            _ccAuthenticator = null;
        }
    }
}
