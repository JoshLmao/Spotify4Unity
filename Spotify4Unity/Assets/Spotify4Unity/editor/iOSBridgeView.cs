using Spotify4Unity.Bridges;
using UnityEditor;

/// <summary>
/// Custom view for the iOS Bridge script for Spotify4Unity
/// </summary>
[CustomEditor(typeof(iOSBridge))]
public class iOSBridgeView : Spotify4UnityBaseView
{
    private iOSBridge m_bridge = null;

    private void OnEnable()
    {
        m_bridge = (iOSBridge)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Spotify4Unity iOS Bridge", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Helper class for performing iOS platform specific code for Spotify4Unity. If you are building for iOS, " +
            "make sure this is next to the 'Mobile Spotify Service', otherwise it can be removed", EditorStyles.wordWrappedLabel);
    }
}
