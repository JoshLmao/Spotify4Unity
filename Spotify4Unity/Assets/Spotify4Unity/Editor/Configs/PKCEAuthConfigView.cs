using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor view for the PKCE config class
/// </summary>
[CustomEditor(typeof(PKCE_AuthConfig))]
public class PKCEAuthConfigView : AuthorizationConfigView
{
    private PKCE_AuthConfig _pkceConfig;

    protected override void OnEnable()
    {
        base.OnEnable();

        _pkceConfig = (PKCE_AuthConfig)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUIContent content;

        // Title
        EditorGUILayout.LabelField("PKCE Config", EditorStyles.boldLabel);

        // PKCE codes length
        content = new GUIContent("Custom Length", "Length of the automatically generated verifier string. Value defaults to 100, only change if you know what you're doing");
        _pkceConfig.Length = EditorGUILayout.IntField(content, _pkceConfig.Length);

        // PKCE codes verifier
        content = new GUIContent("Custom Verifier", "Your own custom verifier string. If this is set, it takes priority over the length. Value defaults to empty, only change if you know what you're doing");
        _pkceConfig.Verifier = EditorGUILayout.TextField(content, _pkceConfig.Verifier);

        EditorGUILayout.Space();

        // Save type
        content = new GUIContent("Token Save Type", "Method to use for saving and reusing the PKCE token");
        _pkceConfig.TokenSaveType = (PKCETokenSaveType)EditorGUILayout.EnumPopup(content, _pkceConfig.TokenSaveType);

        // Display only TokenPath or PlayerPrefsKey UI depending on TokenSaveType
        if (_pkceConfig.TokenSaveType == PKCETokenSaveType.File)
        {
            // File Path
            content = new GUIContent("Local File Path", "If TokenSaveType is File, use this local path to store the PKCE authorization token");
            _pkceConfig.TokenPath = EditorGUILayout.TextField(content, _pkceConfig.TokenPath);
        }
        else if (_pkceConfig.TokenSaveType == PKCETokenSaveType.PlayerPrefs)
        {
            // PlayerPrefs key
            content = new GUIContent("Player Preferences Key", "If TokenSaveType is PlayerPrefs, store the current credentials in player preferences using this key");
            _pkceConfig.PlayerPrefsKey = EditorGUILayout.TextField(content, _pkceConfig.PlayerPrefsKey);
        }
    }
}
