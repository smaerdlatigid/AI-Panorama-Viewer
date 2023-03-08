using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.ComponentModel;
using TMPro;
using GLTFast;
using Photon.Pun;
using Photon.Realtime;

public class MediaContainer : MonoBehaviourPun
{
    // Media Objects
    public GameObject image;
    public Texture2D imageTexture;
    public GameObject video;
    public GameObject model;
    // UI components
    public GameObject NFTController;
    public GameObject Multiplayer;
    // selected NFT inside the container
    public nft token;
    MediaType mediatype;


    public TextMeshProUGUI Status;
    public string buttonState = "";
    public GameObject canvas = null;
    public GameObject buyButton = null;
    public GameObject infoButton = null;
    public GameObject favoriteButton = null;
    public GameObject sendButton = null;
    public GameObject syncButton = null;
    public GameObject neighborsButton = null;
    public GameObject loadingIcon = null;
    public GameObject mintButton = null;
    public GameObject shuffleButton = null;
    public GameObject SendMenu = null;
    public GameObject NFTButton = null;
    public GameObject AIButton = null;
    public TMP_InputField priceInput = null;
    public TMP_InputField nameInput = null;
    

    public TMP_InputField descriptionInput = null;

    public Material videoMaterial;

    List<Vector3> UIStartingPositions = new List<Vector3>();
    List<GameObject> UIObjects = new List<GameObject>();
    // networksync button

    public Sprite defaultSprite; 

