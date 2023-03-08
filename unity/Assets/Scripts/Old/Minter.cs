using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
#if UNITY_ANDROID || UNITY_IOS
using NativeGalleryNamespace;
#endif
using UnityEngine.UI;
using TMPro;

public class Minter : MonoBehaviour
{
    public GameObject mediaContainer;
    public GameObject painting;

    // Start is called before the first frame update
    public string account; // wallet address of user
    nft_metadata metadata;
    public TextMeshProUGUI status;
    string token_uri = "";
    string chain;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenAIPrompt()
    {
        // check account is not empty
        if (String.IsNullOrEmpty(PlayerPrefs.GetString("Account")))
        {
            status.text = "Please sign in before minting";
            return;
        }
        if(!GetComponent<NFTWallet>().loggedIn)
        {
            status.text = "Please sign in before minting";
            GetComponent<NFTWallet>().OnLogin(null);
            return;
        }

        mediaContainer.GetComponent<MediaContainer>().OpenAIPrompt();
    }

    public void PickImage()
    {
        // check account is not empty
        if (String.IsNullOrEmpty(PlayerPrefs.GetString("Account")))
        {
            status.text = "Please sign in before minting";
            return;
        }
        if(!GetComponent<NFTWallet>().loggedIn)
        {
            status.text = "Please sign in before minting";
            GetComponent<NFTWallet>().OnLogin(null);
            return;
        }

        // change media container button listeners

        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery( ( path ) =>
        {
            mediaContainer.GetComponent<MediaContainer>().ShowLoadingIcon();

            Debug.Log( "Image path: " + path );
            if( path != null )
            {
                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath( path, 4096 );
                if( texture == null )
                {
                    Debug.Log( "Couldn't load texture from " + path );
                    return;
                }

                mediaContainer.GetComponent<MediaContainer>().SetupMinter( texture, path );
                GetComponent<NFTWallet>().HideWalletUI();
                mediaContainer.GetComponent<MediaContainer>().HideLoadingIcon();
                mediaContainer.GetComponent<MediaContainer>().GetComponent<TouchControls>().enabled = true;
            }
        } );

        Debug.Log( "Permission result: " + permission );
    }


    MemoryStream stream;
    
    public void UpdateMetadataImage(string ipfs_response)
    {
        IPFSResponse response = JsonUtility.FromJson<IPFSResponse>(ipfs_response);
        metadata.image_url = $"ipfs://{response.IpfsHash}";
        status.text += $"\n\nImage uploaded to IPFS ({response.IpfsHash})\nUploading model...";
        Debug.Log("Image URL: " + metadata.image_url);

        //StartCoroutine(UploadJSONPinata(metadata, MintMetadata));
        StartCoroutine(UploadFileStreamPinata(UpdateMetadataModel));
    }

    public void UpdateMetadataModel(string ipfs_response)
    {
        // update meta data with model after uploading to ipfs
        IPFSResponse response = JsonUtility.FromJson<IPFSResponse>(ipfs_response);
        metadata.animation_url = $"ipfs://{response.IpfsHash}?.glb";
        status.text += $"\n\n3D model uploaded to IPFS ({response.IpfsHash})\nUploading metadata...";

        Debug.Log("Model URL: " + metadata.animation_url);    

        // Upload metadata to ipfs then mint metadata
        StartCoroutine(UploadJSONPinata(metadata, MintMetadata));
    }

    IEnumerator UploadFilePinata(string filePath, Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", File.ReadAllBytes(filePath));

        UnityWebRequest request = UnityWebRequest.Post("https://api.pinata.cloud/pinning/pinFileToIPFS", form);

        request.SetRequestHeader("pinata_api_key", "be6fba300350e47e995b"); 
        request.SetRequestHeader("pinata_secret_api_key", "f6f841fdf0992142b3b8e5dfc433b59f2031d6f3bdb0d3ba81c9c3eebbd3b076"); 

        yield return request.Send();

        Debug.Log(request.downloadHandler.text);
        callback(request.downloadHandler.text);
    }

