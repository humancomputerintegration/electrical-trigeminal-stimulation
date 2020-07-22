using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ControllerMapping : MonoBehaviour
{
    // Reference to the action
    public SteamVR_Action_Boolean MenuOnOff = null;

    // Reference to the hand
    public SteamVR_Input_Sources handType;

    // Reference to the Calibration Menu
//    public GameObject CalibrationMenu;
    public ContextMenu test_menu = null;

    // The state indicating that the test is paused (for calibration)
    public static bool TestIsPaused = false;
    public static bool Search = false;
    public static bool Found = false;



    // Start is called before the first frame update
    void Start()
    {
        MenuOnOff.AddOnStateUpListener(MenuButtonPressed, handType);
    }

    public void MenuButtonPressed(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /*
            if (TestIsPaused)
            {
                CalibrationMenu.SetActive(false);
                Time.timeScale = 1f;
                TestIsPaused = false;
            }
            else
            {
                CalibrationMenu.transform.position = transform.position + transform.forward * 2.5f;
                CalibrationMenu.transform.forward = transform.forward;
                CalibrationMenu.SetActive(true);
                Time.timeScale = 0f;
                TestIsPaused = true;
            }
        */
        if (!Search) Search = true;
        else
        {
            if (!Found)
                Found = true;
            else
                return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
