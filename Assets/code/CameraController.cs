using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{

    [Header("Camera Target")]
    public Transform Target;


    [Header("Camera Settings")]
    public float MouseSensistivity = 5f;

    public float maximumYAngle = 90f;
    public float minimumYAngle = 0f;

    float xRot;
    float yRot;

    // Start is called before the first frame update
    void Awake()
    {
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        GetMouseInput();
        FollowTarget();
    }

    void GetMouseInput() 
    {
        //Use Unity's input system to get mouse feedback
        float MouseXDelta = Input.GetAxis("Mouse X");
        float MouseYDelta = Input.GetAxis("Mouse Y");

        //Update the rotation of the camera pivot based on the feedback
        float finalYDelta = MouseSensistivity * MouseYDelta * Time.deltaTime;
        float finalXDelta = MouseSensistivity * MouseXDelta * Time.deltaTime;
        xRot -= finalYDelta;
        yRot += finalXDelta;
        xRot = Mathf.Clamp(xRot, minimumYAngle, maximumYAngle);
        transform.rotation = Quaternion.Euler(xRot, yRot, 0f);
    }

    void FollowTarget() 
    {
        //camera position following target
        transform.position = new Vector3(Target.position.x, transform.position.y, Target.position.z);
    }
}
