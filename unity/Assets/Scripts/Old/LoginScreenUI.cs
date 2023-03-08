using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using VivoxUnity;
using System;

public class LoginScreenUI : MonoBehaviourPunCallbacks
{
    private VivoxVoiceManager _vivoxVoiceManager;
    public static LoginScreenUI _photonManager;
    public GameObject MediaContainer;

    public Button LoginButton;
    public InputField DisplayNameInput;
    public InputField ChannelNameInput;
    public GameObject LoginScreen;
    public GameObject LobbyScreen;
    public Text InfoText;
    //public GameObject prefab_parent;
    public GameObject RoomBrowserScroller;
    public GameObject playerAvatar = null;

    private bool loginBool; 
    public bool inRoom = false;
    private int defaultMaxStringLength = 9;
    private int PermissionAskedCount = 0;
    #region Unity Callbacks

    public string room_name = "";

    private EventSystem _evtSystem;

    // Photon stuff
    private RoomOptions defaultRoomOptions;
    public Dictionary<string, RoomInfo> cachedRoomList;
    
    private void Awake()
    {
        _photonManager = this;
        _evtSystem = FindObjectOfType<EventSystem>();
        _vivoxVoiceManager = VivoxVoiceManager.Instance;
        _vivoxVoiceManager.OnUserLoggedInEvent += OnUserLoggedIn;
        _vivoxVoiceManager.OnUserLoggedOutEvent += OnUserLoggedOut;

        // on login
        LoginButton.onClick.AddListener(() => {
            if (String.IsNullOrEmpty(ChannelNameInput.text) | String.IsNullOrEmpty(DisplayNameInput.text))
            {
                InfoText.text = "Please enter a room name and a display name";
            }
            else
            {
                MultiplayerLogin();
            }
        });

        if (_vivoxVoiceManager.LoginState == VivoxUnity.LoginState.LoggedIn)
        {
            OnUserLoggedIn();
            DisplayNameInput.text = _vivoxVoiceManager.LoginSession.Key.DisplayName;
        }
        else
        {
            OnUserLoggedOut();
            LogoutOfPhotonService();
            var systInfoDeviceName = String.IsNullOrWhiteSpace(SystemInfo.deviceName) == false ? SystemInfo.deviceName : Environment.MachineName;

            DisplayNameInput.text = Environment.MachineName.Substring(0, Math.Min(defaultMaxStringLength, Environment.MachineName.Length));
        }

        ChannelNameInput.onEndEdit.AddListener((string text) => {
            // check if string is empty
            if (String.IsNullOrEmpty(text))
                LoginButton.interactable = false;
            else
                LoginButton.interactable = true;
        }); 

        cachedRoomList = new Dictionary<string, RoomInfo>();
        inRoom = false;
    }

    // called when you click 'join' in room list
    public void MultiplayerLogin(string roomname)
    {
        // TODO parse names better
        ChannelNameInput.text = roomname.Split('+')[0].Replace("_", " ");

        Debug.Log("ChannelNameInput.text: " + roomname);
        room_name = roomname;
        if (DisplayNameInput.text != "" && ChannelNameInput.text != "")
        {
            // TODO check display name doesn't include '+'
            LogoutOfPhotonService();
            LoginToVivoxService();
            LoginToPhotonService();
            loginBool = true;
            LobbyScreen.SetActive(true);
        }
        else
        {
            InfoText.text = $"Please enter a display name: {DisplayNameInput.text}";
        }
    }

    // called when you click the host button
    public void MultiplayerLogin()
    {
        if (DisplayNameInput.text != "" && ChannelNameInput.text != "")
        {
            room_name = $"{ChannelNameInput.text}+{DisplayNameInput.text}";
            room_name = room_name.Replace(" ", "_");
            LogoutOfPhotonService();
            LoginToVivoxService();
            LoginToPhotonService();
            loginBool = true;
            LobbyScreen.SetActive(true);

        }
        else
        {
            InfoText.text = $"Please enter a display name: {DisplayNameInput.text}";
        }
    }

    private void OnDestroy()
    {
        _vivoxVoiceManager.OnUserLoggedInEvent -= OnUserLoggedIn;
        _vivoxVoiceManager.OnUserLoggedOutEvent -= OnUserLoggedOut;
        LogoutOfPhotonService();
        LoginButton.onClick.RemoveAllListeners();
#if UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_STADIA
        DisplayNameInput.onEndEdit.RemoveAllListeners();
#endif
    }

    #endregion

    private void ShowLoginUI()
    {
        LoginScreen.SetActive(true);
        LoginButton.interactable = true;
        _evtSystem.SetSelectedGameObject(LoginButton.gameObject, null);

    }

    private void HideLoginUI()
    {
        LoginScreen.SetActive(false);
    }

#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
    private bool IsAndroid12AndUp()
    {
        // android12VersionCode is hardcoded because it might not be available in all versions of Android SDK
        const int android12VersionCode = 31;
        AndroidJavaClass buildVersionClass = new AndroidJavaClass("android.os.Build$VERSION");
        int buildSdkVersion = buildVersionClass.GetStatic<int>("SDK_INT");

        return buildSdkVersion >= android12VersionCode;
    }

