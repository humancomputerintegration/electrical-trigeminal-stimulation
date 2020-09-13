/*
 * By: Jingxuan Wen, Jas Brooks
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class MovementLog : MonoBehaviour
{
    public Transform Target = null;
    public GameObject Nose = null;
    public Transform rightController = null;
    public int loggingTimeInterval = 20; // milliseconds.
    public string ParticipantNumber = "Jingxuan";
    public string SaveLogFileTo = "C:\\Users\\The Lab\\Desktop\\ETS test environment\\ETS Test Environment Vol. 0.5\\Movement\\";

    /*
    private List<float> timeLog = new List<float>();
    private List<Vector3> positionLog = new List<Vector3>();
    private List<Vector3> directionLog = new List<Vector3>();
    */
    StreamWriter sw = null;
    private bool endLine = false;
    bool finalDeclaration = false;
    
    void Start()
    {
        bool etsStatus = Nose.activeInHierarchy; // Check if the Nose GameObject is being used.
        string etsTitle = etsStatus ? "_ETSon" : "_ETSoff";
        string filename = SaveLogFileTo + ParticipantNumber + etsTitle + ".txt";
        sw = new StreamWriter(filename);
        endLine = false;
        sw.WriteLine("Target " + Target.position.ToString("f4"));
    }


    private float time = 0.0f;

    void Update()
    {
        /*
         * MOVING THIS TO FILE TITLE.  
            if (!ControllerMapping.Search)
            {
                return;
            }
            if (ControllerMapping.Found)
            {
                if (!endLine)
                {
                    sw.WriteLine(Target.transform.position.ToString("f4"));
                    if (settings.ETSMode)
                    {
                        sw.WriteLine("ETS ON");
                        sw.WriteLine(Target.GetComponent<CapsuleCollider>().radius.ToString("f4"));
                        if (settings.RadiusMode)
                        {
                            GameObject R_in = Target.transform.Find("R_in").gameObject;
                            GameObject R_out = Target.transform.Find("R_out").gameObject;
                            sw.WriteLine("Radius " + R_out.GetComponent<CapsuleCollider>().radius.ToString("f4") + " " + R_in.GetComponent<CapsuleCollider>().radius.ToString("f4"));
                        }
                        else
                        {
                            sw.WriteLine("Side");
                        }
                    }
                    else
                    {
                        sw.WriteLine("ETS OFF");
                    }
                    sw.Flush();
                    sw.Close();
                    endLine = true;
                }
                return;
            }
        */
        time += Time.deltaTime;

        if ((time * 1000f >= loggingTimeInterval) && !finalDeclaration)
        {
            // Log the current position and lookAt direction.
            sw.WriteLine(Time.time.ToString() + ", " + transform.position.ToString("f4") + ", " + transform.forward.ToString("f4"));
            sw.Flush();
            time = 0f; // Restart time.
        }

        if (Input.GetKey(KeyCode.D))
        {
            DeclarePosition(rightController.position);
        }
    }

    public void DeclarePosition (Vector3 declaration)
    {
        finalDeclaration = true;
        sw.WriteLine(Time.time.ToString() + ", DECLARED POSITION , " + declaration.ToString("f4"));
        sw.Close(); // End once they declare final location.
    }

    /*
    private void OnApplicationQuit()
    {
        using (StreamWriter outputFile = new StreamWriter("MovementLog.txt"))
        {
            outputFile.WriteLine("time ---> nose (camera) position ---> nose(camera) direction");
            int count = 0;
            foreach (float time in timeLog)
            {
                outputFile.WriteLine(time + ", " + positionLog[count] + ", " + directionLog[count]);
                count++;
            }
        }
    }*/
}
