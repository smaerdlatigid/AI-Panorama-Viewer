using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using System.Text;
using System.Net.Http;


[System.Serializable]
public class pinataJSON
{
    public nft_metadata pinataContent; // NFTWallet.cs
    public pinataMetadata pinataMetadata; // NFTWallet.cs
}

[System.Serializable]
public class pinataMetadata
{
    public string name;
    // could also add another json
}

[System.Serializable]
public class IPFSResponse {
    public string IpfsHash;
    public string PinSize;
    public string Timestamp;
}


public class IPFS
{
    private static string pinata_key = "be6fba300350e47e995b";
    private static string pinata_secret = "f6f841fdf0992142b3b8e5dfc433b59f2031d6f3bdb0d3ba81c9c3eebbd3b076";

    public static async void UploadPinataFile(string filePath, Action<string> callback)
    {
        using (var httpClient = new HttpClient())
        {
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.pinata.cloud/pinning/pinFileToIPFS"))
            {
                request.Headers.TryAddWithoutValidation("pinata_api_key", pinata_key); 
                request.Headers.TryAddWithoutValidation("pinata_secret_api_key", pinata_secret); 

                var multipartContent = new MultipartFormDataContent();
                multipartContent.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(filePath)), "file", Path.GetFileName(filePath));
                request.Content = multipartContent; 

                var response = await httpClient.SendAsync(request);
                //Debug.Log(response);
                //Debug.Log(response.Content.ReadAsStringAsync().Result);
                callback(response.Content.ReadAsStringAsync().Result);
                // https://docs.pinata.cloud/api-pinning/pin-file#response
            }
        }
    }

    public static async void UploadPinataJSON(nft_metadata metadata, Action<string> callback)
    {
        using (var httpClient = new HttpClient())
        {
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.pinata.cloud/pinning/pinJSONToIPFS"))
            {
                request.Headers.TryAddWithoutValidation("pinata_api_key", pinata_key); 
                request.Headers.TryAddWithoutValidation("pinata_secret_api_key", pinata_secret); 

                pinataJSON pjson = new pinataJSON();
                pjson.pinataContent = metadata;
                string json = JsonUtility.ToJson(pjson);
                Debug.Log(json);
                
                // create json request and send pinataJSON
                var jsonContent = new StringContent(JsonUtility.ToJson(pjson), Encoding.UTF8, "application/json");
                request.Content = jsonContent;
        
                var response = await httpClient.SendAsync(request);
                //Debug.Log(response);
                //Debug.Log(response.Content.ReadAsStringAsync().Result);
                callback(response.Content.ReadAsStringAsync().Result);
                // https://docs.pinata.cloud/api-pinning/pin-file#response
            }
        }
    }
}
