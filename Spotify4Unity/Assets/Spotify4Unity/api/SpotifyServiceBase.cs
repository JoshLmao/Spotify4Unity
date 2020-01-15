using Spotify4Unity.Dtos;
using Spotify4Unity.Enums;
using Spotify4Unity.Events;
using Spotify4Unity.Helpers;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using SAPIModels = SpotifyAPI.Web.Models;

namespace Spotify4Unity
{
    /// <summary>
    /// Constant class service for controlling and retrieving information to/from Spotify. For more info, check out the public documentation at https://github.com/JoshLmao/Spotify4Unity/wiki
    /// </summary>
    public class SpotifyServiceBase : MonoBehaviour
    {
        /// <summary>
        /// The current playing local context
        /// </summary>
        private class LocalContext
        {
            public string ContextUri { get; set; }
            public string TrackUri { get; set; }
            public LocalContext(string trackUri, string contextUri)
            {
                ContextUri = contextUri;
                TrackUri = trackUri;
            }
        }

        /// <summary>
        /// Your Client ID for your app.
        /// You must register your application to use the Spotify API online at https://developer.spotify.com/documentation/general/guides/app-settings/#register-your-app
        /// </summary>
        public string ClientId = "";
        /// <summary>
        /// Amount of seconds to wait for authentification before failing
        /// </summary>
        public int ConnectionTimeout = 60;
        /// <summary>
        ///Should the control automatically connect to Spotify when not connected?
        /// </summary>
        public bool AutoConnect = false;
        /// <summary>
        /// The amount of milliseconds to update the internal loop for updating current track/album/artist/etc. 
        /// WARNING: Making the update loop quicker can cause more API calls which will require re-auth to the API
        /// </summary>
        public int UpdateFrequencyMs = 1000;
        /// <summary>
        /// Amount of debug information that should be shown
        /// </summary>
        public Analysis.LogLevel LogLevel = Analysis.LogLevel.Vital;
        /// <summary>
        /// Should the service save and reuse old valid authentification token if it's still valid
        /// </summary>
        public bool ReuseAuthTokens = true;
        /// <summary>
        /// All scopes of access to the Spotify Web API
        /// </summary>
        public Scope Scopes = Scope.PlaylistReadPrivate
                                | Scope.PlaylistReadCollaborative
                                | Scope.PlaylistModifyPublic
                                | Scope.PlaylistModifyPrivate
                                | Scope.PlaylistReadPrivate
                                | Scope.Streaming
                                | Scope.UserReadPrivate
                                | Scope.UserReadEmail
                                | Scope.UserLibraryRead
                                | Scope.UserLibraryModify
                                | Scope.UserFollowModify
                                | Scope.UserFollowRead
                                | Scope.UserTopRead
                                | Scope.PlaylistReadCollaborative
                                | Scope.UserReadRecentlyPlayed
                                | Scope.UserReadPlaybackState
                                | Scope.UserModifyPlaybackState
                                | Scope.UserReadCurrentlyPlaying;

        private bool m_isConnecting = false;
        /// <summary>
        /// Is the service currently trying to connect and get authorization?
        /// </summary>
        public bool IsConnecting
        {
            get { return m_isConnecting; }
            set
            {
                m_isConnecting = value;
                EventManager.QueueEvent(new ConnectingChanged(m_isConnecting));
            }
        }
        /// <summary>
        /// Is Spotify currently playing music?
        /// </summary>
        public bool IsPlaying = false;
        /// <summary>
        /// Are we connected to Spotify and able to control it
        /// </summary>
        public bool IsConnected = false;
        /// <summary>
        /// Is the sounds from Spotify muted
        /// </summary>
        public bool IsMuted = false;
        /// <summary>
        /// All tracks saved to the users Spotify library
        /// </summary>
        public List<Track> SavedTracks = null;
        /// <summary>
        /// The current state of shuffle
        /// </summary>
        public Shuffle ShuffleState = Shuffle.Disabled;
        /// <summary>
        /// Current state of repeat
        /// </summary>
        public Repeat RepeatState = Repeat.Disabled;
        /// <summary>
        /// All playlists created and saved by the user
        /// </summary>
        public List<Playlist> Playlists = null;
        /// <summary>
        /// Currently available devices Spotify can switch to
        /// </summary>
        public List<Device> Devices = null;
        /// <summary>
        /// The current track being played
        /// </summary>
        public Track CurrentTrack = null;
        /// <summary>
        /// The current position in milliseconds the track has played
        /// </summary>
        public float CurrentTrackTimeMs = 0f;
        /// <summary>
        /// The current volume levels
        /// </summary>
        public VolumeInfo Volume = null;
        /// <summary>
        /// Currently active device using Spotify
        /// </summary>
        public Device ActiveDevice = null;
        /// <summary>
        /// Current information about the currently logged in user
        /// </summary>
        public UserInfo UserInformation = null;

        /// <summary>
        /// Time at when the current authorization will expire, will return DateTime.MinValue if no token
        /// </summary>
        public DateTime ExpireTime { get { return m_lastAuthToken != null ? m_lastAuthToken.CreateDate.AddSeconds(m_lastAuthToken.ExpiresIn) : DateTime.MinValue; } }
        /// <summary>
        /// Is the user currently paying for Spotify Premium
        /// </summary>
        public bool IsPremium { get { return UserInformation != null ? UserInformation.IsPremium : false; } }
        /// <summary>
        /// Type of media currently being played in Spotify
        /// </summary>
        public MediaType MediaType { get; private set; } = MediaType.None;

        /// <summary>
        /// The last successful authorized token
        /// </summary>
        protected SAPIModels.Token m_lastAuthToken { get; private set; }
        protected AuthorizationCodeAuth m_lastCodeAuth { get; private set; }

        private EventManager m_eventManager = null;
        /// <summary>
        /// The event manager instance for the service class, given to all SpotifyUIBase scripts
        /// </summary>
        public EventManager EventManager
        {
            get
            {
                if (m_eventManager == null)
                    m_eventManager = gameObject.AddComponent<EventManager>();
                return m_eventManager;
            }
        }

        /// <summary>
        /// SpotifyAPI.NET client - WARNING: This should ONLY be used for access all of the Get or Set functions. 
        /// Connecting to and from the Spotify API is handled by the relevent SpotifyService inside Spotify4Unity. You DO NOT 
        /// need to follow the authorization guides, simple call the Connect() function on this service
        /// </summary>
        public SpotifyWebAPI WebAPI { get { return m_webAPI; } }
        protected SpotifyWebAPI m_webAPI = null;

