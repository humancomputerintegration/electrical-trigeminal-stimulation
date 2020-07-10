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

public class Unity_Client_2 : MonoBehaviour
{
    private bool ETS_On;

    private bool Radius;

    [Header("Stimulation Settings")]
    //public float interval  = 0.1f; // save positions each 0.1 second
    public float tSample   = 10.0f; // sampling starts after this time
    public Transform OdorSource; // target location
    public int StimulationInterval = 20;
    //public Text OutputText; // to update current state (ETS on left / right / adjusting intensity)
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

    private bool inZone = false;

    private bool[] visit = new bool[5]; // for possible multiple radiuses, 0 for the furthest radius.

    // Start is called before the first frame update
    void Start()
    {
        ETS_On = settings.ETSMode;
        Radius = settings.RadiusMode;

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

        for (int i = 0; i < 5; i++) visit[i] = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!inZone) return;

        if (!ETS_On) return;

        if (!ControllerMapping.Search)
        {
            return;
        }
        if (ControllerMapping.Found)
        {
            return;
        }

        if (Time.frameCount % StimulationInterval == 0)
        {
            relativePosition = transform.InverseTransformPoint(OdorSource.position);
            /*
            if (relativePosition.x < 0) {
                //PulseType.text = stimulator.stimulate("left");
                OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation", 1);
            }
            else if (relativePosition.x > 0) {
                //PulseType.text = stimulator.stimulate("right");
                OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation", 0);
            }*/
            string position = relativePosition.x.ToString() + ' ' + relativePosition.y.ToString() + ' ' + relativePosition.z.ToString();
            string zone = "0";
            for (int i = 4; i >= 0; i--)
            {
                if (visit[i])
                {
                    zone = i.ToString();
                    break;
                }
            }
            string message = position + ' ' + zone;

            if (Radius)
            {
                OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation_R", message);
            }
            else
            {
                OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation_S", message);
            }
            
            //Debug.Log(message);
        }
    }

    /*
        void InitializeCalibration()
        {
            // Calibration should happen in a separate function that we then create an Editor button for.
        }

        public static void SendStimulation(int type) // 1 for left and 0 for right
        {
            OSCHandler.Instance.SendMessageToClient("myClient", "/stimulation", type);
        }

        public static void ChangeParameter(int LeftFactor, int RightFactor){
            IntensityFactorLeft = LeftFactor;
            IntensityFactorRight = RightFactor;
            string message = IntensityFactorLeft.ToString() + ' ' + IntensityFactorRight.ToString();
            OSCHandler.Instance.SendMessageToClient("myClient", "/parameter", message);
        }
    */

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Odors") inZone = true;
        if (other.gameObject.tag == "inner radius") visit[1] = true;
        if (other.gameObject.tag == "outward radius") visit[0] = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Odors") inZone = false;
        if (other.gameObject.tag == "inner radius") visit[1] = false;
        if (other.gameObject.tag == "outward radius") visit[0] = false;
    }
}
