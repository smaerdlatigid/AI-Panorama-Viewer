using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skybox_controller : MonoBehaviour
{
    public List<Material> skybox_list = new List<Material>();
    public int current_skybox = 0;

    // Start is called before the first frame update
    void Start()
    {
        setSkyBox();
    }

    public void change_skybox()
    {
        current_skybox++;
        if (current_skybox >= skybox_list.Count)
        {
            current_skybox = 0;
        }
        RenderSettings.skybox = skybox_list[current_skybox];
        //Camera.main.GetComponent<Skybox>().material = skybox_list[current_skybox];

        PlayerPrefs.SetInt("SkyBox", current_skybox);
    }

    public void setSkyBox()
    {
        // use when coming out of AR mode
        current_skybox = PlayerPrefs.GetInt("SkyBox");
        RenderSettings.skybox = skybox_list[current_skybox];
        // set on camera
        //Camera.main.GetComponent<Skybox>().material = skybox_list[current_skybox];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