        private int m_lastVolumeLevel = 0;
        private Thread m_trackTimeUpdateThread = null;
        private Timer m_trackTimeLoopTimer = null;
        private Thread m_loadTracksThread = null;
        private LocalContext m_preQueueContext = null;
        private LocalContext m_currentContextUri = null;
        /// <summary>
        /// Has the service initialized it's values
        /// </summary>
        private bool m_isInit = false;
        private QueueService m_queue = new QueueService();

        /// <summary>
        /// Amount of time to remove from authenticated token's time to renew before the previous expire
        /// </summary>
        protected const int AUTH_TOKEN_MARGIN_SECONDS = 60;

        /// <summary>
        /// The max number for volume to be set
        /// </summary>
        private const float MAX_VOLUME_AMOUNT = 100f;
        /// <summary>
        /// The id for premium on the users profile
        /// </summary>
        private const string PREMIUM_ID = "premium";
        /// <summary>
        /// The id for a user (non-premium) on the users profile
        /// </summary>
        private const string USER_ID = "user";
        

        #region MonoBehavious
        protected virtual void Awake()
        {
            Analysis.LogsLevel = LogLevel;
        }

        protected virtual void Start()
        {
            EventManager.AddListener<ConnectedChanged>(OnConnectedChangedEventCallback);
            EventManager.AddListener<PlaylistsChanged>(OnLoadedPlaylists);
            EventManager.AddListener<SavedTracksLoaded>(OnSavedTracksLoaded);

            if (AutoConnect)
            {
                Connect();
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_trackTimeLoopTimer != null)
            {
                m_trackTimeLoopTimer.Dispose();
                m_trackTimeLoopTimer = null;
            }
            if (m_trackTimeUpdateThread != null)
            {
                m_trackTimeUpdateThread.Abort();
                m_trackTimeUpdateThread = null;
            }
            if (m_loadTracksThread != null)
            {
                m_loadTracksThread.Abort();
                m_loadTracksThread = null;
            }

            CurrentTrack = null;
            Playlists = null;
            SavedTracks = null;
            Devices = null;

            m_queue.Dispose();

            if (m_eventManager != null)
                m_eventManager.RemoveAll();
        }
        #endregion

        /// <summary>
        /// Make an attempt to authorize with Spotify. Returns a bool to represent if an attempt can/has been made
        /// </summary>
        /// <returns>If the attempt sucessfully managed to be attempted. Doesn't represent if the service is connected or not</returns>
        public virtual bool Connect()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Configures the initial SpotifyWebAPI using the latest authorization details
        /// </summary>
        /// <param name="token">The latest authorization token</param>
        /// <param name="auth">The initial authorization code class</param>
        protected void Configure(SAPIModels.Token token, AuthorizationCodeAuth auth = null)
        {
            if (token == null)
            {
                Analysis.LogError("Unable to setup SpotifyWebAPI - Missing token to configure Service!", Analysis.LogLevel.Vital);
                return;
            }

            m_webAPI = new SpotifyWebAPI()
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType,
                UseAutoRetry = true,
            };

            m_lastAuthToken = token;
            m_lastCodeAuth = auth;

            IsConnected = m_webAPI != null;
            EventManager.QueueEvent(new ConnectedChanged(IsConnected));
            
            if (IsConnected && !m_isInit)
            {
                Analysis.Log($"Successfully connected to the Spotify Web API{Environment.NewLine}Scopes: {Scopes.ToString()}", Analysis.LogLevel.Vital);

                InitializeService();
                m_isInit = true;
            }
        }

        /// <summary>
        /// Check's if system has a saved auth token and connects the service
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        protected bool ReuseAuth(AuthorizationCodeAuth auth = null)
        {
            bool shouldDeleteAuth = false;
            if (ReuseAuthTokens)
            {
                if (TokenSaver.HasSavedTokenInfo())
                {
                    SAPIModels.Token token = TokenSaver.LoadToken();
                    if (token != null && !string.IsNullOrEmpty(token.AccessToken))
                    {
                        /// ToDo: Fix to allow reusing old authentification until it's expired
                        //Make sure token's are within margin for reauth and configure
                        //DateTime expireTime = token.CreateDate.AddSeconds(token.ExpiresIn);
                        //DateTime renewTimeMargin = DateTime.Now.Subtract(TimeSpan.FromSeconds(AUTH_TOKEN_MARGIN_SECONDS));
                        //if (expireTime > renewTimeMargin)
                        //{
                        //    Analysis.Log($"Using saved auth token from previous authentification. Expires at '{expireTime}'", Analysis.LogLevel.Vital);
                        //    Configure(token, auth);
                        //    return true;
                        //}
                        if(auth != null && !string.IsNullOrEmpty(token.RefreshToken))
                        {
                            Task<SAPIModels.Token> task = Task.Run(async () => await auth.RefreshToken(token.RefreshToken));
                            SAPIModels.Token newToken = task.Result;
                            // Transfer refresh token to new token
                            newToken.RefreshToken = token.RefreshToken;

                            Analysis.Log("Using previous authorization's Refresh Token to renew", Analysis.LogLevel.Vital);
                            Configure(newToken, auth);
                            return true;
                        }
                        else
                        {
                            shouldDeleteAuth = true;
                            Analysis.Log("Has old authentification saved. Reaquiring new authorization", Analysis.LogLevel.Vital);
                            return false;
                        }
                    }
                    else
                    {
                        Analysis.LogWarning("Previously saved auth token has errors. Removing...", Analysis.LogLevel.Vital);
                        shouldDeleteAuth = true;
                    }
                }
            }

            if (shouldDeleteAuth)
            {
                // Delete the token if an old one is kept
                if (TokenSaver.HasSavedTokenInfo())
                    TokenSaver.DeleteToken();
            }
            return false;
        }

        private void OnConnectedChangedEventCallback(ConnectedChanged e)
        {
            GotAuth(m_lastAuthToken, m_lastCodeAuth);
        }

        protected virtual void GotAuth(SAPIModels.Token token, AuthorizationCodeAuth auth = null)
        {
            IsConnecting = false;

            if (ReuseAuthTokens)
            {
                bool didSave = TokenSaver.SaveToken(token);
                if(didSave)
                    Analysis.Log("Saved auth token to reuse", Analysis.LogLevel.Vital);
            }
        }