    // Start is called before the first frame update
    void Start()
    {
        // Add listener to buy/sell/mint button
        nameInput.onEndEdit.AddListener(delegate { OnNameChanged(); });    
        descriptionInput.onEndEdit.AddListener(delegate { OnDescriptionChanged(); });

        loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");

        buttonState = "";
        Status.text = "";

        // add UI elements to list
        UIObjects.Add(buyButton);
        UIObjects.Add(infoButton);
        UIObjects.Add(favoriteButton);
        UIObjects.Add(sendButton);
        UIObjects.Add(syncButton);
        UIObjects.Add(neighborsButton);
        UIObjects.Add(priceInput.gameObject);
        UIObjects.Add(Status.gameObject);
        UIObjects.Add(loadingIcon);
        UIObjects.Add(mintButton); // ai prompt upload
        UIObjects.Add(NFTButton);
        UIObjects.Add(nameInput.gameObject);
        UIObjects.Add(descriptionInput.gameObject);
        UIObjects.Add(shuffleButton);
        UIObjects.Add(AIButton);
        // save starting positions of UI elements
        foreach (GameObject obj in UIObjects)
        {
            Vector3 pos = new Vector3(obj.transform.position.x, obj.transform.position.y, obj.transform.position.z);
            UIStartingPositions.Add(pos);
            obj.SetActive(false);
        }
        infoButton.SetActive(false);
        Status.gameObject.SetActive(true);
        loadingIcon.SetActive(false);
        buyButton.SetActive(false);
        SendMenu.SetActive(false);
        shuffleButton.SetActive(true);
        AIButton.SetActive(false);
        NFTButton.SetActive(false);
        
        Status.text = "Welcome! \n\nLog in with MetaMask to view your NFTs, explore the marketplace, generate your own AI art or share your NFTs in an online room.";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void finishedCopying()
    {
        Debug.Log("Finished Copying Texture");
        //Do something else
    }
    public void updatePrice()
    {
        if(token.price > 0)
        {
            priceInput.placeholder.GetComponent<TextMeshProUGUI>().text = $"{token.price/1000000000000000000} {NFTController.GetComponent<MarketPlace>().currency[token.chain]}";
        }
        else
        {
            priceInput.placeholder.GetComponent<TextMeshProUGUI>().text = $"{NFTController.GetComponent<MarketPlace>().currency[token.chain]}...";
        }
    }

    IEnumerator updateButton(int secs)
    {
        sendButton.SetActive(false);
        syncButton.SetActive(false);
        buyButton.SetActive(false);
        priceInput.gameObject.SetActive(false);
        Status.text = "";
        ShowLoadingIcon();

        // query for some on-chain data
        NFTController.GetComponent<MarketPlace>().checkPrice(token, updatePrice); // Price -> Buy/Sell

        yield return new WaitForSeconds(secs);

        if (PlayerPrefs.GetString("Account") == token.owner & PlayerPrefs.GetString("Account") != "")
        {
            sendButton.SetActive(true);

            var market = NFTController.GetComponent<MarketPlace>().market_address;
            if (market.ContainsKey(token.chain))
            {
                if (token.marketApproved & !token.mintable)
                {
                    // Change button to sell
                    buyButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("sell_icon");
                    buyButton.SetActive(true);
                    buttonState = "sell";
                    priceInput.gameObject.SetActive(true);
                    HideLoadingIcon();
                    
                    // set placeholder text
                    priceInput.placeholder.GetComponent<TextMeshProUGUI>().text = $"{token.price/1000000000000000000} {NFTController.GetComponent<MarketPlace>().currency[token.chain]}";
                }
                else
                {
                    if(!token.mintable & PlayerPrefs.GetString("Account") != "")
                    {
                        // Change button to approve marketplace contract
                        buyButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("approve_icon");
                        buyButton.SetActive(true);
                        buttonState = "approve";
                        priceInput.gameObject.SetActive(false);
                        ShowLoadingIcon();
                        NFTController.GetComponent<MarketPlace>().Approve(token, txApproveFinishedSuccess, txFinishedFailure);
                    }
                    else
                    {
                        // chain Not Supported by marketplace contract
                        buyButton.SetActive(false);
                        buttonState = "";
                        priceInput.gameObject.SetActive(false);
                        HideLoadingIcon();
                    }
                }
            }
            else
            {
                // chain Not Supported by marketplace contract
                buyButton.SetActive(false);
                buttonState = "";
                priceInput.gameObject.SetActive(false);
                HideLoadingIcon();
            }
        }
        else
        {
            sendButton.SetActive(false);

            if (token.price > 0)
            {
                // Change button to buy
                buyButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("buy_icon");
                buyButton.SetActive(true);
                buttonState = "buy";
                priceInput.gameObject.SetActive(false);
                HideLoadingIcon();
            }
            else
            {
                // nada
                buyButton.SetActive(false);
                priceInput.gameObject.SetActive(false);
                buttonState = "";
                HideLoadingIcon();
            }
        }

        if (Multiplayer.GetComponent<LoginScreenUI>().inRoom)
        {
            syncButton.SetActive(true);
        }
        else
        {
            syncButton.SetActive(false);
        }
    }

    public void HideLoadingIcon()
    {
        loadingIcon.SetActive(false);
    }
    public void ShowLoadingIcon()
    {
        loadingIcon.SetActive(true);
        loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");
    }
    public void ShowLoadingIconComplete()
    {
        loadingIcon.SetActive(true);
        loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("success_icon");
    }

    public void buttonAction()
    {
        switch (buttonState)
        {
            case "buy":
                NFTController.GetComponent<MarketPlace>().Buy(token, txFinished);
                ShowLoadingIcon();
                break;
            case "sell":
                // check text is not empty
                if (priceInput.text != "")
                {
                    // convert string to float
                    token.price = float.Parse(priceInput.text);
                    if (token.price > 0)
                    {
                        NFTController.GetComponent<MarketPlace>().Sell(token, txFinished);
                        ShowLoadingIcon();
                        Status.text = "Please approve contract interaction to sell this token";
                    }
                    else
                    {
                        Status.text = "Price must be greater than 0";
                    }
                }
                else
                {
                    Status.text = "Please enter a price";
                }
                break;
            case "approve":
                // Approve
                ShowLoadingIcon();
                NFTController.GetComponent<MarketPlace>().checkApproved(token, checkFinished);
                // TODO continously poll until tx is mined
                break;
            case "":
                // Nada
                break;
            default:
                // Not Supported
                break;
        }
    }

    public void checkFinished() 
    { // after checking if token is approved for marketplace
        StartCoroutine(updateButton(0));
    }

    public void txFinishedSuccess()
    {
        //descriptionText.text = "Transaction Finished";
        loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("success_icon");
        loadingIcon.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);
        // set rotation to 0
        StartCoroutine(updateButton(1));
    }

