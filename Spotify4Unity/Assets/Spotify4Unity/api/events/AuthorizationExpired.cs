using SpotifyAPI.Web.Models;

namespace Spotify4Unity.Events
{
    public class AuthorizationExpired : GameEventBase
    {
        public Token PreviousToken { get; set; }
        public AuthorizationExpired(Token prevToken)
        {
            PreviousToken = prevToken;
        }
    }
}