        private void InitializeService()
        {
            SetDevicesInternal(GetDevices());

            SAPIModels.PlaybackContext playingTrack = m_webAPI.GetPlayingTrack();
            SetTrack(playingTrack.Item);
            SetPlaying(playingTrack.IsPlaying);
            SetShuffleInternal(playingTrack.ShuffleState ? Shuffle.Enabled : Shuffle.Disabled);
            ///ToDo: Investigate why RepeatState is "{...}"
            //SetRepeatInternal((Repeat)playingTrack.RepeatState);
            SetRepeat(Repeat.Disabled);

            //Set intial volume
            int targetVol = playingTrack.Device != null ? playingTrack.Device.VolumePercent : 0;
            if (ActiveDevice != null)
                targetVol = ActiveDevice.VolumePercent;
            m_lastVolumeLevel = targetVol;
            SetVolumeInternal(new VolumeInfo(targetVol, MAX_VOLUME_AMOUNT));
            SetMute(targetVol == 0);

            Task.Run(async () => await LoadUserInfo());

            PlaylistLoader playlistLoader = this.gameObject.AddComponent<PlaylistLoader>();
            playlistLoader.Load(this, EventManager);
            SavedTracksLoader tracksLoader = this.gameObject.AddComponent<SavedTracksLoader>();
            tracksLoader.Load(this, EventManager);

            m_trackTimeLoopTimer = new Timer(OnRunUpdateInteval, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(UpdateFrequencyMs));
        }

        private void OnRunUpdateInteval(object state)
        {
            m_trackTimeUpdateThread = new Thread(UpdateInternal);
            m_trackTimeUpdateThread.Start();
        }

        /// <summary>
        /// Play a song in Spotify, in the context of the album
        /// </summary>
        /// <param name="t">The track</param>
        public void PlayTrack(Track t)
        {
            Task.Run(() => PlayTrackAsync(t));
        }

        public async Task PlayTrackAsync(Track t)
        {
            if (t == null)
            {
                Analysis.LogError("Unable to play track since the track is null", Analysis.LogLevel.All);
                return;
            }

            await PlaySongAsync(t.TrackUri, t.AlbumUri, ActiveDevice.Id);
        }

        /// <summary>
        /// Play a song in Spotify. Can be played by supplying either
        /// </summary>
        /// <param name="trackUri">The Uri of the track to play. If left blank, AlbumUri needs to be supplied!</param>
        /// <param name="albumUri">The Uri of the album the track belongs to. If left blank, TrackURI needs to be supplied!</param>
        /// <param name="deviceId">The id of the device to play the track on. Leaving blank will play on the current active device</param>
        public void PlaySong(string trackUri = "", string albumUri = "", string deviceId = "")
        {
            Task.Run(() => PlaySongAsync(trackUri, albumUri, deviceId));
        }

        public async Task PlaySongAsync(string trackUri = "", string albumUri = "", string deviceId = "")
        {
            /* Note: For the SpotifyAPI, you should only supply either the TrackUri or the 
             * AlbumUri (also known as the ContextUri). Giving both will return a bad request 
             * from Spotify, causing your track not to be played.
             */

            await m_webAPI.ResumePlaybackAsync(deviceId, albumUri, null, trackUri);
        }

        /// <summary>
        /// Play a list of tracks inside Spotify. Will be played without any other context
        /// </summary>
        /// <param name="trackUris">The list of tracks to play</param>
        /// <param name="deviceId">The id of the device to play the track on. Leaving blank will play on the current active device</param>
        public void PlaySongs(List<string> trackUris = null, string deviceId = "")
        {
            Task.Run(() => PlaySongsAsync(trackUris, deviceId));
        }

        public async Task PlaySongsAsync(List<string> trackUris = null, string deviceId = "")
        {
            await m_webAPI.ResumePlaybackAsync(deviceId, null, trackUris, "");
        }

        /// <summary>
        /// Play a playlist in Spotify. First song to play is decided by Shuffle mode
        /// </summary>
        /// <param name="p">The playlist context to play from</param>
        public void PlayPlaylist(Playlist p)
        {
            Task.Run(() => PlayPlaylistAsync(p));
        }

        public async Task PlayPlaylistAsync(Playlist p)
        {
            if (p == null)
            {
                Analysis.LogError("Unable to play playlist since it's null", Analysis.LogLevel.All);
                return;
            }

            await PlaySongAsync(string.Empty, p.Uri, ActiveDevice.Id);
        }

        /// <summary>
        /// Plays an album in Spotify
        /// </summary>
        /// <param name="a">The album to play from</param>
        public void PlayAlbum(Album a)
        {
            if (a == null)
            {
                Analysis.LogError("Unable to play album since it's null", Analysis.LogLevel.All);
                return;
            }

            PlaySong(string.Empty, a.Uri, ActiveDevice.Id);
        }

        /// <summary>
        /// Plays the top songs of an artist in Spotify
        /// </summary>
        /// <param name="artist"></param>
        public void PlayArtist(Artist artist)
        {
            if (artist == null)
            {
                Analysis.LogError("Unable to play artist since it's null", Analysis.LogLevel.All);
                return;
            }

            PlaySong(null, artist.Uri, ActiveDevice.Id);
        }

        /// <summary>
        /// Queues a track to play after the current track has finished. If the queue has tracks inside, it will be added to the back of the queue
        /// </summary>
        /// <param name="t">The track to add to the back of the queue</param>
        public void QueueTrack(Track t)
        {
            m_queue.QueueTrack(t);
            Analysis.Log($"Added '{t.Artist.Name}' - '{t.Title}' to queue", Analysis.LogLevel.All);
        }

        private async Task LoadUserInfo()
        {
            UserInformation = await LoadUserInformationAsync();
            if(UserInformation != null)
                EventManager.QueueEvent(new UserInfoLoaded(UserInformation));
        }

        private async Task<UserInfo> LoadUserInformationAsync()
        {
            SAPIModels.PrivateProfile privateProfile = null;
            try
            {
                privateProfile = await m_webAPI.GetPrivateProfileAsync();
            }
            catch (Exception e)
            {
                Analysis.LogError($"Can't load Private Profile information of authorized user - {e}", Analysis.LogLevel.Vital);
            }
            if (privateProfile == null)
                return null;

            string profilePictureUrl = privateProfile.Images.Count > 0 ? privateProfile.Images.FirstOrDefault().Url : null;
            return new UserInfo()
            {
                Username = privateProfile.DisplayName,
                Name = privateProfile.DisplayName,

                Followers = privateProfile.Followers.Total,
                IsPremium = privateProfile.Product == PREMIUM_ID,

                ProfilePictureURL = profilePictureUrl,
                Country = privateProfile.Country,
                Id = privateProfile.Id,
                Birthdate = ServiceHelper.ParseBirthdate(privateProfile.Birthdate),
            };
        }

