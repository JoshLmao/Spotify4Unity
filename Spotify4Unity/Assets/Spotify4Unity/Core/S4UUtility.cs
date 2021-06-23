using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Static methods that are useful when interfacing with the SpotifyAPI.NET library
/// </summary>
public class S4UUtility
{
    /// <summary>
    /// Loads an image from a url and runs an action on load. You need to call this method using StartCoroutine(LoadImageFromUrl(...))
    /// </summary>
    /// <param name="url">The url of the image</param>
    /// <param name="resolution">The target resolution to resize the image to</param>
    /// <param name="onLoaded">Lambda expression for what to do once the load has finished</param>
    /// <returns></returns>
    public static IEnumerator LoadImageFromUrl(string url, System.Action<Sprite> onLoaded)
    {
        if (string.IsNullOrEmpty(url))
            yield return null;

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log($"LoadImageFromUrl error on url '{url}' - {www.error}");
        }
        else
        {
            Texture2D webTexture2D = ((DownloadHandlerTexture)www.downloadHandler).texture as Texture2D;
            Sprite sprite = Sprite.Create(webTexture2D, new Rect(0, 0, webTexture2D.width, webTexture2D.height), Vector2.zero);
            
            if (onLoaded != null)
            {
                onLoaded.Invoke(sprite);
            }
        }
    }

    /// <summary>
    /// Converts milliseconds into a formatted time string like "00:00"
    /// </summary>
    /// <param name="milliseconds">Time in milliseconds</param>
    /// <returns></returns>
    public static string MsToTimeString(int milliseconds)
    {
        if (milliseconds < 0)
        {
            return "00:00";
        }

        int totalSeconds = milliseconds / 1000;

        int currentSeconds = totalSeconds % 60;
        int minutes = totalSeconds / 60;

        string secondsStr = "";
        if (currentSeconds < 10)
        {
            secondsStr = "0";
        }
        secondsStr += currentSeconds.ToString();

        return $"{minutes}:{secondsStr}";
    }

    /// <summary>
    /// Gets all entries in a paging list and returns the result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="client">The current SpotifyClient instance</param>
    /// <param name="startingPageList">The beginning of a paging list (first 20/X entries)</param>
    /// <param name="limit">Limit the amount of paging items to retrieve. Limit must be a multiple of 20</param>
    /// <returns></returns>
    public static async Task<IEnumerable<T>> GetAllOfPagingAsync<T>(SpotifyClient client, Paging<T> startingPageList, int limit = -1) where T : class
    {
        List<T> list = new List<T>();
        while (startingPageList.Next != null)
        {
            // Add current range and await next set of items
            list.AddRange(startingPageList.Items);
            startingPageList = await client.NextPage(startingPageList);

            // if a limit is given and list is more than the limit, break and return
            if (limit > 0 && list.Count > limit)
            {
                break;
            }
        }
        // Return final list once complete
        return list;
    }

    /// <summary>
    /// Converts a list of artists to a string with a separator
    /// </summary>
    /// <returns></returns>
    public static string ArtistsToSeparatedString(string separator, List<SpotifyAPI.Web.SimpleArtist> artists)
    {
        if (artists == null || (artists != null && artists.Count <= 0))
        {
            return string.Empty;
        }
        else
        {
            return string.Join(separator, artists.Select(x => x.Name));
        }
    }

    /// <summary>
    /// Gets all possible API scopes for the Spotify API. You can access individual scopes in the SpotifyAPI.Web.Scopes class
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllScopes()
    {
        return new List<string>()
        {
            Scopes.AppRemoteControl,
            Scopes.PlaylistModifyPrivate,
            Scopes.PlaylistModifyPublic,
            Scopes.PlaylistReadCollaborative,
            Scopes.PlaylistReadPrivate,
            Scopes.Streaming,
            Scopes.UgcImageUpload,
            Scopes.UserFollowModify,
            Scopes.UserFollowRead,
            Scopes.UserLibraryModify,
            Scopes.UserLibraryRead,
            Scopes.UserModifyPlaybackState,
            Scopes.UserReadCurrentlyPlaying,
            Scopes.UserReadEmail,
            Scopes.UserReadPlaybackPosition,
            Scopes.UserReadPlaybackState,
            Scopes.UserReadPrivate,
            Scopes.UserReadRecentlyPlayed,
            Scopes.UserTopRead,
        };
    }

    /// <summary>
    /// Gets a string to display a song and it's properties, separated with a dash. For example, "BLACKPINK - Don't Know What To Do"
    /// </summary>
    /// <param name="currentTrack"></param>
    /// <returns></returns>
    public static string GetTrackString(FullTrack track)
    {
        string artists = S4UUtility.ArtistsToSeparatedString(",", track.Artists);
        return artists + " - " + track.Name;
    }

    /// <summary>
    /// Checks if the current user who provided authorization has Spotify Premium, allowing use to the Spotify API.
    /// You still need to check the authorization scopes to see if you can access other areas of the API.
    /// </summary>
    /// <param name="client">The current client</param>
    /// <returns>True if the user has premium, if the user's product property is "premium"</returns>
    public static async Task<bool> IsUserPremium(SpotifyClient client)
    {
        if (client != null)
        {
            PrivateUser user = await client.UserProfile.Current();
            if (user != null)
            {
                return user.Product == "premium";
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the two lists of artists have any difference, such as in name or length
    /// </summary>
    /// <param name="a">First list of artists</param>
    /// <param name="b">Second list of artists</param>
    /// <returns>If the two lists of artists differ</returns>
    public static bool HasArtistsChanged(List<SimpleArtist> a, List<SimpleArtist> b)
    {
        // If lists are different size, it's changed
        if (a.Count != b.Count)
            return true;

        // Iterate through equal length lists for name difference
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].Name != b[i].Name)
                return true;    // Name differs, changed
        }

        // No change
        return false;
    }

    /// <summary>
    /// Able to determine the expire time, from a created at date time and expires in seconds time
    /// </summary>
    /// <param name="createdAt">DateTime the token was created</param>
    /// <param name="expiresIn">The amount of seconds the token will expire in</param>
    /// <returns></returns>
    public static DateTime GetTokenExpiry(DateTime createdAt, int expiresIn)
    {
        return createdAt.AddSeconds(expiresIn);
    }

    /// <summary>
    /// Gets the lowest resolution image stored in an array of images, but is still above the minimumWidth and minimumHeight.
    /// </summary>
    /// <param name="images">The list of images available</param>
    /// <param name="minimumWidth">Minimum width of the target image</param>
    /// <param name="minimumHeight">Minimum height of the target image</param>
    /// <returns></returns>
    public static Image GetLowestResolutionImage(List<Image> images, int minimumWidth = 50, int minimumHeight = 50)
    {
        if (images == null || images != null && images.Count <= 0)
        {
            return null;
        }

        Image lowest = null;
        foreach(Image img in images)
        {
            if (lowest == null)
            {
                lowest = img;
            }
            else
            {
                // Check if current img width and height is less than current lowest
                if (img.Width < lowest.Width && img.Height < lowest.Height)
                {
                    // Check that current is more than minimum width
                    if (img.Width > minimumWidth && img.Height > minimumHeight)
                    {
                        lowest = img;
                    }
                }
            }
        }

        return lowest;
    }
}
