using Spotify4Unity.Events;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using SAPIModels = SpotifyAPI.Web.Models;

namespace Spotify4Unity
{
    /// <summary>
    /// Spotify Service for PC
    /// </summary>
    public class SpotifyService : SpotifyServiceBase
    {
        /// <summary>
        /// Event for running refresh token authorization on main thread
        /// </summary>
        private class RunRefreshToken : GameEventBase
        {
            /// <summary>
            /// The amount of seconds until the last auth expires
            /// </summary>
            public float ExpireSeconds { get; set; }
            /// <summary>
            /// The refresh token given from the last authorization
            /// </summary>
            public string RefreshToken { get; set; }
            /// <summary>
            /// The authorization code auth class
            /// </summary>
            public AuthorizationCodeAuth CodeAuth { get; set; }
            /// <summary>
            /// The time the tokens were created
            /// </summary>
            public DateTime CreationTime { get; set; }

            public RunRefreshToken(AuthorizationCodeAuth auth, float expireSeconds, DateTime creationTime, string refreshToken)
            {
                ExpireSeconds = expireSeconds;
                CodeAuth = auth;
                RefreshToken = refreshToken;
                CreationTime = creationTime;
            }
        }

        /// <summary>
        /// The secret ID for your app. Follow the same instructions as the ClientID to get it
        /// </summary>
        public string SecretId = "";
        /// <summary>
        /// The port to use when authenticating. Should be the same as your "Redirect URI" in your application's Spotify Dashboard
        /// </summary>
        public int ConnectionPort = 8000;

        private string m_refreshToken = null;

        private Coroutine m_timeoutRoutine = null;
        private Coroutine m_refreshTokenRoutine = null;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            EventManager.AddListener<RunRefreshToken>(OnRunRefresh);
            base.Start();
        }

        protected override void OnDestroy()
        {
            if (m_refreshTokenRoutine != null)
            {
                StopCoroutine(m_refreshTokenRoutine);
                m_refreshTokenRoutine = null;
            }
            if (m_timeoutRoutine != null)
            {
                StopCoroutine(m_timeoutRoutine);
                m_timeoutRoutine = null;
            }

            base.OnDestroy();
        }

        /// <summary>
        /// Make an attempt to authorize with Spotify. Returns a bool to represent if an attempt can/has been made
        /// </summary>
        /// <returns>If the attempt sucessfully managed to be attempted. Doesn't represent if the service is connected or not</returns>
        public override bool Connect()
        {
            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(SecretId))
            {
                Analysis.LogError("Can't start SpotifyAPI Connect, missing ClientID or SecretID", Analysis.LogLevel.Vital);
                return false;
            }

            if (IsConnecting)
            {
                Analysis.Log("Already attempting connection. Wait before trying to connect again", Analysis.LogLevel.Vital);
                return false;
            }

            IsConnecting = true;

            string redirectUrl = GetRedirectUrl(ConnectionPort);
            AuthorizationCodeAuth auth = new AuthorizationCodeAuth(ClientId, SecretId, redirectUrl, redirectUrl, Scopes);
            auth.AuthReceived += OnAuthorizationRecieved;

