using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.ARFoundation
{
    public class ARToggle : MonoBehaviour
    {
        public GameObject ARSession;
        public Button ARButton;

        public GameObject ARReset; // reset origin button
        public GameObject MediaContainer;

        // Start is called before the first frame update
        void Start()
        {
            ARSession.GetComponent<ARSession>().enabled = false;
            ARButton.GetComponentInChildren<Text>().text = "AR";
            ARReset.SetActive(false);
            MediaContainer.GetComponent<TouchControls>().enableHorizontalTouch = true;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void OnToggle()
        {
            bool state = ARSession.GetComponent<ARSession>().enabled;
            if(state)
            {
                TurnOff();
                MediaContainer.GetComponent<TouchControls>().enableHorizontalTouch = true;
            }
            else
            {
                TurnOn();
                MediaContainer.GetComponent<TouchControls>().enableHorizontalTouch = false;
            }
        }

        public void TurnOff()
        {
            StartCoroutine(WaitTurnOff(0.25f));
        }

        IEnumerator WaitTurnOff(float seconds)
        {
            ARSession.GetComponent<ARSession>().Reset();

            yield return new WaitForSeconds(seconds);
            // wait a second
            ARSession.GetComponent<ARSession>().enabled = false;
            ARButton.GetComponentInChildren<Text>().text = "AR";
            ARReset.SetActive(false);
        }

        public void TurnOn()
        {
            ARSession.GetComponent<ARSession>().enabled = true;
            ARButton.GetComponentInChildren<Text>().text = "3D";
            ARReset.SetActive(true);
        }
    }
}