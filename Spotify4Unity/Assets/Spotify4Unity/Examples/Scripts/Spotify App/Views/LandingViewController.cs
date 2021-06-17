using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingViewController : ViewControllerBase
{
    [SerializeField]
    private GameObject _freeUserWarningParent;

    private List<Action> _dispatcher = new List<Action>();

    private void Start()
    {
        if (_freeUserWarningParent != null)
        { 
            _freeUserWarningParent.SetActive(false);
        }
    }

    private void Update()
    {
        if (_dispatcher.Count > 0)
        {
            foreach (Action actn in _dispatcher)
                actn.Invoke();

            _dispatcher.Clear();
        }
    }

    protected override async void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        if (client != null)
        {
            bool isPremium = await S4UUtility.IsUserPremium(client);
            _dispatcher.Add(() =>
            {
                _freeUserWarningParent.SetActive(!isPremium);
            });
        }
    }
}
