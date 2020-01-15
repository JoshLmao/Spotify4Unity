using Spotify4Unity;
using SpotifyAPI.Web.Enums;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom view for the PC Script for Spotify4Unity
/// </summary>
[CustomEditor(typeof(SpotifyService))]
public class SpotifyServiceView : Spotify4UnityBaseView
{
    private SpotifyService m_service = null;

    private static bool m_displayAdvancedSettings = false;

    private void OnEnable()
    {
        m_service = (SpotifyService)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("PC Spotify Authorization", EditorStyles.boldLabel);

        // Client ID
        GUIContent content = new GUIContent("Client Id", "The Client Id of your application, from Spotify Dashboard");
        m_service.ClientId = EditorGUILayout.TextField(content, m_service.ClientId);
        // Secret ID
        content = new GUIContent("Secret Id", "The Secret Id of your application, from Spotify Dashboard");
        m_service.SecretId = EditorGUILayout.TextField(content, m_service.SecretId);
        // Connection Port
        content = new GUIContent("Connection Port", "The port to use when authenticating. Should be the same as your 'Redirect URI(s)' in your application's Spotify Dashboard");
        m_service.ConnectionPort = EditorGUILayout.IntField(content, m_service.ConnectionPort);

        EditorGUILayout.Space();

        // Auto Connect
        content = new GUIContent("Auto Connect", "Should the service call Connect() on Start");
        m_service.AutoConnect = EditorGUILayout.Toggle(content, m_service.AutoConnect);

        // Reuse Auth Token
        content = new GUIContent("Reuse Auth Token", "When enabled, the service will only need authentification once. The service uses a 'refresh token' to keep getting new authentification from Spotify without prompting the user." + System.Environment.NewLine + "The service also automatically keeps getting new authentification when the old one expires");
        m_service.ReuseAuthTokens = EditorGUILayout.Toggle(content, m_service.ReuseAuthTokens);

        EditorGUILayout.Space();


        // Log Level
        content = new GUIContent("Log Level", "Level of logs that should be output to Unity console");
        m_service.LogLevel = (Analysis.LogLevel)EditorGUILayout.EnumPopup(content, m_service.LogLevel);

        EditorGUILayout.Space();

        m_displayAdvancedSettings = EditorGUILayout.Foldout(m_displayAdvancedSettings, "Advanced Settings");
        if (m_displayAdvancedSettings)
        {
            EditorGUILayout.HelpBox("Selecting different scopes CAN & WILL break functionality! Read the Wiki and understand how 'Scopes' work before changing them", MessageType.Warning);

            string[] enums = Enum.GetNames(typeof(Scope));
            m_service.Scopes = (Scope)EditorGUILayout.MaskField("API Scopes", (int)m_service.Scopes, enums);

            // Connection Timeout
            content = new GUIContent("Connection Timeout (s)", "Amount of seconds before failing the authorization process");
            m_service.ConnectionTimeout = EditorGUILayout.IntField(content, m_service.ConnectionTimeout);
            // Update Frequency (ms)
            content = new GUIContent("Update Frequency (ms)", "Should the service prompt the user for reauthorization when the current token has exired - Warning: Will instantly open a browser");
            m_service.UpdateFrequencyMs = EditorGUILayout.IntField(content, m_service.UpdateFrequencyMs);
        }

        EditorGUILayout.Space();
        MarkDirty();
    }
}
