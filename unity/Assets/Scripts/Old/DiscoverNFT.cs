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

public class DiscoverNFT : MonoBehaviour
{
    public TMP_Text Status;

    public List <nft> nfts = new List<nft>();
    int nftIndex = 0;
    public List <nft> neighbors = new List<nft>();
    int neighborIndex = 0;

    public Dictionary<string, List<nft>> nftNeighbors = new Dictionary<string, List<nft>>();

    // AWS endpoint for Discover NFT 
    public string api_url = "http://127.0.0.1:8888";

    // load into media container
    public GameObject mediaContainer;
    public GameObject uiContainer;

    // Start is called before the first frame update
    void Start()
    {
        // strip last / in api url
        if (api_url.EndsWith("/"))
        {
            api_url = api_url.Substring(0, api_url.Length - 1);
        }
        
        nftIndex = 0;
        neighborIndex = 0;
        // ping server for some daily stats
        //random();
    }



    class MidjourneyInput {
        public string image;
        public string prompt;
        public string options;
    
        public MidjourneyInput (string image, string prompt, string options) {
            this.image = image;
            this.prompt = prompt;
            this.options = options;
        }
    }

    class ArtJob {
        public string id;
        public string job;
        public int option;
        public ArtJob (string id, string job, int option) {
            this.id = id;
            this.job = job;
            this.option = option;
        }
    }

