using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchViewController : ViewControllerBase
{
    [SerializeField]
    private InputField _searchField;
    [SerializeField]
    private Button _searchBtn;

    [SerializeField]
    private Transform _tracksParent;
    [SerializeField]
    private GameObject _singleSearchTrackPrefab;

    private SearchResponse _lastSearchResponse;

    private List<Action> _dispatcher = new List<Action>();

    void Start()
    {
        if (_searchBtn != null)
        {
            _searchBtn.onClick.AddListener(this.OnPerformSearch);
        }
    }

    private void Update()
    {
        if (_dispatcher.Count > 0)
        {
            foreach (Action actn in _dispatcher)
                actn?.Invoke();

            _dispatcher.Clear();
        }
    }

    private async void OnPerformSearch()
    {
        var client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null && _searchField != null)
        {
            string query = _searchField.text;
            SearchRequest request = new SearchRequest(SearchRequest.Types.All, query);

            _lastSearchResponse = await client.Search.Item(request);

            Debug.Log($"Spotify app | Query '{query}' returned '{_lastSearchResponse.Tracks.Total.Value}'");

            _dispatcher.Add(() =>
            {
                UpdateUI();
            });
        }
    }

    private void UpdateUI()
    {
        if (_tracksParent != null)
        {
            if (_tracksParent.transform.childCount > 0)
            {
                foreach (Transform child in _tracksParent.transform)
                    Destroy(child.gameObject);
            }

            foreach(FullTrack track in _lastSearchResponse.Tracks.Items)
            {
                GameObject instGO = Instantiate(_singleSearchTrackPrefab, _tracksParent);
                instGO.GetComponent<SingleSearchTrackController>().SetTrack(track);
            }

            float singlePrefabHeight = _singleSearchTrackPrefab.GetComponent<RectTransform>().rect.height;
            VerticalLayoutGroup group = _tracksParent.GetComponent<VerticalLayoutGroup>();
            float totalHeight = (singlePrefabHeight + group.padding.top + group.padding.bottom + group.spacing) * _lastSearchResponse.Tracks.Items.Count;

            _tracksParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }
    }
}
