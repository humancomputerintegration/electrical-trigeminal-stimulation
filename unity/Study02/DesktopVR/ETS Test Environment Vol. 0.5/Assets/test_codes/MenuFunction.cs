using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuFunction : MonoBehaviour
{

    public GameObject Menu;

    public void ConfirmAndContinue()
    {
        Menu.SetActive(false);
        Time.timeScale = 1f;
        ControllerMapping.TestIsPaused = false;
        //Debug.Log("Confirm button pressed");
    }

    public void LeftCheck()
    {
        Unity_Client.SendStimulation(1);
    }

    public void LeftIncrease()
    {
        float l = Unity_Client.IntensityFactorLeft;
        float r = Unity_Client.IntensityFactorRight;
        l = l > 5 ? 5.5f : l + 0.5f;
        Unity_Client.ChangeParameter(l,r);
        Unity_Client.SendStimulation(1);
    }
    
    public void LeftDecrease()
    {
        float l = Unity_Client.IntensityFactorLeft;
        float r = Unity_Client.IntensityFactorRight;
        l = l < 1 ? 0.5f : l - 0.5f;
        Unity_Client.ChangeParameter(l,r);
        Unity_Client.SendStimulation(1);
    }

    public void RightCheck()
    {
        Unity_Client.SendStimulation(0);
    }

    public void RightIncrease()
    {
        float l = Unity_Client.IntensityFactorLeft;
        float r = Unity_Client.IntensityFactorRight;
        r = r > 5 ? 5.5f : r + 0.5f;
        Unity_Client.ChangeParameter(l,r);
        Unity_Client.SendStimulation(0);
    }
    
    public void RightDecrease()
    {
        float l = Unity_Client.IntensityFactorLeft;
        float r = Unity_Client.IntensityFactorRight;
        r = r < 1 ? 0.5f : r - 0.5f;
        Unity_Client.ChangeParameter(l,r);
        Unity_Client.SendStimulation(0);
    }

    
}
