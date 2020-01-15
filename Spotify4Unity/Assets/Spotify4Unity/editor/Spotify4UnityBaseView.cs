using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Spotify4UnityBaseView : Editor
{
    public override void OnInspectorGUI()
    {
        AddSharedHeader();
    }

    /// <summary>
    /// Shared header between all Spotify4Unity views
    /// </summary>
    protected void AddSharedHeader()
    {
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Wiki"))
            Application.OpenURL("https://github.com/JoshLmao/Spotify4Unity/wiki/");
        if (GUILayout.Button("Open Spotify Dashboard"))
            Application.OpenURL("https://developer.spotify.com/dashboard/");
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    /// <summary>
    /// Mark the target dirty if any of the properties have changed
    /// </summary>
    protected void MarkDirty()
    {
        if (GUI.changed && !EditorApplication.isPlaying)
        {
            // Force update
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
