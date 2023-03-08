using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class UI_Navigation_Bar : MonoBehaviour
{
    public GameObject MultiplayerUI;
    public GameObject MultiplayerButton;
    public GameObject MediaContainer;

    // Start is called before the first frame update
    void Start()
    {
        MultiplayerUI.SetActive(false);

        // add listener to toggle UI
        MultiplayerButton.GetComponent<Button>().onClick.AddListener(ToggleMultiplayer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    #region Photon-functions

    public void LoginToPhotonService()
    {
        PhotonNetwork.ConnectUsingSettings(); // connects to master server for room browser
    }
    #endregion


    public void ToggleMultiplayer()
    {
        // toggle UI
        MultiplayerUI.SetActive(!MultiplayerUI.activeSelf);

        // If active, disable touch controls and login to server browser
        if(MultiplayerUI.activeSelf) 
        {
            if (!PhotonNetwork.InRoom)
            {
                LoginToPhotonService(); // check LoginScreen.cs for overrides
                //MultiplayerUI.GetComponent<LoginScreenUI>().AskForPermissions(); // ask for permissions
            }
            MediaContainer.GetComponent<TouchControls>().enabled = false;
        }
        else{ // Hide UI
            // check if photon is not connected to a room
            if (!PhotonNetwork.InRoom)
            {
                PhotonNetwork.Disconnect();
            }
            else
            {
                PhotonNetwork.LeaveRoom();
            }
            MediaContainer.GetComponent<TouchControls>().enabled = true;
        }
    }
}
