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
using WebP;
using Photon.Pun;
using Photon.Realtime;

// docs: https://stablehorde.net/api/

class StableHordeImageResponse {
    /* https://stablehorde.net/api/v2/generate/status/{id} 
    {
        "finished": 0,
        "processing": 0,
        "restarted": 0,
        "waiting": 0,
        "done": true,
        "faulted": false,
        "wait_time": 0,
        "queue_position": 0,
        "kudos": 0,
        "is_possible": true,
        "generations": [
            {
            "worker_id": "string",
            "worker_name": "string",
            "model": "string",
            "state": "ok",
            "img": "string",
            "seed": "string",
            "id": "string",
            "censored": true
            }
        ],
        "shared": true
        }
    */
    public int finished;
    public int processing;
    public int restarted;
    public int waiting;
    public bool done;
    public bool faulted;
    public int wait_time;
    public int queue_position;
    public float kudos;
    public bool is_possible;
    public List<StableHordeImage> generations = new List<StableHordeImage>();
    public bool shared;
}

class StableHordeImage
{
    /* https://stablehorde.net/api/v2/generate/status/{id} 
    {
        "worker_id": "string",
        "worker_name": "string",
        "model": "string",
        "state": "ok",
        "img": "string",
        "seed": "string",
        "id": "string",
        "censored": true
    }
    */
    public string worker_id;
    public string worker_name;
    public string model;
    public string state;
    public string img;
    public string seed;
    public string id;
    public bool censored;
}


class StableHordeCheck {
   /* https://stablehorde.net/api/v2/generate/check/{id}
        {'finished': 1,
        'processing': 0,
        'restarted': 0,
        'waiting': 0,
        'done': True,
        'faulted': False,
        'wait_time': 0,
        'queue_position': 0,
        'kudos': 19.0,
        'is_possible': True}
    */
    public int finished;
    public int processing;
    public int restarted;
    public int waiting;
    public bool done;
    public bool faulted;
    public int wait_time;
    public int queue_position;
    public float kudos;
    public bool is_possible;
}


class StableHordeRequest {
    /* https://stablehorde.net/api/v2/generate */
    public string prompt = "matrix, ghost in the shell, cyberpunk, dreamlike";

    public Dictionary<string, object> @params = new Dictionary<string, object> {
        //'k_lms', 'k_heun', 'k_euler', 'k_euler_a', 'k_dpm_2', 'k_dpm_2_a', 
        // 'k_dpm_fast', 'k_dpm_adaptive', 'k_dpmpp_2s_a', 'k_dpmpp_2m', 'dpmsolver'
        {"cfg_scale", 9},
        {"sampler_name", "k_euler_a"},
        {"denoising_strength", 0.85},
        {"seed", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()}, // TODO set to unique hash
        {"height", 512},
        {"width", 1024},
        {"seed_variation", 1},
        {"post_processing", new List<string> {"RealESRGAN_x4plus"}},
        {"karras", true},
        {"hires_fix", true},
        {"clip_skip", 1},
        {"tiling", false},
        {"steps", 50},
        {"n", 1},
    };
    public List<string> workers = new List<string> {
       // "fbc7e816-9e96-45a7-a13e-604617723bed"
    };
    public List<string> models = new List<string> {
        "Deliberate",
        //"Project Unreal Engine 5",
        //"stable_diffusion"
    };

    //public string source_image = "string"; // base64 encoded image webp format
    public string source_processing = "img2img";
}

public class StableHordeRequestHeader {
    string apikey = "KAAOmTev_747cF7iKaN60w";
}

public class StableHordeResponse {
    public string id;
}

// post request
public class StableHordeManager : MonoBehaviourPun
{
    StableHordeRequest request;
    StableHordeResponse response;
    StableHordeImageResponse imageResponse;
    StableHordeCheck checkResponse;

    public GameObject Multiplayer; // LoginScreenUI element from UI Object
    public GameObject image;

    public TextMeshProUGUI text;
    
    // text input for prompt
    public TMP_InputField promptInput;

    // submit button
    public GameObject promptSubmit= null;
    // info button
    public GameObject infoButton= null;
    // button for synchronizing with other players
    public GameObject syncButton = null;
    public GameObject loadingIcon = null;
    public GameObject aiButton = null;

    // prompt padding
    string preprompt = "equirectangular panoramic, 360 photo,";
    string endprompt = "unreal engine, artstation";
    string ogprompt = "";
    string image_url = "";

    // Start is called before the first frame update
    void Start()
    {
        request = new StableHordeRequest();
        CreateUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // create Buttons and TextInputs for the user to submit a request
    void CreateUI()
    {
        // add listener to prompt input
        promptInput.onValueChanged.AddListener(delegate { UpdatePrompt(promptInput.text); });
        // enable text wrapping
        promptInput.textComponent.enableWordWrapping = true;

        // add listener to buttons
        promptSubmit.GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(StableHordePostRequest()); });
        infoButton.GetComponent<Button>().onClick.AddListener(delegate { toggleInfo(); });

        // add listener to ai button
        aiButton.GetComponent<Button>().onClick.AddListener(delegate { togglePrompting(); });

        // add listener to sync button
        syncButton.GetComponent<Button>().onClick.AddListener(delegate { NetworkSync(); });

