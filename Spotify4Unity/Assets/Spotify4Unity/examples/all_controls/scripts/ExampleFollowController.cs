using Spotify4Unity;
using Spotify4Unity.Dtos;
using Spotify4Unity.Enums;
using Spotify4Unity.Events;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Retrieved the public profile of the supplied user id and follows/unfollows them when btn is pressed
/// </summary>
public class ExampleFollowController : SpotifyUIBase
{
    [Tooltip("Is the target follow id a User or Artist")]
    public AccountType TargetIdType = AccountType.User;
    [Tooltip("The spotify id of the user to retrieve public information about and follow/unfollow")]
    public string TargetId = "1121645175"; // Follow me pls ;)
   
    [SerializeField, Tooltip("The button that will follow/unfollow the target id")]
    private Button m_followBtn = null;

    [SerializeField, Tooltip("Image to display the of the target id")]
    private Image m_userImg = null;

    [SerializeField, Tooltip("Display name of the target id")]
    private Text m_name = null;

    [SerializeField, Tooltip("Text to show how many followers the target id has")]
    private Text m_followers = null;

    private bool m_isFollowingId = false;

    private void Start()
    {
        if (m_followBtn != null)
            m_followBtn.onClick.AddListener(OnFollowId);
    }

    protected override void OnConnectedChanged(ConnectedChanged e)
    {
        base.OnConnectedChanged(e);

        if(e.IsConnected && !string.IsNullOrEmpty(TargetId))
        {
            m_isFollowingId = TargetIdType == AccountType.Artist ? SpotifyService.IsUserFollowingArtist(TargetId) : SpotifyService.IsUserFollowingUser(TargetId);

            // Gets information about the target id
            AccountInfo details = TargetIdType == AccountType.Artist ? (AccountInfo)SpotifyService.GetArtistDetails(TargetId) : SpotifyService.GetUserDetails(TargetId);
            if(details != null)
            {
                StartCoroutine(Spotify4Unity.Helpers.Utility.LoadImageFromUrl(details.ProfilePictureURL, Spotify4Unity.Enums.Resolution.x128, (sprite) =>
                {
                    m_userImg.sprite = sprite;
                }));

                if(m_name != null)
                    m_name.text = details.Name;
                if(m_followers != null)
                    m_followers.text = "Followers: " + details.Followers.ToString();
            }

            m_followBtn.GetComponentInChildren<Text>().text = m_isFollowingId ? "Unfollow" : "Follow";
        }
    }

    private void OnFollowId()
    {
        if (string.IsNullOrEmpty(TargetId))
            return;

        bool followResult = false;
        if (m_isFollowingId)
        {
            followResult = TargetIdType == AccountType.Artist ? SpotifyService.UnfollowArtists(TargetId) : SpotifyService.UnfollowUsers(TargetId);
        }
        else
        {
            followResult = TargetIdType == AccountType.Artist ? SpotifyService.FollowArtists(TargetId) : SpotifyService.FollowUsers(TargetId);
        }

        m_isFollowingId = !m_isFollowingId;
        m_followBtn.GetComponentInChildren<Text>().text = m_isFollowingId ? "Unfollow" : "Follow";

        if (!followResult)
            Analysis.LogError("Not able to follow/unfollow the id", Analysis.LogLevel.All);
    }
}