    private string GetBluetoothConnectPermissionCode()
    {
        if (IsAndroid12AndUp())
        {
            // UnityEngine.Android.Permission does not contain the BLUETOOTH_CONNECT permission, fetch it from Android
            AndroidJavaClass manifestPermissionClass = new AndroidJavaClass("android.Manifest$permission");
            string permissionCode = manifestPermissionClass.GetStatic<string>("BLUETOOTH_CONNECT");

            return permissionCode;
        }

        return "";
    }
#endif

    private bool IsMicPermissionGranted()
    {
        bool isGranted = Permission.HasUserAuthorizedPermission(Permission.Microphone);
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (IsAndroid12AndUp())
        {
            // On Android 12 and up, we also need to ask for the BLUETOOTH_CONNECT permission for all features to work
            isGranted &= Permission.HasUserAuthorizedPermission(GetBluetoothConnectPermissionCode());
        }
#endif
        return isGranted;
    }

    private void AskForPermissions()
    {
        string permissionCode = Permission.Microphone;

#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (PermissionAskedCount == 1 && IsAndroid12AndUp())
        {
            permissionCode = GetBluetoothConnectPermissionCode();
        }
#endif
        PermissionAskedCount++;
        Permission.RequestUserPermission(permissionCode);
    }

    private bool IsPermissionsDenied()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        // On Android 12 and up, we also need to ask for the BLUETOOTH_CONNECT permission
        if (IsAndroid12AndUp())
        {
            return PermissionAskedCount == 2;
        }
#endif
        return PermissionAskedCount == 1;
    }

    public void LoginToVivoxService()
    {
        if (IsMicPermissionGranted())
        {
            // The user authorized use of the microphone.
            LoginToVivox();   
        }
        else
        {
            // We do not have the needed permissions.
            // Ask for permissions or proceed without the functionality enabled if they were denied by the user
            if (IsPermissionsDenied())
            {
                PermissionAskedCount = 0;
                LoginToVivox(); // ? continue to login after denying permissions
            }
            else
            {
                AskForPermissions();
            }
        }
    }

    private void LoginToVivox()
    {
        LoginButton.interactable = false;
        _vivoxVoiceManager.Login(DisplayNameInput.text);
    }

    #region Vivox Callbacks

    private void OnUserLoggedIn()
    {
        HideLoginUI();
        RoomBrowserScroller.SetActive(false);
        inRoom = true;
    }

    private void OnUserLoggedOut()
    {
        ShowLoginUI();
        LogoutOfPhotonService();
        RoomBrowserScroller.SetActive(true);
        inRoom = false;
    }

    #endregion


    #region Photon-functions

    public void LoginToPhotonService()
    {
        PhotonNetwork.ConnectUsingSettings(); // connects to master server
    }
    private void LogoutOfPhotonService()
    {
        PhotonNetwork.Disconnect();
        inRoom = false;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("<Photon> Player has connected to the Photon master server");
        // need to join to get a list of rooms
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        // player will get regular updates on room list once in lobby
        Debug.Log("<Photon> OnJoinedLobby()");

        // check if we're ready to join a room
        if (loginBool)
        {
            // create custom room name - TODO add nft or url for image?
            CreateJoinRoom(room_name);
        }
    }

    // Called for any update of the room-listing while in a lobby (InLobby) on the Master Server
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        cachedRoomList.Clear();
        Debug.Log(roomList.Count + " rooms");
        //InfoText.text = "Rooms: " + roomList.Count;

        foreach (RoomInfo info in roomList)
        {
            Debug.Log("Found room: " + info.ToString());
            //InfoText.text += "\n" + info.ToString();

            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            else
            {
                cachedRoomList.Add(info.Name, info);
                Debug.Log("Added room: " + info.Name);
            }
        }
        
        Debug.Log("<Photon> Cached rooms: " + cachedRoomList.Count + " rooms");
        // Populate the UI with the room list
        // access another namespace and call LoadData function
        RoomBrowserScroller.GetComponent<Controller>().LoadData(cachedRoomList);
    }


    void CreateJoinRoom(string name)
    {
        PhotonNetwork.JoinOrCreateRoom(name, defaultRoomOptions, null);
        loginBool = false;
        // TODO query somewhere for an asset json
    }

    public override void OnCreateRoomFailed(short returnCode, string debugMsg)
    {
        Debug.Log("<Photon> OnCreateRoomFailed: " + returnCode + " " + debugMsg);
        InfoText.text = "Failed to create room: " + debugMsg;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("<Photon> Entered room: " + PhotonNetwork.CurrentRoom.Name);
        // instatiate the player prefab on the network + parent to camera
        playerAvatar = PhotonNetwork.Instantiate("sloth_head_prefab", Vector3.zero, Quaternion.identity, 0);
        // TODO check for an asset in PlayerPrefs
        inRoom = true;
        //MediaContainer.GetComponent<StableHordeManager>().NetworkSync();
        // TODO need to fetch current state of the room
    }

    void Update()
    {
        // check if avatar exists
        if (playerAvatar != null)
        {
            // update the avatar position
            playerAvatar.transform.position = Camera.main.transform.position - Camera.main.transform.forward * 0.05f;
            playerAvatar.transform.rotation = Camera.main.transform.rotation;
        }
    }

    #endregion
}