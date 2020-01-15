using System;

namespace Spotify4Unity.Events
{
    public class TrackTimeChanged : GameEventBase
    {
        /// <summary>
        /// The current position of the track in milliseconds
        /// </summary>
        public float CurrentPositionMs { get; set; }
        /// <summary>
        /// The current position of the track as a time span
        /// </summary>
        public TimeSpan CurrentPositionSpan { get { return TimeSpan.FromMilliseconds(CurrentPositionMs); } }

        /// <summary>
        /// The total time of the track in milliseconds
        /// </summary>
        public float TotalTimeMs { get; set; }
        /// <summary>
        /// The total time of the track as a time span
        /// </summary>
        public TimeSpan TotalTimeSpan { get { return TimeSpan.FromMilliseconds(TotalTimeMs); } }

        public TrackTimeChanged(float currentPositionMs, float totalTrackTimeMs)
        {
            CurrentPositionMs = currentPositionMs;
            TotalTimeMs = totalTrackTimeMs;
        }
    }
}