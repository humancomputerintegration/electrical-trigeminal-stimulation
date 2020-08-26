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
    public GameObject Target = null;
    public int LoggingTimeInterval = 10; // Milliseconds.
    public string ParticipantNumber = "Jingxuan";
    public string SaveLogFileTo = "C:\\Users\\The Lab\\Desktop\\ETS test environment\\ETS Test Environment Vol. 0.5\\Movement\\";

    /*
    private List<float> timeLog = new List<float>();
    private List<Vector3> positionLog = new List<Vector3>();
    private List<Vector3> directionLog = new List<Vector3>();
    */
    StreamWriter sw = null;
    private bool endLine = false;
    
    void Start()
    {
        /*
        timeLog.Clear();
        positionLog.Clear();
        directionLog.Clear();
        */
        
        string ets = settings.ETSMode ? "_ETSon" : "_ETSoff";
        string mode = settings.RadiusMode ? "_Radius" : "_Side";
        mode = settings.ETSMode ? mode : "";
        string filename = SaveLogFileTo + ParticipantNumber + ets + mode + ".txt";
        sw = new StreamWriter(filename);
//        sw.WriteLine("time ---> nose (camera) position ---> nose(camera) direction");
//        sw.Flush();
        endLine = false;
    }
    
    void Update()
    {
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

        if(Time.frameCount % LoggingTimeInterval == 0)
        {
            // time ---> nose (camera) position ---> nose(camera) direction
            sw.WriteLine(Time.time + ", " + transform.position.ToString("f4") + ", " + transform.forward.ToString("f4"));
            sw.Flush();
            /*
            timeLog.Add(Time.time);
            positionLog.Add(transform.position);
            directionLog.Add(transform.forward);
            */
        }
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
