using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // LINQ: https://unity3d.college/2017/07/01/linq-unity-developers/


public class armsym_hand : MonoBehaviour {

    public GameObject PPa, PPb, PPc; //Proximal phalanges
    public GameObject DPa, DPb, DPc; //Distal phalanges
    public GameObject Ca, Cb, Cc; // Colliders
    private GameObject[] PP, DP; // Vectors
    private bool closing, opening, closed, open; // Status 
    private bool attachment; 
    private string status; // status open/closed 
    private float percentage; // Percentage of how open is the hand
    private objectsensor CaCM, CbCM, CcCM; //Checks if collides are colliding 
    private GameObject holdingitem;
    private List<string> AttachedObjects;

    public datamanagement TrialData;

    /// <summary>
    /// Some variables we need for the controller. Two tutorials from which I took small drops of code (April 2018):
    /// -https://www.raywenderlich.com/149239/htc-vive-tutorial-unity
    /// -http://academyofvr.com/intro-vive-development-introduction-setup/
    /// </summary>
    public GameObject ControllerGameObject; //Here We need the game object of the controller we want to attach to the robot
    protected SteamVR_TrackedObject ControllerTackedObject;
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

        open = true; // State machine 
        closed = false;
        closing = false;
        opening = false;
        attachment = false;

        AttachedObjects = new List<string>(); 

        PP = new GameObject[] { PPa, PPb, PPc };
        DP = new GameObject[] { DPa, DPb, DPc };
        CaCM = Ca.GetComponent<objectsensor>(); // Gets component
        CbCM = Cb.GetComponent<objectsensor>();
        CcCM = Cc.GetComponent<objectsensor>();

        status = "Open"; // At the beginning the hand is opened
        percentage = 0f;

        // We set ip the hand: 
        for (int i = 0; i < 3; i++)
        {
            PP[i].transform.localEulerAngles = new Vector3(90, 0, 0);
            DP[i].transform.localEulerAngles = new Vector3(180, 0, 0);

        }



    }

    // Update is called once per frame
    void FixedUpdate()
    {


        //MODIFY THE INPUT IF YOU WISH TO REPLACE THE TRIGGER-TO-GRAB SYSTEM.
        // Otherwise call frame-wise StateMachineOpenHand() and StateMachineCloseHand(). 
        if (ControllerDevice.GetPress(SteamVR_Controller.ButtonMask.Trigger) == true) { // Back button pressed
            StateMachineCloseHand();

        }

        if (ControllerDevice.GetPress(SteamVR_Controller.ButtonMask.Trigger) == false) { // button open
            StateMachineOpenHand();

        }



    }


    ///// State machines.
    // What does the hand do upon opening and closing statements? 

    // State machine to open the hand:
    // If it is closed, starts opening it. If it has something attached, releases it. If it was opening already: keep opening. If it was closing, start opening. 
    private void StateMachineOpenHand()
    {
  
            if (closed)
            {
                closed = false;
                opening = true;
                status = "opening";
                openhand(PP, DP, ref percentage);
            }

            if (attachment)
            {
                DeattachObjects(AttachedObjects);
                attachment = false;
                //TrialData.releasedacube(Time.time);
                TrialData.ArmSym_Message("ReleasedACube");
            }

            if (opening)
            {
                if (percentage <= 0)
                {
                    percentage = 0;
                    opening = false;
                    open = true;
                    status = "open";
                }

                if (percentage > 0)
                {
                    openhand(PP, DP, ref percentage);
                }
            }


            if (closing)
            {
             
                closing = false;
                opening = true;
                status = "opening";

                if (percentage <= 0)
                {
                    percentage = 0;
                    opening = false;
                    open = true;
                    status = "open";
                }

                if (percentage > 0)
                {
                    openhand(PP, DP, ref percentage);
                }
 
            }
    }



    // State machine to close the hand:
    // If it is open, starts closing it. If it is closing, keeps closing it, and if it detects and object, stops there and "grabs" the object".  If its opening, it begins closing.  

    private void StateMachineCloseHand() {
        if (open)
        {
            open = false;
            closing = true;
            status = "closing";
            closehand(PP, DP, ref percentage);

        }

        if (closing)
        {
            if (percentage >= 100)
            {
                percentage = 100;
                closing = false;
                closed = true;
                status = "closed";
            }


            if (DetectObjects() != null)
            {

                if (DetectObjects().Any())
                {
                    AttachedObjects = DetectObjects();
                    attachment = true;

                    AttachObjects(AttachedObjects, DPa);
                    //TrialData.gotacube(Time.time);
                    TrialData.ArmSym_Message("GotACube");


                    closing = false;
                    closed = true;
                    status = "closed";
                }

            }

            if (percentage < 100 & closing)
            {
                closehand(PP, DP, ref percentage);
            }
        }
        if (opening)
        {
            opening = false;
            closing = true;
            status = "closing";

            if (percentage >= 100)
            {
                percentage = 100;
                closing = false;
                closed = true;
                status = "closed";

            }

            if (percentage < 100)
            {
                closehand(PP, DP, ref percentage);
            }

        }
    }




    /// <summary>
    /// closehand and openhand perform the operation and modify the "percentage of opennes".
    ///  Speeds are purely arbitrary
    /// </summary>



    private static void closehand(GameObject[] PP, GameObject[] DP, ref float percentage) {

        for (int i = 0; i < 3; i++)
        {
            PP[i].transform.Rotate(Vector3.right * Time.deltaTime * 190);
            DP[i].transform.Rotate(-Vector3.right * Time.deltaTime * 190);
            percentage = percentage + Time.deltaTime * 175;



        }


    }


    private static void openhand(GameObject[] PP, GameObject[] DP, ref float percentage)
    {

        for (int i = 0; i < 3; i++)
        {
            PP[i].transform.Rotate(-Vector3.right * Time.deltaTime * 190);
            DP[i].transform.Rotate(Vector3.right * Time.deltaTime * 190);
            percentage = percentage - Time.deltaTime * 175;

        }


    }


    // Detects Objects
    private List<string> DetectObjects() {
        if ((CaCM.colisionidx & CbCM.colisionidx & CbCM.colisionidx) == true)
        {

            
            return CaCM.ObjectsAttached.Intersect(CbCM.ObjectsAttached).ToList().Intersect(CcCM.ObjectsAttached).ToList();
           

        }



        return null;

        

    }


    // Attaches item

    private static void AttachObjects(List<string> Objects, GameObject DPa)
    {

        foreach (var item in Objects)
        {
            var holdingitem = GameObject.Find(item);
            holdingitem.GetComponent<Rigidbody>().isKinematic = true;
            holdingitem.transform.parent = DPa.transform;
        }

        


    }


    // Deataches item
    private static void DeattachObjects(List<string> Objects)
    {
        foreach (var item in Objects)
        {
            var holdingitem = GameObject.Find(item);
            holdingitem.GetComponent<Rigidbody>().isKinematic = false;
            holdingitem.transform.parent = null;
            
            if (holdingitem.GetComponent<armsym_object>().velocity.magnitude > 1f){ holdingitem.GetComponent<Rigidbody>().AddForce(20f * holdingitem.GetComponent<armsym_object>().velocity); }
            
                
        }


    }



}


