using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NavigationBar : MonoBehaviour
{
    public GameObject MultiplayerUI;
    public GameObject NFTController;
    public GameObject MediaContainer;
    public GameObject SendMenu;
    public GameObject uiContainer;

    // Start is called before the first frame update
    void Start()
    {
        MultiplayerUI.SetActive(false);
        // toggle wallet if active
        NFTController.GetComponent<NFTWallet>().HideWalletUI();
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

    public void ToggleWallet()
    {
        NFTController.GetComponent<NFTWallet>().WalletToggleButton();

        // toggle multiplayer if active
        if (MultiplayerUI.activeSelf)
        {
            ToggleMultiplayer();
        }

    }

    public void ToggleUIContainer()
    {
        if (uiContainer.activeSelf)
        {
            NFTController.GetComponent<NFTWallet>().WalletToggleButton();
            MediaContainer.GetComponent<TouchControls>().enabled = true;
        }
        else
        {
            uiContainer.SetActive(true);
            MediaContainer.GetComponent<TouchControls>().enabled = false;
            if (MultiplayerUI.activeSelf)
            {
                MultiplayerUI.SetActive(false);
            }

            NFTController.GetComponent<DiscoverNFT>().StartAILatestQuery();
        }
    }

        public void ToggleMidjourneyContainer()
    {
        if (uiContainer.activeSelf)
        {
            NFTController.GetComponent<NFTWallet>().WalletToggleButton();
            MediaContainer.GetComponent<TouchControls>().enabled = true;
        }
        else
        {
            uiContainer.SetActive(true);
            MediaContainer.GetComponent<TouchControls>().enabled = false;
            if (MultiplayerUI.activeSelf)
            {
                MultiplayerUI.SetActive(false);
            }

            NFTController.GetComponent<DiscoverNFT>().StartMidJourneyLatestQuery();
        }
    }

    // public void WalletRefresh()
    // {
    //     WalletController.GetComponent<NFTWallet>().RefreshWallet();
    // }

    public void ToggleMultiplayer()
    {
        MultiplayerUI.SetActive(!MultiplayerUI.activeSelf);
        
        if(MultiplayerUI.activeSelf) // show UI
        {
            if (!PhotonNetwork.InRoom)
            {
                LoginToPhotonService(); // check LoginScreen.cs for overrides
            }
            MediaContainer.GetComponent<TouchControls>().enabled = false;
            SendMenu.SetActive(false);
        }
        else{ // Hide UI
            // check if photon is not connected to a room
            if (!PhotonNetwork.InRoom)
            {
                PhotonNetwork.Disconnect();
            }
            MediaContainer.GetComponent<TouchControls>().enabled = true;
        }
        // toggle wallet if active
        if (NFTController.GetComponent<NFTWallet>().WalletContainer.activeSelf)
        {
            NFTController.GetComponent<NFTWallet>().WalletToggleButton();
        }
        
    }
}
