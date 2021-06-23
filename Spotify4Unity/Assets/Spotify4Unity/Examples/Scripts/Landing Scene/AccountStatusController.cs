using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple controller script for signing in/out of Spotify using S4U
/// This is an example script of how/what you could/should implement
/// </summary>
public class AccountStatusController : SpotifyServiceListener
{
    public bool ServiceAuthOnStart = false;
    
    [SerializeField]
    private Button _signInButton, _signOutButton;

    private void Start()
    {
        SpotifyService.Instance.AuthorizeUserOnStart = ServiceAuthOnStart;

        if (_signInButton != null)
        {
            _signInButton.onClick.AddListener(() => this.OnSignIn());
        }
        if (_signOutButton != null)
        {
            _signOutButton.onClick.AddListener(() => this.OnSignOut());
        }

        if (!ServiceAuthOnStart)
        {
            _signInButton.gameObject.SetActive(true);
            _signOutButton.gameObject.SetActive(false);
        }
    }

    private void OnSignIn()
    {
        SpotifyService service = SpotifyService.Instance;

        if (!service.IsConnected)
        {
            service.AuthorizeUser();
        }
        else
        {
            Debug.LogError("Can't sign in. Already connected");
        }
    }

    private void OnSignOut()
    {
        SpotifyService service = SpotifyService.Instance;
        if (service.IsConnected)
        {
            service.DeauthorizeUser();
        }
    }

    protected override void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        bool isConnected = client != null;
        _signInButton.gameObject.SetActive(!isConnected);
        _signOutButton.gameObject.SetActive(isConnected);
    }
}
