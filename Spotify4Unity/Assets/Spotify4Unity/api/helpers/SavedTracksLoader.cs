using Spotify4Unity.Dtos;
using Spotify4Unity.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Spotify4Unity.Helpers
{
    class SavedTracksLoader : MonoBehaviour
    {
        private Thread m_loadThread = null;

        private EventManager m_eventManager = null;
        private SpotifyServiceBase m_service = null;

        public void Load(SpotifyServiceBase service, EventManager eventManager)
        {
            m_service = service;
            m_eventManager = eventManager;
            m_eventManager.AddListener<SavedTracksLoaded>(OnChanged);

            m_loadThread = new Thread(LoadTracks);
            m_loadThread.Start();
        }

        private async void LoadTracks()
        {
            List<Track> savedTracks = await m_service.GetSavedTracksAsync();
            m_eventManager.QueueEvent(new SavedTracksLoaded(savedTracks));
        }

        private void OnChanged(SavedTracksLoaded e)
        {
            Destroy(this);
        }
    }
}
