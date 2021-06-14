using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpotifyAPI.Web;

public class SpotifyService : Singleton<SpotifyService>
{
    // Spotify Dashboard client id
    public string SPOTIFY_CLIENT_ID = "8c42bb0a9bd8483986929038af40ed4a";

    public SpotifyService()
    {
        StartService();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private async void StartService()
    {
        var client = new SpotifyClient(SPOTIFY_CLIENT_ID);

        var me = await client.UserProfile.Current();
        Debug.Log(me.DisplayName);
    }
}
