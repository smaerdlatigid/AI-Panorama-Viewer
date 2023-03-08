using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SimpleJSON;

public class ParseTool
{
    public static void parseMetaData(nft n)
    {
        // parse metadata
        JSONNode meta = JSON.Parse(n.data);
    
        // check for image
        n.image_url = checkForImage(meta);
        (n.media_type, n.media_url) = checkMediaType(meta);
        n.name = ParseTool.checkForName(meta);
        n.description = ParseTool.checkForDescription(meta);

        // modify if ipfs link for speed loading
        if (n.image_url.Contains("ipfs"))
        {
            n.image_url = n.image_url.Replace("ipfs://", "https://morpheus.mypinata.cloud/ipfs/");
            n.image_url = n.image_url.Replace("https://ipfs.io/ipfs/", "https://morpheus.mypinata.cloud/ipfs/");
            n.image_url = n.image_url.Replace("https://gateway.pinata.cloud/ipfs/", "https://morpheus.mypinata.cloud/ipfs/");
        }

        // modify if ipfs link for speed loading
        if (n.media_url.Contains("ipfs"))
        {
            n.media_url = n.media_url.Replace("ipfs://", "https://morpheus.mypinata.cloud/ipfs/");
            n.media_url = n.media_url.Replace("https://ipfs.io/ipfs/", "https://morpheus.mypinata.cloud/ipfs/");
            n.media_url = n.media_url.Replace("https://gateway.pinata.cloud/ipfs/", "https://morpheus.mypinata.cloud/ipfs/");
        }


    }

    // public static nft loadMetaData(string metadata, string chain)
    async public static void DownloadTokenURI(nft n)
    {
        // usually a good place to start
        // https://docs.opensea.io/docs/metadata-standards

        // skip if metadata is missing
        if (String.IsNullOrWhiteSpace(n.tokenuri) == false)
        {
            // download text at link
            using (var client = new System.Net.Http.HttpClient())
            {
                var response = await client.GetAsync(n.tokenuri);
                n.data = await response.Content.ReadAsStringAsync();
            }
            // load json metadata
            JSONNode meta = JSON.Parse(n.data);

            // quick parse
            n.name = meta["name"];

            // check for image & other media
            n.image_url = ParseTool.checkForImage(meta);
            (n.media_type, n.media_url) = ParseTool.checkMediaType(meta);
        }
    }

    public static string checkForDescription(JSONNode meta)
    {
        if (!String.IsNullOrWhiteSpace(meta["description"]))
        {
            return meta["description"];
        }
        else
        {
            return "";
        }
    }

    public static string checkForName(JSONNode meta)
    {
        if (!String.IsNullOrWhiteSpace(meta["name"]))
        {
            return meta["name"];
        }
        else
        {
            return "";
        }
    }

    // checks the meta data for various keys and determines if the value has an image link
    public static string checkForImage(JSONNode metadata)
    {
        if (key_contains(metadata, "image_url", ".png") || key_contains(metadata, "image_url", ".jpg")  || key_contains(metadata, "image_url", "ipfs"))
        {
            return metadata["image_url"];
        }
        else if (key_contains(metadata, "imageUrl", ".png") || key_contains(metadata, "imageUrl", ".jpg")  || key_contains(metadata, "imageUrl", "ipfs"))
        {
            return metadata["imageUrl"];
        }
        else if (key_contains(metadata, "image", ".png") || key_contains(metadata, "image", ".jpg") || key_contains(metadata, "image", "ipfs"))
        {
            return metadata["image"];
        }
        else if (key_contains(metadata, "preview_url", ".png") || key_contains(metadata, "preview_url", ".jpg") || key_contains(metadata, "preview_url", "ipfs"))
        {
            return metadata["preview_url"];
        }
        else
        {
            // some default fall backs
            if (key_contains(metadata, "preview_url", "https"))
                return metadata["preview_url"];
            else if (key_contains(metadata, "image_url", "https"))
                 return metadata["image_url"];
            else if (key_contains(metadata, "image", "https")) // usually a centralized api
                return metadata["image"];
            return "";
        }
    }


