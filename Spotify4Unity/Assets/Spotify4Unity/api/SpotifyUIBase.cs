using Spotify4Unity.Dtos;
using Spotify4Unity.Events;
using Spotify4Unity.Helpers;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Spotify4Unity
{
    /// <summary>
    /// Base class to override for creating custom Unity UI
    /// </summary>
    public class SpotifyUIBase : MonoBehaviour
    {
        /// <summary>
        /// Current Spotify Service that exists in the scene
        /// </summary>
        [Tooltip("The game object that hosts the constant Spotify Service. Will be found if in scene and not specified here")]
        public SpotifyServiceBase SpotifyService = null;

        /// <summary>
        /// Event Manager used to propegate events from SpotifyService. Can be used for other events by you (inhert from Spotify4Unity.Events.GameEventBase)
        /// </summary>
        protected EventManager m_eventManager = null;

        #region MonoBehavious
        protected virtual void Awake()
        {
            if (SpotifyService == null)
            {
                SpotifyService = GameObject.FindObjectOfType<SpotifyServiceBase>();

                if (SpotifyService == null)
                {
                    Analysis.LogError($"No SpotifyService set for GameObject {this.gameObject.name}, unable to function!", Analysis.LogLevel.Vital);
                    return;
                }
            }

            m_eventManager = SpotifyService.EventManager;

            if (m_eventManager != null)
            {
                m_eventManager.AddListener<AuthorizationExpired>(OnAuthorizationExpired);
                m_eventManager.AddListener<ConnectedChanged>(OnConnectedChanged);
                m_eventManager.AddListener<ConnectingChanged>(OnConnectingChanged);
                m_eventManager.AddListener<PlayStatusChanged>(OnPlayStatusChanged);
                m_eventManager.AddListener<TrackChanged>(OnTrackChanged);
                m_eventManager.AddListener<TrackTimeChanged>(OnTrackTimeChanged);
                m_eventManager.AddListener<VolumeChanged>(OnVolumeChanged);
                m_eventManager.AddListener<MuteChanged>(OnMuteChanged);
                m_eventManager.AddListener<SavedTracksLoaded>(OnSavedTracksLoaded);
                m_eventManager.AddListener<UserInfoLoaded>(OnUserInformationLoaded);
                m_eventManager.AddListener<RepeatChanged>(OnRepeatChanged);
                m_eventManager.AddListener<ShuffleChanged>(OnShuffleChanged);
                m_eventManager.AddListener<PlaylistsChanged>(OnPlaylistsChanged);
                m_eventManager.AddListener<DevicesChanged>(OnDevicesChanged);
                m_eventManager.AddListener<MediaTypeChanged>(OnMediaTypeChanged);
            }
            else
            {
                Analysis.LogError("Unable to find EventManager for Spotify4Unity - Won't be able to hear events", Analysis.LogLevel.Vital);
            }

            if (SpotifyService == null)
            { 
                Analysis.LogError("Unable to listen to Service Change events. SpotifyService is null", Analysis.LogLevel.Vital);
            }
        }
        #endregion

        #region Passthrough Methods
        /// <summary>
        /// Gets the current volume information with current and max volume level 
        /// </summary>
        /// <returns>The current volume information</returns>
        protected VolumeInfo GetVolume()
        {
            if (SpotifyService.IsConnected)
                return SpotifyService.Volume;
            else
                return null;
        }

        /// <summary>
        /// Sets the volume of Spotify to a new amount. Number between 0 - 100
        /// </summary>
        /// <param name="newVolume">New volume amount between 0 - 100</param>
        protected void SetVolume(float newVolume)
        {
            SpotifyService.SetVolume(newVolume);
        }

        protected async Task SetVolumeAsync(float volume)
        {
            await SpotifyService.SetVolumeAsync((int)volume);
        }

        /// <summary>
        /// Sets the current track to a position in milliseconds
        /// </summary>
        /// <param name="positionMs"></param>
        protected void SetCurrentTrackTime(float positionMs)
        {
            if (positionMs > SpotifyService.CurrentTrackTimeMs)
                return;

            if (positionMs != SpotifyService.CurrentTrackTimeMs)
            {
                SpotifyService.SetTrackPosition(positionMs);
            }
        }

        protected async Task SetCurrentTrackTimeAsync(float positionMs)
        {
            if (positionMs > SpotifyService.CurrentTrackTimeMs)
                return;

            if (positionMs != SpotifyService.CurrentTrackTimeMs)
            {
                await SpotifyService.SetTrackPositionAsync(positionMs);
            }
        }

        /// <summary>
        /// Sets the current track a new position using total seconds. For example, using 90 would set the track to 1 minute, 30 seconds
        /// </summary>
        /// <param name="totalSeconds"></param>
        protected void SetCurrentTrackTime(int totalSeconds)
        {
            SetCurrentTrackTime(totalSeconds * 1000f);
        }

        protected async Task SetCurrentTrackTimeAsync(int totalSeconds)
        {
            await SetCurrentTrackTimeAsync(totalSeconds * 1000f);
        }

        /// <summary>
        /// Sets the current track position using minutes and seconds
        /// </summary>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        protected void SetCurrentTrackTime(int minutes, int seconds)
        {
            float totalSeconds = (minutes * 60) + seconds;
            if (totalSeconds > SpotifyService.CurrentTrackTimeMs)
                return;

            SpotifyService.SetTrackPosition(minutes, seconds);
        }

        /// <summary>
        /// Gets information on the currently playing track like title, arists, album name, etc
        /// </summary>
        /// <returns>All information on the current track</returns>
        protected Track GetCurrentSongInfo()
        {
            if (SpotifyService.IsConnected)
                return SpotifyService.CurrentTrack;
            else
                return null;
        }

        /// <summary>
        /// Gets if Spotify is currently playing a song or not
        /// </summary>
        /// <returns></returns>
        protected bool GetPlayingStatus()
        {
            if (SpotifyService.IsConnected)
                return SpotifyService.IsPlaying;
            else
                return false;
        }
        #endregion

        #region Override Events
        /// <summary>
        /// Callback for when the service status of trying to authorize has changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnectingChanged(ConnectingChanged e)
        {
        }

        /// <summary>
        /// Callback for when the service status of being connected to Spotify has changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnectedChanged(ConnectedChanged e)
        {
        }
        
        /// <summary>
        /// Callback for when the playback status has changed between Playing and Paused
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPlayStatusChanged(PlayStatusChanged e)
        {
        }

        /// <summary>
        /// Callback for when the current track time has changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTrackTimeChanged(TrackTimeChanged e)
        {
        }

        /// <summary>
        /// Callback for when Spotify has it's volume changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnVolumeChanged(VolumeChanged e)
        {
        }

        /// <summary>
        /// Callback for when Spotify has changed it's mute state. Note: use OnVolumeChanged for callback on volume change
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMuteChanged(MuteChanged e)
        {
        }

        /// <summary>
        /// Callback for when all the user's saved tracks have been initially loaded
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSavedTracksLoaded(SavedTracksLoaded e)
        {
        }

        /// <summary>
        /// Callback for when the current playing track has been changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTrackChanged(TrackChanged e)
        {
        }

        /// <summary>
        /// Callback for when the current authorized user's information has been loaded
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnUserInformationLoaded(UserInfoLoaded e)
        {
        }

        /// <summary>
        /// Callback for when the shuffle state has been changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnShuffleChanged(ShuffleChanged e)
        {
        }

        /// <summary>
        /// Callback for when the repeat state has been changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRepeatChanged(RepeatChanged e)
        {
        }

        /// <summary>
        /// Callback for when the service loads all user playlists, a new one is created or one is removed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPlaylistsChanged(PlaylistsChanged e)
        {
        }

        /// <summary>
        /// Callback for when the service loads all Spotify playback devices or finds a new device
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDevicesChanged(DevicesChanged e)
        {
        }

        /// <summary>
        /// Callback for when the type of media being played has changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMediaTypeChanged(MediaTypeChanged e)
        {
        }

        /// <summary>
        /// Callback for when the current Spotify user authentification has expired
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAuthorizationExpired(AuthorizationExpired e)
        {
        }
        #endregion
    }
}