namespace Spotify4Unity.Dtos
{
    public class SongInfo
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string AlbumName { get; set; }

        public double TotalDuration { get; set; }
        public double CurrentTime { get; set; }
        public bool IsPlaying { get; set; }
    }
}