        // show UI
        promptSubmit.SetActive(false);
        promptInput.gameObject.SetActive(false);
        infoButton.SetActive(true);
        syncButton.SetActive(false);
        loadingIcon.SetActive(false);
        text.gameObject.SetActive(false);
    }
    
    void toggleInfo()
    {   
        //toggle text
        text.gameObject.SetActive(!text.gameObject.activeSelf);

        // sanity check
        text.text = ogprompt + "\n" + image_url;

        // turn off prompting
        promptSubmit.gameObject.SetActive(false);
        promptInput.gameObject.SetActive(false);

        // check multiplayer status
        if (Multiplayer.GetComponent<LoginScreenUI>().inRoom)
        {
            syncButton.SetActive(true);
        }
        else
        {
            syncButton.SetActive(false);
        }
    }

    void togglePrompting()
    {
        if (promptSubmit.gameObject.activeSelf)
        {
            promptSubmit.gameObject.SetActive(false);
            promptInput.gameObject.SetActive(false);
        }
        else
        {
            promptSubmit.gameObject.SetActive(true);
            promptInput.gameObject.SetActive(true);
        }
    }

    void UpdatePrompt(string prompt)
    {
        // pad prompt
        request.prompt = preprompt + " " + prompt + " " + endprompt;
        ogprompt = prompt;
    }

    [PunRPC]
    public void NetworkLoad(string imageurl)
    {
        Debug.Log("Recieved image: " + imageurl);
        if (imageurl != "")
        {
            StartCoroutine(LoadTextureFromUrl(imageurl));
        }
    }

    public void NetworkSync()
    {
        if (Multiplayer.GetComponent<LoginScreenUI>().inRoom)
        {
            Debug.Log("Sending data: " + image_url);

            // Get photon view to send RPC
            PhotonView photonView = PhotonView.Get(this);

            // execute remote procedure call on all clients
            photonView.RPC("NetworkLoad", RpcTarget.All, image_url);

            // turn on sync button
            syncButton.SetActive(true);
        }
        else
        {
            // turn button off, it shouldn't be on in the first place
            syncButton.SetActive(false);
        }
    }



    IEnumerator StableHordePostRequest()
    {
        string url = "https://stablehorde.net/api/v2/generate/async";
        
        string json = JsonConvert.SerializeObject(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        
        using(UnityWebRequest www = UnityWebRequest.Post(url, json))
        {
            // change UI
            loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("refresh_icon");
            loadingIcon.SetActive(true);
            promptSubmit.SetActive(false);

            // set headers
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("apikey", "KAAOmTev_747cF7iKaN60w");
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            // send request
            yield return www.SendWebRequest();
            //Debug.Log(www.downloadHandler.text);

            // convert response to json
            response = JsonConvert.DeserializeObject<StableHordeResponse>(www.downloadHandler.text);
            Debug.Log(response.id);

            // set text description to id
            text.text = "id: " + response.id;
            text.gameObject.SetActive(true);
        }

        if (response.id != null)
        {
            StartCoroutine(StableHordeCheckId());
        }
    }

    IEnumerator StableHordeCheckId()
    {
        string url = "https://stablehorde.net/api/v2/generate/check/" + response.id;
        checkResponse = new StableHordeCheck();
        using(UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            Debug.Log(www.downloadHandler.text);
            checkResponse = JsonConvert.DeserializeObject<StableHordeCheck>(www.downloadHandler.text);
            Debug.Log($"id: {response.id}, finished: {checkResponse.finished}, $ timestamp: {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
            //text.text = $"id: {response.id}, finished: {checkResponse.finished}, $ timestamp: {DateTimeOffset.UtcNow.ToUnixTimeSeconds()}, wait_time: {checkResponse.wait_time}";
        }

        if (checkResponse.finished == 1)
        {
            loadingIcon.GetComponent<Image>().sprite = Resources.Load<Sprite>("success_icon");
            StartCoroutine(StableHordeGetImage());
        }
        else
        {
            // start coroutine after a few seconds and check again
            yield return new WaitForSeconds( Mathf.Max(checkResponse.wait_time, 5));
            StartCoroutine(StableHordeCheckId());
        }
    }

    IEnumerator LoadTextureFromUrl(string imageurl)
    {
        // check for valid url
        if (imageurl == "")
        {
            Debug.Log("Invalid url");
            yield break;
        }

        // load image from url
        byte[] imageBytes;
        using(UnityWebRequest wwww = UnityWebRequest.Get(imageurl))
        {
            yield return wwww.SendWebRequest();
            imageBytes = wwww.downloadHandler.data;
        }

        // load webp image into unity
        Texture2D texture = Texture2DExt.CreateTexture2DFromWebP(imageBytes, lMipmaps: true, lLinear: true, lError: out Error lError);
        image.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
    }

    IEnumerator StableHordeGetImage()
    {
        imageResponse = new StableHordeImageResponse();
        string url = "https://stablehorde.net/api/v2/generate/status/" + response.id;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            Debug.Log(www.downloadHandler.text);
            imageResponse = JsonConvert.DeserializeObject<StableHordeImageResponse>(www.downloadHandler.text);

            // cache image url
            image_url = imageResponse.generations[0].img;

            // set UI to prompt
            text.text = ogprompt + "\n" + image_url;
            loadingIcon.SetActive(false);

            // hide prompting
            toggleInfo();
        }

        // load image
        if (image_url != "")
        {
            StartCoroutine(LoadTextureFromUrl(image_url));
        }
    }
}