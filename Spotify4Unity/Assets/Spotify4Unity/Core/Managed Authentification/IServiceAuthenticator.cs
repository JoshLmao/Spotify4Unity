using SpotifyAPI.Web;
using System;

/// <summary>
/// Interface for providing authentication to a Spotify4Unity Spotify Service
/// </summary>
public interface IServiceAuthenticator 
{
    // Triggered when authentification has been successfully loaded or retrieved from the user
    public event Action<object> OnAuthenticatorComplete;


    // Configures the authenticator with it's own custom config class
    void Configure(object config);

    // Check if previous authentification exists and loads into memory
    bool HasPreviousAuthentification();

    // Gets new authentification from this auth method. Either prompts user if no previous or init's the SpotifyAPI.Web.IAuthenticator
    void StartAuthentification();
    // Deauthorizes the current user, removing any auth in memory.
    void DeauthorizeUser();
    // Removes any previous authentification, either in memory or on system
    void RemoveSavedAuth();

    // Gets the DateTime of when the authorization will expire
    DateTime GetExpiryDateTime();

}
