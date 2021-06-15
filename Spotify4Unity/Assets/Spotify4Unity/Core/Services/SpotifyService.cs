using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// Enum of all implemented/supported authentification methods in S4U
/// </summary>
public enum AuthenticationType
{
    PKCE = 0,
}

/// <summary>
/// Central spotify service to manage authorization and accessing the SpotifyAPI.Web.SpotifyClient.
/// Uses DontDestroyOnLoad() and lasts for the lifecycle of the app. Use SpotifyService.Instance to access anywhere in code.
/// Call SpotifyService.StartService() or use AuthorizeUserOnStart to begin authorization.
/// </summary>
public class SpotifyService : SceneSingleton<SpotifyService>
{
    // Spotify Dashboard client id
    public string ClientID = "";
    // Should the service attempt to authorize the user on MonoBehaviour.Start()
    public bool AuthorizeUserOnStart = true;

    /// <summary>
    /// Selected method of authentification to use
    /// </summary>
    public AuthenticationType AuthType = AuthenticationType.PKCE;

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
    // Current authenticator type
    private IServiceAuthenticator _authenticator;

    private static SpotifyClientConfig _defaultConfig = SpotifyClientConfig.CreateDefault();

    /// <summary>
    /// List of actions to run on the main thread
    /// </summary>
    private List<Action> _dispatcher = new List<Action>();

    #region Mono Behaviour Methods

    protected virtual void Awake()
    {
        switch(AuthType)
        {
            case AuthenticationType.PKCE:
                _authenticator = this.gameObject.AddComponent<PKCE_Authentification>();
                _authenticator.Configure(new PKCE_Config()
                {
                    ClientID = ClientID,
                    APIScopes = S4UUtility.GetAllScopes(),
                });
                break;
            default:
                break;
        }

        if (_authenticator != null)
        {
            _authenticator.OnAuthenticatorComplete += this.OnAuthenticatorComplete;
        }
    }

    protected virtual void Start()
    {
        if (AuthorizeUserOnStart)
        {
            StartService();
        }
    }

    protected virtual void Update()
    {
        // Run any actions on main thread and clear once complete
        if (_dispatcher.Count > 0)
        {
            foreach(Action actn in _dispatcher)
            {
                actn.Invoke();
            }
            _dispatcher.Clear();
        }
    }

    #endregion

    /// <summary>
    /// Starts the service, either reusing previous authentification or gathering new authentification
    /// </summary>
    public void StartService()
    {
        if (_authenticator != null)
        {
            _authenticator.StartAuthentification();
        }
    }

    /// <summary>
    /// Begin to get authorization from the current user and connects to Spotify, creating a SpotifyClient
    /// </summary>
    public void AuthorizeUser()
    {
        // Dont need to authorize if already done
        if (_client != null)
            return;

        if (_authenticator != null)
        {
            _authenticator.StartAuthentification();
        }
    }

    /// <summary>
    /// Signs the current user out, removes any saved authorization and requires user to re-authorize next time
    /// </summary>
    public void DeauthorizeUser()
    {
        if (_client != null)
        {
            _client = null;
        }

        if (_authenticator != null)
        {
            _authenticator.RemoveAuthentification();
        }

        // Client no longer connected
        OnClientConnectionChanged?.Invoke(_client);
    }

    private void OnAuthenticatorComplete(IAuthenticator apiAuthenticator)
    {
        if (apiAuthenticator != null)
        {
            // Get config from authenticator
            _defaultConfig = SpotifyClientConfig.CreateDefault().WithAuthenticator(apiAuthenticator);

            // Create the Spotify client
            SpotifyClient client = new SpotifyClient(_defaultConfig);

            if (client != null)
            {
                _client = client;

                Action clientCompleteAction = () =>
                {
                    OnClientConnectionChanged?.Invoke(_client);
                    Debug.Log($"Successfully connected to Spotify using '{AuthType}' authentificiation");
                };

                // If authenticator completed on another thread, add event to dispatcher to run on main thread
                if (Thread.CurrentThread.IsBackground)
                {
                    _dispatcher.Add(clientCompleteAction);
                }
                else
                {
                    // Is main thread, invoke now
                    clientCompleteAction.Invoke();
                }
            }
            else
            {
                Debug.LogError("Unknown error creating SpotifyAPI client");
            }
        }
        else
        {
            Debug.LogError($"Authenticator '{AuthType}' is complete but not provided a valid authenticator");
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
