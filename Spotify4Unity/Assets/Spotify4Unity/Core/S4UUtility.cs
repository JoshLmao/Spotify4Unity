using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class S4UUtility
{
    /// <summary>
    /// Loads an image from a url and runs an action on load
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
            Debug.Log(www.error);
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
    /// <returns></returns>
    public static async Task<IEnumerable<T>> GetAllOfPagingAsync<T>(SpotifyAPI.Web.SpotifyClient client, SpotifyAPI.Web.Paging<T> startingPageList) where T : class
    {
        List<T> list = new List<T>();
        while (startingPageList.Next != null)
        {
            // Add current range and await next set of items
            list.AddRange(startingPageList.Items);
            startingPageList = await client.NextPage(startingPageList);
        }
        // Return final list once complete
        return list;
    }
}
