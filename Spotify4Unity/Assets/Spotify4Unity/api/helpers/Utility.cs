using Spotify4Unity.Enums;
using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace Spotify4Unity.Helpers
{
    /// <summary>
    /// Utility class to assist Spotify4Unity
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Converts Resolutions to their correct Rect size
        /// </summary>
        /// <param name="r"></param>
        /// <param name="originalWidth"></param>
        /// <param name="originalHeight"></param>
        /// <returns></returns>
        public static Rect ResolutionToRect(Enums.Resolution r, float originalWidth, float originalHeight)
        {
            switch (r)
            {
                case Enums.Resolution.x64:
                    return new Rect(0f, 0f, 64f, 64f);
                case Enums.Resolution.x128:
                    return new Rect(0f, 0f, 64f, 64f);
                case Enums.Resolution.x256:
                    return new Rect(0f, 0f, 256f, 256f);
                case Enums.Resolution.x512:
                    return new Rect(0f, 0f, 512f, 512f);
                case Enums.Resolution.Original:
                    return new Rect(0f, 0f, originalWidth, originalHeight);
                default:
                    throw new NotImplementedException("Not implemented extra resolution type");
            }
        }

        /// <summary>
        /// Loads an image from a url and runs an action on load
        /// </summary>
        /// <param name="url">The url of the image</param>
        /// <param name="resolution">The target resolution to resize the image to</param>
        /// <param name="onLoaded">Lambda expression for what to do once the load has finished</param>
        /// <returns></returns>
        public static IEnumerator LoadImageFromUrl(string url, Enums.Resolution resolution, Action<Sprite> onLoaded)
        {
            if (string.IsNullOrEmpty(url))
                yield return null;

            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
            yield return webRequest.SendWebRequest();

            while (!webRequest.downloadHandler.isDone)
                yield return new WaitForEndOfFrame();

            Sprite s = null;
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Analysis.LogError($"Unable to load image from url '{url}'", Analysis.LogLevel.Vital);
            }
            else
            {
                Texture2D webTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture as Texture2D;

                // Get the target resolution and scale
                Rect targetResolution = Helpers.Utility.ResolutionToRect(resolution, webTexture.width, webTexture.height);
                TextureScaler.scale(webTexture, (int)targetResolution.width, (int)targetResolution.height, FilterMode.Bilinear);

                s = Sprite.Create(webTexture, new Rect(0f, 0f, targetResolution.width, targetResolution.height), Vector2.zero, targetResolution.width, 0, SpriteMeshType.FullRect);
            }

            onLoaded.Invoke(s);
        }

        /// <summary>
        /// Use if you want to run a Coroutine on a game object that isn't active
        /// </summary>
        /// <param name="routine"></param>
        public static void RunCoroutineEmptyObject(System.Collections.IEnumerator routine)
        {
            var obj = new GameObject("LoadRoutineObject");
            var loadCoroutine = obj.AddComponent<LoadCoroutine>();
            loadCoroutine.StartCoroutine(routine);
            // Destroy after 60 seconds if routine isn't finished
            // ToDo: Fix to destroy once routine is finished
            GameObject.Destroy(obj, 60f);
        }

        /// <summary>
        /// Destroys all children of the parent transform, won't destroy the parent transform passed through
        /// </summary>
        /// <param name="parent">The parent Transform containing children to destroy</param>
        public static void DestroyChildren(Transform parent)
        {
            List<Transform> children = parent.GetComponentsInChildren<Transform>().ToList();
            if (children.Contains(parent))
                children.Remove(parent);

            if (children.Count > 0)
            {
                foreach (Transform child in children)
                    GameObject.Destroy(child.gameObject);
            }
        }
    }
}