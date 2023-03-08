using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net;
using SimpleJSON;
using TMPro;
using WebP;

[Serializable]
class PromptPayload {
    /*
    payload = {
        "type":2,
        "application_id":"936929561302675456",
        "guild_id":"1007706882338717736",
        "channel_id":"1007706882338717739",
        "session_id":session_id,
        "data":{
            "version":"994261739745050686",
            "id":"938956540159881230",
            "name":"imagine","type":1,
            "options":[{"type":3,"name":"prompt","value":"a neon cyber punk antlantis city with neon lights and floating fish"}],
            "application_command":{
                "id":"938956540159881230",
                "application_id":"936929561302675456",
                "version":"994261739745050686",
                "type":1,
                "name":"imagine",
                "description":"There are endless possibilities...",
                "dm_permission":True,
                "options":[{"type":3,"name":"prompt","description":"The prompt to imagine","required":True}]
                },
            "attachments":[]},
        }
    */
    public int type = 2;
    public string application_id = "936929561302675456";
    public string guild_id = "1007706882338717736";
    public string channel_id = "1007706882338717739";
    public string session_id = "09a25c601d0911ed91abe1f80f76b4de"; // can be anything unique
    public DiscordData data;

    public PromptPayload(string session_id, string prompt) {
        this.session_id = session_id;
        this.data = new DiscordData();
        this.data.options.Add(new DiscordOption() {value = prompt});
        // set up application command
        this.data.application_command.options.Add(new DiscordApplicationOption() {name = "prompt", description = "The prompt to imagine", required = true});
    }

    public string ToJson() {
        return JsonUtility.ToJson(this);
    }
}

[Serializable]
public class DiscordData {
    public string version = "994261739745050686";
    public string id = "938956540159881230";
    public string name = "imagine";
    public int type = 1;
    public List<DiscordOption> options = new List<DiscordOption>();
    public DiscordApplicationCommand application_command = new DiscordApplicationCommand();
    public List<string> attachments = new List<string>();
}

[Serializable]
public class DiscordOption {
    public int type = 3;
    public string name = "prompt";
    public string value = "";

}

[Serializable]
public class DiscordApplicationOption {
    public string name = "prompt";
    public string description = "The prompt to imagine";
    public bool required = true;
    public int type = 3;
}

[Serializable]
public class DiscordApplicationCommand {
    /*
        "application_command":{
            "id":"938956540159881230",
            "application_id":"936929561302675456",
            "version":"994261739745050686",
            "type":1,
            "name":"imagine",
            "description":"There are endless possibilities...",
            "dm_permission":True,
            "options":[{"type":3,"name":"prompt","description":"The prompt to imagine","required":True}]
        }
    */
    public string id = "938956540159881230";
    public string application_id = "936929561302675456";
    public string version = "994261739745050686";
    public int type = 1;
    public string name = "imagine";
    public string description = "There are endless possibilities...";
    public bool dm_permission = true;
    public List<DiscordApplicationOption> options = new List<DiscordApplicationOption>();
}

[Serializable]
public class DiscordJob {
    /*
    URL: https://discord.com/api/v9/interactions

    {
        "type": 3,
        "nonce": "1055593584062889984",
        "guild_id": "1007706882338717736",
        "channel_id": "1007706882338717739",
        "message_flags": 0,
        "message_id": "1055581971553730611",
        "application_id": "936929561302675456",
        "session_id": "e3d3414eb615997f57d62f214fab0b4f",
        "data": {
            "component_type": 2,
            "custom_id": "MJ::JOB::upsample::1::8212edee-e9e1-4e16-a4a9-8cf29bd27570"
        }
    }

        payload['data']['custom_id'] = f"MJ::JOB::{job}::{option}"
        req = requests.post(interaction_url, headers=header, json=payload)
        return req.status_code
    */
    public int type = 3;
    public string nonce = "";
    public string guild_id = "1007706882338717736";
    public string channel_id = "1007706882338717739";
    public int message_flags = 0;
    public string message_id = "938956540159881230";
    public string application_id = "936929561302675456";
    public string session_id = "09a25c601d0911ed91abe1f80f76b4de";
    public DiscordJobData data = new DiscordJobData();

