using Spotify4Unity;
using UnityEngine;
using UnityEngine.UI;

public class ConnectToSpotify : MonoBehaviour
{
    public SpotifyServiceBase Service;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnConnectToSpotify);
    }

    private void OnConnectToSpotify()
    {
        if (Service != null)
            Service.Connect();
        else
            Debug.LogError("Service hasn't been set!");
    }
}
