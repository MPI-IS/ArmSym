using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra; // This is necessary to use linear algebra while calculating the robot inverse kinematics analysis.
using System.IO;
using Assets.LSL4Unity.Scripts.Examples;


public class armsym_controlmodes : MonoBehaviour {
    /// <summary>
    /// This class allows a modularization of the control modes, in such a way tha that new control modes can be added by a user.
    /// We make use of delegates (https://unity3d.com/learn/tutorials/topics/scripting/delegates).
    /// Delegates are a simple way of encapsulating a method into an object. This object can be modified dinamically.
    /// Each control mode that we make is a method, but we only have to execute one of them at the time in runtime. 
    /// 
    /// In the beginning og the execution, we assign our delegate to the control mode of the experiment we are running. 
    /// During execution, our *update* method simply runs the method that was allocated in the delegate.. 
    /// 
    /// We instanciate some objects that will be needed in control; most importantly, a component armsym_robot and the controller of the HTC Vive. 
    /// 
    /// </summary>

    // Necessary for all control modes: 
    private armsym_robot robot_control;
    public GameObject robolab; 
    private GameObject[] robangles; // Theta angles of the robot 
    public TrialMasterScript CurrentTrial;
    private Vector3 kinemtask; // kinemtask is the task to be solved by the inverse kinematics. Pelb_tracker is pos of elbow with respect to tracker.
    private float aux_th5; // In controlmode 0, helps establishing the limits of th5.
    private RobotKinematics WAM; // Our WAM robot, inheritable from the other script. 
    delegate void SelfControlMode();   SelfControlMode controlmode_delegate;
    public armsym_logger Logger;
    

    // For control mode 0 and 1:
    public GameObject shouldertracker, elbowtracker;//This is intended to place the base of the robot on the shoulder.
    private Vector3 Pelb_tracker;

    // For control mode 2.
    public GameObject RedCursor; // An extra controller-based cursor. 
    public datamanagement MyDataManagement;

    // For control mode 3.
    public Assets.LSL4Unity.Scripts.Examples.DemoInletForFloatSamples DemoInlet;

    /// Some variables we need for the controller. Two tutorials from which we took small drops of code (April 2018):
    /// -https://www.raywenderlich.com/149239/htc-vive-tutorial-unity
    /// -http://academyofvr.com/intro-vive-development-introduction-setup/
    public GameObject ControllerGameObject; //Here We need the game object of the controller we want to attach to the robot
    protected SteamVR_TrackedObject ControllerTackedObject;
    public SteamVR_Controller.Device ControllerDevice //this is what we handle in "update"
    {
        get { return SteamVR_Controller.Input((int)ControllerTackedObject.index);}
    }

    // Awake calls stuff before "Start"
    void Awake()  { ControllerTackedObject = ControllerGameObject.GetComponent<SteamVR_TrackedObject>();}


