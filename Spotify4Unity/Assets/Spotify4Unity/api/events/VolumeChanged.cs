namespace Spotify4Unity.Events
{
    public class VolumeChanged : GameEventBase
    {
        public float Volume { get; set; }
        public float MaxVolume { get; set; }
        public VolumeChanged(float currentVol, float maxVol)
        {
            Volume = currentVol;
            MaxVolume = maxVol;
        }
    }
}