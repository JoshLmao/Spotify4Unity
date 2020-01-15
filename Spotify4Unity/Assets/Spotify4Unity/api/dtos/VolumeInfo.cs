namespace Spotify4Unity.Dtos
{
    public class VolumeInfo
    {
        public float CurrentVolume { get; set; }
        public float MaxVolume { get; set; }

        public VolumeInfo() { }

        public VolumeInfo(float currentVol, float maxVol)
        {
            CurrentVolume = currentVol;
            MaxVolume = maxVol;
        }
    }
}