    public DiscordJob(string message_id, string job, int option, string uuid) {
        this.message_id = message_id;
        this.data.custom_id = $"MJ::JOB::{job}::{option}::{uuid}"; // variation/upsample
    }
}

[Serializable]
public class DiscordJobData {
    public int component_type = 2;
    public string custom_id = "MJ::JOB::upsample::1::f844e29e-f1a8-4e19-b23b-31d06bb7081d";
}


[Serializable]
public class MidJourneyImage {
    public string id = "";
    public string url = "";
    public string prompt = "";
    // TODO
}


public class DiscordMidjourney : MonoBehaviour
{
    /*
      This script to submit Midjourney jobs through discord API
    */

    // Legalize Bot Channel on Discord
    public string channel_id = "1007706882338717739";

    // Discord API
    public string auth = "MzY0OTM4OTQyNDk3MjkyMjk4.X8FPwQ.6NImQ1pN88DVIjnXWENwSgQQA_M";
    string interaction_url = "https://discord.com/api/v9/interactions";
    public string limit = "2";

    // Midjourney API
    string midjourney_url = "https://www.midjourney.com/api/app/recent-jobs/";
    string cookie_file = "cookie.txt";
    string cookie = "";


    // TODO move to separate class per image
    public string prompt = "portrait of android with half its skin torn off revealing the lush white and blue flowers, exoskeleton, wires, led lights, futuristic, profile view";
    public string model = "";
    public string size = "--ar 2:3";
    public string message_id = "";
    public string upsample_uuid = "";
    public string variation_uuid = "";


    [Header("UI")]
    public Image image;
    // text input for prompt
    public TMP_InputField promptInput;
    // model dropdown
    public TMP_Dropdown modelDropdown;
    // size dropdown
    public TMP_Dropdown sizeDropdown;
    // submit button
    public Button submitButton;
    // info button
    public Button infoButton;
    public Button refreshButton;
    // list of job buttons
    public List<Button> jobButtons;


    // https://midjourney.gitbook.io/docs/imagine-parameters
    // Dict of models and their arguments
    Dictionary<string, string> models = new Dictionary<string, string>()
    {
        {"MidJourney V4", ""},
        {"MidJourney V3", "--v3"},
        {"High Definition", "--hd"},
        {"Creative", "--creative"},
        {"Anime", "--niji"},
    };
    List<string> modelList;


    Dictionary<string, string> sizes = new Dictionary<string, string>()
    {
        {"Portrait", "--ar 2:3"},
        {"Landscape", "--ar 3:2"},
        {"Square", ""},
    };
    List<string> sizeList;


    void Start()
    {
        CreateUI();
        StartCoroutine(GetMessages());
        List<string> modelList = new List<string>(models.Keys);

        // load cookie from file
        if (File.Exists(cookie_file))
        {
            cookie = File.ReadAllText(cookie_file);
        }
        else
        {
            Debug.Log("Cookie file not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // create Buttons and TextInputs for the user to submit a request
    void CreateUI()
    {
        // create dropdown options
        modelDropdown.ClearOptions();
        modelList = new List<string>(models.Keys);
        modelDropdown.AddOptions(modelList);

        sizeDropdown.ClearOptions();
        sizeList = new List<string>(sizes.Keys);
        sizeDropdown.AddOptions(sizeList);

        // add listener to prompt input
        promptInput.onValueChanged.AddListener(delegate { UpdatePrompt(promptInput.text); });
        // enable text wrapping
        promptInput.textComponent.enableWordWrapping = true;
        // set prompt text
        promptInput.text = prompt;

        // add listener to buttons
        submitButton.onClick.AddListener(delegate { SubmitPrompt();});
        infoButton.onClick.AddListener(delegate { toggleInfo(); });

        // update dropdowns on change
        modelDropdown.onValueChanged.AddListener(delegate { UpdateModel(modelDropdown.value); });
        sizeDropdown.onValueChanged.AddListener(delegate { UpdateSize(sizeDropdown.value); });

        // refreshButton calls GetMessages
        refreshButton.onClick.AddListener(delegate { StartCoroutine(GetMessages()); });

        // loop through jobButtons and add listener based on button Name
        foreach (Button button in jobButtons)
        {
            //name of format: "job_option"
            string[] name = button.name.Split('_');
            button.onClick.AddListener(delegate { SubmitJob(name[0].ToLowerInvariant(), int.Parse(name[1]) ); });
        }

        // show UI
        submitButton.gameObject.SetActive(false);
        modelDropdown.gameObject.SetActive(false);
        sizeDropdown.gameObject.SetActive(false);
        promptInput.gameObject.SetActive(false);
        infoButton.gameObject.SetActive(true);
    }

    void UpdatePrompt(string newPrompt)
    {
        prompt = newPrompt;
    }

    void UpdateModel(int modelIndex)
    {
        model = models[modelList[modelIndex]];
    }

    void UpdateSize(int sizeIndex)
    {
        size = sizes[sizeList[sizeIndex]];
    }

    public async void SubmitPrompt()//(Action<string> callback)
    {
        using (var httpClient = new HttpClient())
        {
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), interaction_url))
            {
                request.Headers.TryAddWithoutValidation("authorization", auth);

                string full_prompt = $"{model} {prompt} {size}";
                // set session to hash of prompt
                string session_id = "09a25c601d0911ed91abe1f80f76b4de";//
                //string session_id = GetHash32(full_prompt);

                // create payload
                PromptPayload payload = new PromptPayload(session_id, full_prompt);
                Debug.Log(JsonUtility.ToJson(payload));

                // create json request and send pinataJSON
                var jsonContent = new StringContent(JsonUtility.ToJson(payload), Encoding.UTF8, "application/json");
                request.Content = jsonContent;
        
                // send post request
                var response = await httpClient.SendAsync(request);
                
                // print status
                Debug.Log(response.StatusCode);
                //callback(response.Content.ReadAsStringAsync().Result);
            }
        }
    }

