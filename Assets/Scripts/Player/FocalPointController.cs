using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FocalPointController : NetworkBehaviour
{
    [SerializeField] private float sensitivity = 30f;
    [SerializeField] private Vector2 turn;
    [SerializeField] private float xRotLimit = 90;
    [SerializeField] private float currentXRot;

    void LateUpdate()
    {

        if (Input.GetKey(KeyCode.Mouse1))
        {
            turn.y += Input.GetAxis("Mouse Y") * sensitivity;
            if (turn.y < -xRotLimit + 11.25f)
            {
                turn.y = -xRotLimit + 11.25f;
            }
            if (turn.y > xRotLimit + 11.25f)
            {
                turn.y = xRotLimit + 11.25f;
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                //turn.y = transform.eulerAngles.x;
                Debug.Log("down down down down down");
            }
            Cursor.lockState = CursorLockMode.Locked;
            transform.localRotation = Quaternion.Euler(-turn.y, 0, 0);
        }
        else
        {
            currentXRot = transform.eulerAngles.x;
            turn.y = 0;
            Cursor.lockState = CursorLockMode.None;
            transform.localRotation = Quaternion.Euler(currentXRot, 0, 0);
            //Input.GetKey(KeyCode.)
        }
    }

}
