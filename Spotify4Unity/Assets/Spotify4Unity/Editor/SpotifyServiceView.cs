using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor view for the main Spotify Service
/// </summary>
[CustomEditor(typeof(SpotifyService))]
public class SpotifyServiceEditor : Editor
{
    private SpotifyService _service;

    private void OnEnable()
    {
        _service = (SpotifyService)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Open Spotify4Unity Repo"))
            Application.OpenURL("https://github.com/JoshLmao/Spotify4Unity/");
        if (GUILayout.Button("Open Spotify Dashboard"))
            Application.OpenURL("https://developer.spotify.com/dashboard/");
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Spotify Service Options", EditorStyles.boldLabel);
        
        GUIContent content;

        content = new GUIContent("Authorize On Start", "Should the service authorize the user on Start(). If disabled, service will not start until you call SpotifyService.StartService()");
        _service.AuthorizeUserOnStart = EditorGUILayout.Toggle(content, _service.AuthorizeUserOnStart);

        content = new GUIContent("Authentification Type", "Which method of authentification to use with the service.");
        _service.AuthType = (AuthenticationType)EditorGUILayout.EnumPopup(content, _service.AuthType);

        // Validate if a config file is next to service, display warning if not
        GameObject selectedGO = Selection.activeGameObject;
        if (selectedGO != null)
        {
            if (selectedGO.GetComponent<AuthorizationConfig>() == null)
            {
                EditorGUILayout.HelpBox("Add the correct configuration component next to this service! The component's name ends with '_AuthConfig'", MessageType.Warning);
            }
        }
    }
}
