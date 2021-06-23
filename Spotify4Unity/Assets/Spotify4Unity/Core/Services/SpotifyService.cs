using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// Enum of all implemented/supported authentification methods in S4U
/// </summary>
public enum AuthenticationType
{
    /// <summary>
    /// Proof Key for Code Exchange method.
    /// Authenticates once, saves to file locally, can be refreshed.
    /// </summary>
    PKCE = 0,

    /// <summary>
    /// Implicit grant method.
    /// Gets single token, lasts 60 minutes, cannot be refreshed
    /// </summary>
    ImplicitGrant = 1,

    /// <summary>
    /// Client Credentials method.
    /// Server-to-server authentification. Can't use scopes that access user information.
    /// </summary>
    ClientCredentials = 2,
}

/// <summary>
/// Central spotify service to manage authorization and accessing the SpotifyAPI.Web.SpotifyClient.
/// Uses DontDestroyOnLoad() and lasts for the lifecycle of the app. Use SpotifyService.Instance to access anywhere in code.
/// Call SpotifyService.StartService() or use AuthorizeUserOnStart to begin authorization.
/// </summary>
public class SpotifyService : SceneSingleton<SpotifyService>
{
    // Should the service attempt to authorize the user on MonoBehaviour.Start()
    public bool AuthorizeUserOnStart = true;

    /// <summary>
    /// Selected method of authentification to use
    /// </summary>
    public AuthenticationType AuthType = AuthenticationType.PKCE;

    [HideInInspector]
    public AuthorizationConfig _authMethodConfig;

    /// <summary>
    /// Is the service connected to Spotify with user authentification
    /// </summary>
    public bool IsConnected { get { return _client != null; } }

    /// <summary>
    /// Is the service started, either awaiting authorization or live and connected
    /// </summary>
    public bool IsStarted { get { return _authenticator != null; } }

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
        _authMethodConfig = this.GetComponent<AuthorizationConfig>();
        if (!_authMethodConfig)
        {
            Debug.LogError("No authorization config found on Spotify Service! Is the selected authorization method's config next to the service?");
            return;
        }

        StartService();
    }

    protected virtual void Start()
    {
        if (AuthorizeUserOnStart)
        {
            AuthorizeUser();
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
    /// Starts the service and prepares authentification ready for AuthorizeUser(). Option to prompt user for auth if prompAuth is true
    /// <param name="promptAuth">Should user be promped for auth?</param>
    /// </summary>
    public void StartService(bool promptAuth = false)
    {
        if (_authenticator == null)
        {
            switch (AuthType)
            {
                case AuthenticationType.PKCE:
                    _authenticator = this.gameObject.AddComponent<PKCE_Authentification>();
                    break;
                case AuthenticationType.ImplicitGrant:
                    _authenticator = this.gameObject.AddComponent<ImplicitGrant_Authentification>();
                    break;
                case AuthenticationType.ClientCredentials:
                    _authenticator = this.gameObject.AddComponent<ClientCredentials_Authorization>();
                    break;
                default:
                    break;
            }

            _authenticator.OnAuthenticatorComplete += this.OnAuthenticatorComplete;
        }

        if (_authenticator != null)
        {
            _authenticator.Configure(_authMethodConfig);

            if (promptAuth)
                AuthorizeUser();
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
    /// Signs the current user out. Requires AuthorizeUser() to authorize again.
    /// </summary>
    /// <param name="removeSavedAuth">Should any saved auth be removed</param>
    public void DeauthorizeUser(bool removeSavedAuth = false)
    {
        if (_client != null)
        {
            _client = null;
        }

        if (_authenticator != null)
        {
            _authenticator.DeauthorizeUser();

            if (removeSavedAuth)
                _authenticator.RemoveSavedAuth();
        }

        // Client no longer connected
        OnClientConnectionChanged?.Invoke(_client);
    }

    /// <summary>
    /// Deauthorizes the user if connected and cleans up service. Requires StartService() before being able to be used again
    /// </summary>
    /// <param name="removeAuth">Should any saved auth be removed, prompting the user for auth when service starts next</param>
    public void EndService(bool removeSavedAuth = false)
    {
        if (_authenticator != null)
        {
            if (IsConnected)
            {
                DeauthorizeUser(removeSavedAuth);
            }

            _authenticator.OnAuthenticatorComplete -= this.OnAuthenticatorComplete;

            Destroy(_authenticator as MonoBehaviour);

            _authenticator = null;
        }
    }

    private async void OnAuthenticatorComplete(object authObject)
    {
        if (authObject != null)
        {
            // Get config from authenticator
            if (authObject is IAuthenticator apiAuthenticator)
            {
                _defaultConfig = SpotifyClientConfig.CreateDefault().WithAuthenticator(apiAuthenticator);
            }
            else if (authObject is string authToken)
            {
                _defaultConfig = SpotifyClientConfig.CreateDefault().WithToken(authToken);
            }
            else
            {
                Debug.LogError("Auth complete object is unknown. Authentification failed.");
                return;
            }
            
            // Create the Spotify client
            _client = new SpotifyClient(_defaultConfig);

            if (_client != null)
            {
                // Make one test api request to validate/refresh auth
                await SendValidationRequest();

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

    /// <summary>
    /// Make a test api request to check client is working and to refresh auth if needed
    /// </summary>
    private async System.Threading.Tasks.Task SendValidationRequest()
    {
        if (_client != null)
        {
            try
            {
                var newReleases = await _client.Browse.GetNewReleases();
                if (newReleases != null)
                {
                    //Debug.Log("Confirmation request success!");
                    return;
                }
                else
                {
                    Debug.LogError("Confirmation request is null");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Confirmation request exception: {e.ToString()}");
            }
        }
    }

    /// <summary>
    /// Checks if the given scopes to see if they are authorized. Use SpotifyAPI.Web.Scopes to access each individual scope
    /// </summary>
    /// <param name="scopes">Scopes to check are authorized. </param>
    public bool AreScopesAuthorized(params string[] scopes)
    {
        // ToDo: Way to check given scopes against the current authorization.
        // This method assumes the APIScopes in config match the current authentification scopes

        foreach (string checkScope in scopes)
        {
            bool contains = _authMethodConfig.APIScopes.Contains(checkScope);
            // Check if requested scope is in api scopes
            if (!contains)
            {
                return false;
            }

            // if AuthType is ClientCredentials, can't access user-read-private and user-read-email
            if (AuthType == AuthenticationType.ClientCredentials)
            {
                if (checkScope == Scopes.UserReadPrivate || checkScope == Scopes.UserReadEmail || checkScope == Scopes.UserReadPlaybackState || checkScope == Scopes.PlaylistReadPrivate)
                {
                    return false;
                }
            }
        }

        // All passed contain check
        return true;
    }
    
    /// <summary>
    /// Gets the DateTime that the current authentification will expire. If you are using an authentication method that can automatically renew auth, then the authenticator will automatically do so.
    /// </summary>
    /// <returns></returns>
    public DateTime GetAuthExpiry()
    {
        if (_authenticator != null)
        {
            return _authenticator.GetExpiryDateTime();
        }
        return DateTime.MinValue;
    }
}
