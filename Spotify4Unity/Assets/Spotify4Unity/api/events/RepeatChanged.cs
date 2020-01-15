using Spotify4Unity.Enums;

namespace Spotify4Unity.Events
{
    public class RepeatChanged : GameEventBase
    {
        public Repeat State { get; set; }
        public RepeatChanged(Repeat state)
        {
            State = state;
        }
    }
}