        private void OnSavedTracksLoaded(SavedTracksLoaded e)
        {
            SavedTracks = e.SavedTracks;
            Analysis.Log($"{SavedTracks.Count} saved tracks loaded", Analysis.LogLevel.All);
        }

        private void OnLoadedPlaylists(PlaylistsChanged e)
        {
            Playlists = e.Playlists;
            Analysis.Log($"{Playlists.Count} playlists loaded", Analysis.LogLevel.All);
        }

        /// <summary>
        /// Disconnects and removes any information from the service
        /// </summary>
        public void Disconnect()
        {
            if (m_webAPI != null)
            {
                m_webAPI.Dispose();
                m_webAPI = null;
            }

            if (m_trackTimeLoopTimer != null)
            {
                m_trackTimeLoopTimer.Dispose();
                m_trackTimeLoopTimer = null;
            }

            IsPlaying = false;
            IsMuted = false;

            CurrentTrack = null;
            CurrentTrackTimeMs = 0f;
            Volume = null;
            SavedTracks = null;
            Playlists = null;

            IsConnected = false;
            m_isInit = false;
            EventManager.QueueEvent(new ConnectedChanged(IsConnected));
        }

        /// <summary>
        /// Plays the song currently in Spotify
        /// </summary>
        public void Play()
        {
            Task.Run(() => PlayAsync());
        }

        public async Task PlayAsync()
        {
            if (!IsPlaying)
            {
                await m_webAPI.ResumePlaybackAsync("", "", null, "");
                IsPlaying = true;
                SetPlaying(IsPlaying);

                if (CurrentTrack != null)
                    Analysis.Log($"Resuming song '{CurrentTrack.Artist.Name} - {CurrentTrack.Title}'", Analysis.LogLevel.All);
            }
        }

        /// <summary>
        /// Pauses the current song
        /// </summary>
        public void Pause()
        {
            Task.Run(() => PauseAsync());
        }

        public async Task PauseAsync()
        {
            if (IsPlaying)
            {
                await m_webAPI.PausePlaybackAsync();
                IsPlaying = false;
                SetPlaying(IsPlaying);

                Analysis.Log($"Pausing song '{CurrentTrack.Artist.Name} - {CurrentTrack.Title}'", Analysis.LogLevel.All);
            }
        }

        /// <summary>
        /// Sets the current Spotify device to be muted or not
        /// </summary>
        /// <param name="isMuted">True is muted. False is Unmuted</param>
        public void SetMute(bool isMuted)
        {
            Task.Run(() => SetMuteAsync(isMuted));
        }

        public async Task SetMuteAsync(bool isMuted)
        {
            SetMuteInternal(isMuted);

            if (isMuted)
            {
                int muteVolume = 0;
                await m_webAPI.SetVolumeAsync(muteVolume);
                SetVolumeInternal(new VolumeInfo(0, MAX_VOLUME_AMOUNT));
                Analysis.Log($"Muted volume", Analysis.LogLevel.All);
            }
            else
            {
                m_webAPI.SetVolume(m_lastVolumeLevel);
                SetVolumeInternal(new VolumeInfo(m_lastVolumeLevel, MAX_VOLUME_AMOUNT));
                Analysis.Log($"Unmuted volume & set to '{m_lastVolumeLevel}'", Analysis.LogLevel.All);
            }
        }

        /// <summary>
        /// Set the current Spotify device to be muted or not from the current volume percentage
        /// </summary>
        /// <param name="volumePercentage">The current volume as a percentage</param>
        public void SetMute(int volumePercentage)
        {
            SetMute(volumePercentage == 0);
        }

        /// <summary>
        /// Sets the mute state relative to the service, excludes the SpotifyAPI WebAPI
        /// </summary>
        /// <param name="isMuted"></param>
        private void SetMuteInternal(bool isMuted)
        {
            IsMuted = isMuted;
            EventManager.QueueEvent(new MuteChanged(IsMuted));
        }

        /// <summary>
        /// Move the current track position using the minutes & seconds (For example (2, 15) will set the track to 2:15 in the track)
        /// </summary>
        /// <param name="minutes">The minutes to move the track position to</param>
        /// <param name="totalSeconds">The total seconds to move the track position to</param>
        public void SetTrackPosition(float minutes, float seconds)
        {
            float minutesMs = minutes * 60000;
            float secondsMs = seconds * 1000;
            float ms = minutesMs + secondsMs;

            SetTrackPosition(ms);
        }

        /// <summary>
        /// Move the current track position using milliseconds
        /// </summary>
        /// <param name="positionMs">The position in milliseconds to set track position to</param>
        public void SetTrackPosition(float positionMs)
        {
            Task.Run(() => SetTrackPositionAsync(positionMs));
        }

        /// <summary>
        /// Move the current track position using milliseconds
        /// </summary>
        /// <param name="positionMs"></param>
        /// <returns></returns>
        public async Task SetTrackPositionAsync(float positionMs)
        {
            if (CurrentTrack == null)
                return;

            if (positionMs > CurrentTrack.TotalTimeMs)
            {
                Analysis.LogError("Can't set current track position since given number is higher than track time", Analysis.LogLevel.All);
                return;
            }

            await m_webAPI.SeekPlaybackAsync((int)positionMs);

            float seconds = (positionMs / 1000) % 60;
            float minutes = ((positionMs - seconds) / 1000) / 60;
            Analysis.Log($"Set '{CurrentTrack.Artist.Name} - {CurrentTrack.Title}' position to {(int)minutes}:{(int)seconds}", Analysis.LogLevel.All);
        }

        /// <summary>
        /// Skips the current song and plays the next song
        /// </summary>
        public void NextSong()
        {
            Task.Run(() => NextSongAsync());
        }

        /// <summary>
        /// Skips the current song and plays the next song
        /// </summary>
        /// <returns></returns>
        public async Task NextSongAsync()
        {
            if (m_queue.IsQueued())
            {
                PlayNextQueueTrack();
            }
            else
            {
                await m_webAPI.SkipPlaybackToNextAsync();
                Analysis.Log($"Playing next song '{CurrentTrack.Artist.Name} - {CurrentTrack.Title}'", Analysis.LogLevel.All);
            }
        }

        /// <summary>
        /// Changes the playback of Spotify to the previous song
        /// </summary>
        public void PreviousSong()
        {
            Task.Run(() => PreviousSongAsync());
        }

