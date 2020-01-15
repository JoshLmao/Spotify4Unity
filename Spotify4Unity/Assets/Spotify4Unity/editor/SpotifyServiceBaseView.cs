using Spotify4Unity;
using System;
using UnityEditor;

[CustomEditor(typeof(SpotifyServiceBase))]
public class SpotifyServiceBaseView : Spotify4UnityBaseView
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.HelpBox("Error: Wrong script used" + Environment.NewLine + "This is the base class. Make sure you use the main Spotify Service classes", MessageType.Error);
    }
}
