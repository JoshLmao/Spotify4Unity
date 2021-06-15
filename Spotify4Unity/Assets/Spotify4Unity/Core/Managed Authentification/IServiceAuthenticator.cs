using SpotifyAPI.Web;
using System;

/// <summary>
/// Interface for providing authentication to a Spotify4Unity Spotify Service
/// </summary>
public interface IServiceAuthenticator 
{
    // Triggered when authentification has been successfully loaded or retrieved from the user
    public event Action<IAuthenticator> OnAuthenticatorComplete;


    // Configures the authenticator with it's own custom config class
    void Configure(object config);

    // Gets the current SpotifyAPI.Web.IAuthenticator instance, can return null
    IAuthenticator GetAPIAuthenticator();

    // Check if previous authentification exists and loads into memory
    bool HasPreviousAuthentification();

    // Gets new authentification from this auth method. Either prompts user if no previous or init's the SpotifyAPI.Web.IAuthenticator
    void StartAuthentification();
    // Removes any previous authentification, either in memory or on system
    void RemoveAuthentification();

}
