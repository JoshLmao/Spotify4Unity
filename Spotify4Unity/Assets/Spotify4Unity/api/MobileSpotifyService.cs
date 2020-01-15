using Spotify4Unity.Dtos;
using Spotify4Unity.Events;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Spotify4Unity
{
    /// <summary>
    /// Service to use for mobile authorization and communication
    /// </summary>
    public class MobileSpotifyService : SpotifyServiceBase
    {
        /// <summary>
        /// All platforms supported by the Mobile Spotify Service
        /// </summary>
        public enum Platforms
        {
            /// <summary>
            /// Android platform
            /// </summary>
            Android,
            /// <summary>
            /// iOS platform
            /// </summary>
            iOS
        }

        /// <summary>
        /// The intended platform for the service to run on. It's important to set the correct 
        /// one and follow the setup instructions correctly
        /// </summary>
        public Platforms Platform = Platforms.Android;
        /// <summary>
        /// Full scheme name to use with the Android application
        /// </summary>
        public string Scheme = "myScheme";
        /// <summary>
        /// Suffix of the redirect url after scheme and "://"
        /// </summary>
        public string RedirectUrlSuffix = "oauth/callback/spotify/";
        /// <summary>
        /// Should the service prompt the user for reauthorization when the current token has exired?
        /// </summary>
        public bool AutoReauthorize = false;
        /// <summary>
        /// Full url authorization for debugging in Editor
        /// </summary>
        public string DebugURLAuthorization = null;

        /// <summary>
        /// Triggered when the current authorization has expired and is no longer valid
        /// </summary>
        public event Action OnAuthorizationExpired;

        /// <summary>
        /// Formatted link with the Scheme and Suffix to create a deep link
        /// </summary>
        protected string m_formattedDeepLink { get { return $"{Scheme}://{RedirectUrlSuffix}"; } }
        /// <summary>
        /// Determine if the code is currently being run in the Editor or on Android
        /// </summary>
        protected bool m_isInEditor { get { return Application.isEditor; } }

        private Coroutine m_expireRoutine = null;

        protected override void Start()
        {
            EventManager.AddListener<AuthorizationExpired>(OnAuthExpired);

            base.Start();

            if (!m_isInEditor && Platform == Platforms.iOS)
            {
                IiOSServiceBridge bridge = FindObjectsOfType<MonoBehaviour>().OfType<IiOSServiceBridge>().FirstOrDefault();
                if(bridge != null)
                {
                    bridge.Configure(this.gameObject.name, "URLOpened");
                    Analysis.Log("Configured iOS callback for Spotify Service", Analysis.LogLevel.All);
                }
                else
                {
                    Analysis.LogError("Unable to find the iOS Service Bridge. Make sure it is next to the MobileSpotifyService!", Analysis.LogLevel.Vital);
                }
            }
        }

        /// <summary>
        /// Make an attempt to authorize with Spotify. Returns a bool to represent if an attempt can/has been made
        /// </summary>
        /// <returns>If the attempt sucessfully managed to be attempted. Doesn't represent if the service is connected or not</returns>
        public override bool Connect()
        {
            if (string.IsNullOrEmpty(ClientId))
            {
                Analysis.Log("Missing ClientId in mobile service", Analysis.LogLevel.Vital);
                return false;
            }

            if (IsConnecting)
                return false;

            IsConnecting = true;

            if (m_isInEditor)
            {
                string args = GetArgs();
                bool succeeded = OnRecievedAuth(args);
                return succeeded;
            }
            else
            {
                bool canReuseAuth = ReuseAuth();
                if (!canReuseAuth)
                    BeginImplicitAuth(ClientId, m_formattedDeepLink);
            }
            return true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if(Platform == Platforms.Android)
            {
                if (hasFocus && IsConnecting)
                {
                    string urlArgs = GetArgs();
                    if (!string.IsNullOrEmpty(urlArgs))
                        OnRecievedAuth(urlArgs);
                    else
                        IsConnecting = false;
                }
            }
        }

        /// <summary>
        /// Entry method in recieveing and parsing auth from Deep Linking
        /// </summary>
        /// <param name="authArgs"></param>
        protected bool OnRecievedAuth(string authArgs)
        {
            if (string.IsNullOrEmpty(authArgs))
            {
                Analysis.LogWarning("Recieved empty authorization arguments, not proceeding", Analysis.LogLevel.Vital);
                return false;
            }

            ImplicitAuthDto auth = ParseAuth(authArgs);
            if (auth == null)
            {
                Analysis.LogError("Unable to get Mobile Authorization - User denied", Analysis.LogLevel.Vital);
                return false;
            }

            // Build the Spotify API token
            Token token = new Token()
            {
                AccessToken = auth.AccessToken,
                ExpiresIn = auth.ExpiresIn,
                TokenType = auth.TokenType,
                CreateDate = DateTime.Now,
            };

            Analysis.Log($"Recieved Inplicit authorization on Mobile, Expires at '{ExpireTime}'", Analysis.LogLevel.Vital);

            Configure(token);
            return true;
        }

        /// <summary>
        /// Entry point for parsing URL parameters from iOS
        /// </summary>
        /// <param name="url"></param>
        public void URLOpened(string url)
        {
            if (url.Contains(m_formattedDeepLink))
            {
                // Only get data past '#' character
                string data = new Uri(url).Fragment;
                if (!string.IsNullOrEmpty(data))
                    OnRecievedAuth(data);
                else
                    Analysis.LogError("Data (after '#') recieved from Spotify4UnityDelegate is empty", Analysis.LogLevel.Vital);
            }
        }

        /// <summary>
        /// Gets the arguments for the current platform
        /// </summary>
        /// <returns></returns>
        private string GetArgs()
        {
            string arguments = null;
            if(m_isInEditor)
            {
                if(string.IsNullOrEmpty(DebugURLAuthorization))
                {
                    Analysis.LogError("Can't authorize since no Spotify Debug parameters have been provided", Analysis.LogLevel.Vital);
                    return null;
                }

                string[] splitted = DebugURLAuthorization.Split('#');
                arguments = $"{m_formattedDeepLink}{splitted[1]}";
            }
            else
            {
                arguments = GetAndroidDataString();
            }

            if (!string.IsNullOrEmpty(arguments))
            {
                if (arguments != null && arguments.StartsWith(m_formattedDeepLink))
                    return arguments;
            }
            else
            {
                Analysis.LogWarning("Unable to get any application redirect arguments when expecting them", Analysis.LogLevel.Vital);
            }
            return null;
        }

        /// <summary>
        /// Begin the authorization process for Android (Implicit authorization)
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="redirectUrl"></param>
        private void BeginImplicitAuth(string clientId, string redirectUrl)
        {
            Analysis.Log($"Beginning Implicit Auth on {Platform}", Analysis.LogLevel.Vital);
            Application.OpenURL(GetAuthURL(clientId, redirectUrl));
        }

        /// <summary>
        /// Builds and returns the appropriate authorization URL for Spotify
        /// </summary>
        /// <param name="clientId">Your client id for Spotify</param>
        /// <param name="redirectUrl">Your Redirect URL for Spotify</param>
        /// <returns></returns>
        public string GetAuthURL(string clientId, string redirectUrl)
        {
            string responseType = "token";
            string redirectUri = Uri.EscapeDataString(redirectUrl);
            string scopes = GetEscapedScopes();
            string baseUrl = "https://accounts.spotify.com/authorize";
            string totalUrl = $"{baseUrl}?client_id={clientId}&response_type={responseType}&redirect_uri={redirectUri}&scope={scopes}";
            return totalUrl;
        }

        /// <summary>
        /// Parses the auth url
        /// </summary>
        /// <param name="urlAuth"></param>
        /// <returns></returns>
        private ImplicitAuthDto ParseAuth(string urlAuth)
        {
            string[] split = urlAuth.Split('&');
            if (split.Length <= 2)
            {
                return null;
            }

            string accessToken = split[0].Split('=')[1];
            string tokenType = split[1].Split('=')[1];
            string expiresIn = split[2].Split('=')[1];
            return new ImplicitAuthDto()
            {
                AccessToken = accessToken,
                TokenType = tokenType,
                ExpiresIn = double.Parse(expiresIn),
            };
        }

        protected override void GotAuth(Token token, AuthorizationCodeAuth auth = null)
        {
            base.GotAuth(token, auth);

            if (m_expireRoutine != null)
                m_expireRoutine = StartCoroutine(AwaitTokenExpire(token.ExpiresIn, token.CreateDate));
        }

        private IEnumerator AwaitTokenExpire(double expireSeconds, DateTime tokenCreationTime)
        {
            DateTime renewDateTime = tokenCreationTime.AddSeconds(expireSeconds);
            TimeSpan differenceTime = renewDateTime - DateTime.Now;

            Analysis.Log($"Waiting '{differenceTime.TotalSeconds}' seconds until reauthorizing Spotify authorization (Reauth at '{renewDateTime}')", Analysis.LogLevel.All);
            yield return new WaitForSeconds((float)differenceTime.TotalSeconds);

            EventManager.QueueEvent(new AuthorizationExpired(m_lastAuthToken));
            m_expireRoutine = null;
        }

        private void OnAuthExpired(AuthorizationExpired e)
        {
            OnAuthorizationExpired?.Invoke();

            if (AutoReauthorize)
            {
                Connect();
            }
        }

        /// <summary>
        /// Returns an escaped string of scopes to unclude in the URL
        /// </summary>
        /// <returns></returns>
        private string GetEscapedScopes()
        {
            string[] strScopes = Scopes.ToString().Split(',');
            string allScopes = "";
            for (int i = 0; i < strScopes.Length; i++)
            {
                var newStr = strScopes[i].Replace(" ", "");
                var split = System.Text.RegularExpressions.Regex.Split(newStr, @"(?<!^)(?=[A-Z])");
                allScopes += string.Join("-", split);

                if (i < strScopes.Length)
                    allScopes += " "; //Add white space between scopes
            }

            return Uri.EscapeDataString(allScopes.ToLower());
        }

        private string GetAndroidDataString()
        {
            string dataString = null;
            try
            {
                AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
                dataString = intent.Call<string>("getDataString");
            }
            catch (Exception e)
            {
                Analysis.LogError($"Unable to getDataString from UnityPlayer class on Android - '{e.ToString()}'", Analysis.LogLevel.Vital);
            }
            return dataString;
        }
    }
}