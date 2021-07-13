using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class robotboxcollider : MonoBehaviour {


    /// <summary>
    /// ROBOT BOX COLLIDER.
    /// This script detects if the robot touches a separator or the box, adverting the user this cube is not valid.
    /// </summary>
    /// 
    public Light MyLightSource;
    public Material FixedRobotMaterial; // Sometimes it happened that the robot material is changed, if at the latest second of the trial it was touching the wall. This prevents that.
    public Material RobotMaterial;
    private Color robotcolor;
    public GameObject ControllerGameObject; //Here We need the game object of the controller we want to attach to the robot
    protected SteamVR_TrackedObject ControllerTackedObject;
    public TrialMasterScript CurrentTrial;



    public SteamVR_Controller.Device ControllerDevice //this is what we handle in "update"
    {
        get
        {
            return SteamVR_Controller.Input((int)ControllerTackedObject.index);
        }
    }

    void Awake()
    {
        ControllerTackedObject = ControllerGameObject.GetComponent<SteamVR_TrackedObject>();
    }
    // /// // /// // /// // /// // /// //


    void Start()
    {
        MyLightSource.color = Color.red;
        robotcolor = FixedRobotMaterial.color;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag != "GrippableObject" & other.tag != "Box" & other.tag != "Ray" & ExperimentClockUtils.shouldWAMmove==true) // Only turns red if the current trial is ongoing.
        {

            // More info on vibrations
            // https://steamcommunity.com/app/358720/discussions/0/405693392914144440/
            ControllerDevice.TriggerHapticPulse(3999);
            RobotMaterial.color = Color.red;
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag != "GrippableObject" & other.tag != "Box")
        {

            RobotMaterial.color = robotcolor;
        }

    }


}