        /// <summary>
        /// Changes the playback of Spotify to the previous song
        /// </summary>
        /// <returns></returns>
        public async Task PreviousSongAsync()
        {
            await m_webAPI.SkipPlaybackToPreviousAsync();
            Analysis.Log($"Playing previous song '{CurrentTrack.Artist.Name} - {CurrentTrack.Title}'", Analysis.LogLevel.All);
        }

        /// <summary>
        /// Sets the volume of Spotify
        /// </summary>
        /// <param name="newVolume">New volume amount. Should be between 0 - 100</param>
        public void SetVolume(float newVolume)
        {
            int newVolPercent = (int)newVolume;
            SetVolume(newVolPercent);
        }

        /// <summary>
        /// Sets the volume of Spotify
        /// </summary>
        /// <param name="newVolume">The new volume to set to. Should ne a number between 0 - 100</param>
        public void SetVolume(int newVolume)
        {
            Task.Run(() => SetVolumeAsync(newVolume));
        }

        /// <summary>
        /// Sets the volume of Spotify
        /// </summary>
        /// <param name="newVolume"></param>
        /// <returns></returns>
        public async Task SetVolumeAsync(int newVolume)
        {
            if (newVolume > 100)
                newVolume = 100;

            //Only set restore value when not muted
            if (!IsMuted)
                m_lastVolumeLevel = newVolume;

            SetVolumeInternal(new VolumeInfo(m_lastVolumeLevel, MAX_VOLUME_AMOUNT));
            await m_webAPI.SetVolumeAsync(newVolume);
            Analysis.Log($"Set Spotify volume to {newVolume}", Analysis.LogLevel.All);
        }


        /// <summary>
        /// Set the volume relative to the service, excluding the SpotifyAPI
        /// </summary>
        /// <param name="info"></param>
        private void SetVolumeInternal(VolumeInfo info)
        {
            Volume = info;
            if (info != null)
                EventManager.QueueEvent(new VolumeChanged(info.CurrentVolume, info.MaxVolume));
        }

        private void SetRepeatInternal(RepeatState state)
        {
            Repeat r = Repeat.Disabled;
            switch (state)
            {
                case SpotifyAPI.Web.Enums.RepeatState.Context:
                    r = Repeat.Playlist;
                    break;
                case SpotifyAPI.Web.Enums.RepeatState.Track:
                    r = Repeat.Track;
                    break;
                case SpotifyAPI.Web.Enums.RepeatState.Off:
                    r = Repeat.Disabled;
                    break;
                default:
                    throw new NotImplementedException("Unable to set repeat state since a state is missing");
            }

            SetRepeatInternal(r);
        }

        private void SetRepeatInternal(Repeat state)
        {
            RepeatState = state;
            EventManager.QueueEvent(new RepeatChanged(RepeatState));
            Analysis.Log($"Set Repeat mode to {state.ToString()}", Analysis.LogLevel.All);
        }

        private void SetShuffleInternal(Shuffle state)
        {
            ShuffleState = state;
            EventManager.QueueEvent(new ShuffleChanged(ShuffleState));
            Analysis.Log($"Set Shuffle mode to {state.ToString()}", Analysis.LogLevel.All);
        }

        private void SetTrack(SAPIModels.FullTrack t)
        {
            if (t == null)
                return;

            CurrentTrack = new Track(t);
            EventManager.QueueEvent(new TrackChanged(CurrentTrack));

            Analysis.Log($"Set current track to '{CurrentTrack.Artist.Name} - {CurrentTrack.Title}'", Analysis.LogLevel.All);
        }

        private void SetPlaying(bool isPlaying)
        {
            IsPlaying = isPlaying;
            EventManager.QueueEvent(new PlayStatusChanged(IsPlaying));
        }

        /// <summary>
        /// Gets the latest song information
        /// </summary>
        /// <returns></returns>
        public SongInfo GetSongInfo()
        {
            return Task.Run(() => GetSongInfoAsync()).Result;
        }

        /// <summary>
        /// Gets the latest song information
        /// </summary>
        /// <returns></returns>
        public async Task<SongInfo> GetSongInfoAsync()
        {
            if (!ConnectedToApi())
                return null;

            SAPIModels.PlaybackContext context = await m_webAPI.GetPlayingTrackAsync();
            if (context == null)
                return null;

            SongInfo info = new SongInfo()
            {
                Title = context.Item.Name,
                Artist = context.Item.Artists.FirstOrDefault()?.Name,
                AlbumName = context.Item.Album.Name,

                IsPlaying = context.IsPlaying,
                CurrentTime = context.ProgressMs,
                TotalDuration = context.Item.DurationMs,
            };
            return info;
        }

        /// <summary>
        /// Gets all tracks saved to the users library in Spotify in the order they were added to their library
        /// </summary>
        /// <returns></returns>
        public List<Track> GetSavedTracks()
        {
            return Task.Run(() => GetSavedTracksAsync()).Result;
        }

        /// <summary>
        /// Gets all tracks saved to the users library in Spotify in the order they were added to their library
        /// </summary>
        /// <returns></returns>
        public async Task<List<Track>> GetSavedTracksAsync()
        {
            if (!ConnectedToApi())
                return null;

            //How many tracks to retrieve on one attempt
            int retrieveCount = 50;
            SAPIModels.Paging<SAPIModels.SavedTrack> savedTracks = m_webAPI.GetSavedTracks(retrieveCount);
            List<SAPIModels.SavedTrack> savedTrackList = await ServiceHelper.GetAllFromPagingAsync(m_webAPI, savedTracks);

            SavedTracks = await ServiceHelper.ConvertSavedTracksAsync(savedTrackList);
            return SavedTracks;
        }

        /// <summary>
        /// Gets all playlists created by the logged in user
        /// </summary>
        /// <returns></returns>
        public async Task<List<Playlist>> GetUserPlaylistsAsync()
        {
            if (!ConnectedToApi())
                return null;

            List<Playlist> playlists = new List<Playlist>();

            string userId = string.Empty;
            if (UserInformation == null)
                userId = (await m_webAPI.GetPrivateProfileAsync()).Id;
            else
                userId = UserInformation.Id;

            if (string.IsNullOrEmpty(userId))
            {
                Analysis.LogError("Can't get user playlists - Unable to get their UserID", Analysis.LogLevel.Vital);
                return null;
            }

            //Get all and convert
            SAPIModels.Paging<SAPIModels.SimplePlaylist> apiPlaylists = await m_webAPI.GetUserPlaylistsAsync(userId, 20);
            List<SAPIModels.SimplePlaylist> all = await ServiceHelper.GetAllFromPagingAsync(m_webAPI, apiPlaylists);

            playlists = await ServiceHelper.ConvertSimplePlaylist(all, true, m_webAPI, userId);

            return playlists;
        }

