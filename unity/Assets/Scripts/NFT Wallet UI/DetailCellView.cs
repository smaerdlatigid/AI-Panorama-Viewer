using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using Unity.VectorGraphics;
using TMPro;
using System;

namespace EnhancedScrollerDemos.NestedWallet
{
    /// <summary>
    /// This is the view of our cell which handles how the cell looks.
    /// </summary>
    public class DetailCellView : EnhancedScrollerCellView
    {
        public Text descriptionText;
        public Image cellImage;
        public Sprite defaultSprite;
        public GameObject buyButton = null;
        public GameObject sendButton = null;

        public GameObject imageButton = null;
        public GameObject favoriteButton = null;
        public GameObject loadingIcon = null;
        public TMP_InputField priceInput = null;

        Button BuySellMintButton;
        private Coroutine _loadImageCoroutine;

        public nft token;

        // objects that get passed in
        public GameObject MediaContainer;
        public GameObject NFTController;
        public GameObject NavigationBar;
        public GameObject SendMenu;

        public GameObject RemixButtons;

        public string buttonState;
        // color dictionary for each chain
        public Dictionary<string, Color> chainColor = new Dictionary<string, Color>() {
            {"mumbai", new Color(0,0,0)},
            {"polygon", new Color(20f/255f,20f/255f,20f/255f)},// ETH3D
            {"midjourney", new Color(20f/255f,20f/255f,20f/255f)},// ETH3D
            {"eth", new Color(186f/255f,218f/255f,1f)}, // ETH3D
            {"bsc", new Color(1f,1f,210f/255f)} // ETH3D
        };

