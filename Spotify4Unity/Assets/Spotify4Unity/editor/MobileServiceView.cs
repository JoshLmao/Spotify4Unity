using Spotify4Unity;
using Spotify4Unity.Bridges;
using SpotifyAPI.Web.Enums;
using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom view for the Android Script for Spotify4Unity
/// </summary>
[CustomEditor(typeof(MobileSpotifyService))]
public class MobileServiceView : Spotify4UnityBaseView
{
    private MobileSpotifyService m_service = null;

    private static bool m_displayAdvancedSettings = true;

    private void OnEnable()
    {
        m_service = (MobileSpotifyService)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Mobile Spotify Authorization", EditorStyles.boldLabel);

        GUIContent content = null;

        // Platform
        content = new GUIContent("Platform", "Select the platform you're currently building for");
        m_service.Platform = (MobileSpotifyService.Platforms)EditorGUILayout.EnumPopup(content, m_service.Platform);

        // Warning box, platform specific TL;DR instructions
        string text = $"Make sure you select the right platform and follow the correct setup instructions. {Environment.NewLine}{Environment.NewLine}";
        if (m_service.Platform == MobileSpotifyService.Platforms.Android)
            text += "Android: Make sure you setup your 'AndroidManifest.xml' correctly";
        else if (m_service.Platform == MobileSpotifyService.Platforms.iOS)
            text += "iOS: Make sure you setup your 'Spotify4UnityDelegate.mm' and 'link.xml' files AND imported the 'JSON.NET for Unity' asset";
        EditorGUILayout.HelpBox(text, MessageType.Warning);

        // Client ID
        content = new GUIContent("Client Id", "The Client Id of your application, from Spotify Dashboard");
        m_service.ClientId = EditorGUILayout.TextField(content, m_service.ClientId);
        
        // Scheme
        content = new GUIContent("Scheme", "Name of the scheme to use, should match the same as inside your AndroidManifest.xml inside your 'intent-filter'");
        m_service.Scheme = EditorGUILayout.TextField(content, m_service.Scheme);
        
        // Redirect URL
        content = new GUIContent("Redirect URL Suffix", "Rest of the redirect URL after the scheme. If you're unsure what to put here, make it a unique path like 'oauth/spotify/'");
        m_service.RedirectUrlSuffix = EditorGUILayout.TextField(content, m_service.RedirectUrlSuffix);

        EditorGUILayout.HelpBox($"Your full redirect url will be '{m_service.Scheme}://{m_service.RedirectUrlSuffix}'.{Environment.NewLine}This needs to match the redirect url inside your Spotify Dashboard", MessageType.Info);

        EditorGUILayout.Space();

        // Auto Connect
        content = new GUIContent("Auto Connect", "Should the service start the authorize process on Start");
        m_service.AutoConnect = EditorGUILayout.Toggle(content, m_service.AutoConnect);

        // Reuse Auth Token
        content = new GUIContent("Reuse Auth Token", "Should the service save the current token to reuse on reopen the application");
        m_service.ReuseAuthTokens = EditorGUILayout.Toggle(content, m_service.ReuseAuthTokens);

        // Auto Reauthorize
        content = new GUIContent("Auto Reauthorize", "Should the service prompt the user for reauthorization when the current token has expired - " +
            "WARNING: Will instantly reauthorize the app and can happen when the user doesn't expect unless told");
        m_service.AutoReauthorize = EditorGUILayout.Toggle(content, m_service.AutoReauthorize);

        if(m_service.AutoReauthorize)
            EditorGUILayout.HelpBox("This will automatically open a browser for authentification when it expires. It's recommended that you advice the user that this will happen", MessageType.Warning);

        EditorGUILayout.Space();

        // Log Level
        content = new GUIContent("Log Level", "Level of logs from Spotify4Unity that should be output to Unity console. Must have 'S4U_LOGS' in your 'Scripting Define Symbols' to show them");
        m_service.LogLevel = (Analysis.LogLevel)EditorGUILayout.EnumPopup(content, m_service.LogLevel);

        EditorGUILayout.Space();

#if UNITY_EDITOR
        EditorGUILayout.LabelField("Editor Debug Authorization", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Please read the 'Mobile Setup' page of the Wiki for more information on being able to test in Editor", MessageType.Info);

        GUILayout.BeginHorizontal();

        content = new GUIContent("Debug Redirect URL", "The Client Id of your application, from Spotify Dashboard");
        string pcRedirect = EditorGUILayout.TextField(content, "http://localhost:4002");

        if (GUILayout.Button("Authorize"))
        {
            Application.OpenURL(m_service.GetAuthURL(m_service.ClientId, pcRedirect));
        }
        GUILayout.EndHorizontal();

        content = new GUIContent("URL Authentification", "The Client Id of your application, from Spotify Dashboard");
        m_service.DebugURLAuthorization = EditorGUILayout.TextField(content, m_service.DebugURLAuthorization);

        EditorGUILayout.Space();
#endif

        // Advanced Settings
        m_displayAdvancedSettings = EditorGUILayout.Foldout(m_displayAdvancedSettings, "Advanced Settings");
        if (m_displayAdvancedSettings)
        {
            EditorGUILayout.HelpBox("Selecting different scopes can and will break functionality! Read the Wiki and understand how 'Scopes' work before changing them", MessageType.Warning);

            string[] enums = Enum.GetNames(typeof(Scope));
            m_service.Scopes = (Scope)EditorGUILayout.MaskField("API Scopes", (int)m_service.Scopes, enums);

            // Connection Timeout
            content = new GUIContent("Connection Timeout (s)", "Amount of seconds before failing the authorization process");
            m_service.ConnectionTimeout = EditorGUILayout.IntField(content, m_service.ConnectionTimeout);
            // Update Frequency (ms)
            content = new GUIContent("Update Frequency (ms)", "Amount of milliseconds between doing an internal update check");
            m_service.UpdateFrequencyMs = EditorGUILayout.IntField(content, m_service.UpdateFrequencyMs);
        }

        EditorGUILayout.Space();
        MarkDirty();

        // Adds the iOS bridge script to the object if iOS platform is selected
        if (m_service.Platform == MobileSpotifyService.Platforms.iOS)
        {
            if (Selection.activeGameObject.GetComponent<iOSBridge>() == null)
                Selection.activeGameObject.AddComponent<iOSBridge>();
        }
        else
        {
            iOSBridge component = Selection.activeGameObject.GetComponent<iOSBridge>();
            if (this != null && component != null)
            {
                DestroyImmediate(component);

                EditorGUIUtility.ExitGUI();
            }
        }
    }
}