        /// <summary>
        /// Gets all available Spotify devices that playback can be transfered to
        /// </summary>
        /// <returns></returns>
        public List<Device> GetDevices()
        {
            return Task.Run(() => GetDevicesAsync()).Result;
        }

        /// <summary>
        /// Gets all available Spotify devices that playback can be transfered to
        /// </summary>
        /// <returns></returns>
        public async Task<List<Device>> GetDevicesAsync()
        {
            if (!ConnectedToApi())
                return null;

            List<Device> list = new List<Device>();
            SAPIModels.AvailabeDevices devices = await m_webAPI.GetDevicesAsync();
            if (devices == null || devices?.Error != null)
                return null;

            await Task.Run(() =>
            {
                foreach (SAPIModels.Device d in devices.Devices)
                {
                    if (d == null)
                        continue;

                    Device newDevice = new Device(d);
                    if (ActiveDevice == null && d.IsActive)
                        ActiveDevice = newDevice;

                    list.Add(newDevice);
                }
            });

            return list;
        }

        private void SetDevicesInternal(List<Device> devices)
        {
            Devices = devices;
            EventManager.QueueEvent(new DevicesChanged(devices));
        }

        /// <summary>
        /// Sets the current playback Spotify device
        /// </summary>
        /// <param name="d"></param>
        public void SetActiveDevice(Device d)
        {
            Task.Run(() => SetActiveDeviceAsync(d));
        }

        /// <summary>
        /// Sets the current playback Spotify device
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public async Task SetActiveDeviceAsync(Device d)
        {
            //Make current in list inactive
            Device currentActive = Devices.FirstOrDefault(x => x.IsActive);
            currentActive.IsActive = false;

            await m_webAPI.ResumePlaybackAsync(d.Id, "", null, "");
            Analysis.Log($"Changed playback device to '{d.Name}'", Analysis.LogLevel.All);

            //Set new active device & update list
            ActiveDevice = Devices.FirstOrDefault(x => x.Id == d.Id);
            ActiveDevice.IsActive = true;

            SetDevicesInternal(Devices);
        }

        /// <summary>
        /// Gets all saved tracks and sorts the list by an option
        /// </summary>
        /// <param name="sortType">The sort order the list should be in</param>
        /// <returns>The list of saved tracks sorted by the order</returns>
        public List<Track> GetSavedTracksSorted(Sort sortType)
        {
            return Task.Run(() => GetSavedTracksSortedAsync(sortType)).Result;
        }

        /// <summary>
        /// Gets all saved tracks and sorts the list by an option
        /// </summary>
        /// <param name="sortType"></param>
        /// <returns></returns>
        public async Task<List<Track>> GetSavedTracksSortedAsync(Sort sortType)
        {
            List<Track> allSavedTracks = null;
            if (SavedTracks != null)
                allSavedTracks = SavedTracks;
            else
                allSavedTracks = await GetSavedTracksAsync();

            switch (sortType)
            {
                case Sort.Title:
                    return allSavedTracks.OrderBy(x => x.Title).ToList();
                case Sort.Artist:
                    return allSavedTracks.OrderBy(x => x.Artist).ToList();
                case Sort.Album:
                    return allSavedTracks.OrderBy(x => x.Album).ToList();
                case Sort.Unsorted:
                    return allSavedTracks;
                default:
                    throw new NotImplementedException("Unimplemented sort type to Saved Tracks");
            }
        }

        /// <summary>
        /// Gets the currently loaded user information
        /// </summary>
        /// <returns></returns>
        public UserInfo GetProfileInfo()
        {
            return UserInformation;
        }

        /// <summary>
        /// Set the repeat state of Spotify
        /// </summary>
        /// <param name="state">The repeat state to set to</param>
        public void SetRepeat(Repeat state)
        {
            Task.Run(() => SetRepeatAsync(state));
        }

        /// <summary>
        /// Set the repeat state of Spotify
        /// </summary>
        /// <param name="state">The repeat state to set to</param>
        /// <returns></returns>
        public async Task SetRepeatAsync(Repeat state)
        {
            RepeatState repeatState = SpotifyAPI.Web.Enums.RepeatState.Off;
            switch (state)
            {
                case Repeat.Disabled:
                    repeatState = SpotifyAPI.Web.Enums.RepeatState.Off;
                    break;
                case Repeat.Playlist:
                    repeatState = SpotifyAPI.Web.Enums.RepeatState.Context;
                    break;
                case Repeat.Track:
                    repeatState = SpotifyAPI.Web.Enums.RepeatState.Track;
                    break;
            }

            RepeatState = state;
            await m_webAPI.SetRepeatModeAsync(repeatState);
            EventManager.QueueEvent(new RepeatChanged(RepeatState));

            Analysis.Log($"Set Repeat state to {state.ToString()}", Analysis.LogLevel.All);
        }

        /// <summary>
        /// Sets the shuffle state of Spotify
        /// </summary>
        /// <param name="state">The shuffle state to set to</param>
        public void SetShuffle(Shuffle state)
        {
            Task.Run(() => SetShuffleAsync(state));
        }

        /// <summary>
        /// Sets the shuffle state of Spotify
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task SetShuffleAsync(Shuffle state)
        {
            ShuffleState = state;
            await m_webAPI.SetShuffleAsync(state == Shuffle.Enabled);
            EventManager.QueueEvent(new ShuffleChanged(ShuffleState));

            Analysis.Log($"Set Shuffle state to {state.ToString()}", Analysis.LogLevel.All);
        }

        /// <summary>
        /// Internal update loop for detecting changes in the WebAPI since it has no callbacks
        /// </summary>
        private void UpdateInternal()
        {
            try
            {
                if (m_webAPI != null)
                {
                    UpdateTrackInfo();
                    UpdateDevices();
                }
            }
            catch (ThreadAbortException tEx)
            {
                Analysis.LogError($"Aborted SpotifyService internal update - {tEx}", Analysis.LogLevel.All);
            }
            catch (Exception e)
            {
                Analysis.LogError($"SpotifyService internal Update loop exception - '{e}'", Analysis.LogLevel.All);
            }
        }