    public async void SubmitJob(string job, int option)
    {
        /*
        def send_job(id='1007792377706065973', job='variation', option=2):
            payload = {
                "type": 3,
                "guild_id": "1007706882338717736",
                "channel_id": "1007706882338717739",
                "message_flags": 0,
                "message_id": id,
                "application_id": "936929561302675456",
                "session_id": session_id,
                "data": {
                    "component_type": 2,
                    "custom_id": "MJ::JOB::upsample::1"
                }
            }
            payload['data']['custom_id'] = f"MJ::JOB::{job}::{option}"
            req = requests.post(interaction_url, headers=header, json=payload)
            return req.status_code
        */
        using (var httpClient = new HttpClient())
        {
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), interaction_url))
            {
                request.Headers.TryAddWithoutValidation("authorization", auth);

                string full_prompt = $"{model} {prompt} {size}";
                // set session to hash of prompt
                //string session_id = "09a25c601d0911ed91abe1f80f76b4de";//
                string session_id = GetHash32(full_prompt);

                // create DiscordJob
                DiscordJob jobPayload;
                // send uuid based on job and option
                if (job == "variation")
                {
                    jobPayload = new DiscordJob(message_id, job, option, variation_uuid);
                }
                else
                {
                    jobPayload = new DiscordJob(message_id, job, option, upsample_uuid);
                }

                Debug.Log(JsonUtility.ToJson(jobPayload));
                // create json request and send pinataJSON
                var jsonContent = new StringContent(JsonUtility.ToJson(jobPayload), Encoding.UTF8, "application/json");
                request.Content = jsonContent;
        
                // send post request
                var response = await httpClient.SendAsync(request);
                
                // print status
                Debug.Log(response.StatusCode);
                //callback(response.Content.ReadAsStringAsync().Result);
            }
        }

    }

    public async void QueryMidjourneyJobs(string type)
    {   // type = ['rising', 'latest', 'top']

        // build a get request with MidjourneyHeader and params
        using (var httpClient = new HttpClient())
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.midjourney.com/v1/jobs/latest"))
            {
                // add header for referer and cookie
                request.Headers.TryAddWithoutValidation("referer", "https://mintmore.art/");
                request.Headers.TryAddWithoutValidation("cookie", cookie);
                
                // Add query params
                var queryParams = new Dictionary<string, string>
                {
                    {"amount", "50"},
                    {"jobType", "upscale"},
                    {"orderBy", type},
                    {"jobStatus", "completed"},
                    {"dedupe", "true"},
                    {"refreshApi", "0"}
                };
                request.Content = new FormUrlEncodedContent(queryParams);

                // send get request
                var response = await httpClient.SendAsync(request);

                // print status
                Debug.Log(response.StatusCode);
                //callback(response.Content.ReadAsStringAsync().Result);
            }
        }
    }

    public void CheckResponse(string response)
    {
        // check response
        Debug.Log(response);
    }

    string GetHash32(string input)
    {
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    // '**fish swimming around a neon atlantis, underwater city, glowing coral --ar 2:3** - <@364938942497292298> (0%) (relaxed)'
    // example content after prompt submission

    IEnumerator GetMessages()
    {
        string url = "https://discord.com/api/v9/channels/" + channel_id + "/messages?limit=" + limit;
        using(UnityWebRequest www = UnityWebRequest.Get(url))
        {
            // add headers
            www.SetRequestHeader("authorization", auth);
            www.SetRequestHeader("content-type", "application/json");

            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                Debug.Log(www.downloadHandler.text);

                var json = JSON.Parse(www.downloadHandler.text);

                // show image at url in attachments
                // loop over messages
                foreach (JSONNode message in json){
                    // check for attachments
                    if(message["attachments"].Count > 0){
                        message_id = message["id"];

                        // get url
                        // check for png extension
                        string image_url = message["attachments"][0]["url"];
                        if (image_url.Contains(".png"))
                        {
                            // set image
                            StartCoroutine(LoadImage(image_url));
                        }
                        else
                        {
                            // could be glb
                            continue;
                        }
                        Debug.Log(image_url);

                        // set prompt
                        promptInput.text = message["content"];
                        // extract contents between **
                        string[] split = promptInput.text.Split(new string[] {"**"}, StringSplitOptions.None);
                        // set prompt to contents between **
                        promptInput.text = split[1];
                        // split by arguments
                        string[] args = promptInput.text.Split(new string[] {"--"}, StringSplitOptions.None);
                        promptInput.text = args[0];

                        // for each model see if it's in prompt
                        foreach (string model in models.Keys)
                        {
                            if (promptInput.text.Contains(model))
                            {
                                modelDropdown.value = modelList.IndexOf(model);
                                promptInput.text = promptInput.text.Replace(model, "");
                                break;
                            }
                        }
                        // for each size see if it's in prompt
                        foreach (string size in sizes.Keys)
                        {
                            if (promptInput.text.Contains(size))
                            {
                                sizeDropdown.value = sizeList.IndexOf(size);
                                promptInput.text = promptInput.text.Replace(size, "");
                                break;
                            }
                        }

                        // extract job id from json
                        upsample_uuid = message["components"][0]["components"][0]["custom_id"];
                        upsample_uuid = upsample_uuid.Split(new string[] {"::"}, StringSplitOptions.None)[4];

                        if ( message["components"][1]["components"].Count > 1)
                        {
                            // TODO check for RATING
                            variation_uuid = message["components"][1]["components"][0]["custom_id"];
                            Debug.Log(variation_uuid);
                            if (variation_uuid.Contains("RATING"))
                            {
                                variation_uuid = "";
                            }
                            else
                            {
                                variation_uuid = variation_uuid.Split(new string[] {"::"}, StringSplitOptions.None)[4];
                            }
                        }
                        else
                        {
                            variation_uuid = "";
                        }

                        StartCoroutine(LoadImage(image_url));
                        break;
                    }
                }
            }
        }
    }

    IEnumerator LoadImage(string url)
    {
        using(UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Get downloaded asset bundle
                Texture myTexture = DownloadHandlerTexture.GetContent(www);
                float aspect = (float)myTexture.width / (float)myTexture.height;
                // set image sprite to portion of screen width
                image.sprite = Sprite.Create(myTexture as Texture2D, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f));
                // set image size
                image.rectTransform.sizeDelta = new Vector2(Screen.width * 0.8f, Screen.width * 0.8f / aspect);
            }
        }
    }

    void toggleInfo()
    {
        if (!submitButton.gameObject.activeSelf)
        {
            submitButton.gameObject.SetActive(true);
            modelDropdown.gameObject.SetActive(true);
            sizeDropdown.gameObject.SetActive(true);
            promptInput.gameObject.SetActive(true);
            image.gameObject.SetActive(false);
        }
        else
        {
            submitButton.gameObject.SetActive(false);
            modelDropdown.gameObject.SetActive(false);
            sizeDropdown.gameObject.SetActive(false);
            promptInput.gameObject.SetActive(false);
            image.gameObject.SetActive(true);
        }
    }

}



