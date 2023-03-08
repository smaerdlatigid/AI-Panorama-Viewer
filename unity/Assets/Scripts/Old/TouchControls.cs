using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchControls : MonoBehaviour
{
    // Variables for Touch controls
    float distance=0f;
    float startDistance=0f;
    Vector3 startScale, startPosition;
    float startAngle1, startAngle2, angle;
    float dx, dy;
    Vector2 startTouchPosition;
    bool dirtyDoubleTouch = false;
    public bool enableHorizontalTouch = false;
    // 2 finger angle = rotation
    // 2 finger distance = scale
    // 1 finger y position = y position
    
    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount >= 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            dirtyDoubleTouch = true; // tracks if touchEnded doesn't get called
            
            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                startDistance = Vector3.Distance(touch1.position, touch2.position) / Screen.width;
                startScale = transform.localScale;
                dx = touch1.position.x - touch2.position.x;
                dy = touch1.position.y - touch2.position.y;
                startAngle1 = transform.eulerAngles.y;
                startAngle2 = 180f * Mathf.Atan2(dy, dx) / 3.1415926f;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                distance = Vector3.Distance(touch1.position, touch2.position) / Screen.width;
                transform.localScale = startScale * (1 + (distance - startDistance));

                dx = touch2.position.x - touch1.position.x;
                dy = touch2.position.y - touch1.position.y;
                angle = 180f * Mathf.Atan2(dy, dx) / 3.1415926f;
                transform.eulerAngles = new Vector3(
                    transform.eulerAngles.x,
                    startAngle1 + (startAngle2 - angle) +180,
                    transform.eulerAngles.z);
            }

            // 60% of the time this triggers 100% of the time
            if (touch1.phase == TouchPhase.Ended)
            {
                startPosition = transform.position;
                startTouchPosition = touch2.position;
                dirtyDoubleTouch = false;
                // doesn't always trigger
            }
            
            // 60% of the time this triggers 100% of the time
            if (touch2.phase == TouchPhase.Ended)
            {
                startPosition = transform.position;
                startTouchPosition = touch1.position;
                dirtyDoubleTouch = false;
                // doesn't always trigger
            }
        }
        else if (Input.touchCount == 1) // TODO ignore UI interactions
        {
            Touch touch1 = Input.GetTouch(0);

            if (touch1.phase == TouchPhase.Began)
            {
                startPosition = transform.position;
                startTouchPosition = touch1.position;
            }
            else if (touch1.phase == TouchPhase.Moved)
            {
                // prevents snapping after two finger touch
                // finger 1 down, finger 2 down, move both, finger 1 up, slightly move finger 2 -> position snap
                if (dirtyDoubleTouch)
                {
                    // reset reference position to other finger
                    startPosition = transform.position;
                    startTouchPosition = touch1.position;
                    dirtyDoubleTouch = false;
                }
                else
                {
                    if(enableHorizontalTouch)
                    {
                        transform.position = new Vector3(
                            startPosition.x + (touch1.position.x - startTouchPosition.x) / Screen.width,
                            startPosition.y + (touch1.position.y - startTouchPosition.y) / Screen.width,
                            startPosition.z);
                    }
                    else
                    {
                        transform.position = new Vector3(
                            startPosition.x,
                            startPosition.y + (touch1.position.y - startTouchPosition.y) / Screen.width,
                            startPosition.z);
                    }
                    
                }
            }
        }
    }
}
