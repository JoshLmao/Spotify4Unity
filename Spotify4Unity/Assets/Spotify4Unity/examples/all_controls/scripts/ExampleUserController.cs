using Spotify4Unity;
using Spotify4Unity.Events;
using Spotify4Unity.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spotify4Unity
/// Example Controller script for displaying user information
/// (Make sure you read the Wiki documentation for all information & help!)
/// </summary>
public class ExampleUserController : SpotifyUIBase
{
    [SerializeField]
    private Text m_username = null;

    [SerializeField]
    private Text m_displayName = null;

    [SerializeField]
    private Text m_userId = null;

    [SerializeField]
    private Text m_country = null;

    [SerializeField]
    private Text m_birthday = null;

    [SerializeField]
    private Text m_followersCount = null;

    [SerializeField]
    private Text m_isPremium = null;

    [SerializeField]
    private Image m_profilePicture = null;

    [SerializeField, Tooltip("The resolution to load the users profile picture at")]
    private Spotify4Unity.Enums.Resolution m_albumArtResolution = Spotify4Unity.Enums.Resolution.Original;

    protected override void OnUserInformationLoaded(UserInfoLoaded e)
    {
        base.OnUserInformationLoaded(e);

        m_username.text = e.Info.Username;
        m_displayName.text = e.Info.Name;
        m_country.text = e.Info.Country;
        m_birthday.text = e.Info.Birthdate.ToString("dd/MM/yyyy");
        m_userId.text = e.Info.Id;
        m_followersCount.text = e.Info.Followers.ToString();
        m_isPremium.text = e.Info.IsPremium.ToString();

        if (!string.IsNullOrEmpty(e.Info.ProfilePictureURL))
        {
            IEnumerator routine = Spotify4Unity.Helpers.Utility.LoadImageFromUrl(e.Info.ProfilePictureURL, m_albumArtResolution, sprite => OnUserProfilePictureLoaded(sprite));
            if (this.isActiveAndEnabled)
                StartCoroutine(routine);
            else
                Utility.RunCoroutineEmptyObject(routine);
        }
    }

    private void OnUserProfilePictureLoaded(Sprite s)
    {
        m_profilePicture.sprite = s;
    }
}