        private void UpdateTrackInfo()
        {
            //Track changed
            SAPIModels.PlaybackContext playback = m_webAPI.GetPlayback();
            if (playback == null)
                return; // Temp fix for when advert is playing

            float currentTimeMs = playback.ProgressMs;
            float? totalTimeMs = playback.Item?.DurationMs;

            CurrentTrackTimeMs = currentTimeMs;
            EventManager.QueueEvent(new TrackTimeChanged(currentTimeMs, totalTimeMs.HasValue ? totalTimeMs.Value : -1f));

            UpdateQueue(playback);

            if (playback.Context != null && playback.Item != null)
            {
                if(m_currentContextUri != null && m_currentContextUri.ContextUri != playback.Item.Uri 
                    || m_currentContextUri == null)
                {
                    m_currentContextUri = new LocalContext(playback.Item.Uri, playback.Context.Uri);
                }
            }

            // Track changed check.
            if (CurrentTrack?.TrackUri != playback?.Item?.Uri)
            {
                PlayNextQueueTrack();
                SetTrack(playback.Item);
            }
            if (playback.Item == null && playback.Context == null)
            {
                if (m_currentContextUri != null)
                    m_currentContextUri = null;
            }
            //Play/Pause check
            if (playback.IsPlaying != IsPlaying)
            {
                SetPlaying(playback.IsPlaying);
            }
            // Media/Advert Check
            MediaType playbackType = ConvertType(playback.CurrentlyPlayingType);
            if (playbackType != MediaType)
            {
                MediaType = playbackType;
                EventManager.QueueEvent(new MediaTypeChanged(MediaType));
            }
        }

        private void UpdateDevices()
        {
            if (m_webAPI != null)
            {
                List<Device> devices = GetDevices();
                if (devices == null)
                    return;

                Device webActiveDevice = null;
                //Get the first active device if available, otherwise set ActiveDevice to first in list
                if (devices.Any(x => x.IsActive))
                {
                    webActiveDevice = devices.FirstOrDefault(x => x.IsActive);
                }
                else
                {
                    webActiveDevice = devices.FirstOrDefault();
                }

                if (webActiveDevice != null)
                {
                    //Device changed check
                    if (webActiveDevice.Id != ActiveDevice?.Id)
                    {
                        ActiveDevice = Devices.FirstOrDefault(x => x.Id == webActiveDevice.Id);
                        ActiveDevice.IsActive = true;

                        SetDevicesInternal(Devices);
                    }
                    //Check if any devices has gone offline/online and update
                    else if (Devices?.Count != devices.Count)
                    {
                        SetDevicesInternal(devices);
                    }
                    //If Devices is null, add latest
                    else if (Devices == null && devices != null)
                    {
                        Devices = devices;
                    }

                    //Has the device volume changed from the internal
                    if (webActiveDevice.VolumePercent != Volume?.CurrentVolume)
                    {
                        SetVolumeInternal(new VolumeInfo((float)webActiveDevice.VolumePercent, MAX_VOLUME_AMOUNT));
                    }
                }
            }
        }

        /// <summary>
        /// Own system for keeping an internal queue and making it function
        /// </summary>
        /// <param name="playback"></param>
        private void UpdateQueue(SAPIModels.PlaybackContext playback)
        {
            //Check if the song is 1 second before finishing to get a pre-change check
            float msBeforeEnd = ((1f) * 1000);
            if (playback?.ProgressMs >= playback?.Item?.DurationMs - msBeforeEnd)
            {
                PlayNextQueueTrack();
            }
        }

        private void PlayNextQueueTrack()
        {
            Track nextQueuedTrack = m_queue.WithdrawNextTrack();
            if (nextQueuedTrack != null)
            {
                //Save the current context and track if the queue hasn't started
                if (m_preQueueContext == null && m_currentContextUri != null)
                    m_preQueueContext = new LocalContext(m_currentContextUri.TrackUri, m_currentContextUri.ContextUri);

                Analysis.Log($"Using Queued song '{nextQueuedTrack.Artist.Name}' - '{nextQueuedTrack.Title}'", Analysis.LogLevel.All);

                PlayTrack(nextQueuedTrack);
            }
            else if (m_preQueueContext != null && CurrentTrack.TrackUri != m_preQueueContext.TrackUri)
            {
                Analysis.Log("Resuming previous context after finishing queue", Analysis.LogLevel.All);

                //On queue finish, start from old context and track, but skip to next song
                //ToDo: Change to be able to get track from ContextUri and save next track instead of using NextSong method (when Spotify supports this)
                PlaySong(m_preQueueContext.TrackUri, m_preQueueContext.ContextUri);
                NextSong();
                m_preQueueContext = null;
            }
        }

        /// <summary>
        /// Searches Spotify using a query string and return top amount results
        /// </summary>
        /// <param name="query">The string query to search</param>
        /// <param name="amount">The maximum amount of relevant results to return</param>
        /// <returns></returns>
        public SearchQuery Search(string query, int amount = 50)
        {
            return Task.Run(() => SearchAsync(query, amount)).Result;
        }

        /// <summary>
        /// Searches Spotify using a query string and return top results
        /// </summary>
        /// <param name="query">The query to search</param>
        /// <param name="amount">The maximum amount of relevant results to return</param>
        /// <returns></returns>
        public async Task<SearchQuery> SearchAsync(string query, int amount = 50)
        {
            if (!ConnectedToApi())
                return null;

            int searchMax = 50;
            if (amount > searchMax)
                amount = searchMax;
            SAPIModels.SearchItem items = await m_webAPI.SearchItemsAsync(query, SearchType.All, amount);
            if (items == null)
                return null;

            List<Playlist> playlists = null;
            List<Track> tracks = null;
            List<Artist> artists = null;
            List<Album> albums = null;

            if (items.Playlists != null)
            {
                playlists = await ServiceHelper.ConvertSimplePlaylist(items.Playlists.Items, false, null, null);
            }

            if (items.Tracks != null)
            {
                tracks = await ServiceHelper.ConvertFullTracksAsync(items.Tracks.Items);
            }

            if (items.Artists != null)
            {
                await Task.Run(() =>
                {
                    artists = items.Artists.Items.Select(x => new Artist()
                    {
                        Name = x.Name,
                        Uri = x.Uri,
                        ImageUrl = x.Images.FirstOrDefault()?.Url,
                        Popularity = x.Popularity
                    }).ToList();
                });
            }

            if (items.Albums != null)
            {
                await Task.Run(() =>
                {
                    albums = items.Albums.Items.Select(x => new Album()
                    {
                        Name = x.Name,
                        Uri = x.Uri,
                        ImageUrl = x.Images.FirstOrDefault()?.Url,
                    }).ToList();
                });
            }

            return new SearchQuery()
            {
                Artists = artists,
                Tracks = tracks,
                Playlists = playlists,
                Albums = albums,
            };
        }