    IEnumerator UploadFileStreamPinata(Action<string> callback)
    {
        WWWForm form = new WWWForm();
        // convert stream to byte array
        byte[] bytes = stream.ToArray();
        form.AddBinaryData("file", bytes);

        UnityWebRequest request = UnityWebRequest.Post("https://api.pinata.cloud/pinning/pinFileToIPFS", form);

        request.SetRequestHeader("pinata_api_key", "be6fba300350e47e995b"); 
        request.SetRequestHeader("pinata_secret_api_key", "f6f841fdf0992142b3b8e5dfc433b59f2031d6f3bdb0d3ba81c9c3eebbd3b076"); 

        yield return request.Send();

        Debug.Log(request.downloadHandler.text);
        callback(request.downloadHandler.text);
    }

    IEnumerator UploadJSONPinata(nft_metadata mdata, Action<string> callback)
    {
        pinataJSON pinata = new pinataJSON();
        pinata.pinataContent = mdata;
        pinata.pinataMetadata = new pinataMetadata();
        pinata.pinataMetadata.name = "metadata.json";
        string json = JsonUtility.ToJson(pinata);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

        UnityWebRequest request = new UnityWebRequest("https://api.pinata.cloud/pinning/pinJSONToIPFS", "POST");
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.SetRequestHeader("pinata_api_key", "be6fba300350e47e995b");
        request.SetRequestHeader("pinata_secret_api_key", "f6f841fdf0992142b3b8e5dfc433b59f2031d6f3bdb0d3ba81c9c3eebbd3b076");

        yield return request.SendWebRequest();
        Debug.Log(request.downloadHandler.text);
        callback(request.downloadHandler.text);
    }

    public void Upload(nft token)
    {
        mediaContainer.GetComponent<MediaContainer>().ShowLoadingIcon();
        token_uri = ""; // gets set later
        chain = "polygon";
        // create new meta data
        metadata = new nft_metadata();
        metadata.traits = new List<traits>();

        metadata.name = token.name;
        metadata.description = token.description + "\n\n https://mintmore.art/";

        traits trait = new traits();
        trait.trait_type = "Algorithm";
        if (token.chain == "midjourney")
        {
            trait.value =  "MidJourney";
        }
        else
        {
            trait.value = "Human";
        }

        metadata.traits.Add(trait);

        trait = new traits();
        trait.trait_type = "Creator";
        trait.value = token.owner;

        metadata.traits.Add(trait);

        status.text = "Uploading to IPFS...";
        if (token.media_path != null)
        {
            // zero rotation of painting
            painting.transform.rotation = Quaternion.Euler(0, 0, 0);

            stream = new MemoryStream();
            GLTF.AdvancedExport(stream);

            StartCoroutine(UploadFilePinata(token.media_path, UpdateMetadataImage));

            // rotate back to original rotation
            painting.transform.eulerAngles = new Vector3(0, 180f, 0);

        }
    }

    public void MintMetadata(string ipfs_response)
    {
        // update meta data with image after uploading to ipfs
        IPFSResponse response = JsonUtility.FromJson<IPFSResponse>(ipfs_response);

        Debug.Log($"Token URI: {response.IpfsHash}");
        //status.text += $"\n\nToken URI uploaded to IPFS ({response.IpfsHash})";
        status.text = "Please confirm transaction to mint token";
        mediaContainer.GetComponent<MediaContainer>().Status.text = "Please confirm transaction to mint token";
        // Call Mint function with token URI
        token_uri = $"ipfs://{response.IpfsHash}"; 
        MintMobile();
    }

    Dictionary<string, string> chainIds = new Dictionary<string, string>() {
        {"mumbai", "80001"},
        {"polygon", "137"}
    };

    Dictionary<string, string> contractAddresses = new Dictionary<string, string>() {
        {"mumbai", "0x1eE48aB522a998887806f504b8f2A6790B9d5772"},
        {"polygon", "0xb035A5401d8E669675659d58191383D70b1f02bE"} // MintMore(ART)
    };

