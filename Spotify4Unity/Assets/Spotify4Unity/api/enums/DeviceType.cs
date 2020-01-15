namespace Spotify4Unity.Enums
{
    /// <summary>
    /// Different devices able to run Spotify
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Unable to define the device type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Normal computer
        /// </summary>
        Computer,
        /// <summary>
        /// Tablet device
        /// </summary>
        Tablet,
        /// <summary>
        /// Mobile phone
        /// </summary>
        Smartphone,
        /// <summary>
        /// Smart speaker
        /// </summary>
        Speaker,
        /// <summary>
        /// Smart TV
        /// </summary>
        TV,
        /// <summary>
        /// Audio/Video Reciever
        /// </summary>
        AudioVideoReciever,
        /// <summary>
        /// Set-Top Box Device
        /// </summary>
        SetTopBox,
        /// <summary>
        /// Standard audio dongle
        /// </summary>
        AudioDongle,
        /// <summary>
        /// Games console
        /// </summary>
        GameConsole,
        /// <summary>
        /// Chromecase device capable of audio/video
        /// </summary>
        ChromecastVideo,
        /// <summary>
        /// Chromecase device capable of only audio
        /// </summary>
        ChromecastAudio,
        /// <summary>
        /// Car automobile
        /// </summary>
        Automobile,
    }
}
