using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class S4UUtility : MonoBehaviour
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
}