    // START FUNCTION - We define our delegate with respect to the experiment.
    void Start(){
        robot_control = GetComponent<armsym_robot>();   // they take part on the same GameObjects 
        WAM = robot_control.WAM; // hold of them by reference.
        robangles = new GameObject[] { robot_control.T1, robot_control.T2, robot_control.T3, robot_control.T4, robot_control.T5, robot_control.T6, robot_control.T7 }; // These are the transforms on rotations.
        aux_th5 = 0f; // Dummy


        switch (AAExperimentMasterScript.controlmode_idx) { // This is easy because controlmode is static!! 

            // Here, different control modes (which we set in the configuration scene) can allocate a method in the delegate. 

            case 0:

                controlmode_delegate = ControlMode0;
                break;

            case 1:
                controlmode_delegate = ControlMode1;
                break;

            case 2:
                controlmode_delegate = ControlMode2;
                break;

            case 3:
                controlmode_delegate = ControlMode3_biosignal;
                break;

            default:
                { }


                break;
        }
    }

    
    // Update is called once per frame. Here, we simply run the delegate
    void Update()
    {

            if (ExperimentClockUtils.shouldWAMmove == true)  // We only our robot when the trials sare in trial phase 1 (that is, when the user is allowed to move the robot), OR when there is a practice trial (because practice trial don't have phases.
            {
                 controlmode_delegate();
                 Logger.addData(ControllerGameObject.transform.position, robolab.transform.position, ControllerDevice.GetAxis(), robot_control.GetStatus()); // public void addData(Vector3 controllerposition, Vector3 baserobotposition, Vector2 AnalgousController, float[] kinemang)

            }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    /// <summary>
    /// METHODS FOR CONTROL MODES.
    /// Each control mode must be new method, which defines what is going to happen with the robot during the execution of "update".
    /// This means that the control mode method will be called exactly once per frame. Thus, you should write the things that you want to happen 
    /// with the robot on frame-way basis. For this, you can use logical gates or implement your own delegates.
    /// 
    /// To simplify things, on the script 'Armsym_robot' we have written three different controllers that you can use while creating your own control mode:
    /// 1. A "direct control" - Use ExecuteAngle to simply configure the robot in the position that you want.
    /// 2. Open-loop speed control  - Use the overload ExecuteSpeed to move the joints of the robot with a certain speed.
    /// 3. A go-to controller, in which the movement profile is linear with time (i.e., the robot makes constant steps towards the target).  - Use
    /// "Set_target" to establish a rarget for this controller, and then in your control_mode use "ReachForTarget" or "AwayFromTarget" to navigate the robot to and
    /// from the target.
    /// 
    /// For the moment, we do not have a control with realistic acceleration profiles. You are welcome to add it to arm_sym robot!
    /// 
    /// </summary>


    //// CONTROL MODE 0:
    private void ControlMode0() {
        robolab.transform.position = shouldertracker.transform.position;

        robolab.transform.localEulerAngles = new Vector3(0, shouldertracker.transform.eulerAngles.y + 130, 0); // shoulder rotates with the tracker. 

        if (robangles[4].transform.localEulerAngles.z >= 180) { aux_th5 = robangles[4].transform.localEulerAngles.z - 360; }
        else { aux_th5 = robangles[4].transform.localEulerAngles.z; } // This heeuristic is necessary!

        if (ControllerDevice.GetAxis() != Vector2.zero)
        {

            if ((robangles[3].transform.localEulerAngles.z > 20 & robangles[3].transform.localEulerAngles.z < 140) | (robangles[3].transform.localEulerAngles.z <= 20 & ControllerDevice.GetAxis().y > 0) | (robangles[3].transform.localEulerAngles.z >= 140 & ControllerDevice.GetAxis().y < 0))
            {
                robot_control.ExecuteSpeed(robangles[3], 3f * ControllerDevice.GetAxis().y);

            }


            if ((aux_th5 > -90 & aux_th5 < 90) | (aux_th5 <= -90 & ControllerDevice.GetAxis().x > 0) | (aux_th5 >= 90 & ControllerDevice.GetAxis().x < 0))
            {
                robot_control.ExecuteSpeed(robangles[4], 3f * ControllerDevice.GetAxis().x);

            }
        }
        
    }


    //// CONTROL MODE 1:
    private void ControlMode1()
    {
        robolab.transform.position = shouldertracker.transform.position;

        kinemtask = (ControllerGameObject.transform.position - shouldertracker.transform.position);
        Pelb_tracker = elbowtracker.transform.position - shouldertracker.transform.position;
        WAM.phi = ((float)Math.Atan(Pelb_tracker.y / Pelb_tracker.x));
        if (WAM.phi > 0)
        {
            WAM.phi = -WAM.phi;
        }



        WAM.InverseKinematics(kinemtask.x, kinemtask.y, kinemtask.z, WAM.phi, ControllerGameObject.transform.forward, ControllerGameObject.transform.up);
        robot_control.ExecuteAngle(WAM.kinemang);


    }


    ///// New control mode!
    private void ControlMode2() {
        
        robolab.transform.position = new Vector3(0.4f, 1.3f, -0.24f);
        if (!RedCursor.activeSelf) { RedCursor.SetActive(true); }

        if (ControllerDevice.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
            kinemtask = (ControllerGameObject.transform.position - robolab.transform.position);
            WAM.phi = 0f * (float)Math.PI;
            WAM.InverseKinematics(kinemtask.x, kinemtask.y, kinemtask.z, WAM.phi, ControllerGameObject.transform.forward, ControllerGameObject.transform.up);
            robot_control.SetTarget(WAM.kinemang, 1);
            MyDataManagement.ArmSym_Message("New Command"); 



        }

        if (!robot_control.reached_target) {
            robot_control.ReachForTarget();

        }
    }



    //// Experimental - Biosignal control
    private void ControlMode3_biosignal()
    {
        robolab.transform.position = new Vector3(0.4f, 1.3f, -0.24f);
        robot_control.ExecuteSpeed(robangles[3], 3f * DemoInlet.signal); /// DemoInlet.signal is a float that is updated every time a biosignal arrives from LSL.


    }

    
    ///// YOUR METHOD HERE

    private void YOURMETHODHERE() {
        

    }






}










