using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor view for base config class
/// </summary>
[CustomEditor(typeof(AuthorizationConfig))]
public class AuthorizationConfigView : Editor
{
    private AuthorizationConfig _config;

    private int _selectedScopesFlag = int.MaxValue;

    protected virtual void OnEnable()
    {
        _config = (AuthorizationConfig)target;
    }

    public override void OnInspectorGUI()
    {
        GUIContent content;

        // Title
        EditorGUILayout.LabelField("General Config", EditorStyles.boldLabel);

        // Client ID
        content = new GUIContent("Spotify Client ID", "Your client id, found in your Spotify Dashboard. Don't have an id? Go here: https://developer.spotify.com/dashboard/");
        _config.ClientID = EditorGUILayout.TextField(content, _config.ClientID);

        content = new GUIContent("Redirect URI", "The redirect uri used to pass Spotify authentification onto your app. This exact URI needs to be in your Spotify Dashboard. Dont change this if you don't know what you are doing.");
        _config.RedirectUri = EditorGUILayout.TextField(content, _config.RedirectUri);

        // API scopes
        content = new GUIContent("API Scopes", "All API scopes that will the user will be asked to authorize.");

        List<string> allScopes = S4UUtility.GetAllScopes();
        _selectedScopesFlag = EditorGUILayout.MaskField(content, _selectedScopesFlag, allScopes.ToArray());
        _config.APIScopes = FlagToAPIScopes(_selectedScopesFlag);

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("The 'API Scopes' allow you to pick and choose which elements of the API you wish to use. It is recommended to select only the required scopes you need. \n\nFor more information, view the Spotify API guide on authorization scopes (Click the button below)", MessageType.Info);

        if (GUILayout.Button("Scopes Documentation"))
            Application.OpenURL("https://developer.spotify.com/documentation/general/guides/scopes/");

        EditorGUILayout.Space();
    }

    /// <summary>
    /// Converts a flag number into the enabled API scopes
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    private List<string> FlagToAPIScopes(int flag)
    {
        List<string> allScopes = S4UUtility.GetAllScopes();
        List<string> selectedScopes = new List<string>();

        // Convert integer into binary string "10010101"
        string binaryString = Convert.ToString(flag, 2);
        // Reverse string so order matches list order
        var array = binaryString.ToCharArray();
        Array.Reverse(array);
        binaryString = new string(array);

        // If binary longer than list, all selected
        if (binaryString.Length > allScopes.Count)
        {
            return allScopes;           // All selected
        }
        else if (binaryString.Length <= 0)
        {
            return new List<string>();  // Empty, none selected
        }
        else
        {
            // Iterate through binary string, add scope if it is enabled in bianry string
            for (int i = 0; i < binaryString.Length; i++)
            {
                char currentBinaryValue = binaryString[i];
                if (currentBinaryValue == '1')
                {
                    selectedScopes.Add(allScopes[i]);
                }
            }

            return selectedScopes;
        }
    }
}
