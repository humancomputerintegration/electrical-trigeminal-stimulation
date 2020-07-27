using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
/*for IP*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityOSC;
/*for Unity*/

public class Unity_Client : MonoBehaviour
{

    [Header("Stimulation Settings")]
    //public float interval  = 0.1f; // save positions each 0.1 second
    public float tSample   = 10.0f; // sampling starts after this time
    public Transform OdorSource; // target location
    public int StimulationInterval = 20;
    public Text OutputText; // to update current state (ETS on left / right / adjusting intensity)
    private Vector3 relativePosition; //the relative position of the odor source in Camera's coordinate system
    private OSCServer myServer;
    private bool calibrated = false; // by default, it is not calibrated.
    public static float IntensityFactorLeft = 1.0f; // the intensity factor of left side
    public static float IntensityFactorRight = 1.0f; // the intensity factor of right side
     /*
      * PYTHON SERVER COMMUNICATION.
      * brief: for communicating with the RehaMove system via Python script over a server.
      */
    [Header("OSC Settings")]
    public string outIP    = "127.0.0.1";
    public int outPort     = 9001;
    public int inPort      = 9998;
    public int bufferSize  = 100; // buffer size of the application (stores 100 messages from different servers)
 

    // Start is called before the first frame update
    void Start()
    {
        // init OSC
        OSCHandler.Instance.Init(); 

         // Initialize OSC clients (transmitters)
        OSCHandler.Instance.CreateClient("myClient", IPAddress.Parse(outIP), outPort);
        
         // Initialize OSC servers (listeners)
        myServer = OSCHandler.Instance.CreateServer("myServer", inPort);

         // Set buffer size (bytes) of the server (default 1024)
        myServer.ReceiveBufferSize = 1024;

         // Set the sleeping time of the thread (default 10)
        myServer.SleepMilliseconds = 10;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (calibrated) {
            if (Time.frameCount % StimulationInterval == 0)
            {
                relativePosition = transform.InverseTransformPoint(OdorSource.position);
                if (relativePosition.x < 0) {
                    //PulseType.text = stimulator.stimulate("left");
                    OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation", 1);
                }
                else if (relativePosition.x > 0) {
                    //PulseType.text = stimulator.stimulate("right");
                    OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation", 0);
                }
            }
        }
    }

    void InitializeCalibration()
    {
        // Calibration should happen in a separate function that we then create an Editor button for.
        //calibrated = true;
    }

    public static void SendStimulation(int type) // 1 for left and 0 for right
    {
        OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation", type);
    }

    public static void ChangeParameter(float LeftFactor, float RightFactor){
        IntensityFactorLeft = LeftFactor;
        IntensityFactorRight = RightFactor;
        string message = IntensityFactorLeft.ToString() + ' ' + IntensityFactorRight.ToString();
        OSCHandler.Instance.SendMessageToClient("myClient", "/parameter", message);
    }
}
