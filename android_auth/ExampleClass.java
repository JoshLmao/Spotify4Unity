package com.JoshLmao.Spotify4Unity;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import android.content.Intent;
import android.net.Uri;

// Handles a redirect url that has been configured for Spotify4Unity's specific use on Android
// http://oferei.com/2013/06/serverless-instagram-authentication/
// If you're not sure what to do, check out the Spotify4Unity wiki (https://github.com/JoshLmao/Spotify4Unity/wiki/)
public class Spotify4Unity extends UnityPlayerActivity {
	
	// Scheme name set inside AndroidManifest.xml
	public static String Scheme = "unknownScheme";
	// Game Object name used that contains receiving method
	public static String GameObjectName = "SpotifyService";
	// Name of method to receive auth string
	public static String MethodName = "OnRecievedAndroidToken";

	@Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        handleAccessToken(intent);
    }
	
	// Handles directing the data to our Unity application, requires static properties to be set first
    private void handleAccessToken(Intent intent) {
        Uri uri = intent.getData();
        if (uri != null && uri.toString().startsWith(Scheme)) {
            String accessToken = uri.getFragment();
            UnityPlayer.UnitySendMessage(GameObjectName, MethodName, accessToken);
        }
    }
}
