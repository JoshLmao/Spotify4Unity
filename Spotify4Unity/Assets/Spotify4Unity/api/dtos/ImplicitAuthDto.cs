namespace Spotify4Unity.Dtos
{
    public class ImplicitAuthDto
    {
        public string AccessToken { get; set; }
        public double ExpiresIn { get; set; }
        public string TokenType { get; set; }
    }
}