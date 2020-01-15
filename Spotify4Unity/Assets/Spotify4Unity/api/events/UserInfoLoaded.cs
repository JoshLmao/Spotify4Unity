using Spotify4Unity.Dtos;

namespace Spotify4Unity.Events
{
    public class UserInfoLoaded : GameEventBase
    {
        public UserInfo Info { get; set; }
        public UserInfoLoaded(UserInfo info)
        {
            Info = info;
        }
    }
}