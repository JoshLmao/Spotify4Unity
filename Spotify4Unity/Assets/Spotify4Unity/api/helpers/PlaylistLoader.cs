using Spotify4Unity.Dtos;
using Spotify4Unity.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Spotify4Unity.Helpers
{
    class PlaylistLoader : MonoBehaviour
    {
        private Thread m_loadThread = null;

        private EventManager m_eventManager = null;
        private SpotifyServiceBase m_service = null;

        public void Load(SpotifyServiceBase service, EventManager eventManager)
        {
            m_service = service;
            m_eventManager = eventManager;
            m_eventManager.AddListener<PlaylistsChanged>(OnChanged);

            m_loadThread = new Thread(LoadPlaylists);
            m_loadThread.Start();
        }

        private async void LoadPlaylists()
        {
            List<Playlist> playlists = await m_service.GetUserPlaylistsAsync();
            m_eventManager.QueueEvent(new PlaylistsChanged(playlists));
        }

        private void OnChanged(PlaylistsChanged e)
        {
            Destroy(this);
        }
    }
}
