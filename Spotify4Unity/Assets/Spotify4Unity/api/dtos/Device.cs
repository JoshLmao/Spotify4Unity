using Spotify4Unity.Enums;

namespace Spotify4Unity.Dtos
{
    /// <summary>
    /// A device that is capable of having Spotify streamed to
    /// </summary>
    public class Device
    {
        /// <summary>
        /// The display name of the device
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The internal ID of the device
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Is the device active and controlling Spotify
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// The current volume percentage of the device
        /// </summary>
        public int VolumePercent { get; set; }
        /// <summary>
        /// The type of device it is
        /// </summary>
        public DeviceType Type { get; set; }

        public Device(SpotifyAPI.Web.Models.Device d)
        {
            Name = d.Name;
            Id = d.Id;
            IsActive = d.IsActive;
            VolumePercent = d.VolumePercent;
            Type = TypeConverter(d.Type);
        }

        private DeviceType TypeConverter(string type)
        {
            type = type.ToLower();
            switch (type)
            {
                case "computer": //Laptop or desktop computer device
                    return DeviceType.Computer;
                case "tablet": //Tablet PC device
                    return DeviceType.Tablet;
                case "smartphone": // Smartphone device
                    return DeviceType.Smartphone;
                case "speaker": //Smartphone device
                    return DeviceType.Speaker;
                case "tv": //Television device
                    return DeviceType.TV;
                case "avr": //Audio/Video reciever device
                    return DeviceType.AudioVideoReciever;
                case "stb": //Set-Top Box Device
                    return DeviceType.SetTopBox;
                case "audio_dongle": //Audio dongle device
                    return DeviceType.AudioDongle;
                case "game_console": //Game console device
                    return DeviceType.GameConsole;
                case "cast_video": //Chromecast device
                    return DeviceType.ChromecastVideo;
                case "cast_audio": //Cast for audio device
                    return DeviceType.ChromecastAudio;
                case "audiodongle": //Cast to a dongle which supports audio
                    return DeviceType.AudioDongle;
                case "automobile": //Car device
                    return DeviceType.Automobile;
                default:
                    return DeviceType.Unknown;
            }
        }
    }
}