    string mint_contract_abi = "[{\"inputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"constructor\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"address\", \"name\": \"approved\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"Approval\", \"type\": \"event\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"address\", \"name\": \"operator\", \"type\": \"address\"}, {\"indexed\": false, \"internalType\": \"bool\", \"name\": \"approved\", \"type\": \"bool\"}], \"name\": \"ApprovalForAll\", \"type\": \"event\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"previousOwner\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"address\", \"name\": \"newOwner\", \"type\": \"address\"}], \"name\": \"OwnershipTransferred\", \"type\": \"event\"}, {\"anonymous\": false, \"inputs\": [{\"indexed\": true, \"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"indexed\": true, \"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"Transfer\", \"type\": \"event\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"approve\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\"}], \"name\": \"balanceOf\", \"outputs\": [{\"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"default_uri\", \"outputs\": [{\"internalType\": \"string\", \"name\": \"\", \"type\": \"string\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"getApproved\", \"outputs\": [{\"internalType\": \"address\", \"name\": \"\", \"type\": \"address\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"getContractBalance\", \"outputs\": [{\"internalType\": \"uint256\", \"name\": \"\", \"type\": \"uint256\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"owner\", \"type\": \"address\"}, {\"internalType\": \"address\", \"name\": \"operator\", \"type\": \"address\"}], \"name\": \"isApprovedForAll\", \"outputs\": [{\"internalType\": \"bool\", \"name\": \"\", \"type\": \"bool\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"string\", \"name\": \"_uri\", \"type\": \"string\"}], \"name\": \"mint\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"name\", \"outputs\": [{\"internalType\": \"string\", \"name\": \"\", \"type\": \"string\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"owner\", \"outputs\": [{\"internalType\": \"address\", \"name\": \"\", \"type\": \"address\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"ownerOf\", \"outputs\": [{\"internalType\": \"address\", \"name\": \"\", \"type\": \"address\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"renounceOwnership\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\"}, {\"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"safeTransferFrom\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\"}, {\"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}, {\"internalType\": \"bytes\", \"name\": \"_data\", \"type\": \"bytes\"}], \"name\": \"safeTransferFrom\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"operator\", \"type\": \"address\"}, {\"internalType\": \"bool\", \"name\": \"approved\", \"type\": \"bool\"}], \"name\": \"setApprovalForAll\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"string\", \"name\": \"uri\", \"type\": \"string\"}], \"name\": \"setDefaultURI\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"bytes4\", \"name\": \"interfaceId\", \"type\": \"bytes4\"}], \"name\": \"supportsInterface\", \"outputs\": [{\"internalType\": \"bool\", \"name\": \"\", \"type\": \"bool\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"symbol\", \"outputs\": [{\"internalType\": \"string\", \"name\": \"\", \"type\": \"string\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"tokenURI\", \"outputs\": [{\"internalType\": \"string\", \"name\": \"\", \"type\": \"string\"}], \"stateMutability\": \"view\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"from\", \"type\": \"address\"}, {\"internalType\": \"address\", \"name\": \"to\", \"type\": \"address\"}, {\"internalType\": \"uint256\", \"name\": \"tokenId\", \"type\": \"uint256\"}], \"name\": \"transferFrom\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"address\", \"name\": \"newOwner\", \"type\": \"address\"}], \"name\": \"transferOwnership\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [{\"internalType\": \"uint256\", \"name\": \"_tokenId\", \"type\": \"uint256\"}, {\"internalType\": \"string\", \"name\": \"_uri\", \"type\": \"string\"}], \"name\": \"updateMetadataOwner\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"inputs\": [], \"name\": \"withdrawContractBalance\", \"outputs\": [], \"stateMutability\": \"nonpayable\", \"type\": \"function\"}, {\"stateMutability\": \"payable\", \"type\": \"receive\"}]";
    // TODO update for get_price()

    async public void MintMobile()
    {
        // https://chainlist.org/
        // value in wei
        string value = "1000000000000000000"; // 1 ether
        // smart contract method to call
        string method = "mint";
        // token uri for nft
        string args = $"[\"{token_uri}\"]";
        // gas limit OPTIONAL
        string gasLimit = "";
        // gas price OPTIONAL
        string gasPrice = "60000000000";
        // send transaction
        string response = "";

        try
        {
            string data = await EVM.CreateContractData(mint_contract_abi, method, args);
            response = await Web3Wallet.SendTransaction(chainIds[chain], contractAddresses[chain], value, data, gasLimit, gasPrice);
        
            // load complete icon
            mediaContainer.GetComponent<MediaContainer>().ShowLoadingIconComplete();

            print(response);
            status.text = $" {response} ({chain})";
            token_uri = "";

            // dispose of stream
            stream.Dispose();

            // TODO upload to aws server db
            
            // add token to user in NFT wallet then reload UI
            GetComponent<NFTWallet>().user.nfts.Add(mediaContainer.GetComponent<MediaContainer>().token);
            GetComponent<NFTWallet>().uiCallback();
            mediaContainer.GetComponent<MediaContainer>().Status.text = $"Confirmed {response} ({chain})";
        }
        catch (Exception e)
        {
            status.text = "error: " + e;
        }
    }
    
}
