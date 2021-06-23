using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

    [SerializeField]
    private Transform _artistsParent;
    [SerializeField]
    private GameObject _singleArtistPrefab;

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
            UpdateSongsList();
            UpdateArtistsList();
        }
    }

    void UpdateSongsList()
    {
        UpdateScrollView(_lastSearchResponse.Tracks.Items, _singleSearchTrackPrefab, _tracksParent, RectTransform.Axis.Vertical, (instGO, dataObj) =>
        {
            instGO.GetComponent<SingleSearchTrackController>().SetTrack(dataObj);
        });
    }

    void UpdateArtistsList()
    {
        var artistsFiltered = _lastSearchResponse.Artists.Items.Select((x) =>
        {
            if (x.Type == "artist")
                return x;
            return null;
        }).ToList();

        UpdateScrollView(artistsFiltered, _singleArtistPrefab, _artistsParent, RectTransform.Axis.Horizontal, (instGO, dataObj) =>
        {
            instGO.GetComponent<SingleArtistWidgetController>().SetArtist(dataObj);
        });
    }

    void UpdateScrollView<T>(List<T> objList, GameObject prefab, Transform parent, RectTransform.Axis axis, Action<GameObject, T> afterPrefabInst) where T : class
    {
        if (parent.transform.childCount > 0)
        {
            foreach (Transform child in parent.transform)
                Destroy(child.gameObject);
        }

        foreach (T obj in objList)
        {
            GameObject instGO = Instantiate(prefab, parent);
            afterPrefabInst?.Invoke(instGO, obj);
        }


        // get width/height of prefab
        float singlePrefabHeightOrWidth = prefab.GetComponent<RectTransform>().rect.height;
        if (axis == RectTransform.Axis.Horizontal)
            singlePrefabHeightOrWidth = prefab.GetComponent<RectTransform>().rect.width;

        // Add on spacing + padding
        HorizontalOrVerticalLayoutGroup group = parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
        float total = singlePrefabHeightOrWidth + +group.spacing;

        // Padding on either top/bottom or right/left
        if (axis == RectTransform.Axis.Horizontal)
        {
            total += group.padding.left + group.padding.right;
        }
        else if (axis == RectTransform.Axis.Vertical)
        {
            total += group.padding.top + group.padding.bottom;
        }

        total = total * objList.Count; 

        parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(axis, total);
    }
}
