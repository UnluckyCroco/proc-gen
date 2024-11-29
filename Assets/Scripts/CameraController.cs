using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraSpeed = 10f;
    public float speedMultiplier = 5f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float verticalMove = 0f;
        float superSpeed = 1f;

        if (Input.GetKey(KeyCode.Q))
        {
            verticalMove = 1f;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            verticalMove = -1f;
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            superSpeed = speedMultiplier;
        }

        Vector3 move = new(Input.GetAxis("Horizontal") * superSpeed, Input.GetAxis("Vertical") * superSpeed, verticalMove * superSpeed);
        transform.Translate(move * Time.deltaTime * cameraSpeed);
    }
}
