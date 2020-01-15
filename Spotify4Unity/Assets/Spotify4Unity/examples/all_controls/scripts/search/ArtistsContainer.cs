using Spotify4Unity.Dtos;
using UnityEngine;
using UnityEngine.UI;

public class ArtistsContainer : SearchContainer<Artist>
{
    protected override void SetPrefabInfo(GameObject instantiatedPrefab, Artist artist)
    {
        base.SetPrefabInfo(instantiatedPrefab, artist);

        instantiatedPrefab.GetComponentInChildren<Text>().text = artist.Name;
        instantiatedPrefab.transform.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(() => OnPlayArtist(artist));

        if (!string.IsNullOrEmpty(artist.ImageUrl))
        {
            StartCoroutine(Spotify4Unity.Helpers.Utility.LoadImageFromUrl(artist.ImageUrl, Spotify4Unity.Enums.Resolution.Original, x => OnLoadedArtistPreview(instantiatedPrefab, x, instantiatedPrefab.GetComponentInChildren<Image>())));
        }
    }

    private void OnPlayArtist(Artist artist)
    {
        SpotifyService.PlayArtist(artist);
    }

    private void OnLoadedArtistPreview(GameObject go, Sprite sprite, Image img)
    {
        if (go != null && go.activeInHierarchy)
        {
            img.sprite = sprite;
        }
    }
}

