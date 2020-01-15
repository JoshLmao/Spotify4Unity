using Spotify4Unity.Dtos;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TracksContainer : SearchContainer<Track>
{
    protected override void SetPrefabInfo(GameObject instantiatedPrefab, Track track)
    {
        base.SetPrefabInfo(instantiatedPrefab, track);

        SetChildText(instantiatedPrefab, "Title", track.Title);
        SetChildText(instantiatedPrefab, "Artist", System.String.Join(", ", track.Artists.Select(x => x.Name)));
        SetChildText(instantiatedPrefab, "Album", track.Album);

        instantiatedPrefab.transform.Find("PlayBtn").GetComponent<Button>().onClick.AddListener(() => OnPlayTrack(track));
        instantiatedPrefab.transform.Find("QueueBtn").GetComponent<Button>().onClick.AddListener(() => OnQueueTrack(track));
    }

    private void SetChildText(GameObject parent, string childName, string content)
    {
        parent.transform.Find(childName).GetComponent<Text>().text = content;
    }

    private void OnPlayTrack(Track t)
    {
        SpotifyService.PlayTrack(t);
    }

    private void OnQueueTrack(Track t)
    {
        SpotifyService.QueueTrack(t);
    }
}