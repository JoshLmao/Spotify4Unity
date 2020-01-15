using Spotify4Unity;
using Spotify4Unity.Dtos;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlbumsContainer : SearchContainer<Album>
{
    protected override void SetPrefabInfo(GameObject instantiatedPrefab, Album album)
    {
        instantiatedPrefab.GetComponentInChildren<Text>().text = album.Name;
        instantiatedPrefab.transform.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(() => OnPlayAlbum(album));

        if (!string.IsNullOrEmpty(album.ImageUrl))
        {
            StartCoroutine(Spotify4Unity.Helpers.Utility.LoadImageFromUrl(album.ImageUrl, Spotify4Unity.Enums.Resolution.Original, x => OnLoadedArtistPreview(x, instantiatedPrefab.GetComponentInChildren<Image>())));
        }
    }

    private void OnPlayAlbum(Album a)
    {
        SpotifyService.PlayAlbum(a);
    }

    private void OnLoadedArtistPreview(Sprite sprite, Image img)
    {
        img.sprite = sprite;
    }
}