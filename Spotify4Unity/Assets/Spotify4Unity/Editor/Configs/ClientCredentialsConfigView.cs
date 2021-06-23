using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor view for the Client Credentials config
/// </summary>
[CustomEditor(typeof(ClientCredentials_AuthConfig))]
public class ClientCredentialsConfigView : AuthorizationConfigView
{
    private ClientCredentials_AuthConfig _ccConfig;

    protected override void OnEnable()
    {
        base.OnEnable();

        _ccConfig = (ClientCredentials_AuthConfig)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUIContent content;

        // Client secret
        content = new GUIContent("Client Secret", "The client secret id of your app. Can be located in your Spotify dashboard.");
        _ccConfig.ClientSecret = EditorGUILayout.TextField(content, _ccConfig.ClientSecret);
    }
}
