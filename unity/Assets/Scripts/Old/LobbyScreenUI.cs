using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VivoxUnity;
using Photon.Pun;
using Photon.Realtime;
using System;

public class LobbyScreenUI : MonoBehaviour
{
    private VivoxVoiceManager _vivoxVoiceManager;

    public string LobbyChannelName = "lobbyChannel";
    public InputField ChannelNameInput;
    public InputField DisplayNameInput;

    public Text InfoText;

    private EventSystem _evtSystem;

    public Button LogoutButton;
    public GameObject LobbyScreen;
    public GameObject ConnectionIndicatorDot;
    public GameObject ConnectionIndicatorText;
    public GameObject Multiplayer;

    private Image _connectionIndicatorDotImage;
    private Text _connectionIndicatorDotText;

    #region Unity Callbacks

    private void Awake()
    {
        _evtSystem = EventSystem.current;
        if (!_evtSystem)
        {
            Debug.LogError("Unable to find EventSystem object.");
        }
        _connectionIndicatorDotImage = ConnectionIndicatorDot.GetComponent<Image>();
        if (!_connectionIndicatorDotImage)
        {
            Debug.LogError("Unable to find ConnectionIndicatorDot Image object.");
        }
        _connectionIndicatorDotText = ConnectionIndicatorText.GetComponent<Text>();
        if (!_connectionIndicatorDotText)
        {
            Debug.LogError("Unable to find ConnectionIndicatorText Text object.");
        }

        LogoutButton.onClick.AddListener(() => { 
            LogoutOfVivoxService(); 
            LogoutOfPhotonService();
        });

    }

    void Start()
    {
        try
        {
            _vivoxVoiceManager = VivoxVoiceManager.Instance;
            _vivoxVoiceManager.OnUserLoggedInEvent += OnUserLoggedIn;
            _vivoxVoiceManager.OnUserLoggedOutEvent += OnUserLoggedOut;
            _vivoxVoiceManager.OnRecoveryStateChangedEvent += OnRecoveryStateChanged;

            
            if (_vivoxVoiceManager.LoginState == LoginState.LoggedIn)
            {
                OnUserLoggedIn();
            }
            else
            {
                OnUserLoggedOut();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Unable to find VivoxVoiceManager object. Exception: " + ex.Message);
            StartCoroutine(waitThenSetup());
        }
    }

    IEnumerator waitThenSetup()
    {
        yield return new WaitForSeconds(1);
        setupVivox();
    }

    void setupVivox()
    {
        _vivoxVoiceManager = VivoxVoiceManager.Instance;
        _vivoxVoiceManager.OnUserLoggedInEvent += OnUserLoggedIn;
        _vivoxVoiceManager.OnUserLoggedOutEvent += OnUserLoggedOut;
        _vivoxVoiceManager.OnRecoveryStateChangedEvent += OnRecoveryStateChanged;

        
        if (_vivoxVoiceManager.LoginState == LoginState.LoggedIn)
        {
            OnUserLoggedIn();
        }
        else
        {
            OnUserLoggedOut();
        }
    }
    
    public void LogOutOfAll()
    {
        LogoutOfVivoxService(); 
        LogoutOfPhotonService();
    }

    private void OnDestroy()
    {
        _vivoxVoiceManager.OnUserLoggedInEvent -= OnUserLoggedIn;
        _vivoxVoiceManager.OnUserLoggedOutEvent -= OnUserLoggedOut;
        _vivoxVoiceManager.OnParticipantAddedEvent -= VivoxVoiceManager_OnParticipantAddedEvent;
        _vivoxVoiceManager.OnRecoveryStateChangedEvent -= OnRecoveryStateChanged;

        if (PhotonNetwork.InRoom)
        {
            // if so, disconnect from the room
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        LogoutButton.onClick.RemoveAllListeners();
    }

    #endregion
    #region Photon-functions
    private void LogoutOfPhotonService()
    {
        InfoText.text = "*You will be joined into an audio channel and visible with others immediately after login.";
        if (PhotonNetwork.InRoom)
        {
            // if so, disconnect from the room
            PhotonNetwork.LeaveRoom();
            // join lobby to get room list
            PhotonNetwork.ConnectUsingSettings(); // connects to master server
        }
    }
    #endregion
    private void JoinLobbyChannel()
    {
        LobbyChannelName = Multiplayer.GetComponent<LoginScreenUI>().room_name;
        //LobbyChannelName = $"{ChannelNameInput.text}+{DisplayNameInput.text}";
        //LobbyChannelName = LobbyChannelName.Replace(" ", "_");

        // Do nothing, participant added will take care of this
        _vivoxVoiceManager.OnParticipantAddedEvent += VivoxVoiceManager_OnParticipantAddedEvent;
        _vivoxVoiceManager.JoinChannel(LobbyChannelName, ChannelType.NonPositional, VivoxVoiceManager.ChatCapability.TextAndAudio);
    }

    private void LogoutOfVivoxService()
    {
        LogoutButton.interactable = false;

        _vivoxVoiceManager.DisconnectAllChannels();

        _vivoxVoiceManager.Logout();
    }

    #region Vivox Callbacks

    private void VivoxVoiceManager_OnParticipantAddedEvent(string username, ChannelId channel, IParticipant participant)
    {
        if (channel.Name == LobbyChannelName && participant.IsSelf)
        {
            // if joined the lobby channel and we're not hosting a match
            // we should request invites from hosts
        }
    }

    private void OnUserLoggedIn()
    {
        LobbyScreen.SetActive(true);
        LogoutButton.interactable = true;
        _evtSystem.SetSelectedGameObject(LogoutButton.gameObject, null);

        var lobbychannel = _vivoxVoiceManager.ActiveChannels.FirstOrDefault(ac => ac.Channel.Name == LobbyChannelName);
        if ((_vivoxVoiceManager && _vivoxVoiceManager.ActiveChannels.Count == 0) 
            || lobbychannel == null)
        {
            JoinLobbyChannel();
        }
        else
        {
            if (lobbychannel.AudioState == ConnectionState.Disconnected)
            {
                // Ask for hosts since we're already in the channel and part added won't be triggered.

                lobbychannel.BeginSetAudioConnected(true, true, ar =>
                {
                    Debug.Log("Now transmitting into lobby channel");
                });
            }

        }
    }

    private void OnUserLoggedOut()
    {
        _vivoxVoiceManager.DisconnectAllChannels();
        // check if photon is connected to a room
        if (PhotonNetwork.InRoom)
        {
            // if so, disconnect from the room
            PhotonNetwork.LeaveRoom();
        }
        LobbyScreen.SetActive(false);
    }

    private void OnRecoveryStateChanged(ConnectionRecoveryState recoveryState)
    {
        Color indicatorColor;
        switch (recoveryState)
        {
            case ConnectionRecoveryState.Connected:
                indicatorColor = Color.green;
                break;
            case ConnectionRecoveryState.Disconnected:
                indicatorColor = Color.red;
                break;
            case ConnectionRecoveryState.FailedToRecover:
                indicatorColor = Color.black;
                break;
            case ConnectionRecoveryState.Recovered:
                indicatorColor = Color.green;
                break;
            case ConnectionRecoveryState.Recovering:
                indicatorColor = Color.yellow;
                break;
            default:
                indicatorColor = Color.white;
                break;
        }
        _connectionIndicatorDotImage.color = indicatorColor;
        _connectionIndicatorDotText.text = recoveryState.ToString();
    }

    #endregion
}