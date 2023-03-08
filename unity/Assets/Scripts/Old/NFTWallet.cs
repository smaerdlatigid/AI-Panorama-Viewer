using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using TMPro;

public class NFTWallet : MonoBehaviour
{
    public nft_user user;
    public TMP_Text Status;

    public GameObject WalletContainer;
    public GameObject RefreshButton;
    public GameObject MediaContainer;
    public GameObject SendMenu;
    public bool loggedIn = false;

    nft dnft; // default nft that loads on start

    void Start()
    {
        string addr = PlayerPrefs.GetString("Account");

        // if remember me is checked, set the account to the saved account
        if (addr != "")
        {
            Status.text = $"{addr}";
            // query for new nfts in the background
            RefreshWallet();
        }
        else
        {
            Status.text = "";
        }
        loggedIn = false;
        HideWalletUI();
    }

    void LoadStartingNFT()
    {
        // TODO query api or smart contract and pull random token
        dnft = new nft();
    }

    async public void OnLogin(Action callback = null)
    {
        // get current timestamp
        int timestamp = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;

        // set expiration time
        int expirationTime = timestamp + 60;
        // set message
        string message = $"Mint More Art wants you to sign in with your Polygon account. Authentication expires in {expirationTime - timestamp} seconds.";

        // sign message
        try
        {
            string signature = await Web3Wallet.Sign(message);
            // verify account
            string account = await EVM.Verify(message, signature);
            int now = (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
            // validate
            if (account.Length == 42 && expirationTime >= now) {
                // save account
                PlayerPrefs.SetString("Account", account);
                Status.text = $"Welcome {account}!";
                loggedIn = true;
                if (callback != null)
                {
                    callback();
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void uiCallback()
    {
        // set NFT data to UI
        GetComponent<EnhancedScrollerDemos.NestedWallet.Controller>().LoadData(user.nfts);
    }

    public void RefreshWallet()
    {
        // query for new NFTs using moralis
        if (PlayerPrefs.GetString("Account").Length == 42)
        {
            user = new nft_user(PlayerPrefs.GetString("Account"), uiCallback);
        }
    }

    public void WalletToggleButton()
    {
        // first time -> login with metamask otherwise toggle wallet UI
        string addr = PlayerPrefs.GetString("Account");
        if (addr.Length != 42)
        {
            Status.text = "Login with Authentication.";
            OnLogin(RefreshWallet);
        }
        else
        {
            if (WalletContainer.activeSelf)
            {
                WalletContainer.SetActive(false);
                SendMenu.SetActive(false);
                RefreshButton.SetActive(false);
                Status.text = "";
                MediaContainer.GetComponent<TouchControls>().enabled = true;
            }
            else
            {
                WalletContainer.SetActive(true);
                RefreshButton.SetActive(true);
                MediaContainer.GetComponent<TouchControls>().enabled = false;
                SendMenu.SetActive(false);

                // reload data if the first nft is not the owners
                if (GetComponent<DiscoverNFT>().nfts.Count > 0)
                {
                    if (GetComponent<DiscoverNFT>().nfts.Count > 0)
                    {
                        GetComponent<EnhancedScrollerDemos.NestedWallet.Controller>().LoadData(user.nfts);
                    }
                }
            }
        }
    }
    
    public void HideWalletUI()
    {
        WalletContainer.SetActive(false);
        RefreshButton.SetActive(false);
        SendMenu.SetActive(false);
        MediaContainer.GetComponent<TouchControls>().enabled = true;
    }
}

public enum MediaType
{
    None = 0,
    Image = 1,
    Video = 2,
    Model = 3,
    Effect = 4,
    SVG = 5
};

[System.Serializable]
public class nft_metadata
{
    public string name;
    public string description;
    public string image_url;
    public string animation_url;

    public List<traits> traits = new List<traits>();
}

[System.Serializable]
public class traits
{
    public string trait_type;
    public string value;
}

public class nft
{
    public string chain;
    public string address; // contract address
    public string name; // contract name
    public int token_id;
    public string tokenuri; // uri for token id
    public string data; // text of data at uri, usually json 
    public float price = 0; // currency based on chain
    public bool marketApproved = false;    // has nft approved market contract for selling
    public bool mintable = false; // can mint new nft
    public string description = "";
    public string contract_name = "";
    public bool hasNeighbors = false;
    public string owner = "";       // don't want to serialize this
    public string image_url = "";
    public string thumbnail_url = "";
    public string media_url = "";
    
    public MediaType media_type;
    
    // stuff for loading and saving media
    Dictionary<MediaType, string> MediaTypeExtensions = new Dictionary<MediaType, string>()
    {
        { MediaType.None, "" },
        { MediaType.Image, ".jpg" },
        { MediaType.Video, ".mp4" },
        { MediaType.Model, ".glb" },
        { MediaType.Effect, ".jpg" },
        { MediaType.SVG, ".svg" }
    };

    public nft(){}

    public string key
    {
        get
        {
            return $"{chain}:{address}:{token_id}";
        } 
    }

    public string media_path {
        // build file path to store media on disk
        get {
            string ext = Path.GetExtension(media_url);
            // if empty, set to default of mediatype
            if (ext == "")
            {
                ext = MediaTypeExtensions[media_type];
            }
            string filename = Application.persistentDataPath + "/" + key.Replace(" ","_").Replace(":","_").Replace("-","_")+ext;
            return filename;
        }
    }

    public string image_path {
        // build file path to store media on disk
        get {
            string ext = Path.GetExtension(image_url);
            // if empty, set to default of mediatype
            if (ext == "")
            {
                ext = MediaTypeExtensions[MediaType.Image];
            }
            string filename = Application.persistentDataPath + "/" + key.Replace(" ","_").Replace(":","_").Replace("-","_")+ext;
            return filename;
        }
    }

    public bool isAI {
        get {
            if (chain == "midjourney")
            {
                return true;
            }
            return false;
        }
    }
}


public class nft_user
{
    List<string> chains = new List<string> { "polygon" ,"eth", "bsc", "avalanche", "fantom"};

    List<string> test_chains = new List<string> {"mumbai", "avalanche testnet", "bsc testnet", "ropsten", "rinkeby", "kovan", "goerli" };

    public List<nft> nfts = new List<nft>();

    public Dictionary<string, nft> dnfts = new Dictionary<string, nft>();

    public string address;

    public nft_user(string addr, Action callback)
    {
        address = addr;
        queryChains(callback);
    }

    private string moralis_uri = "https://deep-index.moralis.io/api/v2/{0}/nft?chain={1}&format=decimal"; // add limit 1000?

    async void queryChains(Action callback)
    {
        // clear old data
        nfts = new List<nft>();
        dnfts = new Dictionary<string, nft>();

        // query each chain
        foreach (string chain in chains) // swap here for test/ main
        {
            string url = string.Format(moralis_uri, address, chain);

            Debug.Log("Querying...");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader("X-API-Key", "pTYZq6zWqKjlXZSoUyMXCYxmugAVJLDK3Kc2nlW1bQ8j7QbBHrykDL75OQVO1GJA");
                // Request and wait for the desired page.
                await webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    Debug.Log(url + ": Error: " + webRequest.error);
                }
                else
                {
                    Debug.Log(url + ":\nReceived: " + webRequest.downloadHandler.text);
                    parseResponse(webRequest.downloadHandler.text, chain);
                }
            }

            // add toggle for each?
            //callback();
        }

        Debug.Log("Done querying");
        callback();
    }

    // TODO function (chain, address, tokenid) - query token URI + parse metadata

    private JSONNode obj;
    // parsing for moralis query
    void parseResponse(string response, string chain)
    {
        obj = JSON.Parse(response);

        // convert to galeri_nft and add to list
        foreach (JSONNode jnft in obj["result"])
        {
            // skip if metadata is missing
            if (String.IsNullOrWhiteSpace(jnft["metadata"]) == true)
                continue;

            // check blacklist
            if (addressBlackList.Contains(jnft["token_address"]))
                continue;
            
            // only ERC721 functions

            nft g = new nft();
            g.chain = chain;
            g.address = jnft["token_address"];
            g.token_id = jnft["token_id"];
            g.data = jnft["metadata"]; // tokenuri
            g.contract_name = jnft["name"];
            g.owner = PlayerPrefs.GetString("Account");
            g.hasNeighbors = false;

            JSONNode meta = JSON.Parse(g.data);
            Debug.Log("Parsing metadata for " + g.data);

            // check for image & other media
            g.image_url = ParseTool.checkForImage(meta);
            (g.media_type, g.media_url) = ParseTool.checkMediaType(meta);
            g.name = ParseTool.checkForName(meta);
            g.description = ParseTool.checkForDescription(meta);

            // modify if ipfs link for speed loading
            if (g.image_url.Contains("ipfs"))
            {
                g.image_url = g.image_url.Replace("ipfs://", "https://loopring.mypinata.cloud/ipfs/");
                g.image_url = g.image_url.Replace("https://ipfs.io/ipfs/", "https://loopring.mypinata.cloud/ipfs/");
                g.image_url = g.image_url.Replace("https://gateway.pinata.cloud/ipfs/", "https://loopring.mypinata.cloud/ipfs/");
            }
                        // modify if ipfs link for speed loading
            if (g.media_url.Contains("ipfs"))
            {
                g.media_url = g.media_url.Replace("ipfs://", "https://loopring.mypinata.cloud/ipfs/");
                g.media_url = g.media_url.Replace("https://ipfs.io/ipfs/", "https://loopring.mypinata.cloud/ipfs/");
                g.media_url = g.media_url.Replace("https://gateway.pinata.cloud/ipfs/", "https://loopring.mypinata.cloud/ipfs/");
            }

            // todo download media
            // media type = GetMimeType

            // async download media to persistent path
            // if (g.media_type != MediaType.None)
            // {
            //     Debug.Log("Downloading media for " + g.data);
            //     ParseTool.downloadMedia(g.media_url, g.media_path);
            // }
            // else
            // {
            //     Debug.Log("No media for " + g.data);
            // }

            // add to list
            nfts.Add(g);
            // dont add duplicates to dictionary
            if (!dnfts.ContainsKey(g.key))
            {
                dnfts.Add(g.key, g);
            }
        }
    }

    public List<string> addressBlackList = new List<string>(){
        "0xd47eb1c2f8105b8d0d50487614934ccc309ae41f",
        "0x7319f53ca908b43ee87fce7e3cce75cdfe2169ea",
        "0x2953399124f0cbb46d2cbacd8a89cf0599974963", // opensea polygon
    };

}