/*
    header = {
        'authorization':'MzY0OTM4OTQyNDk3MjkyMjk4.X8FPwQ.6NImQ1pN88DVIjnXWENwSgQQA_M',
        'content-type':'application/json'
    }
    def get_messages(channel_id = "1007706882338717739", limit=20):
        url = f"https://discord.com/api/v9/channels/{channel_id}/messages?limit={limit}"
        req = requests.get(url, headers=header)
        if req.status_code == 200:
            return req.json()
        else:
            return None

    messages[0].keys()                                                                                                                                                                                                                                                                                
    dict_keys(['id', 'type', 'content', 'channel_id', 'author', 'attachments', 'embeds', 'mentions', 'mention_roles', 'pinned', 'mention_everyone', 'tts', 'timestamp', 'edited_timestamp', 'flags', 'components'])


    {'id': '1055365392278761532',
    'type': 0,
    'content': '**Lovecraftian women, cyberpunk, character design, full body, octane render, volumetric lighting, cinematic, detailed, ornate, pink neon, intricate detail** - <@364938942497292298> (fast)',
    'channel_id': '1007706882338717739',
    'author': {'id': '936929561302675456',
    'username': 'Midjourney Bot',
    'avatar': '4a79ea7cd151474ff9f6e08339d69380',
    'avatar_decoration': None,
    'discriminator': '9282',
    'public_flags': 65536,
    'bot': True},
    'attachments': [{'id': '1055365392043868231',
    'filename': 'professormunchies_Lovecraftian_women_cyberpunk_character_design_e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a.png',
    'size': 1714360,
    'url': 'https://cdn.discordapp.com/attachments/1007706882338717739/1055365392043868231/professormunchies_Lovecraftian_women_cyberpunk_character_design_e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a.png',
    'proxy_url': 'https://media.discordapp.net/attachments/1007706882338717739/1055365392043868231/professormunchies_Lovecraftian_women_cyberpunk_character_design_e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a.png',
    'width': 1024,
    'height': 1024,
    'content_type': 'image/png'}],
    'embeds': [],
    'mentions': [{'id': '364938942497292298',
    'username': 'professormunchies',
    'avatar': '2e492558906d1011e8e0e46c9e17e64f',
    'avatar_decoration': None,
    'discriminator': '4347',
    'public_flags': 0}],
    'mention_roles': [],
    'pinned': False,
    'mention_everyone': False,
    'tts': False,
    'timestamp': '2022-12-22T06:05:17.260000+00:00',
    'edited_timestamp': None,
    'flags': 0,
    'components': [{'type': 1,
    'components': [{'type': 2,
        'style': 2,
        'label': 'U1',
        'custom_id': 'MJ::JOB::upsample::1::e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a'},
        {'type': 2,
        'style': 2,
        'label': 'U2',
        'custom_id': 'MJ::JOB::upsample::2::e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a'},
        {'type': 2,
        'style': 2,
        'label': 'U3',
        'custom_id': 'MJ::JOB::upsample::3::e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a'},
        {'type': 2,
        'style': 2,
        'label': 'U4',
        'custom_id': 'MJ::JOB::upsample::4::e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a'},
        {'type': 2,
        'style': 2,
        'emoji': {'name': '🔄'},
        'custom_id': 'MJ::JOB::reroll::0::e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a::SOLO'}]},
    {'type': 1,
    'components': [{'type': 2,
        'style': 2,
        'label': 'V1',
        'custom_id': 'MJ::JOB::variation::1::e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a'},
        {'type': 2,
        'style': 2,
        'label': 'V2',
        'custom_id': 'MJ::JOB::variation::2::e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a'},
        {'type': 2,
        'style': 2,
        'label': 'V3',
        'custom_id': 'MJ::JOB::variation::3::e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a'},
        {'type': 2,
        'style': 2,
        'label': 'V4',
        'custom_id': 'MJ::JOB::variation::4::e7ae3754-7cef-4b8d-a4d2-ba850e3b8b3a'}]}]}
*/
