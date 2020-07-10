using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class PlayerMovement : MonoBehaviour
{
    public int logFrequency = 10;

    public CharacterController controller;

    public float speed = 10f;
    public float gravity = -9.8f;
    public float jumpHeight = 2f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Transform cameraDirection;
    private List<float> timeLog = new List<float>();
    private List<Vector3> positionLog = new List<Vector3>();
    private List<Vector2> rotationLog = new List<Vector2>();

    Vector3 velocity;
    bool isGrounded;

    void Start()
    {
        timeLog.Clear();
        positionLog.Clear();
        rotationLog.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z);

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if(Time.frameCount % logFrequency == 0)
        {
            timeLog.Add(Time.time);
            positionLog.Add(cameraDirection.position);
            Vector3 temp = cameraDirection.rotation.eulerAngles;
            rotationLog.Add(temp.x * Vector2.right + temp.y * Vector2.up);
        }
    }

    private void OnApplicationQuit()
    {
        using(StreamWriter outputFile = new StreamWriter("MovementLog.txt"))
        {
            outputFile.WriteLine("time ---> nose (camera) position ---> nose(camera) rotation");
            int count = 0;
            foreach(float time in timeLog)
            {
                outputFile.WriteLine(time + ", " + positionLog[count] + ", " + rotationLog[count]);
                count++;
            }
        }
    }
}