            bool canReuseAuth = ReuseAuth(auth);
            if(!canReuseAuth)
                ConnectSpotifyWebHelper(auth, ConnectionPort);
            return true;
        }


        /// <summary>
        /// Connectes to the WebHelper with your ClientId
        /// </summary>
        /// <param name="clientId">Custom client id</param>
        protected virtual void ConnectSpotifyWebHelper(AuthorizationCodeAuth auth, int port = 8000)
        {
            if (!IsConnected)
            {
                auth.Start();
                auth.OpenBrowser();

                if (m_timeoutRoutine == null)
                    m_timeoutRoutine = StartCoroutine(AwaitConnectionTimeout(ConnectionTimeout, auth));

                Analysis.Log("Awaiting authentification completion in browser", Analysis.LogLevel.Vital);
            }
        }

        private void OnAuthorizationRecieved(object sender, AuthorizationCode payload)
        {
            AuthorizationCodeAuth auth = sender as AuthorizationCodeAuth;
            auth.Stop();

            SAPIModels.Token token = auth.ExchangeCode(payload.Code).Result;
            Analysis.Log($"Gained the initial Spotify authorization at '{token.CreateDate}", Analysis.LogLevel.Vital);
            Configure(token, auth);
        }

        protected override void GotAuth(SAPIModels.Token token, AuthorizationCodeAuth auth)
        {
            base.GotAuth(token, auth);

            if (string.IsNullOrEmpty(m_refreshToken) && !string.IsNullOrEmpty(token.RefreshToken))
                m_refreshToken = token.RefreshToken;

            if (ReuseAuthTokens && auth != null && token != null)
                EventManager.QueueEvent(new RunRefreshToken(auth, (float)token.ExpiresIn, token.CreateDate, m_refreshToken));
        }

        /// <summary>
        /// Awaits the intial connection timeout if not recieved within given time
        /// </summary>
        /// <param name="timeoutSeconds"></param>
        /// <param name="auth"></param>
        /// <returns></returns>
        private System.Collections.IEnumerator AwaitConnectionTimeout(int timeoutSeconds, AuthorizationCodeAuth auth)
        {
            yield return new WaitForSeconds(timeoutSeconds);

            if (IsConnecting && !IsConnected)
            {
                Analysis.LogError("Failed to get authentification - Timed out", Analysis.LogLevel.Vital);

                auth.Stop();
                auth.AuthReceived -= OnAuthorizationRecieved;

                IsConnecting = false;
            }

            m_timeoutRoutine = null;
        }

        private void OnRunRefresh(RunRefreshToken e)
        {
            if(m_refreshTokenRoutine == null)
                m_refreshTokenRoutine = StartCoroutine(AwaitTokenRefresh(e.CodeAuth, e.ExpireSeconds, e.RefreshToken, e.CreationTime));
        }

        private System.Collections.IEnumerator AwaitTokenRefresh(AuthorizationCodeAuth auth, float expireSeconds, string refreshToken, DateTime tokenCreationTime)
        {
            DateTime renewDateTime = tokenCreationTime.AddSeconds(expireSeconds);
            TimeSpan differenceTime = renewDateTime - DateTime.Now;
            ///Remove some time at the end so we renew while the old tokens are valid
            TimeSpan margin = TimeSpan.FromSeconds(AUTH_TOKEN_MARGIN_SECONDS);
            differenceTime = differenceTime.Subtract(margin);

            Analysis.Log($"Waiting '{Math.Round(differenceTime.TotalSeconds, MidpointRounding.ToEven)}' seconds until refreshing Spotify authorization (Reauth at '{renewDateTime.Subtract(margin)}')", Analysis.LogLevel.All);
            yield return new WaitForSeconds((float)differenceTime.TotalSeconds);

            Task.Run(async () => await RefreshTokenAndConfigure(auth, refreshToken));
        }

        private async Task RefreshTokenAndConfigure(AuthorizationCodeAuth auth, string refreshToken)
        {
            SAPIModels.Token token = await auth.RefreshToken(refreshToken);
            // Inject RefreshToken back into after each re-auth
            token.RefreshToken = m_refreshToken;

            if (token != null && token.Error == null)
            {
                m_refreshTokenRoutine = null;
                Analysis.Log($"Obtained a new authorization token at '{token.CreateDate}'", Analysis.LogLevel.Vital);
                Configure(token, auth);
            }
            else
            {
                string errorMsg = token != null ? $"Error - {token.Error}" : "Token is null";
                Analysis.LogError($"Unable to refresh authorization token - {errorMsg}", Analysis.LogLevel.Vital);
            }
        }

        private string GetRedirectUrl(int port)
        {
            return $"http://localhost:{port}";
        }

        public override bool IsSpotifyClientOpen()
        {
            Process[] procs = Process.GetProcessesByName("Spotify");
            return procs != null && procs.Length > 0;
        }

        public override void OpenSpotifyClient()
        {
            Application.OpenURL("spotify://");
        }

        public override void CloseSpotifyClient()
        {
            Process[] procs = Process.GetProcessesByName("Spotify");
            if(procs != null && procs.Length > 0)
            {
                foreach(Process p in procs)
                {
                    p.Close();
                }
                Analysis.Log("Closed Spotify client", Analysis.LogLevel.All);
            }
        }
    }
}