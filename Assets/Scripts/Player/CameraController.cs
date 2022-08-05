using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraController : MonoBehaviour
{
    public GameObject focalPoint;
    public Vector3 offset = new Vector3(0, 3, -6);
    public float scrollInput;
    public Vector3 scrollIn = new Vector3(0, -0.75f, 2);
    public Vector3 scrollOut = new Vector3(0, 0.75f, -2);
    private float scrollInLimitY = 0.75f;
    private float scrollOutLimitY = 10;
    public Vector3 focusPos;
    public Vector2 turn;
    public float sensitivity = 30;

    [SerializeField] private float xRotLimit = 90f;

    private void Awake()
    {
        focalPoint = transform.parent.gameObject;

    }

    void LateUpdate()
    {
        scrollInput = Input.GetAxis("Mouse ScrollWheel");
        focusPos = focalPoint.transform.position;
        //CameraRotationLimits();
        // Debug.Log(focalPoint.transform.localRotation.x);

        //allows changing camera position using scroll wheel
        if (scrollInput > 0f && transform.position.y > scrollInLimitY)
        {
            offset += scrollIn;
        }
        else if (scrollInput < 0f && transform.position.y < scrollOutLimitY)
        {
            offset += scrollOut;
        }


        //if (Input.GetKey(KeyCode.Mouse1))
        //{
        //    turn.y += Input.GetAxis("Mouse Y") * sensitivity;
        //    if(turn.y < -xRotLimit + 11.25f)
        //    {
        //        turn.y = -xRotLimit + 11.25f;
        //    }
        //    if (turn.y > xRotLimit + 11.25f)
        //    {
        //        turn.y = xRotLimit + 11.25f;
        //    }
        //    if (Input.GetKeyDown(KeyCode.Mouse1))
        //    {
        //        turn.y += focalPoint.transform.eulerAngles.x;
        //    }
        //    Cursor.lockState = CursorLockMode.Locked;
        //    focalPoint.transform.localRotation = Quaternion.Euler(-turn.y, 0, 0);
        //}
        //else
        //{
        //    turn.y = 0;
        //    Cursor.lockState = CursorLockMode.None;
        //    //focalPoint.transform.localRotation = Quaternion.Euler(focalPoint.transform.localRotation.y, 0, 0);

        //}
        transform.position = focusPos + focalPoint.transform.TransformDirection(offset);
    }

    private void CameraRotationLimits()
    {
        Vector3 fPEulerAngles = focalPoint.transform.localRotation.eulerAngles;
        fPEulerAngles.y = (fPEulerAngles.y > 180) ? fPEulerAngles.y - 360 : fPEulerAngles.y;
        fPEulerAngles.y = Mathf.Clamp(fPEulerAngles.y, -xRotLimit, xRotLimit);
        focalPoint.transform.localRotation = Quaternion.Euler(fPEulerAngles);
        //if(focalPoint.transform.localRotation.x > xRotLimit)
        //{
        //    focalPoint.transform.localRotation = Quaternion.Euler(xRotLimit, focalPoint.transform.localRotation.y, 0);
        //    Debug.Log("TOO FAR");
        //}

        //if (focalPoint.transform.localRotation.x < -xRotLimit)
        //{
        //    focalPoint.transform.localRotation = Quaternion.Euler(-xRotLimit, focalPoint.transform.localRotation.y, 0);
        //    Debug.Log("TOO FAR");
        //}
    }

}