    public async void UploadArtPrompt(nft token, string prompt, Action callback)
    {
        // get nft metadata as string
        Debug.Log("Uploading txhash...");

        if (prompt.Contains("Write a prompt"))
        {

        }
        else if (prompt.Contains("Process started"))
        {

        }
        else
        {
            MidjourneyInput msg;

            // use existing options in prompt
            if (token.data.Contains("--"))
            {
                msg = new MidjourneyInput("", prompt, "");
            }
            else
            {
                //msg = new MidjourneyInput(token.thumbnail_url, prompt, "--iw 0.1 --ar 10:16");
                msg = new MidjourneyInput("", prompt, "--ar 10:16");
            }

            var itemToSend = JsonUtility.ToJson(msg);
            
            // build web request
            var request = (HttpWebRequest) WebRequest.Create(new Uri($"{api_url}/upload/prompt"));
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Timeout = 4000; //ms
            
            // stream request
            using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                streamWriter.Write(itemToSend);
                streamWriter.Flush();
                streamWriter.Dispose();
            }

            // Send the request to the server and wait for the response:  
            using (var response = await request.GetResponseAsync())
            {
                // Get a stream representation of the HTTP web response:  
                using (var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    var message = reader.ReadToEnd();

                    Debug.Log($"Sent transaction to server: {message}");
                    callback();
                }
            }
        }
    }

    public async void UploadArtJob(nft token, string job, int option, Action callback)
    {
        // get nft metadata as string
        Debug.Log("Uploading...");

        // address = message_id from discord
        ArtJob msg = new ArtJob( token.address, job, option);
        var itemToSend = JsonUtility.ToJson(msg);
        Debug.Log(itemToSend);
    
        // build web request
        var request = (HttpWebRequest) WebRequest.Create(new Uri($"{api_url}/upload/job"));
        request.ContentType = "application/json";
        request.Method = "POST";
        request.Timeout = 4000; //ms
        
        // stream request
        using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
        {
            streamWriter.Write(itemToSend);
            streamWriter.Flush();
            streamWriter.Dispose();
        }

        // Send the request to the server and wait for the response:  
        using (var response = await request.GetResponseAsync())
        {
            // Get a stream representation of the HTTP web response:  
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                var message = reader.ReadToEnd();

                Debug.Log($"Sent transaction to server: {message}");
                callback();
            }
        }
    }

    public async void UploadToDiscover(nft snft)
    {
        // get nft metadata as string
        Debug.Log("Uploading...");
        var itemToSend = JsonUtility.ToJson(snft);
        
        // build web request
        var request = (HttpWebRequest) WebRequest.Create(new Uri($"{api_url}/upload/nft"));
        request.ContentType = "application/json";
        request.Method = "POST";
        request.Timeout = 4000; //ms
        
        // stream request
        using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
        {
            streamWriter.Write(itemToSend);
            streamWriter.Flush();
            streamWriter.Dispose();
        }

        // Send the request to the server and wait for the response:  
        using (var response = await request.GetResponseAsync())
        {
            // Get a stream representation of the HTTP web response:  
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                var message = reader.ReadToEnd();

                // update UI
                Debug.Log(message);
                Status.text = message;
            }
        }
    }

    public void randomNFT()
    {
        if ((nftIndex == nfts.Count))
        {
            nftIndex = 0;
            nfts = new List<nft>();
            StartCoroutine(randomCoroutine($"{api_url}/nft/random?for_sale=false&batch_size=10"));
        }
        else if (nfts.Count > 0)
        {
            if (nfts[0].price > 0 || nfts[0].isAI)
            {
                //
                nftIndex = 0;
                nfts = new List<nft>();
                StartCoroutine(randomCoroutine($"{api_url}/nft/random?for_sale=false&batch_size=10"));
            }
            else
            {
                mediaContainer.SetActive(true);
                mediaContainer.GetComponent<MediaContainer>().Load(nfts[nftIndex]);
            }
        }
        
        nftIndex++;
        // if (neighbors.Count > 0)
        // {
        //     neighborIndex = 0;
        //     neighbors = new List<nft>();
        // }
    }

    
    public void randomAI()
    {
        if ((nftIndex == nfts.Count))
        {
            nftIndex = 0;
            nfts = new List<nft>();
            StartCoroutine(randomAICoroutine($"{api_url}/ai/random?upscaled=true&batch_size=10"));
        }
        else if (nfts.Count > 0)
        {
            if (nfts[0].price > 0 || nfts[nftIndex].token_id==0)
            {
                nftIndex = 0;
                nfts = new List<nft>();
                StartCoroutine(randomAICoroutine($"{api_url}/ai/random?upscaled=true&batch_size=10"));
            }

            mediaContainer.SetActive(true);
            mediaContainer.GetComponent<MediaContainer>().Load(nfts[nftIndex]);
        }
        
        nftIndex++;
        // if (neighbors.Count > 0)
        // {
        //     neighborIndex = 0;
        //     neighbors = new List<nft>();
        // }
    }

    public void StartAILatestQuery() // gets personal feed
    {
        StartCoroutine(latestAICoroutine($"{api_url}/ai/latest?batch_size=150"));
    }

    int midj_cycle = 0;
    public void StartMidJourneyLatestQuery()
    {   
        // cycle querying different midjourney endpoints
        switch (midj_cycle)
        {
            case 0:
                StartCoroutine(latestAICoroutine($"{api_url}/midjourney/latest?batch_size=100&order_by=top"));
                break;
            case 1:
                StartCoroutine(latestAICoroutine($"{api_url}/midjourney/latest?batch_size=100&order_by=rising"));
                break;
            case 2:
                StartCoroutine(latestAICoroutine($"{api_url}/midjourney/latest?batch_size=100&order_by=hot"));
                break;
        }
        midj_cycle = (midj_cycle + 1) % 3;
    }

    public void randomNFTForSale()
    {
        if (nftIndex == nfts.Count)
        {
            nftIndex = 0;
            nfts = new List<nft>();
            StartCoroutine(randomCoroutine($"{api_url}/nft/random?batch_size=10&for_sale=true"));
        }
        else if (nfts.Count > 0)
        {
            mediaContainer.SetActive(true);
            mediaContainer.GetComponent<MediaContainer>().Load(nfts[nftIndex]);
        }

        try{
            if (nfts[0].price == 0)
            {
                nftIndex = 0;
                nfts = new List<nft>();
                StartCoroutine(randomCoroutine($"{api_url}/nft/random?batch_size=10&for_sale=true"));
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        nftIndex++;        
    }

    public void nearestNeighbors(nft token)
    {
        if ((neighborIndex == neighbors.Count))
        {
            neighborIndex = 0;
            neighbors = new List<nft>();
            StartCoroutine(neighborsCoroutine(token));
        }
        else
        {
            mediaContainer.SetActive(true);
            mediaContainer.GetComponent<MediaContainer>().Load(neighbors[neighborIndex]);
        }
        neighborIndex++;
    }

    IEnumerator randomCoroutine(string url)
    {
        // query endpoint /nft/random
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.timeout = 5;
            webRequest.SetRequestHeader("x-api-key", "v0");
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(webRequest.url + ": Error: " + webRequest.error);
                mediaContainer.SetActive(true);
                mediaContainer.GetComponent<MediaContainer>().Status.text = "Error: " + webRequest.error;
            }
            else
            {
                Debug.Log(webRequest.url + ":\nReceived: " + webRequest.downloadHandler.text);
                // parse response
                var response = JSON.Parse(webRequest.downloadHandler.text);
                
                foreach (JSONNode obj in response["result"])
                {
                    // make new nft object
                    // docs: http://127.0.0.1:8888/docs            
                    nft snft = new nft();
                    snft.chain = obj["chain"];
                    snft.address = obj["address"];
                    snft.token_id = obj["token_id"];
                    snft.data = obj["data"];
                    snft.contract_name = obj["name"];
                    snft.price = obj["price"];
                    snft.thumbnail_url = obj["thumbnail"];
                    snft.mintable = false;
                    if (snft.price == 0)
                    {
                        // todo track this on the backend
                        snft.hasNeighbors = true;
                    }
                    else
                    {
                        snft.hasNeighbors = false;
                    }
                    ParseTool.parseMetaData(snft);
                    nfts.Add(snft);
                    if(nfts.Count == 1)
                    {
                        mediaContainer.SetActive(true);
                        mediaContainer.GetComponent<MediaContainer>().Load(nfts[0]);
                    }

                    // query for neighbors
                    nftNeighbors[snft.key] = new List<nft>();
                }
            }
        }
        //StartCoroutine(neighborsCoroutine());
    }


    IEnumerator latestAICoroutine(string url)
    {
        // query endpoint /nft/random
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.timeout = 5;
            webRequest.SetRequestHeader("x-api-key", "v0");
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(webRequest.url + ": Error: " + webRequest.error);
                mediaContainer.SetActive(true);
                mediaContainer.GetComponent<MediaContainer>().Status.text = "Error: " + webRequest.error;
            }
            else
            {
                Debug.Log(webRequest.url + ":\nReceived: " + webRequest.downloadHandler.text);
                // parse response
                var response = JSON.Parse(webRequest.downloadHandler.text);
                nfts = new List<nft>();
                foreach (JSONNode obj in response["result"])
                {
                    // make new nft object
                    // docs: http://127.0.0.1:8888/docs
                    nft snft = new nft();
                    snft.chain = obj["algorithm"];
                    snft.address = obj["message_id"];
                    snft.data = obj["data"];
                    snft.owner = obj["creator"];
                    // remove options from prompt
                    //if (snft.data.Contains("--"))
                    //{
                    //    snft.data = snft.data.Split(new string[] { "--" }, StringSplitOptions.None)[0]; // prompt
                    //}
                    if (snft.data.Contains("<") & snft.data.Contains(">"))
                    {
                        // parse data
                        snft.data = snft.data.Split(new string[] { ">" }, StringSplitOptions.None)[1];
                    }
                    snft.contract_name = obj["name"];
                    snft.price = obj["price"];
                    snft.thumbnail_url = obj["thumbnail"];
                    snft.image_url = obj["thumbnail"];
                    snft.media_url = obj["thumbnail"];
                    if (obj["upscaled"] == true)
                    { 
                        snft.token_id = 1;
                    }
                    else
                    {
                        snft.token_id = 0;
                    }
                    snft.media_type = MediaType.Image;
                    snft.mintable = true;
                    nfts.Add(snft);
                }

                GetComponent<EnhancedScrollerDemos.NestedWallet.Controller>().LoadData(nfts);
            }
        }
    }
    
    IEnumerator randomAICoroutine(string url)
    {
        // query endpoint /nft/random
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.timeout = 5;
            webRequest.SetRequestHeader("x-api-key", "v0");
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(webRequest.url + ": Error: " + webRequest.error);
                mediaContainer.SetActive(true);
                mediaContainer.GetComponent<MediaContainer>().Status.text = "Error: " + webRequest.error;
            }
            else
            {
                Debug.Log(webRequest.url + ":\nReceived: " + webRequest.downloadHandler.text);

                // check if response is Internal Server Error
                if(webRequest.downloadHandler.text.Contains("Server Error"))
                {
                    mediaContainer.SetActive(true);
                    mediaContainer.GetComponent<MediaContainer>().Status.text = "Server error...try again later :3";
                }
                else
                {
                    // parse response
                    var response = JSON.Parse(webRequest.downloadHandler.text);
                    nfts = new List<nft>();
                    foreach (JSONNode obj in response["result"])
                    {
                        // make new nft object
                        // docs: http://127.0.0.1:8888/docs
                        nft snft = new nft();
                        snft.chain = obj["algorithm"];
                        snft.address = obj["message_id"];
                        snft.data = obj["data"];

                        if (obj["upscaled"] == true)
                        { 
                            snft.token_id = 1;
                        }
                        else
                        {
                            snft.token_id = 0;
                        }

                        // remove options from prompt
                        //if (snft.data.Contains("--"))
                        //{
                        //    snft.data = snft.data.Split(new string[] { "--" }, StringSplitOptions.None)[0]; // prompt
                        //}

                        // remove input image from prompt
                        // check if data contains "<"
                        if (snft.data.Contains("<") & snft.data.Contains(">"))
                        {
                            // parse data
                            snft.data = snft.data.Split(new string[] { ">" }, StringSplitOptions.None)[1];
                        }
                        snft.contract_name = obj["name"];
                        snft.price = obj["price"];
                        snft.thumbnail_url = obj["thumbnail"];
                        snft.media_url = obj["thumbnail"];
                        snft.media_type = MediaType.Image;
                        snft.mintable = true;

                        nfts.Add(snft);
                        if(nfts.Count == 1)
                        {
                            mediaContainer.SetActive(true);
                            mediaContainer.GetComponent<MediaContainer>().Load(nfts[0]);
                        }

                        // query for neighbors
                        nftNeighbors[snft.key] = new List<nft>();
                    }
                }
            }
        }
        //StartCoroutine(neighborsCoroutine());
    }

    IEnumerator neighborsCoroutine(nft token)
    {
        // TODO batch this request
        //nft n = mediaContainer.GetComponent<MediaContainer>().token;

        string url = $"{api_url}/discover/neighbors?chain={token.chain}&address={token.address}&token_id={token.token_id}&batch_size=5";

        // query endpoint /nft/random
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.timeout = 15;
            webRequest.SetRequestHeader("x-api-key", "v0");
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(webRequest.url + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(webRequest.url + ":\nReceived: " + webRequest.downloadHandler.text);
                // parse response
                var response = JSON.Parse(webRequest.downloadHandler.text);
                Debug.Log($"{response["result"].Count}");
                for(int i=0; i < response["result"].Count; i++)
                {
                    var obj = response["result"][i];
                    // make new nft object
                    // docs: http://127.0.0.1:8888/docs            
                    nft snft = new nft();
                    snft.chain = obj["chain"];
                    snft.address = obj["address"];
                    snft.token_id = obj["token_id"];
                    snft.data = obj["data"];
                    snft.contract_name = obj["name"];
                    snft.price = obj["price"];
                    snft.thumbnail_url = obj["thumbnail"];
                    snft.hasNeighbors = true;
                    snft.mintable = false;

                    ParseTool.parseMetaData(snft);
                    neighbors.Add(snft);
                    if(neighbors.Count == 1)
                    {
                        mediaContainer.SetActive(true);
                        mediaContainer.GetComponent<MediaContainer>().Load(neighbors[0]);
                    }
                    //nfts.Add(snft);
                }
                Debug.Log($"neighbor count {neighbors.Count}");
            }
        }
        //GetComponent<EnhancedScrollerDemos.InfinityScroll.Controller>().LoadData();   
    }
    
    // Update is called once per frame
    void Update()
    {
         
    }

}