    public void txApproveFinishedSuccess()
    {
        token.marketApproved = true;
        //descriptionText.text = "Transaction Finished";
        loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("success_icon");
        loadingIcon.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);
        // set rotation to 0
        StartCoroutine(updateButton(1));
    }

    public void txFinishedFailure()
    {
        //descriptionText.text = "Transaction Failed";
        loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("cancel_icon");
        loadingIcon.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);
    }

    public void txFinished()
    {
        Status.text = "Transaction Finished";
        StartCoroutine(updateButton(1));
    }

    IEnumerator DownloadTexture(string url)
    { // non-blocking way of loading texture
        if (!url.Contains("http"))
        {
            url = "file://"+url;
        }
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                //ebug.Log(uwr.error);
                //Status.text = "Error: " + uwr.error;
                //image.GetComponent<Image>( ).sprite = defaultSprite;
                image.transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture>("404");
                image.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                loadingIcon.SetActive(false);
                image.GetComponent<Image>().material = null;
            }
            else
            {
                // Get downloaded asset
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                // Apply texture to image
                //image.GetComponent<Image>( ).sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                //image.GetComponent<Image>().material = null;
                image.transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = texture;
                // picture frame has a ratio aspect ratio of 1:1.6
                float aspect = texture.height / (float)texture.width;
                image.transform.localScale = new Vector3(0.75f, 0.75f*aspect*0.625f, 1);
                image.transform.localPosition = new Vector3(-0.075f, 0.2f*aspect, 0);
                Vector3 offset = new Vector3(0, 0, 0);
                offsetUIElements(offset);
                loadingIcon.SetActive(false);
            }
        }
    }

    public void ToggleSendMenu()
    {
        // if owner of token
        if (PlayerPrefs.GetString("Account") == token.owner & PlayerPrefs.GetString("Account") != "")
        {
            SendMenu.SetActive(true);
            NFTController.GetComponent<SendController>().ShowMenu(token);
        }
    }

    public void offsetUIElements(Vector3 offset)
    {
        foreach (GameObject obj in UIObjects)
        {
            obj.transform.position = UIStartingPositions[UIObjects.IndexOf(obj)] + offset;
        }
    }



    async void LoadImage(string path){
        // async load texture
        StartCoroutine(DownloadTexture(path));
    }


    async void LoadModel()
    {
        model.SetActive(true);

        // remove all children
        foreach (Transform child in model.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // load new model
        var gltf = new GLTFast.GLTFast();

        bool success = false;
        // check that media_path exists
        if (File.Exists(token.media_path))
        {
            // load model
            try
            {
                success = await gltf.Load(token.media_path);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                // delete file at media path
                File.Delete(token.media_path);
                Load(token);
                // could break app
            }
        }
        else
        {
            print("File not found: " + token.media_path);
        }
        

        if (success) {
            var material = gltf.GetMaterial();

            if (gltf.InstantiateScene(model.transform))
            {
                GameObject nft_model = model.transform.GetChild(0).gameObject;
                // set parent
                nft_model.transform.SetParent(model.transform);

                Bounds currentBounds = GetTotalBounds(model);
                float unitSize = 1f/Mathf.Max(currentBounds.size.x, currentBounds.size.y, currentBounds.size.z);
                nft_model.transform.localScale *= unitSize;
                nft_model.transform.localEulerAngles = new Vector3(0, 180f, 0);
                //model.transform.position = new Vector3(-0.5f*bounds.center.x, -0.5f*bounds.center.y, 0);
                nft_model.transform.localPosition = new Vector3(-0.5f*Mathf.Max(bounds.center.x,bounds.center.z), -0.25f*bounds.center.y, 0);
                currentBounds = GetTotalBounds(model);
                float aspect = currentBounds.size.y / (float)currentBounds.size.x;

                Debug.Log("Current size " + currentBounds.size + " LOCAL ROTATION " +
                        nft_model.transform.localEulerAngles + " ASPECT " + aspect);
                Debug.Log($"Bounds Center {currentBounds.center}");

                // adjust aspect ratio
                //Vector3 offset = new Vector3(0, -0.5f, -0.1f);
                //offsetUIElements(offset);
                loadingIcon.SetActive(false);
            }
        } else {
            Debug.LogError($"Loading glTF failed!");
        }
    }

    public void NetworkSync()
    {
        if (Multiplayer.GetComponent<LoginScreenUI>().inRoom)
        {
            PhotonView photonView = PhotonView.Get(this);
            string nft_json = JsonUtility.ToJson(token);
            Debug.Log("Sending NFT: " + nft_json);
            photonView.RPC("NetworkLoad", RpcTarget.All, nft_json);
        }
        else
        {
            syncButton.SetActive(false);
        }

    }

    [PunRPC]
    public void NetworkLoad(string json)
    {
        Load(JsonUtility.FromJson<nft>(json));
    }

    void LoadMedia()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);
        transform.position = new Vector3(0.05f, 0.2f, 0.5f); // todo record starting position
        GetComponent<TouchControls>().enabled = true;

        if (token.mintable)
        {
            sendButton.SetActive(false);
            buyButton.SetActive(false);
            infoButton.SetActive(true);
            favoriteButton.SetActive(false);
            priceInput.gameObject.SetActive(false);
            buttonState = "";
            AIButton.SetActive(true);
            mintButton.SetActive(false);
        }
        else
        {
            favoriteButton.GetComponent<Button>().image.sprite = Resources.Load<Sprite>("heart_open");
            sendButton.SetActive(false);
            buyButton.SetActive(false);
            infoButton.SetActive(true);
            favoriteButton.SetActive(true);
            priceInput.gameObject.SetActive(false);
            nameInput.gameObject.SetActive(false);
            NFTButton.SetActive(false);
            Status.text = "";
            mintButton.SetActive(false);
        }
        descriptionInput.gameObject.SetActive(false);
        nameInput.gameObject.SetActive(false);

        // set up UI
        Status.text = "";

        if (Multiplayer.GetComponent<LoginScreenUI>().inRoom)
        {
            syncButton.SetActive(true);
        }
        else
        {
            syncButton.SetActive(false);
        }

        if (token.hasNeighbors)
        {
            neighborsButton.SetActive(true);
            shuffleButton.SetActive(true);
            AIButton.SetActive(true);
            descriptionInput.gameObject.SetActive(false);
            mintButton.SetActive(false);
        }
        else
        {
            neighborsButton.SetActive(false);
            shuffleButton.SetActive(true);
        }

        if (token.isAI)
        {
            if (token.owner == "")
            {
                NFTButton.SetActive(true);
            }
            else
            {
                NFTButton.SetActive(false);
            }
        }

        // check if user owns token
        if (PlayerPrefs.GetString("Account") == token.owner & PlayerPrefs.GetString("Account") != "")
        {
            buttonState = "approve";
            buyButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("approve_icon");
            buyButton.SetActive(true);
            sendButton.SetActive(true);
        }
        else
        {
            buttonState = "buy";
            if(token.price>0)
            {
                buyButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("buy_icon");
                buyButton.SetActive(true);
            }
            else
            {
                buyButton.SetActive(false);
            }
        }

        var market = NFTController.GetComponent<MarketPlace>().market_address;
        if (!market.ContainsKey(token.chain))
        {
            buyButton.SetActive(false);
        }
    
        // case if switch for media type
        switch (token.media_type)
        {
            case MediaType.Image:
                LoadImage(token.media_path);
                image.SetActive(true);
                video.SetActive(false);
                model.SetActive(false);
                AIButton.SetActive(true);
                break;
            case MediaType.Video:
                // load video
                image.SetActive(true);
                video.SetActive(true);
                model.SetActive(false);
                AIButton.SetActive(true);
                LoadVideo();
                break;
            case MediaType.Model:
                LoadModel();
                image.SetActive(false);
                video.SetActive(false);
                model.SetActive(true);
                AIButton.SetActive(false);
                break;
            case MediaType.Effect:
                // load effect
                break;
            default:
                break;
        }
    }

    public void RefreshToken()
    {
        // search for media on disk and delete
        if (File.Exists(token.media_path))
        {
            File.Delete(token.media_path);
        }
        Load(token);
    }

    public void FindNeighbors(){
        NFTController.GetComponent<DiscoverNFT>().nearestNeighbors(token);
    }

    async public void LoadVideo()
    {
        // set sprite material to video
        //image.GetComponent<Image>().material = videoMaterial;
        image.transform.GetChild(0).GetComponent<Renderer>().material = videoMaterial;

        // Load video file from token.media_path
        video.GetComponent<VideoPlayer>().url = token.media_path;
        video.GetComponent<VideoPlayer>().Prepare();
        while (!video.GetComponent<VideoPlayer>().isPrepared)
        {
            // loading icon
            await new WaitForSeconds(0.5f);
            Debug.Log("Waiting for video to be ready");
        }
        video.GetComponent<VideoPlayer>().Play();
        loadingIcon.SetActive(false);
    }

    // check if in a room, then call NetworkSync
    async public void Load(nft newnft)
    {
        // create copy of token
        token = newnft;

        // check for thumbnail

        if (token != null)
        {

            loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");
            loadingIcon.SetActive(true);

            if (String.IsNullOrWhiteSpace(newnft.media_url))
            {
                Debug.Log("meta data is empty");
                Debug.Log($"{newnft.chain} {newnft.token_id}");
                Debug.Log($"{newnft.data} {newnft.thumbnail_url}");

                // nothing to load
                //status.text = "Metadata is empty.. try again...";
            }
            else
            {
                Debug.Log($"Media type: {token.media_type}");
                Debug.Log($"Media url: {token.media_url}");
                Debug.Log($"Media path: {token.media_path}");
                // Debug.Log(newnft.data);

                // load media or download it
                if (!String.IsNullOrWhiteSpace(token.media_url))
                {
                    if (File.Exists(token.media_path))
                    {
                        LoadMedia();
                    }
                    else // download media to disk
                    {
                        using (WebClient webClient = new WebClient())
                        {
                            // load thumbnail if it exists
                            if (!String.IsNullOrWhiteSpace(token.thumbnail_url) & token.media_type == MediaType.Image)
                            {
                                Uri uri = new Uri(token.thumbnail_url);
                                Debug.Log($"Downloading thumbnail from {uri}");

                                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleteRetry);
                                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                                webClient.DownloadFileAsync(uri, token.media_path);
                            }
                            else
                            {
                                Uri uri = new Uri(token.media_url);

                                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadComplete);
                                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                                webClient.DownloadFileAsync(uri, token.media_path);
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("No media url");
                    //Status.text = $"{token.data}";
                }
            }
        }
    }

    async public void SetupMinter(Texture2D texture, string filename)
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);
        transform.position = new Vector3(0, 0.5f, 2.5f);

        float aspect = texture.height / (float)texture.width;

        image.transform.localScale = new Vector3(2, 2*aspect, 0.75f); // gets over written else where
        image.transform.localPosition = new Vector3(-1, 1.5f*aspect, 0);

        nameInput.gameObject.SetActive(true);
        priceInput.gameObject.SetActive(false);
        descriptionInput.gameObject.SetActive(true);
        neighborsButton.SetActive(false);
        shuffleButton.SetActive(false);
        sendButton.SetActive(false);
        mintButton.SetActive(true);
        token = new nft();
        token.media_type = MediaType.Image;
        token.image_url = filename;
        token.media_url = filename;
        token.data = "";
        token.owner = PlayerPrefs.GetString("Account");
        token.hasNeighbors = false;
        token.price = 0;
        token.chain = "polygon";
        token.token_id = 0;
        token.tokenuri = "";
        token.mintable = true;
        token.contract_name = "Mint More Art";

        // set material texture on painting
        image.transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = texture;

        buyButton.SetActive(false);
        favoriteButton.SetActive(false);

        image.SetActive(true);
        video.SetActive(false);
        model.SetActive(false);
    }

    public void OnNameChanged()
    {
        token.name = nameInput.text;
    }

    public void OnDescriptionChanged()
    {
        token.description = descriptionInput.text;
    }


    public void MintToken()
    {
        // check account is not empty
        if (String.IsNullOrEmpty(PlayerPrefs.GetString("Account")))
        {
            Status.text = "Please sign in before minting";
            NFTController.GetComponent<NFTWallet>().OnLogin(null);
            return;
        }
        if(!NFTController.GetComponent<NFTWallet>().loggedIn)
        {
            Status.text = "Please sign in before minting";
            NFTController.GetComponent<NFTWallet>().OnLogin(null);
            return;
        }
        // check token has name and description
        if (String.IsNullOrWhiteSpace(token.name))
        {
            Status.text = "Please enter a name";
            token.description = $"Generated with {token.chain}\n\n{token.data}";
            nameInput.gameObject.SetActive(true);
            return;
        }
        // check owner
        if (String.IsNullOrWhiteSpace(token.owner))
        {
            token.owner = PlayerPrefs.GetString("Account");
            Status.text = "The mint price is 10 Matic. Click again to confirm.";
            return;
        }

        // loading icon
        ShowLoadingIcon();
        NFTController.GetComponent<Minter>().Upload(token);
    }

    public void FavoriteNFT()
    {
        // add to local favorites - TODO create class to serialize list of favorites
        // change button to favorite
        if(favoriteButton.GetComponent<Button>().image.sprite.name == "heart_open")
        {
            favoriteButton.GetComponent<Button>().image.sprite = Resources.Load<Sprite>("heart_closed");
        }
        else
        {
            favoriteButton.GetComponent<Button>().image.sprite = Resources.Load<Sprite>("heart_open");
        }
    }

    public void OpenAIPrompt()
    {
        if(mintButton.active)
        {
            mintButton.SetActive(false);
            descriptionInput.gameObject.SetActive(false);
            nameInput.gameObject.SetActive(false);
            Status.text = "";
        }
        else
        {
            mintButton.SetActive(true);
            descriptionInput.gameObject.SetActive(true);
            Status.text = "Write a prompt for image generation using Mid Journey.";
            // remove all listeners from mint button
            mintButton.GetComponent<Button>().onClick.RemoveAllListeners();
            mintButton.GetComponent<Button>().onClick.AddListener(() => { 
                // start loading icon
                loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");
                loadingIcon.SetActive(true);
                SendAIPrompt(); });
            if (token.chain == "midjourney")
            {
                descriptionInput.text = token.data;
            }
        }

    }

    public void SendAIPrompt()
    {
        NFTController.GetComponent<DiscoverNFT>().UploadArtPrompt(token, descriptionInput.text, AISendComplete);
    }

    public void AISendComplete()
    {
        loadingIcon.SetActive(false);
        descriptionInput.text = "Process started check the brain feed soon...";
    }

    public void ToggleInfo()
    {
        if (token.chain == "midjourney")
        {
            if (Status.text == "")
            {
                Status.text = $"Generated with {token.chain}\n\n{token.data}";
            }
            else
            {
                Status.text = "";
                mintButton.SetActive(false);
                descriptionInput.gameObject.SetActive(false);
            }
        }
        else
        {
            if (Status.text == "")
            {
                if(token != null)
                {
                    Status.text += $"{token.name}\n\n{token.description}\n\n";
                    
                    if (token.price > 0)
                    {
                        Status.text += $"\nPrice: {token.price/1000000000000000000} {NFTController.GetComponent<MarketPlace>().currency[token.chain]}\n\n";
                    }

                    Status.text += $"{token.contract_name}({token.chain})\nToken ID:{token.token_id}\n";
                }
                else
                {
                    Status.text = "No token selected";
                    loadingIcon.SetActive(false);
                }
            }
            else
            {
                Status.text = "";
            }
        }
    }


    void DownloadCompleteRetry(object sender, AsyncCompletedEventArgs e)
    {
        // check for success
        if (e.Cancelled || e.Error != null)
        {
            Debug.Log("Error downloading file");
            // try again if last url was thumbnail
            using (WebClient webClient = new WebClient())
            {
                Uri uri = new Uri(token.media_url);

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadComplete);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                webClient.DownloadFileAsync(uri, token.media_path);
            }
            return;
        }
        else
        {
            LoadMedia();
        }
    }

        void DownloadComplete(object sender, AsyncCompletedEventArgs e)
    {
        // check for success
        if (e.Cancelled || e.Error != null)
        {
            Status.text = $"Error downloading file";
            return;
        }
        else
        {
            LoadMedia();
        }
    }

    void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        // update status
        Status.text = $"Token {token.token_id} from {token.contract_name}\n Downloading {token.media_type}...\n{e.ProgressPercentage}%";
    }

    private Bounds bounds;

    Bounds GetTotalBounds(GameObject gameObjectToCompute)
    {
        Bounds bounds = new Bounds (gameObjectToCompute.transform.position, Vector3.one);
        Renderer[] renderers = gameObjectToCompute.GetComponentsInChildren<Renderer> ();
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate (renderer.bounds);
        }
        return bounds;
    }

    public void ToggleTouchControls()
    {
        if (GetComponent<TouchControls>().enabled)
        {
            GetComponent<TouchControls>().enabled = false;
        }
        else
        {
            GetComponent<TouchControls>().enabled = true;
        }
    }

    public void ToggleHorizontalControls()
    {
        if(GetComponent<TouchControls>().enableHorizontalTouch)
        {
            GetComponent<TouchControls>().enableHorizontalTouch = false;
        }
        else
        {
            GetComponent<TouchControls>().enableHorizontalTouch = true;
        }
    }
}