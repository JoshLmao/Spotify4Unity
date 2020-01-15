using Spotify4Unity.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spotify4Unity.Helpers
{
    internal class QueueService
    {
        /// <summary>
        /// All tracks that have been queued and waiting to be played
        /// </summary>
        public List<Track> QueuedTracks = new List<Track>();

        public bool IsQueued()
        {
            return QueuedTracks.Count > 0;
        }

        public Track WithdrawNextTrack()
        {
            Track t = QueuedTracks.FirstOrDefault();
            QueuedTracks.Remove(t);
            return t;
        }

        public void QueueTrack(Track track)
        {
            if (QueuedTracks == null)
                QueuedTracks = new List<Track>();

            QueuedTracks.Add(track);
        }

        public void Dispose()
        {
            QueuedTracks.Clear();
            QueuedTracks = null;
        }
    }
}
