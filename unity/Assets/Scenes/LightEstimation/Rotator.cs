using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class Rotator : MonoBehaviour
    {
        float m_Angle;
        public Vector3 speed;
        
        void Update()
        {
            transform.Rotate(speed * Time.deltaTime);
        }
    }
}