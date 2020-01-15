using Spotify4Unity;
using UnityEngine;

namespace Spotify4Unity.Bridges
{
    /// <summary>
    /// Bridge class for performing platform specific code for iOS for Spotify4Unity
    /// </summary>
    public class iOSBridge : MonoBehaviour, IiOSServiceBridge
    {
#if UNITY_IOS
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void configure(string gameObjName, string methodName);
#endif

        /// <summary>
        /// Configures the iOS callback to return to the MobileSpotifyService
        /// </summary>
        /// <param name="gameObjectName">Name of the game object the method name is on</param>
        /// <param name="methodName">Name of the method that handles deep linking</param>
        public void Configure(string gameObjectName, string methodName)
        {
#if UNITY_IOS
        // Configure the Spotify4UnityDelegate bridge to call back to our URLOpened function
        configure(gameObjectName, methodName);
#endif
        }
    }
}
