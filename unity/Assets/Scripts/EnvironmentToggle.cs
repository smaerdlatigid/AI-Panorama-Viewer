using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentToggle : MonoBehaviour
{
    public GameObject environment;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void Toggle()
    {
        environment.SetActive(!environment.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