    /*
    meta data example that gets picked up as an image but should be a video - need to check for the file extension in "image" field

    {"address":"0x3b3ee1931dc30c1957379fac9aba94d1c48a5405","chain":"eth","token_id":55050,"data":"{\"name\":\"Spiro / Torus\",\"description\":\"Real-time visualization of parameterized torus knot with instanced particles. 
    Built in javascript with three/glsl/webxr.\\n\\n1080x1920 capture / running time 0:30min / 60fps\\n\\nLive interactive version viewable in the browser on desktop, mobile, AR and VR. \\n-> 
    https://visualdata.org/nft/spiro/torus\\n\\nCollector receives HD version plus 4 high res stills directly from visualdata HQ.\",\"image\":\"ipfs://QmUpaEQjU1wvDdZzyHHZ14q13WsY1PGaFdEH6pMUVTWg1L/nft.mp4\"}",
    */
    public static (MediaType, string) checkMediaType(JSONNode metadata)
    {
        if (key_contains(metadata, "animation_url", ".mp4") || key_contains(metadata,"animation_url", ".mov"))
        {
            return (MediaType.Video, metadata["animation_url"]);
        }
        else if (key_contains(metadata, "animation_url", ".gltf") || key_contains(metadata, "animation_url", ".glb"))
        {
            return (MediaType.Model, metadata["animation_url"]);
        }
        else if (key_contains(metadata, "image_url", ".svg"))
        {
            return (MediaType.SVG, metadata["image_url"]);
        }
        else if (key_contains(metadata, "image", ".svg"))
        {
            return (MediaType.SVG, metadata["image"]);
        }
        else if (key_contains(metadata, "image_url", ".png") || key_contains(metadata, "image_url", ".jpg")  || key_contains(metadata, "image_url", ".jpeg")  || key_contains(metadata, "image_url", "ipfs"))
        {
            return (MediaType.Image, metadata["image_url"]);
        }
        else if (key_contains(metadata, "imageUrl", ".png") || key_contains(metadata, "imageUrl", ".jpg")  || key_contains(metadata, "imageUrl", ".jpeg")  || key_contains(metadata, "imageUrl", "ipfs"))
        {
            return (MediaType.Image, metadata["imageUrl"]);
        }
        else if (key_contains(metadata, "image", ".png") || key_contains(metadata, "image", ".jpg")  || key_contains(metadata, "image_url", ".jpeg") || key_contains(metadata, "image", "ipfs"))
        {
            return (MediaType.Image, metadata["image"]);
        }
        else if (key_contains(metadata, "model_url", ".gltf") || key_contains(metadata, "model_url", ".glb"))
        {
            return (MediaType.Model, metadata["model_url"]);
        }
        else if (key_contains(metadata, "effect_url", ".glsl"))
        {
            return (MediaType.Effect, metadata["effect_url"]);
        }
        else
        {
            // some default fall backs
            if (key_contains(metadata, "image", "https"))
                return (MediaType.Image, metadata["image"]);
            else if (key_contains(metadata, "preview_url", "https"))
                return (MediaType.Image, metadata["preview_url"]);
            else if (key_contains(metadata, "image_url", "https"))
                return (MediaType.Image, metadata["image_url"]);

            // TODO download the file and read the header
            return (MediaType.None, "");
        }
    }

    async public static void downloadMedia(string media_url, string file_name)
    {
        // check if file exists
        if (System.IO.File.Exists(file_name))
        {
            // if it does, skip
            return;
        }
        // download the media
        using (var client = new System.Net.Http.HttpClient())
        {
            var response = await client.GetAsync(media_url);
            var data = await response.Content.ReadAsByteArrayAsync();
            System.IO.File.WriteAllBytesAsync(file_name, data);
        }
    }

    private JSONNode metadata;
    static bool key_contains(JSONNode jdata, string key, string value)
    {
        if (!String.IsNullOrWhiteSpace(jdata[key]))
        {
            if (jdata[key].Value.Contains(value)) // str
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}