        public void SetData(DetailData data)
        {
            // update the UI text with the cell data
            token = data.token;
            descriptionText.text = data.token.key;
            MediaContainer = data.MediaContainer;
            NavigationBar = data.NavigationBar;
            NFTController = data.NFTController;
            SendMenu = data.SendMenu;

            _loadImageCoroutine = StartCoroutine(LoadRemoteImage(data));

            BuySellMintButton = buyButton.GetComponent<Button>();

            // check if user owns token
            if (PlayerPrefs.GetString("Account") == token.owner)
            {
                buttonState = "approve";
            }
            else
            {
                buttonState = "buy";
            }

            priceInput.gameObject.SetActive(false);
            loadingIcon.SetActive(false);

            if (PlayerPrefs.GetString("Account") == token.owner)
            {
                RemixButtons.SetActive(false);
                sendButton.SetActive(true);

                var market = NFTController.GetComponent<MarketPlace>().market_address;
                if (market.ContainsKey(token.chain))
                {
                    if (token.marketApproved)
                    {
                        // Change button to sell
                        BuySellMintButton.interactable = true;
                        BuySellMintButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("sell_icon");
                        buyButton.SetActive(true);
                        buttonState = "sell";
                        priceInput.gameObject.SetActive(true);
                    }
                    else
                    {
                        // Change button to approve
                        BuySellMintButton.interactable = true;
                        BuySellMintButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("approve_icon");
                        buyButton.SetActive(true);
                        buttonState = "approve";
                        priceInput.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // chain Not Supported by marketplace contract
                    BuySellMintButton.interactable = false;
                    buyButton.SetActive(false);
                    buttonState = "";
                    priceInput.gameObject.SetActive(false);
                }
            }
            else
            {
                //print("Player is not the owner");
                //print("token.owner: " + token.owner);
                //print("user: " + PlayerPrefs.GetString("Account"));
                if (token.price > 0)
                {
                    // Change button to buy
                    BuySellMintButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("buy_icon");
                    BuySellMintButton.interactable = true;
                    buyButton.SetActive(true);
                    buttonState = "buy";
                    priceInput.placeholder.GetComponent<TextMeshProUGUI>().text = $"{token.price/1000000000000000000} {NFTController.GetComponent<MarketPlace>().currency[token.chain]}";
                    priceInput.gameObject.SetActive(true);
                }
                else
                {
                    // nada
                    BuySellMintButton.interactable = false;
                    buyButton.SetActive(false);
                    buttonState = "";
                    priceInput.gameObject.SetActive(false);
                }

                // token_id: 1 = upsampled
                if ((token.chain == "midjourney") & (token.token_id==0))
                {
                    RemixButtons.SetActive(true);
                }
                else
                {
                    RemixButtons.SetActive(false);
                }

                if (token.chain =="midjourney")
                {
                    sendButton.SetActive(false);
                }

            }
            ToggleInfo();

        }
        public void updatePrice()
        {
            if(token.price > 0)
            {
                priceInput.placeholder.GetComponent<TextMeshProUGUI>().text = $"{token.price/1000000000000000000} {NFTController.GetComponent<MarketPlace>().currency[token.chain]}";
            }
        }

        public void toggleImage()
        {
            ToggleInfo();
            // set opacity on image 
            if(cellImage.color.a == 0)
            {
                cellImage.color = new Color(1,1,1,1);
            }
            else
            {
                cellImage.color = new Color(1,1,1,0);
            }
        }
        IEnumerator updateButton(int secs)
        {
            yield return new WaitForSeconds(secs);

            if (PlayerPrefs.GetString("Account") == token.owner)
            {
                var market = NFTController.GetComponent<MarketPlace>().market_address;
                if (market.ContainsKey(token.chain))
                {
                    if (token.marketApproved)
                    {
                        // Change button to sell
                        BuySellMintButton.interactable = true;
                        BuySellMintButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("sell_icon");
                        buyButton.SetActive(true);
                        buttonState = "sell";
                        priceInput.gameObject.SetActive(true);
                        loadingIcon.SetActive(false);
                        priceInput.placeholder.GetComponent<TextMeshProUGUI>().text = $"{token.price/1000000000000000000} {NFTController.GetComponent<MarketPlace>().currency[token.chain]}";
                        // check price
                        NFTController.GetComponent<MarketPlace>().checkPrice(token, updatePrice);
                    }
                    else
                    {
                        // Change button to approve
                        BuySellMintButton.interactable = true;
                        BuySellMintButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("approve_icon");
                        buyButton.SetActive(true);
                        buttonState = "approve";
                        priceInput.gameObject.SetActive(false);
                        loadingIcon.SetActive(true);
                        loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");

                        // call marketplace smart contract
                        NFTController.GetComponent<MarketPlace>().Approve(token, txApproveFinishedSuccess, txFinishedFailure);
                    }
                }
                else
                {
                    // chain Not Supported by marketplace contract
                    BuySellMintButton.interactable = false;
                    buyButton.SetActive(false);
                    buttonState = "";
                    priceInput.gameObject.SetActive(false);
                }
            }
            else
            {
                //print("Player is not the owner");
                //print("token.owner: " + token.owner);
                //print("user: " + PlayerPrefs.GetString("Account"));
                if (token.price > 0)
                {
                    // Change button to buy
                    BuySellMintButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("buy_icon");
                    BuySellMintButton.interactable = true;
                    buyButton.SetActive(true);
                    buttonState = "buy";
                    priceInput.placeholder.GetComponent<TextMeshProUGUI>().text = $"{token.price/1000000000000000000} {NFTController.GetComponent<MarketPlace>().currency[token.chain]}";
                    priceInput.gameObject.SetActive(true);
                    loadingIcon.SetActive(false);

                }
                else
                {
                    // nada
                    BuySellMintButton.interactable = false;
                    buyButton.SetActive(false);
                    buttonState = "";
                    priceInput.gameObject.SetActive(false);
                    loadingIcon.SetActive(false);

                }
            }
            
            if ((token.chain == "midjourney") & (token.token_id==0))
            {
                RemixButtons.SetActive(true);
            }
            else
            {
                RemixButtons.SetActive(false);
            }

        }

        public void ViewButton()
        {
            MediaContainer.GetComponent<MediaContainer>().Load(token);
            NavigationBar.GetComponent<NavigationBar>().ToggleWallet();
            MediaContainer.GetComponent<TouchControls>().enabled = true;
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

        public void ToggleInfo()
        {
            if (token.chain == "midjourney")
            {
                if (descriptionText.text == "")
                {
                    descriptionText.text = $"Generated with {token.chain}\n\n{token.data}";
                }
                else
                {
                    descriptionText.text = "";
                }
            }
            else
            {
                    
                if (descriptionText.text == "")
                {
                    descriptionText.text += $"{token.name}\n{token.description}\n";

                    if (token.price > 0)
                    {
                        descriptionText.text += $"Price: {token.price/1000000000000000000} {NFTController.GetComponent<MarketPlace>().currency[token.chain]}\n";
                    }

                    descriptionText.text += $"{token.contract_name}({token.chain})\nToken ID:{token.token_id}\n";
                    // if approved marketplace contract
                    if (token.marketApproved)
                    {
                        descriptionText.text += "Ready to Sell\n";
                    }
                    else
                    {
                        descriptionText.text += "Check Approved?\n";
                    }
                }
                else
                {
                    descriptionText.text = "";
                }
            }
            
        }

        public void dynamicButtonAction() // Buy/sell/mint button
        {
            //NFTController.GetComponent<MarketPlace>().checkApproved(token); // needed to sell
            switch (buttonState)
            {
                case "buy":
                    NFTController.GetComponent<MarketPlace>().Buy(token);
                    loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");
                    loadingIcon.SetActive(true);
                    break;
                case "sell":
                // check text is not empty
                if (priceInput.text != "")
                {
                    // convert string to float
                    try{
                        token.price = float.Parse(priceInput.text);
                    }
                    catch(Exception e)
                    {
                        print(e);
                        break;
                    }

                    if (token.price > 0)
                    {
                        NFTController.GetComponent<MarketPlace>().Sell(token, txFinishedSuccess);
                        loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");
                        loadingIcon.SetActive(true);
                        descriptionText.text = "Please approve contract interaction to sell this token";
                    }
                    else
                    {
                        descriptionText.text = "Price must be greater than 0";
                    }

                }
                else
                {
                    descriptionText.text = "Please enter a price";
                }
                break;
            case "approve":
                // Approve                
                // query for some on-chain data
                loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");
                loadingIcon.SetActive(true);
                NFTController.GetComponent<MarketPlace>().checkApproved(token, checkFinished);
                break;
                case "":
                    break;
                default:
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

        public void ToggleSendMenu()
        {
            // if owner of token
            if (PlayerPrefs.GetString("Account") == token.owner & PlayerPrefs.GetString("Account") != "")
            {
                SendMenu.SetActive(true);
                NFTController.GetComponent<SendController>().ShowMenu(token);
            }
        }

        public void jobUploadFinished()
        {
            //descriptionText.text = "Transaction Finished";
            loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("success_icon");
            loadingIcon.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);
            loadingIcon.SetActive(true);
            // set rotation to 0
            StartCoroutine(updateButton(1));
        }

        public void SendUpsampleJob(int option)
        {
            // sometimes GUI glitches and buttons show for upsampled images
            if(token.token_id == 1) // 1 = upsample, buttons
            {
                RemixButtons.SetActive(false);
            }
            else
            {
                loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");
                loadingIcon.SetActive(true);
                RemixButtons.transform.GetChild(1).transform.GetChild(option-1).GetComponent<Button>().interactable = false;
                NFTController.GetComponent<DiscoverNFT>().UploadArtJob(token, "upsample", option, jobUploadFinished);
            }
        }
        public void SendVariationJob(int option)
        {
            // sometimes GUI glitches and buttons show for upsampled images
            if(token.token_id == 1) // 1 = upsample, buttons
            {
                RemixButtons.SetActive(false);
            }
            else
            {
                loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");
                loadingIcon.SetActive(true);
                RemixButtons.transform.GetChild(0).transform.GetChild(option-1).GetComponent<Button>().interactable = false;
                NFTController.GetComponent<DiscoverNFT>().UploadArtJob(token, "variation", option, jobUploadFinished);
            }
        }

        private Texture2D ScaleTexture(Texture2D source,int targetWidth,int targetHeight) {
            Texture2D result=new Texture2D(targetWidth,targetHeight,source.format,true);
            Color[] rpixels=result.GetPixels(0);
            float incX=((float)1/source.width)*((float)source.width/targetWidth);
            float incY=((float)1/source.height)*((float)source.height/targetHeight);
            for(int px=0; px<rpixels.Length; px++) {
                    rpixels[px] = source.GetPixelBilinear(incX*((float)px%targetWidth),
                    incY*((float)Mathf.Floor(px/targetWidth)));
            }
            result.SetPixels(rpixels,0);
            result.Apply();
            return result;
        }

        public IEnumerator LoadRemoteImage(DetailData data)
        {
            string path = data.token.thumbnail_url;
            // check if thumbnail is empty
            if (path == "")
            {
                path = data.token.image_url;
            }
            Texture2D texture = null;

            // change background image based on chain
            string key = data.token.chain;
            if (chainColor.ContainsKey(key))
            {
                GetComponent<Image>().color = chainColor[key];
            }
            else
            {
                GetComponent<Image>().color = Color.white;
            }

            // download image from url
            if ((path.Contains("http") || path.Contains("https")))
            {
                // check if file exists
                if (File.Exists(token.image_path))
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(token.image_path);
                        texture = new Texture2D(4, 4);
                        texture.LoadImage(fileData);
                    }
                    catch (Exception e)
                    {
                        print(e);
                        // remove image at path
                        //File.Delete(token.image_path);

                        // try to load again
                        //StartCoroutine(LoadRemoteImage(data));
                    }
                }
                else if (File.Exists(token.image_path.Replace(".svg",".bytes")))
                {
                    LoadSVG(token.image_path.Replace(".svg",".bytes"));
                }
                else
                {
                   // download image to file
                    var webRequest = UnityWebRequest.Get(path);
                    yield return webRequest.SendWebRequest();
                    if (webRequest.isNetworkError || webRequest.isHttpError)
                    {
                        Debug.LogError("Failed to download image [" + path + "]: " + webRequest.error);
                    }
                    else
                    {
                        if(token.image_path.Contains(".svg"))
                        {
                            // replace .svg with .bytes?
                            File.WriteAllBytes(token.image_path.Replace(".svg",".svg"), webRequest.downloadHandler.data);
                            LoadSVG(token.image_path.Replace(".svg",".svg"));
                            //ClearImage();
                        }
                        else{
                            // load file in as jpg,png, etc
                            texture = new Texture2D(4, 4);//, TextureFormat.DXT1, false);
                            texture.LoadImage(webRequest.downloadHandler.data);
                            // free memory
                            webRequest.Dispose();
                        }
                    }

                    if (texture != null)
                    {
                        if (texture.width > 512)
                        {
                            Texture2D resizedTexture;
                            if (texture.width < texture.height)
                            {
                                resizedTexture = ScaleTexture(texture, 512, (int)(texture.height * (512.0f / texture.width)));
                            }
                            else
                            {
                                resizedTexture = ScaleTexture(texture, (int)(texture.width * (512.0f / texture.height)), 512);
                            }

                            // float aspect = (float)texture.width / (float)texture.height;
                            // int height = (int)(512f*aspect);
                            // Texture2D resizedTexture = ScaleTexture(texture, 512, height );
                            // save texture to file
                            byte[] bytes = resizedTexture.EncodeToJPG();
                            System.IO.File.WriteAllBytes(token.image_path, bytes);
                            texture = resizedTexture;
                            Debug.Log($"Saved image to {token.image_path}");
                        }
                        else
                        {
                            // save texture to file
                            //byte[] bytes = texture.EncodeToJPG();
                            //System.IO.File.WriteAllBytes(token.image_path, bytes);
                            //Debug.Log($"Saved image to {token.image_path}");
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"{path} is not a valid image url");
            }


            if (texture != null)
            {
                cellImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), texture.width);
                // set aspect    
                float aspect = (float)texture.width / (float)texture.height;
                if (aspect < 1)
                {
                    cellImage.GetComponent<RectTransform>().sizeDelta = new Vector2(575*aspect, 575);
                }
                else
                {
                    cellImage.GetComponent<RectTransform>().sizeDelta = new Vector2(575, 575/aspect);
                }
            }
        }

        public void ClearImage()
        {
            cellImage.sprite = defaultSprite;
            // destory texture

        }

        private void LoadSVG(string fname)
        {
            ClearImage();

            // can only run below in start?

            // read file in as text asset
            // var textAsset = new TextAsset(File.ReadAllText(fname));

            // // render options
            // var tessOptions = new VectorUtils.TessellationOptions() {
            //     StepDistance = 100.0f,
            //     MaxCordDeviation = 0.5f,
            //     MaxTanAngleDeviation = 0.1f,
            //     SamplingStepSize = 0.01f
            // };

            // // Dynamically import the SVG data, and tessellate the resulting vector scene.
            // var sceneInfo = SVGParser.ImportSVG(new StringReader(textAsset.text));
            // var geoms = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);

            // // Build a sprite with the tessellated geometry.
            // var sprite = VectorUtils.BuildSprite(geoms, 10.0f, VectorUtils.Alignment.Center, Vector2.zero, 128, true);
            // cellImage.sprite = sprite;
        }

        /// <summary>
        /// Stop the coroutine if the cell is going to be recycled
        /// </summary>
        public void WillRecycle()
        {
            if (_loadImageCoroutine != null)
            {
                StopCoroutine(_loadImageCoroutine);
            }
        }

    }
}