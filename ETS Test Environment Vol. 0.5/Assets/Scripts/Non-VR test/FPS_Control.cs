using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS_Control: MonoBehaviour
{
    public float mouseSensitivity = 100.0f;
    public float movingSpeed = 2.0f;

    Transform Camera;
    float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Camera = GameObject.FindGameObjectWithTag("eyes").transform;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Camera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetKey("w"))
            transform.Translate(Vector3.forward * movingSpeed * Time.deltaTime);
        if (Input.GetKey("s"))
            transform.Translate(Vector3.back * movingSpeed * Time.deltaTime);
        if (Input.GetKey("a"))
            transform.Translate(Vector3.left * movingSpeed * Time.deltaTime);
        if (Input.GetKey("d"))
            transform.Translate(Vector3.right * movingSpeed * Time.deltaTime);
        if (Input.GetKey("q"))
            transform.Translate(Vector3.up * movingSpeed * Time.deltaTime);
        if (Input.GetKey("e"))
            transform.Translate(Vector3.down * movingSpeed * Time.deltaTime);

    }
}
