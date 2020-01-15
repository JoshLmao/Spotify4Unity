using Newtonsoft.Json;
using SpotifyAPI.Web.Models;
using System;
using UnityEngine;

/// <summary>
/// Manages the saving and loading of the previous Authorization tokens
/// </summary>
public class TokenSaver
{
    [Serializable]
    private class TokenWrapper
    {
        public string AccessToken;
        public string TokenType;
        public double ExpiresIn;
        public string RefreshToken;
        public string Error;
        public string ErrorDescription;
        public string CreateDate;

        public TokenWrapper(Token token)
        {
            AccessToken = token.AccessToken;
            TokenType = token.TokenType;
            ExpiresIn = token.ExpiresIn;
            RefreshToken = token.RefreshToken;
            Error = token.Error;
            ErrorDescription = token.ErrorDescription;
            CreateDate = token.CreateDate.ToString();
        }

        public Token ToToken()
        {
            return new Token()
            {
                AccessToken = AccessToken,
                TokenType = TokenType,
                ExpiresIn = ExpiresIn,
                RefreshToken = RefreshToken,
                Error = Error,
                ErrorDescription = ErrorDescription,
                CreateDate =  DateTime.Parse(CreateDate),
            };
        }
    }

    /// <summary>
    /// Name of where to save the Json string of the current token
    /// </summary>
    public static readonly string PREFS_TOKEN_NAME = "TokenJson";

    /// <summary>
    /// Have the system saved a previous valid token before
    /// </summary>
    /// <returns></returns>
    public static bool HasSavedTokenInfo()
    {
        return !string.IsNullOrEmpty(PlayerPrefs.GetString(PREFS_TOKEN_NAME));
    }

    /// <summary>
    /// Loads a token from the previous file path
    /// </summary>
    /// <returns></returns>
    public static Token LoadToken()
    {
        string json = PlayerPrefs.GetString(PREFS_TOKEN_NAME);
        if (string.IsNullOrEmpty(json))
            return null;

        Token token = JsonUtility.FromJson<TokenWrapper>(json).ToToken();
        return token;
    }

    /// <summary>
    /// Saves the current token to the target file path
    /// </summary>
    /// <param name="token">The last valid token</param>
    /// <returns>Did the save run successfully</returns>
    public static bool SaveToken(Token token)
    {
        if (token == null)
            return false;

        try
        {
            string json = JsonUtility.ToJson(new TokenWrapper(token));
            PlayerPrefs.SetString(PREFS_TOKEN_NAME, json);
        }
        catch(Exception e)
        {
            Spotify4Unity.Analysis.Log($"Exception occured trying to save current auth token - ${e.ToString()}", Spotify4Unity.Analysis.LogLevel.All);
            return false;
        }
        
        Spotify4Unity.Analysis.Log($"Saved token information in PlayerPrefs under name '{PREFS_TOKEN_NAME}'", Spotify4Unity.Analysis.LogLevel.All);

        return true;
    }

    /// <summary>
    /// Deletes the currently stores authentification token
    /// </summary>
    /// <returns></returns>
    public static bool DeleteToken()
    {
        string json = PlayerPrefs.GetString(PREFS_TOKEN_NAME);
        if(!string.IsNullOrEmpty(json))
        {
            PlayerPrefs.SetString(PREFS_TOKEN_NAME, null);
            return true;
        }
        return false;
    }
}
