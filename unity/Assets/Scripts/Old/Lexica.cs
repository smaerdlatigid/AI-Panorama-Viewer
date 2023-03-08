using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using TMPro;

public class Timestamp {
    public string id = "0482ee68-0368-4eca-8846-5930db866b33";
    public long timestamp = 1624567890;
}

class LexicaImage {
    // The ID of the image
    string id = "0482ee68-0368-4eca-8846-5930db866b33";
    // URL for the image's gallery
    string gallery = "https://lexica.art?q=0482ee68-0368-4eca-8846-5930db866b33";
    // Link to this image
    string src = "https://image.lexica.art/md/0482ee68-0368-4eca-8846-5930db866b33";
    // Link to an compressed & optimized version of this image
    string srcSmall = "https://image.lexica.art/sm/0482ee68-0368-4eca-8846-5930db866b33";
    // The prompt used to generate this image
    string prompt = "cute chubby blue fruits icons for mobile game ui ";
    // Image dimensions
    int width = 512;
    int height = 512;
    // Seed
    string seed = "1413536227";
    // Whether this image is a grid of multiple images
    bool grid = false;
    // The model used to generate this image
    string model = "stable-diffusion";
    // Guidance scale
    float guidance = 7f;
    // The ID for this image's prompt
    string promptid = "d9868972-dad8-477d-8e5a-4a0ae1e9b72b";
    // Whether this image is classified as NSFW
    bool nsfw = false;
}

class LexicaSearchResult {
    // Array of 50 results
    public LexicaImage[] images = new LexicaImage[50];
}

class LexicaFavorites {
    public List<LexicaImage> images = new List<LexicaImage>();
    // add prompts to favorites + feeds
}

class UserFeed { 
    public List<LexicaImage> images = new List<LexicaImage>();
}

class UserHistory {
    public List<LexicaImage> images = new List<LexicaImage>();
    public List<Timestamp> timestamps = new List<Timestamp>(); // last viewed timestamp for each image
}


public class Lexica : MonoBehaviour
{
    // if search includes http then it uses reverse image search
    string search_url = "https://lexica.art/api/v1/search?q=";
    LexicaSearchResult searchResult = new LexicaSearchResult();
       
    // Start is called before the first frame update
    void Start()
    {
        //Search("cute chubby blue fruits icons for mobile game ui");
        Search("https://image.lexica.art/md/0482ee68-0368-4eca-8846-5930db866b33");
        // TODO UI, Add to favorites
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Search(string search)
    {
        string url = search_url + search.Replace(" ", "+");
        StartCoroutine(GetRequest(url));
    }

    IEnumerator GetRequest(string url)
    {
        // query endpoint /nft/random
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.timeout = 10;

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(webRequest.url + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(webRequest.url + ":\nReceived: " + webRequest.downloadHandler.text);
                searchResult = JsonConvert.DeserializeObject<LexicaSearchResult>(webRequest.downloadHandler.text);

                // print length of results
                Debug.Log(searchResult.images.Length);
            }
        }
    }

}