        private bool ConnectedToApi()
        {
            if (m_webAPI == null)
            {
                Analysis.LogError("SpotifyAPI not setup!", Analysis.LogLevel.Vital);
                return false;
            }

            if (!IsConnected)
            {
                Analysis.LogError("Not connected to Spotify!", Analysis.LogLevel.Vital);
            }
            return IsConnected;
        }

        private MediaType ConvertType(TrackType trackType)
        {
            switch (trackType)
            {
                case TrackType.Ad:
                    return MediaType.Advert;
                case TrackType.Episode:
                    return MediaType.Episode;
                case TrackType.Track:
                    return MediaType.Track;
                case TrackType.Unknown:
                    return MediaType.None;
                default:
                    return MediaType.Track;
                    //throw new NotImplementedException($"Unable to set MediaType, Not Implemented case '{trackType}'");
            }
        }

        /// <summary>
        /// Check if the current authorized user is follow another user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsUserFollowingUser(string userId)
        {
            List<bool> list = m_webAPI.IsFollowing(FollowType.User, userId).List;
            return list != null ? list.FirstOrDefault() != false : false;
        }

        /// <summary>
        /// Check if the current authorized user is follow an artist
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns></returns>
        public bool IsUserFollowingArtist(string artistId)
        {
            List<bool> list = m_webAPI.IsFollowing(FollowType.Artist, artistId).List;
            return list != null ? list.FirstOrDefault() != false : false;
        }

        /// <summary>
        /// Follows all of the user's that are given as the "ids" on the current authorized user. Returns true if successful
        /// </summary>
        /// <param name="ids">Any/All user id's. Can be one or more</param>
        /// <returns></returns>
        public bool FollowUsers(params string[] ids) { return FollowAccounts(FollowType.User, ids.ToList()); }
        /// <summary>
        /// Follows all of the artists that are given as the "ids" on the current authorized user. Returns true if successful
        /// </summary>
        /// <param name="ids">Any/All artist id's. Can be one or more</param>
        /// <returns></returns>
        public bool FollowArtists(params string[] ids) { return FollowAccounts(FollowType.Artist, ids.ToList()); }
        /// <summary>
        /// Unfollows all of the user's that are given as the "ids" on the current authorized user. Returns true if successful
        /// </summary>
        /// <param name="ids">Any/All user id's. Can be one or more</param>
        /// <returns></returns>
        public bool UnfollowUsers(params string[] ids) { return UnfollowAccounts(FollowType.User, ids.ToList()); }
        /// <summary>
        /// Unfollows all of the artists that are given as the "ids" on the current authorized user. Returns true if successful
        /// </summary>
        /// <param name="ids">Any/All artist id's. Can be one or more</param>
        /// <returns></returns>
        public bool UnfollowArtists(params string[] ids) { return UnfollowAccounts(FollowType.Artist, ids.ToList()); }

        private bool FollowAccounts(FollowType type, List<string> ids)
        {
            SAPIModels.ErrorResponse response = m_webAPI.Follow(type, ids.ToList());
            return response?.Error != null;
        }

        private bool UnfollowAccounts(FollowType type, List<string> ids)
        {
            SAPIModels.ErrorResponse response = m_webAPI.Unfollow(type, ids.ToList());
            return response?.Error != null;
        }

        /// <summary>
        /// Gets all the public information available about the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UserInfo GetUserDetails(string userId)
        {
            SAPIModels.PublicProfile publicProfile = m_webAPI.GetPublicProfile(userId);

            if(publicProfile == null || publicProfile.Error != null)
                return null;

            return new UserInfo()
            {
                Name = publicProfile.DisplayName,
                Id = publicProfile.Id,
                ProfilePictureURL= publicProfile.Images.FirstOrDefault().Url,
                Followers = publicProfile.Followers.Total,
            };
        }

        /// <summary>
        /// Gets all the information details about the artist
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns></returns>
        public ArtistInfo GetArtistDetails(string artistId)
        {
            SAPIModels.FullArtist artist = m_webAPI.GetArtist(artistId);
            if (artist == null)
                return null;

            return new ArtistInfo()
            {
                Name = artist.Name,
                Id = artist.Id,
                Followers = artist.Followers.Total,
                Popularity = artist.Popularity,
                ProfilePictureURL = artist.Images.FirstOrDefault().Url,
                Genres = artist.Genres,
                ShareURL = artist.ExternalUrls.FirstOrDefault().Value,
            };
        }

        /// <summary>
        /// Saves one or several tracks to the user's library
        /// </summary>
        /// <param name="tracks"></param>
        /// <returns></returns>
        public bool SaveTracks(params Track[] tracks)
        {
            SAPIModels.ErrorResponse response = m_webAPI.SaveTracks(tracks.Select(x => x.TrackId).ToList());
            if(response?.Error != null)
            {
                SavedTracks.AddRange(tracks);
                Analysis.Log($"Added '{tracks.Length}' track(s) to user's library", Analysis.LogLevel.All);
            }

            return response?.Error == null;
        }

        /// <summary>
        /// Unsaves one or several track from the user's library
        /// </summary>
        /// <param name="tracks"></param>
        /// <returns></returns>
        public bool UnsaveTracks(params Track[] tracks)
        {
            SAPIModels.ErrorResponse response = m_webAPI.RemoveSavedTracks(tracks.Select(x => x.TrackId).ToList());
            if (response?.Error != null)
            {
                foreach(Track t in tracks)
                {
                    Track listTrack = SavedTracks.FirstOrDefault(x => x.TrackId == t.TrackId);
                    if (listTrack != null)
                        SavedTracks.Remove(listTrack);
                }
                Analysis.Log($"Removed '{tracks.Length}' track(s) from the user's library", Analysis.LogLevel.All);
            }

            return response?.Error == null;
        }

        /// <summary>
        /// EXPERIMENTAL: Opens the Spotify Client on the device.
        /// </summary>
        public virtual void OpenSpotifyClient()
        {
            throw new NotImplementedException("Need to implement platform-specific implementation. Override this function");
        }

        /// <summary>
        /// EXPERIMENTAL: Checks if the current device has Spotify opened
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSpotifyClientOpen()
        {
            throw new NotImplementedException("Need to implement platform-specific implementation. Override this function");
        }

        /// <summary>
        /// EXPERIMENTAL: Closes the Spotify instance on this device
        /// </summary>
        public virtual void CloseSpotifyClient()
        {
            throw new NotImplementedException("Need to implement platform-specific implementation. Override this function");
        }
    }
}