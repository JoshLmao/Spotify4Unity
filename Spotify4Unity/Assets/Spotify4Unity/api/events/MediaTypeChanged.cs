using Spotify4Unity.Enums;

namespace Spotify4Unity.Events
{
    public class MediaTypeChanged : GameEventBase
    {
        public MediaType MediaType { get; set; }
        public MediaTypeChanged(MediaType type)
        {
            MediaType = type;
        }
    }
}
